using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    internal class EfiDevicePathMediaCdRomData
    {
        public UInt32 BootCatalogEntry;
        public UInt64 PartitionRba;
        public UInt64 Sectors;

        public override string ToString()
        {
            return String.Join(",", BootCatalogEntry.ToString(), $"0x{PartitionRba:x}", $"0x{Sectors:x}");
        }
    }
}
