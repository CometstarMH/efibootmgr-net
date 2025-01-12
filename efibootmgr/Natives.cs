using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    internal class Natives
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr processHandle,
                            uint desiredAccesss,
                            out IntPtr tokenHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            internal int LowPart;
            internal uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            internal LUID Luid;
            internal int Attributes;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetFirmwareEnvironmentVariable(string lpName, string lpGuid, IntPtr pBuffer, uint nSize);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr tokenhandle,
                                 [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
                                 [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES newstate,
                                 uint bufferlength, IntPtr previousState, IntPtr returnlength);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

        internal const int ERROR_NOT_ALL_ASSIGNED = 1300;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;

        internal const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        internal const uint STANDARD_RIGHTS_READ = 0x00020000;
        internal const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const uint TOKEN_DUPLICATE = 0x0002;
        internal const uint TOKEN_IMPERSONATE = 0x0004;
        internal const uint TOKEN_QUERY = 0x0008;
        internal const uint TOKEN_QUERY_SOURCE = 0x0010;
        internal const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const uint TOKEN_ADJUST_GROUPS = 0x0040;
        internal const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        internal const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                            TOKEN_ASSIGN_PRIMARY |
                            TOKEN_DUPLICATE |
                            TOKEN_IMPERSONATE |
                            TOKEN_QUERY |
                            TOKEN_QUERY_SOURCE |
                            TOKEN_ADJUST_PRIVILEGES |
                            TOKEN_ADJUST_GROUPS |
                            TOKEN_ADJUST_DEFAULT |
                            TOKEN_ADJUST_SESSIONID);

        // Undocumented
        [DllImport("ntdll.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.U4)]
        internal static extern NtStatus NtEnumerateBootEntries(IntPtr pBuffer, ref uint bufferLength);

        // Undocumented
        [DllImport("ntdll.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.U4)]
        internal static extern NtStatus NtEnumerateDriverEntries(IntPtr pBuffer, ref uint bufferLength);
    }
    public enum NtStatus : int
    {
        // Success
        Success = 0x00000000,
        Wait0 = 0x00000000,
        Wait1 = 0x00000001,
        Wait2 = 0x00000002,
        Wait3 = 0x00000003,
        Wait63 = 0x0000003f,
        Abandoned = 0x00000080,
        AbandonedWait0 = 0x00000080,
        AbandonedWait1 = 0x00000081,
        AbandonedWait2 = 0x00000082,
        AbandonedWait3 = 0x00000083,
        AbandonedWait63 = 0x000000bf,
        UserApc = 0x000000c0,
        KernelApc = 0x00000100,
        Alerted = 0x00000101,
        Timeout = 0x00000102,
        Pending = 0x00000103,
        Reparse = 0x00000104,
        MoreEntries = 0x00000105,
        NotAllAssigned = 0x00000106,
        SomeNotMapped = 0x00000107,
        OpLockBreakInProgress = 0x00000108,
        VolumeMounted = 0x00000109,
        RxActCommitted = 0x0000010a,
        NotifyCleanup = 0x0000010b,
        NotifyEnumDir = 0x0000010c,
        NoQuotasForAccount = 0x0000010d,
        PrimaryTransportConnectFailed = 0x0000010e,
        PageFaultTransition = 0x00000110,
        PageFaultDemandZero = 0x00000111,
        PageFaultCopyOnWrite = 0x00000112,
        PageFaultGuardPage = 0x00000113,
        PageFaultPagingFile = 0x00000114,
        CrashDump = 0x00000116,
        ReparseObject = 0x00000118,
        NothingToTerminate = 0x00000122,
        ProcessNotInJob = 0x00000123,
        ProcessInJob = 0x00000124,
        ProcessCloned = 0x00000129,
        FileLockedWithOnlyReaders = 0x0000012a,
        FileLockedWithWriters = 0x0000012b,

        // Informational
        Informational = 0x40000000,
        ObjectNameExists = 0x40000000,
        ThreadWasSuspended = 0x40000001,
        WorkingSetLimitRange = 0x40000002,
        ImageNotAtBase = 0x40000003,
        RegistryRecovered = 0x40000009,

        // Warning
        Warning = unchecked((int)0x80000000),
        GuardPageViolation = unchecked((int)0x80000001),
        DatatypeMisalignment = unchecked((int)0x80000002),
        Breakpoint = unchecked((int)0x80000003),
        SingleStep = unchecked((int)0x80000004),
        BufferOverflow = unchecked((int)0x80000005),
        NoMoreFiles = unchecked((int)0x80000006),
        HandlesClosed = unchecked((int)0x8000000a),
        PartialCopy = unchecked((int)0x8000000d),
        DeviceBusy = unchecked((int)0x80000011),
        InvalidEaName = unchecked((int)0x80000013),
        EaListInconsistent = unchecked((int)0x80000014),
        NoMoreEntries = unchecked((int)0x8000001a),
        LongJump = unchecked((int)0x80000026),
        DllMightBeInsecure = unchecked((int)0x8000002b),

        // Error
        Error = unchecked((int)0xc0000000),
        Unsuccessful = unchecked((int)0xc0000001),
        NotImplemented = unchecked((int)0xc0000002),
        InvalidInfoClass = unchecked((int)0xc0000003),
        InfoLengthMismatch = unchecked((int)0xc0000004),
        AccessViolation = unchecked((int)0xc0000005),
        InPageError = unchecked((int)0xc0000006),
        PagefileQuota = unchecked((int)0xc0000007),
        InvalidHandle = unchecked((int)0xc0000008),
        BadInitialStack = unchecked((int)0xc0000009),
        BadInitialPc = unchecked((int)0xc000000a),
        InvalidCid = unchecked((int)0xc000000b),
        TimerNotCanceled = unchecked((int)0xc000000c),
        InvalidParameter = unchecked((int)0xc000000d),
        NoSuchDevice = unchecked((int)0xc000000e),
        NoSuchFile = unchecked((int)0xc000000f),
        InvalidDeviceRequest = unchecked((int)0xc0000010),
        EndOfFile = unchecked((int)0xc0000011),
        WrongVolume = unchecked((int)0xc0000012),
        NoMediaInDevice = unchecked((int)0xc0000013),
        NoMemory = unchecked((int)0xc0000017),
        NotMappedView = unchecked((int)0xc0000019),
        UnableToFreeVm = unchecked((int)0xc000001a),
        UnableToDeleteSection = unchecked((int)0xc000001b),
        IllegalInstruction = unchecked((int)0xc000001d),
        AlreadyCommitted = unchecked((int)0xc0000021),
        AccessDenied = unchecked((int)0xc0000022),
        BufferTooSmall = unchecked((int)0xc0000023),
        ObjectTypeMismatch = unchecked((int)0xc0000024),
        NonContinuableException = unchecked((int)0xc0000025),
        BadStack = unchecked((int)0xc0000028),
        NotLocked = unchecked((int)0xc000002a),
        NotCommitted = unchecked((int)0xc000002d),
        InvalidParameterMix = unchecked((int)0xc0000030),
        ObjectNameInvalid = unchecked((int)0xc0000033),
        ObjectNameNotFound = unchecked((int)0xc0000034),
        ObjectNameCollision = unchecked((int)0xc0000035),
        ObjectPathInvalid = unchecked((int)0xc0000039),
        ObjectPathNotFound = unchecked((int)0xc000003a),
        ObjectPathSyntaxBad = unchecked((int)0xc000003b),
        DataOverrun = unchecked((int)0xc000003c),
        DataLate = unchecked((int)0xc000003d),
        DataError = unchecked((int)0xc000003e),
        CrcError = unchecked((int)0xc000003f),
        SectionTooBig = unchecked((int)0xc0000040),
        PortConnectionRefused = unchecked((int)0xc0000041),
        InvalidPortHandle = unchecked((int)0xc0000042),
        SharingViolation = unchecked((int)0xc0000043),
        QuotaExceeded = unchecked((int)0xc0000044),
        InvalidPageProtection = unchecked((int)0xc0000045),
        MutantNotOwned = unchecked((int)0xc0000046),
        SemaphoreLimitExceeded = unchecked((int)0xc0000047),
        PortAlreadySet = unchecked((int)0xc0000048),
        SectionNotImage = unchecked((int)0xc0000049),
        SuspendCountExceeded = unchecked((int)0xc000004a),
        ThreadIsTerminating = unchecked((int)0xc000004b),
        BadWorkingSetLimit = unchecked((int)0xc000004c),
        IncompatibleFileMap = unchecked((int)0xc000004d),
        SectionProtection = unchecked((int)0xc000004e),
        EasNotSupported = unchecked((int)0xc000004f),
        EaTooLarge = unchecked((int)0xc0000050),
        NonExistentEaEntry = unchecked((int)0xc0000051),
        NoEasOnFile = unchecked((int)0xc0000052),
        EaCorruptError = unchecked((int)0xc0000053),
        FileLockConflict = unchecked((int)0xc0000054),
        LockNotGranted = unchecked((int)0xc0000055),
        DeletePending = unchecked((int)0xc0000056),
        CtlFileNotSupported = unchecked((int)0xc0000057),
        UnknownRevision = unchecked((int)0xc0000058),
        RevisionMismatch = unchecked((int)0xc0000059),
        InvalidOwner = unchecked((int)0xc000005a),
        InvalidPrimaryGroup = unchecked((int)0xc000005b),
        NoImpersonationToken = unchecked((int)0xc000005c),
        CantDisableMandatory = unchecked((int)0xc000005d),
        NoLogonServers = unchecked((int)0xc000005e),
        NoSuchLogonSession = unchecked((int)0xc000005f),
        NoSuchPrivilege = unchecked((int)0xc0000060),
        PrivilegeNotHeld = unchecked((int)0xc0000061),
        InvalidAccountName = unchecked((int)0xc0000062),
        UserExists = unchecked((int)0xc0000063),
        NoSuchUser = unchecked((int)0xc0000064),
        GroupExists = unchecked((int)0xc0000065),
        NoSuchGroup = unchecked((int)0xc0000066),
        MemberInGroup = unchecked((int)0xc0000067),
        MemberNotInGroup = unchecked((int)0xc0000068),
        LastAdmin = unchecked((int)0xc0000069),
        WrongPassword = unchecked((int)0xc000006a),
        IllFormedPassword = unchecked((int)0xc000006b),
        PasswordRestriction = unchecked((int)0xc000006c),
        LogonFailure = unchecked((int)0xc000006d),
        AccountRestriction = unchecked((int)0xc000006e),
        InvalidLogonHours = unchecked((int)0xc000006f),
        InvalidWorkstation = unchecked((int)0xc0000070),
        PasswordExpired = unchecked((int)0xc0000071),
        AccountDisabled = unchecked((int)0xc0000072),
        NoneMapped = unchecked((int)0xc0000073),
        TooManyLuidsRequested = unchecked((int)0xc0000074),
        LuidsExhausted = unchecked((int)0xc0000075),
        InvalidSubAuthority = unchecked((int)0xc0000076),
        InvalidAcl = unchecked((int)0xc0000077),
        InvalidSid = unchecked((int)0xc0000078),
        InvalidSecurityDescr = unchecked((int)0xc0000079),
        ProcedureNotFound = unchecked((int)0xc000007a),
        InvalidImageFormat = unchecked((int)0xc000007b),
        NoToken = unchecked((int)0xc000007c),
        BadInheritanceAcl = unchecked((int)0xc000007d),
        RangeNotLocked = unchecked((int)0xc000007e),
        DiskFull = unchecked((int)0xc000007f),
        ServerDisabled = unchecked((int)0xc0000080),
        ServerNotDisabled = unchecked((int)0xc0000081),
        TooManyGuidsRequested = unchecked((int)0xc0000082),
        GuidsExhausted = unchecked((int)0xc0000083),
        InvalidIdAuthority = unchecked((int)0xc0000084),
        AgentsExhausted = unchecked((int)0xc0000085),
        InvalidVolumeLabel = unchecked((int)0xc0000086),
        SectionNotExtended = unchecked((int)0xc0000087),
        NotMappedData = unchecked((int)0xc0000088),
        ResourceDataNotFound = unchecked((int)0xc0000089),
        ResourceTypeNotFound = unchecked((int)0xc000008a),
        ResourceNameNotFound = unchecked((int)0xc000008b),
        ArrayBoundsExceeded = unchecked((int)0xc000008c),
        FloatDenormalOperand = unchecked((int)0xc000008d),
        FloatDivideByZero = unchecked((int)0xc000008e),
        FloatInexactResult = unchecked((int)0xc000008f),
        FloatInvalidOperation = unchecked((int)0xc0000090),
        FloatOverflow = unchecked((int)0xc0000091),
        FloatStackCheck = unchecked((int)0xc0000092),
        FloatUnderflow = unchecked((int)0xc0000093),
        IntegerDivideByZero = unchecked((int)0xc0000094),
        IntegerOverflow = unchecked((int)0xc0000095),
        PrivilegedInstruction = unchecked((int)0xc0000096),
        TooManyPagingFiles = unchecked((int)0xc0000097),
        FileInvalid = unchecked((int)0xc0000098),
        InstanceNotAvailable = unchecked((int)0xc00000ab),
        PipeNotAvailable = unchecked((int)0xc00000ac),
        InvalidPipeState = unchecked((int)0xc00000ad),
        PipeBusy = unchecked((int)0xc00000ae),
        IllegalFunction = unchecked((int)0xc00000af),
        PipeDisconnected = unchecked((int)0xc00000b0),
        PipeClosing = unchecked((int)0xc00000b1),
        PipeConnected = unchecked((int)0xc00000b2),
        PipeListening = unchecked((int)0xc00000b3),
        InvalidReadMode = unchecked((int)0xc00000b4),
        IoTimeout = unchecked((int)0xc00000b5),
        FileForcedClosed = unchecked((int)0xc00000b6),
        ProfilingNotStarted = unchecked((int)0xc00000b7),
        ProfilingNotStopped = unchecked((int)0xc00000b8),
        NotSameDevice = unchecked((int)0xc00000d4),
        FileRenamed = unchecked((int)0xc00000d5),
        CantWait = unchecked((int)0xc00000d8),
        PipeEmpty = unchecked((int)0xc00000d9),
        CantTerminateSelf = unchecked((int)0xc00000db),
        InternalError = unchecked((int)0xc00000e5),
        InvalidParameter1 = unchecked((int)0xc00000ef),
        InvalidParameter2 = unchecked((int)0xc00000f0),
        InvalidParameter3 = unchecked((int)0xc00000f1),
        InvalidParameter4 = unchecked((int)0xc00000f2),
        InvalidParameter5 = unchecked((int)0xc00000f3),
        InvalidParameter6 = unchecked((int)0xc00000f4),
        InvalidParameter7 = unchecked((int)0xc00000f5),
        InvalidParameter8 = unchecked((int)0xc00000f6),
        InvalidParameter9 = unchecked((int)0xc00000f7),
        InvalidParameter10 = unchecked((int)0xc00000f8),
        InvalidParameter11 = unchecked((int)0xc00000f9),
        InvalidParameter12 = unchecked((int)0xc00000fa),
        MappedFileSizeZero = unchecked((int)0xc000011e),
        TooManyOpenedFiles = unchecked((int)0xc000011f),
        Cancelled = unchecked((int)0xc0000120),
        CannotDelete = unchecked((int)0xc0000121),
        InvalidComputerName = unchecked((int)0xc0000122),
        FileDeleted = unchecked((int)0xc0000123),
        SpecialAccount = unchecked((int)0xc0000124),
        SpecialGroup = unchecked((int)0xc0000125),
        SpecialUser = unchecked((int)0xc0000126),
        MembersPrimaryGroup = unchecked((int)0xc0000127),
        FileClosed = unchecked((int)0xc0000128),
        TooManyThreads = unchecked((int)0xc0000129),
        ThreadNotInProcess = unchecked((int)0xc000012a),
        TokenAlreadyInUse = unchecked((int)0xc000012b),
        PagefileQuotaExceeded = unchecked((int)0xc000012c),
        CommitmentLimit = unchecked((int)0xc000012d),
        InvalidImageLeFormat = unchecked((int)0xc000012e),
        InvalidImageNotMz = unchecked((int)0xc000012f),
        InvalidImageProtect = unchecked((int)0xc0000130),
        InvalidImageWin16 = unchecked((int)0xc0000131),
        LogonServer = unchecked((int)0xc0000132),
        DifferenceAtDc = unchecked((int)0xc0000133),
        SynchronizationRequired = unchecked((int)0xc0000134),
        DllNotFound = unchecked((int)0xc0000135),
        IoPrivilegeFailed = unchecked((int)0xc0000137),
        OrdinalNotFound = unchecked((int)0xc0000138),
        EntryPointNotFound = unchecked((int)0xc0000139),
        ControlCExit = unchecked((int)0xc000013a),
        PortNotSet = unchecked((int)0xc0000353),
        DebuggerInactive = unchecked((int)0xc0000354),
        CallbackBypass = unchecked((int)0xc0000503),
        PortClosed = unchecked((int)0xc0000700),
        MessageLost = unchecked((int)0xc0000701),
        InvalidMessage = unchecked((int)0xc0000702),
        RequestCanceled = unchecked((int)0xc0000703),
        RecursiveDispatch = unchecked((int)0xc0000704),
        LpcReceiveBufferExpected = unchecked((int)0xc0000705),
        LpcInvalidConnectionUsage = unchecked((int)0xc0000706),
        LpcRequestsNotAllowed = unchecked((int)0xc0000707),
        ResourceInUse = unchecked((int)0xc0000708),
        ProcessIsProtected = unchecked((int)0xc0000712),
        VolumeDirty = unchecked((int)0xc0000806),
        FileCheckedOut = unchecked((int)0xc0000901),
        CheckOutRequired = unchecked((int)0xc0000902),
        BadFileType = unchecked((int)0xc0000903),
        FileTooLarge = unchecked((int)0xc0000904),
        FormsAuthRequired = unchecked((int)0xc0000905),
        VirusInfected = unchecked((int)0xc0000906),
        VirusDeleted = unchecked((int)0xc0000907),
        TransactionalConflict = unchecked((int)0xc0190001),
        InvalidTransaction = unchecked((int)0xc0190002),
        TransactionNotActive = unchecked((int)0xc0190003),
        TmInitializationFailed = unchecked((int)0xc0190004),
        RmNotActive = unchecked((int)0xc0190005),
        RmMetadataCorrupt = unchecked((int)0xc0190006),
        TransactionNotJoined = unchecked((int)0xc0190007),
        DirectoryNotRm = unchecked((int)0xc0190008),
        CouldNotResizeLog = unchecked((int)0xc0190009),
        TransactionsUnsupportedRemote = unchecked((int)0xc019000a),
        LogResizeInvalidSize = unchecked((int)0xc019000b),
        RemoteFileVersionMismatch = unchecked((int)0xc019000c),
        CrmProtocolAlreadyExists = unchecked((int)0xc019000f),
        TransactionPropagationFailed = unchecked((int)0xc0190010),
        CrmProtocolNotFound = unchecked((int)0xc0190011),
        TransactionSuperiorExists = unchecked((int)0xc0190012),
        TransactionRequestNotValid = unchecked((int)0xc0190013),
        TransactionNotRequested = unchecked((int)0xc0190014),
        TransactionAlreadyAborted = unchecked((int)0xc0190015),
        TransactionAlreadyCommitted = unchecked((int)0xc0190016),
        TransactionInvalidMarshallBuffer = unchecked((int)0xc0190017),
        CurrentTransactionNotValid = unchecked((int)0xc0190018),
        LogGrowthFailed = unchecked((int)0xc0190019),
        ObjectNoLongerExists = unchecked((int)0xc0190021),
        StreamMiniversionNotFound = unchecked((int)0xc0190022),
        StreamMiniversionNotValid = unchecked((int)0xc0190023),
        MiniversionInaccessibleFromSpecifiedTransaction = unchecked((int)0xc0190024),
        CantOpenMiniversionWithModifyIntent = unchecked((int)0xc0190025),
        CantCreateMoreStreamMiniversions = unchecked((int)0xc0190026),
        HandleNoLongerValid = unchecked((int)0xc0190028),
        NoTxfMetadata = unchecked((int)0xc0190029),
        LogCorruptionDetected = unchecked((int)0xc0190030),
        CantRecoverWithHandleOpen = unchecked((int)0xc0190031),
        RmDisconnected = unchecked((int)0xc0190032),
        EnlistmentNotSuperior = unchecked((int)0xc0190033),
        RecoveryNotNeeded = unchecked((int)0xc0190034),
        RmAlreadyStarted = unchecked((int)0xc0190035),
        FileIdentityNotPersistent = unchecked((int)0xc0190036),
        CantBreakTransactionalDependency = unchecked((int)0xc0190037),
        CantCrossRmBoundary = unchecked((int)0xc0190038),
        TxfDirNotEmpty = unchecked((int)0xc0190039),
        IndoubtTransactionsExist = unchecked((int)0xc019003a),
        TmVolatile = unchecked((int)0xc019003b),
        RollbackTimerExpired = unchecked((int)0xc019003c),
        TxfAttributeCorrupt = unchecked((int)0xc019003d),
        EfsNotAllowedInTransaction = unchecked((int)0xc019003e),
        TransactionalOpenNotAllowed = unchecked((int)0xc019003f),
        TransactedMappingUnsupportedRemote = unchecked((int)0xc0190040),
        TxfMetadataAlreadyPresent = unchecked((int)0xc0190041),
        TransactionScopeCallbacksNotSet = unchecked((int)0xc0190042),
        TransactionRequiredPromotion = unchecked((int)0xc0190043),
        CannotExecuteFileInTransaction = unchecked((int)0xc0190044),
        TransactionsNotFrozen = unchecked((int)0xc0190045),

        MaximumNtStatus = unchecked((int)0xffffffff),
    }
}
