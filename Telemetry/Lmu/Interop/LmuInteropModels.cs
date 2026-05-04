using System.Runtime.InteropServices;

namespace telemetry_tracker.Telemetry.Lmu.Interop;

internal static class SharedMemoryInteropConstants
{
    public const int MaxPath = 260;
    public const int MaxVehicles = 104;
    public const int ScoringStreamLength = 65536;
    public const int EventCount = 16;
}

internal enum SharedMemoryEvent : uint
{
    SME_ENTER,
    SME_EXIT,
    SME_STARTUP,
    SME_SHUTDOWN,
    SME_LOAD,
    SME_UNLOAD,
    SME_START_SESSION,
    SME_END_SESSION,
    SME_ENTER_REALTIME,
    SME_EXIT_REALTIME,
    SME_UPDATE_SCORING,
    SME_UPDATE_TELEMETRY,
    SME_INIT_APPLICATION,
    SME_UNINIT_APPLICATION,
    SME_SET_ENVIRONMENT,
    SME_FFB,
    SME_MAX
}

internal enum IP_VehicleClass : byte
{
    Hypercar = 0x00,
    LMP2_ELMS = 0x02,
    LMP2,
    LMP3,
    GTE,
    GT3,
    PaceCar = 0x08,
    Unknown = 0xFF
}

internal enum IP_VehicleChampionship : byte
{
    WEC_2023 = 0x00,
    WEC_2024,
    WEC_2025,
    WEC_2026,
    ELMS_2025 = 0x10,
    ELMS_2026,
    Unknown = 0xFF
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct TelemVect3
{
    public double x;
    public double y;
    public double z;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct TelemWheelV01
{
    public double mSuspensionDeflection;
    public double mRideHeight;
    public double mSuspForce;
    public double mBrakeTemp;
    public double mBrakePressure;
    public double mRotation;
    public double mLateralPatchVel;
    public double mLongitudinalPatchVel;
    public double mLateralGroundVel;
    public double mLongitudinalGroundVel;
    public double mCamber;
    public double mLateralForce;
    public double mLongitudinalForce;
    public double mTireLoad;
    public double mGripFract;
    public double mPressure;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] mTemperature;

    public double mWear;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string mTerrainName;

    public byte mSurfaceType;

    [MarshalAs(UnmanagedType.I1)]
    public bool mFlat;

    [MarshalAs(UnmanagedType.I1)]
    public bool mDetached;

    public byte mStaticUndeflectedRadius;
    public double mVerticalTireDeflection;
    public double mWheelYLocation;
    public double mToe;
    public double mTireCarcassTemperature;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] mTireInnerLayerTemperature;

