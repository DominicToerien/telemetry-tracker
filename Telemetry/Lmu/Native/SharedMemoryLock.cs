using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace telemetry_tracker.Telemetry.Lmu.Native;

internal sealed unsafe class SharedMemoryLock : IDisposable
{
    private IntPtr _mapHandle;
    private IntPtr _waitEventHandle;
    private LockData* _dataPtr;

    private SharedMemoryLock()
    {
    }

    public static SharedMemoryLock Create()
    {
        var sharedMemoryLock = new SharedMemoryLock();
        try
        {
            sharedMemoryLock.Initialize();
            return sharedMemoryLock;
        }
        catch
        {
            sharedMemoryLock.Dispose();
            throw;
        }
    }

    public bool Lock(uint milliseconds = Win32Native.INFINITE)
    {
        const int maxSpins = 4000;

        for (var spins = 0; spins < maxSpins; spins++)
        {
            if (Interlocked.CompareExchange(ref Unsafe.AsRef<int>(&_dataPtr->busy), 1, 0) == 0)
            {
                return true;
            }

            Thread.SpinWait(1);
        }

        Interlocked.Increment(ref Unsafe.AsRef<int>(&_dataPtr->waiters));
        try
        {
            while (true)
            {
                if (Interlocked.CompareExchange(ref Unsafe.AsRef<int>(&_dataPtr->busy), 1, 0) == 0)
                {
                    return true;
                }

                var waitResult = Win32Native.WaitForSingleObject(_waitEventHandle, milliseconds);
                if (waitResult == Win32Native.WAIT_TIMEOUT)
                {
                    return false;
                }

                if (waitResult != Win32Native.WAIT_OBJECT_0)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Failed while waiting for the LMU shared-memory lock event.");
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref Unsafe.AsRef<int>(&_dataPtr->waiters));
        }
    }

    public void Unlock()
    {
        Interlocked.Exchange(ref Unsafe.AsRef<int>(&_dataPtr->busy), 0);
        if (Volatile.Read(ref Unsafe.AsRef<int>(&_dataPtr->waiters)) > 0)
        {
            _ = Win32Native.SetEvent(_waitEventHandle);
        }
    }

    public void Dispose()
    {
        if (_dataPtr != null)
        {
            _ = Win32Native.UnmapViewOfFile((IntPtr)_dataPtr);
            _dataPtr = null;
        }

        if (_waitEventHandle != IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(_waitEventHandle);
            _waitEventHandle = IntPtr.Zero;
        }

        if (_mapHandle != IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(_mapHandle);
            _mapHandle = IntPtr.Zero;
        }
    }

    private void Initialize()
    {
        _mapHandle = Win32Native.CreateFileMapping(
            Win32Native.InvalidHandleValue,
            IntPtr.Zero,
            Win32Native.PAGE_READWRITE,
            0,
            (uint)sizeof(LockData),
            "LMU_SharedMemoryLockData");

        if (_mapHandle == IntPtr.Zero)
        {
            Win32Native.ThrowLastWin32Error("Unable to create or open the LMU shared-memory lock mapping.");
        }

        _dataPtr = (LockData*)Win32Native.MapViewOfFile(_mapHandle, Win32Native.FILE_MAP_ALL_ACCESS, 0, 0, (nuint)sizeof(LockData));
        if (_dataPtr is null)
        {
            Win32Native.ThrowLastWin32Error("Unable to map the LMU shared-memory lock state.");
        }

        if (Marshal.GetLastPInvokeError() != Win32Native.ERROR_ALREADY_EXISTS)
        {
            Reset();
        }

        _waitEventHandle = Win32Native.CreateEvent(IntPtr.Zero, manualReset: false, initialState: false, "LMU_SharedMemoryLockEvent");
        if (_waitEventHandle == IntPtr.Zero)
        {
            Win32Native.ThrowLastWin32Error("Unable to create or open the LMU shared-memory lock event.");
        }
    }

    private void Reset()
    {
        _dataPtr->waiters = 0;
        _dataPtr->busy = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LockData
    {
        public int waiters;
        public int busy;
    }
}
