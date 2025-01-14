using BinaryCoder;
using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    internal class EfiDevicePathMediaHdData
    {
        public UInt32 PartitionNumber;
        public UInt64 Start;
        public UInt64 Size;
        [ArraySize(16)]
        public byte[] Signature;
        public byte Format;
        public byte SignatureType;
        // byte padding[6]; /* __ia64 Emperically needed */

        public override string ToString()
        {
            switch (SignatureType)
            {
                case Constants.EFIDP_HD_SIGNATURE_MBR:
                    var x = Signature[0] | Signature[1] << 8 | Signature[2] << 16 | Signature[3] << 24;

                    return String.Join(",", PartitionNumber.ToString(), "MBR", $"0x{x:x}", $"0x{Start:x}", $"0x{Size:x}");
                case Constants.EFIDP_HD_SIGNATURE_GUID:
                    return String.Join(",", PartitionNumber.ToString(), "GPT", new Guid(Signature).ToString());
                default:
                    return String.Join(
                        ",",
                        PartitionNumber.ToString(),
                        SignatureType.ToString(),
                        BitConverter.ToString(Signature).Replace("-", ""),
                        $"0x{Start:x}",
                        $"0x{Size:x}"
                    );
            }
        }
    }
}
