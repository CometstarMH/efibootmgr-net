using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;

namespace EfiBootMgr
{
    // heavily modify from https://www.pinvoke.net/default.aspx/advapi32/AdjustTokenPrivileges.html
    internal class Privileges
    {
        // modified from https://stackoverflow.com/a/1343517
        public class SecurityEntity
        {
            private SecurityEntity(string value) { Value = value; }

            public string Value { get; set; }

            public static SecurityEntity SE_CREATE_TOKEN_NAME { get { return new SecurityEntity("SeCreateTokenPrivilege"); } }
            public static SecurityEntity SE_ASSIGNPRIMARYTOKEN_NAME { get { return new SecurityEntity("SeAssignprimarytokenPrivilege"); } }
            public static SecurityEntity SE_LOCK_MEMORY_NAME { get { return new SecurityEntity("SeLockMemoryPrivilege"); } }
            public static SecurityEntity SE_INCREASE_QUOTA_NAME { get { return new SecurityEntity("SeIncreaseQuotaPrivilege"); } }
            public static SecurityEntity SE_UNSOLICITED_INPUT_NAME { get { return new SecurityEntity("SeUnsolicitedInputPrivilege"); } }
            public static SecurityEntity SE_MACHINE_ACCOUNT_NAME { get { return new SecurityEntity("SeMachineAccountPrivilege"); } }
            public static SecurityEntity SE_TCB_NAME { get { return new SecurityEntity("SeTcbPrivilege"); } }
            public static SecurityEntity SE_SECURITY_NAME { get { return new SecurityEntity("SeSecurityPrivilege"); } }
            public static SecurityEntity SE_TAKE_OWNERSHIP_NAME { get { return new SecurityEntity("SeTakeOwnershipPrivilege"); } }
            public static SecurityEntity SE_LOAD_DRIVER_NAME { get { return new SecurityEntity("SeLoadDriverPrivilege"); } }
            public static SecurityEntity SE_SYSTEM_PROFILE_NAME { get { return new SecurityEntity("SeSystemProfilePrivilege"); } }
            public static SecurityEntity SE_SYSTEMTIME_NAME { get { return new SecurityEntity("SeSystemtimePrivilege"); } }
            public static SecurityEntity SE_PROF_SINGLE_PROCESS_NAME { get { return new SecurityEntity("SeProfSingleProcessPrivilege"); } }
            public static SecurityEntity SE_INC_BASE_PRIORITY_NAME { get { return new SecurityEntity("SeIncBasePriorityPrivilege"); } }
            public static SecurityEntity SE_CREATE_PAGEFILE_NAME { get { return new SecurityEntity("SeCreatePagefilePrivilege"); } }
            public static SecurityEntity SE_CREATE_PERMANENT_NAME { get { return new SecurityEntity("SeCreatePermanentPrivilege"); } }
            public static SecurityEntity SE_BACKUP_NAME { get { return new SecurityEntity("SeBackupPrivilege"); } }
            public static SecurityEntity SE_RESTORE_NAME { get { return new SecurityEntity("SeRestorePrivilege"); } }
            public static SecurityEntity SE_SHUTDOWN_NAME { get { return new SecurityEntity("SeShutdownPrivilege"); } }
            public static SecurityEntity SE_DEBUG_NAME { get { return new SecurityEntity("SeDebugPrivilege"); } }
            public static SecurityEntity SE_AUDIT_NAME { get { return new SecurityEntity("SeAuditPrivilege"); } }
            public static SecurityEntity SE_SYSTEM_ENVIRONMENT_NAME { get { return new SecurityEntity("SeSystemEnvironmentPrivilege"); } }
            public static SecurityEntity SE_CHANGE_NOTIFY_NAME { get { return new SecurityEntity("SeChangeNotifyPrivilege"); } }
            public static SecurityEntity SE_REMOTE_SHUTDOWN_NAME { get { return new SecurityEntity("SeRemoteShutdownPrivilege"); } }
            public static SecurityEntity SE_UNDOCK_NAME { get { return new SecurityEntity("SeUndockPrivilege"); } }
            public static SecurityEntity SE_SYNC_AGENT_NAME { get { return new SecurityEntity("SeSyncAgentPrivilege"); } }
            public static SecurityEntity SE_ENABLE_DELEGATION_NAME { get { return new SecurityEntity("SeEnableDelegationPrivilege"); } }
            public static SecurityEntity SE_MANAGE_VOLUME_NAME { get { return new SecurityEntity("SeManageVolumePrivilege"); } }
            public static SecurityEntity SE_IMPERSONATE_NAME { get { return new SecurityEntity("SeImpersonatePrivilege"); } }
            public static SecurityEntity SE_CREATE_GLOBAL_NAME { get { return new SecurityEntity("SeCreateGlobalPrivilege"); } }
            public static SecurityEntity SE_CREATE_SYMBOLIC_LINK_NAME { get { return new SecurityEntity("SeCreateSymbolicLinkPrivilege"); } }
            public static SecurityEntity SE_INC_WORKING_SET_NAME { get { return new SecurityEntity("SeIncWorkingSetPrivilege"); } }
            public static SecurityEntity SE_RELABEL_NAME { get { return new SecurityEntity("SeRelabelPrivilege"); } }
            public static SecurityEntity SE_TIME_ZONE_NAME { get { return new SecurityEntity("SeTimeZonePrivilege"); } }
            public static SecurityEntity SE_TRUSTED_CREDMAN_ACCESS_NAME { get { return new SecurityEntity("SeTrustedCredmanAccessPrivilege"); } }
        }

        public static void EnablePrivilege(SecurityEntity securityEntity)
        {
            var securityEntityValue = securityEntity.Value;
            try
            {
                var locallyUniqueIdentifier = new Natives.LUID();

                if (Natives.LookupPrivilegeValue(null, securityEntityValue, ref locallyUniqueIdentifier))
                {
                    var TOKEN_PRIVILEGES = new Natives.TOKEN_PRIVILEGES() { 
                        PrivilegeCount = 1, 
                        Attributes = Natives.SE_PRIVILEGE_ENABLED, 
                        Luid = locallyUniqueIdentifier
                    };

                    var tokenHandle = IntPtr.Zero;
                    try
                    {
                        var currentProcess = Natives.GetCurrentProcess();
                        if (Natives.OpenProcessToken(currentProcess, Natives.TOKEN_ADJUST_PRIVILEGES | Natives.TOKEN_QUERY, out tokenHandle))
                        {
                            if (Natives.AdjustTokenPrivileges(tokenHandle, false, ref TOKEN_PRIVILEGES, 1024, IntPtr.Zero, IntPtr.Zero))
                            {
                                var lastError = Marshal.GetLastWin32Error();
                                if (lastError == Natives.ERROR_NOT_ALL_ASSIGNED)
                                {
                                    throw new InvalidOperationException("AdjustTokenPrivileges failed.", new Win32Exception(lastError));
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("AdjustTokenPrivileges failed.", new Win32Exception());
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                "OpenProcessToken failed. CurrentProcess: {0}",
                                                currentProcess.ToInt32()), new Win32Exception());
                        }
                    }
                    finally
                    {
                        if (tokenHandle != IntPtr.Zero)
                            Natives.CloseHandle(tokenHandle);
                    }
                }
                else
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                        "LookupPrivilegeValue failed. SecurityEntityValue: {0}",
                                        securityEntityValue), new Win32Exception());
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                 "GrandPrivilege failed. SecurityEntity: {0}",
                                 securityEntityValue), e);
            }
        }
    }

}
