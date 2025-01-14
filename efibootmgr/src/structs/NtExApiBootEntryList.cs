using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    // https://github.com/winsiderss/systeminformer/blob/master/phnt/include/ntexapi.h
    // _BOOT_ENTRY_LIST
    [StructLayout(LayoutKind.Sequential)]
    internal class NtExApiBootEntryList
    {
        public UInt32 NextEntryOffset; // offset from start of whole data to next BOOT_ENTRY_LIST
        public NtExApiBootEntry BootEntry; // only first 3 fields are read
    }
}
