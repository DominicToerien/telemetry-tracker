using System.Runtime.InteropServices;
using telemetry_tracker.Telemetry.Lmu.Interop;

namespace telemetry_tracker.Tests;

public sealed class LmuInteropLayoutTests
{
    [Fact]
    public void TelemVect3_UsesExpectedSize()
    {
        Assert.Equal(24, Marshal.SizeOf<TelemVect3>());
    }

    [Fact]
    public void ApplicationState_UsesExpectedSize()
    {
        Assert.Equal(260, Marshal.SizeOf<ApplicationStateV01>());
    }

    [Fact]
    public void SharedMemoryGeneric_UsesExpectedSize()
    {
        Assert.Equal(332, Marshal.SizeOf<SharedMemoryGeneric>());
    }

    [Fact]
    public void SharedMemoryPathData_UsesExpectedSize()
    {
        Assert.Equal(1300, Marshal.SizeOf<SharedMemoryPathData>());
    }
}
