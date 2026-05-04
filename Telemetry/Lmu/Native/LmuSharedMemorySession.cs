using System.Runtime.InteropServices;
using telemetry_tracker.Telemetry.Lmu.Interop;

namespace telemetry_tracker.Telemetry.Lmu.Native;

internal sealed class LmuSharedMemorySession : IDisposable
{
    private readonly SharedMemoryLock _sharedMemoryLock;
    private IntPtr _eventHandle;
    private IntPtr _mapHandle;
    private IntPtr _mappedView;

    private LmuSharedMemorySession(SharedMemoryLock sharedMemoryLock, IntPtr eventHandle, IntPtr mapHandle, IntPtr mappedView)
    {
        _sharedMemoryLock = sharedMemoryLock;
        _eventHandle = eventHandle;
        _mapHandle = mapHandle;
        _mappedView = mappedView;
    }

    public static LmuSharedMemorySession Open()
    {
        var sharedMemoryLock = SharedMemoryLock.Create();
        var eventHandle = Win32Native.OpenEvent(Win32Native.SYNCHRONIZE, inheritHandle: false, "LMU_Data_Event");
        if (eventHandle == IntPtr.Zero)
        {
            sharedMemoryLock.Dispose();
            Win32Native.ThrowLastWin32Error("Unable to open the LMU shared-memory event.");
        }

        var mapHandle = Win32Native.OpenFileMapping(Win32Native.FILE_MAP_ALL_ACCESS, inheritHandle: false, "LMU_Data");
        if (mapHandle == IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(eventHandle);
            sharedMemoryLock.Dispose();
            Win32Native.ThrowLastWin32Error("Unable to open the LMU shared-memory mapping.");
        }

        var mappedView = Win32Native.MapViewOfFile(mapHandle, Win32Native.FILE_MAP_ALL_ACCESS, 0, 0, (nuint)Marshal.SizeOf<SharedMemoryLayout>());
        if (mappedView == IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(mapHandle);
            _ = Win32Native.CloseHandle(eventHandle);
            sharedMemoryLock.Dispose();
            Win32Native.ThrowLastWin32Error("Unable to map the LMU shared-memory view.");
        }

        return new LmuSharedMemorySession(sharedMemoryLock, eventHandle, mapHandle, mappedView);
    }

    public bool WaitForUpdate(uint milliseconds) =>
        Win32Native.WaitForSingleObject(_eventHandle, milliseconds) == Win32Native.WAIT_OBJECT_0;

    public SharedMemoryObjectOut CopySnapshot()
    {
        if (!_sharedMemoryLock.Lock(Win32Native.INFINITE))
        {
            throw new InvalidOperationException("Unable to acquire the LMU shared-memory lock.");
        }

        try
        {
            var layout = Marshal.PtrToStructure<SharedMemoryLayout>(_mappedView);
            return layout.data;
        }
        finally
        {
            _sharedMemoryLock.Unlock();
        }
    }

    public void Dispose()
    {
        if (_mappedView != IntPtr.Zero)
        {
            _ = Win32Native.UnmapViewOfFile(_mappedView);
            _mappedView = IntPtr.Zero;
        }

        if (_mapHandle != IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(_mapHandle);
            _mapHandle = IntPtr.Zero;
        }

        if (_eventHandle != IntPtr.Zero)
        {
            _ = Win32Native.CloseHandle(_eventHandle);
            _eventHandle = IntPtr.Zero;
        }

        _sharedMemoryLock.Dispose();
    }
}
