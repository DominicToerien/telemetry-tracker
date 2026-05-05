using telemetry_tracker.Telemetry.Lmu;
using telemetry_tracker.Telemetry.Lmu.Interop;

namespace telemetry_tracker.Tests;

public sealed class LmuTelemetryProviderTests
{
    [Fact]
    public void DefaultStatus_StartsDisconnected()
    {
        var provider = new LmuTelemetryProvider();

        var status = provider.GetStatus();
        var consoleSnapshot = provider.GetConsoleSnapshot();

        Assert.Equal("lmu", status.Provider);
        Assert.False(status.Connected);
        Assert.NotNull(status.Message);
        Assert.False(consoleSnapshot.Connected);
        Assert.NotNull(consoleSnapshot.Message);
    }

    [Fact]
    public void ApplySharedMemorySnapshot_UpdatesScoringStateWithoutTelemetry()
    {
        var provider = new LmuTelemetryProvider();
        var snapshot = CreateSnapshot();
        snapshot.generic.events[(int)SharedMemoryEvent.SME_UPDATE_SCORING] = 1;
        snapshot.scoring.scoringInfo.mNumVehicles = 2;
        snapshot.scoring.scoringInfo.mSession = 10;
        snapshot.scoring.scoringInfo.mTrackName = "Le Mans";
        snapshot.scoring.scoringInfo.mPlayerName = "Driver";
        snapshot.scoring.vehScoringInfo[0].mDriverName = "Driver A";
        snapshot.scoring.vehScoringInfo[1].mDriverName = "Driver B";

        provider.ApplySharedMemorySnapshot(snapshot, DateTimeOffset.UtcNow);

        var status = provider.GetStatus();
        var debug = provider.GetDebugSnapshot();

        Assert.True(status.Connected);
        Assert.Equal(nameof(SharedMemoryEvent.SME_UPDATE_SCORING), status.LastEvent);
        Assert.NotNull(status.LastScoringUpdateUtc);
        Assert.Null(status.LastTelemetryUpdateUtc);
        Assert.Equal(2, debug.ScoringVehicleCount);
        Assert.Equal("Le Mans", debug.TrackName);
    }

    [Fact]
    public void ApplySharedMemorySnapshot_UpdatesTelemetryStateWithoutPlayerVehicle()
    {
        var provider = new LmuTelemetryProvider();
        var snapshot = CreateSnapshot();
        snapshot.generic.events[(int)SharedMemoryEvent.SME_UPDATE_TELEMETRY] = 1;
        snapshot.telemetry.activeVehicles = 3;
        snapshot.telemetry.playerVehicleIdx = 0;
        snapshot.telemetry.playerHasVehicle = false;

        provider.ApplySharedMemorySnapshot(snapshot, DateTimeOffset.UtcNow);

        var status = provider.GetStatus();
        var debug = provider.GetDebugSnapshot();

        Assert.True(status.Connected);
        Assert.NotNull(status.LastTelemetryUpdateUtc);
        Assert.Equal(3, debug.ActiveVehicles);
        Assert.False(debug.PlayerHasVehicle);
    }

