using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    // https://github.com/winsiderss/systeminformer/blob/master/phnt/include/ntexapi.h
    // _BOOT_ENTRY
    [StructLayout(LayoutKind.Sequential)]
    internal class NtExApiBootEntry
    {
        public UInt32 Version;
        public UInt32 Length;
        public UInt32 Id; // four-digit hex number part of its name Boot####
        // public uint Attributes;
        // public string FriendlyName; //FriendlyNameOffset: ULONG,
        // public string BootFilePath; //BootFilePathOffset: ULONG,
        // OsOptionsLength: ULONG,
        // OsOptions: [UCHAR; 1],
    }
}
