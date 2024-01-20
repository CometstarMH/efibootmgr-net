using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{

    // BOOT_ENTRY, which is the Windows representation of EFI_LOAD_OPTION 
    struct NtEfiBootEntry
    {
        // public uint Version;
        // public uint Length;
        public uint Id; // four-digit hex number part of its name Boot####
        // public uint Attributes;
        // public string FriendlyName; //FriendlyNameOffset: ULONG,
        // public string BootFilePath; //BootFilePathOffset: ULONG,
        // OsOptionsLength: ULONG,
        // OsOptions: [UCHAR; 1],

        public unsafe static NtEfiBootEntry FromNative(IntPtr data)
        {
            var result = new NtEfiBootEntry();
            result.Id = *(uint*)(data + 8);

            return result;
        }
    }

    // BOOT_ENTRY_LIST, singly linked list of BOOT_ENTRY
    struct NtEfiBootEntryList
    {
        public uint NextEntryOffset; // offset from start of whole data to next BOOT_ENTRY_LIST
        public IntPtr BootEntry; // BOOT_ENTRY, not actually a pointer, just put it here for convenience

        public unsafe static NtEfiBootEntryList FromNative(IntPtr data)
        {
            var result = new NtEfiBootEntryList();
            result.NextEntryOffset = *(uint*)data;
            result.BootEntry = data + 4;

            return result;
        }
    }

    // EFI_DRIVER_ENTRY, which is the Windows representation of EFI_LOAD_OPTION for Driver#### entries
    struct NtEfiDriverEntry
    {
        // public uint Version;
        // public uint Length;
        public uint Id; // four-digit hex number part of its name Boot####
        // public uint Attributes;
        // public string FriendlyName; //FriendlyNameOffset: ULONG,
        // public string BootFilePath; //BootFilePathOffset: ULONG,
        
        public unsafe static NtEfiDriverEntry FromNative(IntPtr data)
        {
            var result = new NtEfiDriverEntry();
            result.Id = *(uint*)(data + 8);

            return result;
        }
    }

    // EFI_DRIVER_ENTRY_LIST, singly linked list of EFI_DRIVER_ENTRY
    struct NtEfiDriverEntryList
    {
        public uint NextEntryOffset; // offset from start of whole data to next BOOT_ENTRY_LIST
        public IntPtr EfiDriverEntry; // EFI_DRIVER_ENTRY, not actually a pointer, just put it here for convenience

        public unsafe static NtEfiBootEntryList FromNative(IntPtr data)
        {
            var result = new NtEfiBootEntryList();
            result.NextEntryOffset = *(uint*)data;
            result.BootEntry = data + 4;

            return result;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EfiDpNodeHeader
    {
        public byte Type;
        public byte Subtype;
        public ushort Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EfiDpNodeHd
    {
        public EfiDpNodeHeader Header;
        public uint PartitionNumber;
        public ulong Start;
        public ulong Size;
        public fixed byte Signature[16];
        public byte Format;
        public byte SignatureType;
        // byte padding[6]; /* __ia64 Emperically needed */

        public override string ToString()
        {
            fixed (EfiDpNodeHd* p = &this)
            {
                var signatureBytes = new byte[16];
                Marshal.Copy((IntPtr)p->Signature, signatureBytes, 0, 16);

                switch (SignatureType)
                {
                    case Constants.EFIDP_HD_SIGNATURE_MBR:
                        // TODO:
                        var x = signatureBytes[0] | signatureBytes[1] << 8 | signatureBytes[2] << 16 | signatureBytes[3] << 24;

                        return $"HD({PartitionNumber},MBR,0x{x.ToString("x")},0x{Start.ToString("x")},0x{Size.ToString("x")})";
                    case Constants.EFIDP_HD_SIGNATURE_GUID:
                        return $"HD({PartitionNumber},GPT,{new Guid(signatureBytes).ToString()})";
                    default:
                        return $"HD({PartitionNumber},{SignatureType},{BitConverter.ToString(signatureBytes).Replace("-", "")},0x{Start.ToString("x")},0x{Size.ToString("x")})";
                }

            }
        }
    }

    // MUST MARSHALL MANUALLY
    public struct EfiDpNodeFile
    {
        public EfiDpNodeHeader Header;
        public string PathName;

        public override string ToString()
        {
            return $"File({PathName})";
        }
    }

    public struct EfiDpInstanceEndNode
    {
        public override string ToString()
        {
            return ",";
        }
    }
}