    [Fact]
    public void GetConsoleSnapshot_UsesPlayerTelemetryValuesWhenAvailable()
    {
        var provider = new LmuTelemetryProvider();
        var snapshot = CreateSnapshot();
        snapshot.generic.events[(int)SharedMemoryEvent.SME_UPDATE_TELEMETRY] = 1;
        snapshot.telemetry.activeVehicles = 1;
        snapshot.telemetry.playerVehicleIdx = 0;
        snapshot.telemetry.playerHasVehicle = true;

        snapshot.telemetry.telemInfo[0].mLapNumber = 7;
        snapshot.telemetry.telemInfo[0].mFilteredThrottle = 0.83;
        snapshot.telemetry.telemInfo[0].mFilteredBrake = 0.05;
        snapshot.telemetry.telemInfo[0].mFilteredSteering = -0.2;
        snapshot.telemetry.telemInfo[0].mGear = 5;
        snapshot.telemetry.telemInfo[0].mEngineRPM = 8120;
        snapshot.telemetry.telemInfo[0].mFuel = 42.5;
        snapshot.telemetry.telemInfo[0].mLocalVel = new TelemVect3 { x = 0, y = 0, z = -50 };
        snapshot.telemetry.telemInfo[0].mWheel[0].mBrakePressure = 0.4;
        snapshot.telemetry.telemInfo[0].mWheel[1].mBrakePressure = 0.7;
        snapshot.telemetry.telemInfo[0].mWheel[2].mBrakePressure = 0.2;
        snapshot.telemetry.telemInfo[0].mWheel[3].mBrakePressure = 0.5;

        provider.ApplySharedMemorySnapshot(snapshot, DateTimeOffset.UtcNow);
        var consoleSnapshot = provider.GetConsoleSnapshot();

        Assert.True(consoleSnapshot.Connected);
        Assert.Equal(7, consoleSnapshot.LapNumber);
        Assert.Equal(180.0, Math.Round(consoleSnapshot.SpeedKph ?? 0.0, 1));
        Assert.Equal(0.83, consoleSnapshot.Throttle);
        Assert.Equal(0.05, consoleSnapshot.Brake);
        Assert.Equal(-0.2, consoleSnapshot.Steering);
        Assert.Equal(5, consoleSnapshot.Gear);
        Assert.Equal(8120, consoleSnapshot.Rpm);
        Assert.Equal(42.5, consoleSnapshot.FuelLiters);
        Assert.Equal(0.7, consoleSnapshot.MaxBrakePressure);
    }

    private static SharedMemoryObjectOut CreateSnapshot() =>
        new()
        {
            generic = new SharedMemoryGeneric
            {
                events = new uint[SharedMemoryInteropConstants.EventCount],
                appInfo = new ApplicationStateV01
                {
                    mOptionsPage = string.Empty,
                    mExpansion = new byte[204]
                }
            },
            paths = new SharedMemoryPathData
            {
                userData = string.Empty,
                customVariables = string.Empty,
                stewardResults = string.Empty,
                playerProfile = string.Empty,
                pluginsFolder = string.Empty
            },
            scoring = new SharedMemoryScoringData
            {
                scoringInfo = new ScoringInfoV01
                {
                    mTrackName = string.Empty,
                    mPlayerName = string.Empty,
                    mPlrFileName = string.Empty,
                    mServerName = string.Empty,
                    mSectorFlag = new sbyte[3],
                    mExpansion = new byte[187]
                },
                vehScoringInfo = Enumerable.Range(0, SharedMemoryInteropConstants.MaxVehicles)
                    .Select(_ => new VehicleScoringInfoV01
                    {
                        mDriverName = string.Empty,
                        mVehicleName = string.Empty,
                        mVehicleClass = string.Empty,
                        mPitGroup = string.Empty,
                        mUpgradePack = new byte[16],
                        mVehFilename = string.Empty,
                        mExpansion = new byte[4],
                        mOri = new TelemVect3[3]
                    })
                    .ToArray(),
                scoringStream = Array.Empty<byte>()
            },
            telemetry = new SharedMemoryTelemetryData
            {
                telemInfo = Enumerable.Range(0, SharedMemoryInteropConstants.MaxVehicles)
                    .Select(_ => new TelemInfoV01
                    {
                        mVehicleName = string.Empty,
                        mTrackName = string.Empty,
                        mOri = new TelemVect3[3],
                        mDentSeverity = new byte[8],
                        mFrontTireCompoundName = string.Empty,
                        mRearTireCompoundName = string.Empty,
                        mUnused = new byte[2],
                        mPhysicsToGraphicsOffset = new float[3],
                        mVehicleModel = string.Empty,
                        mExpansion = new byte[20],
                        mWheel = Enumerable.Range(0, 4)
                            .Select(__ => new TelemWheelV01
                            {
                                mTemperature = new double[3],
                                mTerrainName = string.Empty,
                                mTireInnerLayerTemperature = new double[3],
                                mExpansion = new byte[18]
                            })
                            .ToArray()
                    })
                    .ToArray()
            }
        };
}