    public float mOptimalTemp;
    public byte mCompoundIndex;
    public byte mCompoundType;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] mExpansion;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct TelemInfoV01
{
    public int mID;
    public double mDeltaTime;
    public double mElapsedTime;
    public int mLapNumber;
    public double mLapStartET;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string mVehicleName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string mTrackName;

    public TelemVect3 mPos;
    public TelemVect3 mLocalVel;
    public TelemVect3 mLocalAccel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public TelemVect3[] mOri;

    public TelemVect3 mLocalRot;
    public TelemVect3 mLocalRotAccel;
    public int mGear;
    public double mEngineRPM;
    public double mEngineWaterTemp;
    public double mEngineOilTemp;
    public double mClutchRPM;
    public double mUnfilteredThrottle;
    public double mUnfilteredBrake;
    public double mUnfilteredSteering;
    public double mUnfilteredClutch;
    public double mFilteredThrottle;
    public double mFilteredBrake;
    public double mFilteredSteering;
    public double mFilteredClutch;
    public double mSteeringShaftTorque;
    public double mFront3rdDeflection;
    public double mRear3rdDeflection;
    public double mFrontWingHeight;
    public double mFrontRideHeight;
    public double mRearRideHeight;
    public double mDrag;
    public double mFrontDownforce;
    public double mRearDownforce;
    public double mFuel;
    public double mEngineMaxRPM;
    public byte mScheduledStops;

    [MarshalAs(UnmanagedType.I1)]
    public bool mOverheating;

    [MarshalAs(UnmanagedType.I1)]
    public bool mDetached;

    [MarshalAs(UnmanagedType.I1)]
    public bool mHeadlights;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] mDentSeverity;

    public double mLastImpactET;
    public double mLastImpactMagnitude;
    public TelemVect3 mLastImpactPos;
    public double mEngineTorque;
    public int mCurrentSector;
    public byte mSpeedLimiter;
    public byte mMaxGears;
    public byte mFrontTireCompoundIndex;
    public byte mRearTireCompoundIndex;
    public double mFuelCapacity;
    public byte mFrontFlapActivated;
    public byte mRearFlapActivated;
    public byte mRearFlapLegalStatus;
    public byte mIgnitionStarter;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
    public string mFrontTireCompoundName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
    public string mRearTireCompoundName;

    public byte mSpeedLimiterAvailable;
    public byte mAntiStallActivated;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] mUnused;

    public float mVisualSteeringWheelRange;
    public double mRearBrakeBias;
    public double mTurboBoostPressure;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] mPhysicsToGraphicsOffset;

    public float mPhysicalSteeringWheelRange;
    public double mDeltaBest;
    public double mBatteryChargeFraction;
    public double mElectricBoostMotorTorque;
    public double mElectricBoostMotorRPM;
    public double mElectricBoostMotorTemperature;
    public double mElectricBoostWaterTemperature;
    public byte mElectricBoostMotorState;

    [MarshalAs(UnmanagedType.I1)]
    public bool mLapInvalidated;

    [MarshalAs(UnmanagedType.I1)]
    public bool mABSActive;

    [MarshalAs(UnmanagedType.I1)]
    public bool mTCActive;

    [MarshalAs(UnmanagedType.I1)]
    public bool mSpeedLimiterActive;

    public byte mWiperState;
    public byte mTC;
    public byte mTCMax;
    public byte mTCSlip;
    public byte mTCSlipMax;
    public byte mTCCut;
    public byte mTCCutMax;
    public byte mABS;
    public byte mABSMax;
    public byte mMotorMap;
    public byte mMotorMapMax;
    public byte mMigration;
    public byte mMigrationMax;
    public byte mFrontAntiSway;
    public byte mFrontAntiSwayMax;
    public byte mRearAntiSway;
    public byte mRearAntiSwayMax;
    public byte mLiftAndCoastProgress;
    public byte mTrackLimitsSteps;
    public float mRegen;
    public float mSoC;
    public float mVirtualEnergy;
    public float mTimeGapCarAhead;
    public float mTimeGapCarBehind;
    public float mTimeGapPlaceAhead;
    public float mTimeGapPlaceBehind;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
    public string mVehicleModel;

    public IP_VehicleClass mVehicleClass;
    public IP_VehicleChampionship mVehicleChampionship;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] mExpansion;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public TelemWheelV01[] mWheel;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct VehicleScoringInfoV01
{
    public int mID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string mDriverName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string mVehicleName;

    public short mTotalLaps;
    public sbyte mSector;
    public sbyte mFinishStatus;
    public double mLapDist;
    public double mPathLateral;
    public double mTrackEdge;
    public double mBestSector1;
    public double mBestSector2;
    public double mBestLapTime;
    public double mLastSector1;
    public double mLastSector2;
    public double mLastLapTime;
    public double mCurSector1;
    public double mCurSector2;
    public short mNumPitstops;
    public short mNumPenalties;

    [MarshalAs(UnmanagedType.I1)]
    public bool mIsPlayer;

    public sbyte mControl;

    [MarshalAs(UnmanagedType.I1)]
    public bool mInPits;

    public byte mPlace;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string mVehicleClass;

    public double mTimeBehindNext;
    public int mLapsBehindNext;
    public double mTimeBehindLeader;
    public int mLapsBehindLeader;
    public double mLapStartET;
    public TelemVect3 mPos;
    public TelemVect3 mLocalVel;
    public TelemVect3 mLocalAccel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public TelemVect3[] mOri;

    public TelemVect3 mLocalRot;
    public TelemVect3 mLocalRotAccel;
    public byte mHeadlights;
    public byte mPitState;
    public byte mServerScored;
    public byte mIndividualPhase;
    public int mQualification;
    public double mTimeIntoLap;
    public double mEstimatedLapTime;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
    public string mPitGroup;

    public byte mFlag;

    [MarshalAs(UnmanagedType.I1)]
    public bool mUnderYellow;

    public byte mCountLapFlag;

    [MarshalAs(UnmanagedType.I1)]
    public bool mInGarageStall;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] mUpgradePack;

    public float mPitLapDist;
    public float mBestLapSector1;
    public float mBestLapSector2;
    public ulong mSteamID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string mVehFilename;

    public short mAttackMode;
    public byte mFuelFraction;

    [MarshalAs(UnmanagedType.I1)]
    public bool mDRSState;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] mExpansion;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct ScoringInfoV01
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string mTrackName;

    public int mSession;
    public double mCurrentET;
    public double mEndET;
    public int mMaxLaps;
    public double mLapDist;
    public IntPtr mResultsStream;
    public int mNumVehicles;
    public byte mGamePhase;
    public sbyte mYellowFlagState;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public sbyte[] mSectorFlag;

    public byte mStartLight;
    public byte mNumRedLights;

    [MarshalAs(UnmanagedType.I1)]
    public bool mInRealtime;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string mPlayerName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string mPlrFileName;

    public double mDarkCloud;
    public double mRaining;
    public double mAmbientTemp;
    public double mTrackTemp;
    public TelemVect3 mWind;
    public double mMinPathWetness;
    public double mMaxPathWetness;
    public byte mGameMode;

    [MarshalAs(UnmanagedType.I1)]
    public bool mIsPasswordProtected;

    public ushort mServerPort;
    public uint mServerPublicIP;
    public int mMaxPlayers;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string mServerName;

    public float mStartET;
    public double mAvgPathWetness;
    public float mSessionTimeRemaining;
    public float mTimeOfDay;

    [MarshalAs(UnmanagedType.I1)]
    public bool mIsFixedSetup;

    public byte mTrackGripLevel;
    public byte mCloudCoverage;
    public byte mTrackLimitsStepsPerPenalty;
    public byte mTrackLimitsStepsPerPoint;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 187)]
    public byte[] mExpansion;

    public IntPtr mVehicle;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct ApplicationStateV01
{
    public IntPtr mAppWindow;
    public uint mWidth;
    public uint mHeight;
    public uint mRefreshRate;
    public uint mWindowed;
    public byte mOptionsLocation;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string mOptionsPage;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 204)]
    public byte[] mExpansion;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SharedMemoryGeneric
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryInteropConstants.EventCount)]
    public uint[] events;

    public int gameVersion;
    public float FFBTorque;
    public ApplicationStateV01 appInfo;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
