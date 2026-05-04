using System.ComponentModel;
using System.Runtime.InteropServices;

namespace telemetry_tracker.Telemetry.Lmu.Native;

internal static partial class Win32Native
{
    public const uint SYNCHRONIZE = 0x00100000;
    public const uint FILE_MAP_ALL_ACCESS = 0x000F001F;
    public const uint PAGE_READWRITE = 0x04;
    public const uint WAIT_OBJECT_0 = 0x00000000;
    public const uint WAIT_TIMEOUT = 0x00000102;
    public const uint INFINITE = 0xFFFFFFFF;
    public const int ERROR_ALREADY_EXISTS = 183;
    public static readonly IntPtr InvalidHandleValue = new(-1);

    [LibraryImport("kernel32.dll", EntryPoint = "OpenEventA", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    public static partial IntPtr OpenEvent(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, string name);

    [LibraryImport("kernel32.dll", EntryPoint = "OpenFileMappingW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial IntPtr OpenFileMapping(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, string name);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial IntPtr MapViewOfFile(IntPtr fileMappingObject, uint desiredAccess, uint fileOffsetHigh, uint fileOffsetLow, nuint numberOfBytesToMap);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnmapViewOfFile(IntPtr baseAddress);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileMappingA", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    public static partial IntPtr CreateFileMapping(IntPtr fileHandle, IntPtr securityAttributes, uint protect, uint maximumSizeHigh, uint maximumSizeLow, string name);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateEventA", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    public static partial IntPtr CreateEvent(IntPtr eventAttributes, [MarshalAs(UnmanagedType.Bool)] bool manualReset, [MarshalAs(UnmanagedType.Bool)] bool initialState, string name);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetEvent(IntPtr handle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(IntPtr handle);

    [LibraryImport("kernel32.dll", EntryPoint = "InterlockedCompareExchange")]
    public static unsafe partial int InterlockedCompareExchange(int* destination, int exchange, int comparand);

    [LibraryImport("kernel32.dll", EntryPoint = "InterlockedIncrement")]
    public static unsafe partial int InterlockedIncrement(int* addend);

    [LibraryImport("kernel32.dll", EntryPoint = "InterlockedDecrement")]
    public static unsafe partial int InterlockedDecrement(int* addend);

    [LibraryImport("kernel32.dll", EntryPoint = "InterlockedExchange")]
    public static unsafe partial int InterlockedExchange(int* target, int value);

    public static void ThrowLastWin32Error(string message)
    {
        throw new Win32Exception(Marshal.GetLastPInvokeError(), message);
    }
}