internal struct SharedMemoryPathData
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryInteropConstants.MaxPath)]
    public string userData;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryInteropConstants.MaxPath)]
    public string customVariables;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryInteropConstants.MaxPath)]
    public string stewardResults;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryInteropConstants.MaxPath)]
    public string playerProfile;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SharedMemoryInteropConstants.MaxPath)]
    public string pluginsFolder;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SharedMemoryScoringData
{
    public ScoringInfoV01 scoringInfo;
    public nuint scoringStreamSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryInteropConstants.MaxVehicles)]
    public VehicleScoringInfoV01[] vehScoringInfo;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryInteropConstants.ScoringStreamLength)]
    public byte[] scoringStream;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SharedMemoryTelemetryData
{
    public byte activeVehicles;
    public byte playerVehicleIdx;

    [MarshalAs(UnmanagedType.I1)]
    public bool playerHasVehicle;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SharedMemoryInteropConstants.MaxVehicles)]
    public TelemInfoV01[] telemInfo;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SharedMemoryObjectOut
{
    public SharedMemoryGeneric generic;
    public SharedMemoryPathData paths;
    public SharedMemoryScoringData scoring;
    public SharedMemoryTelemetryData telemetry;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SharedMemoryLayout
{
    public SharedMemoryObjectOut data;
}
