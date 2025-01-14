using BinaryCoder;
using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    public class EfiDevicePathProtocol
    {
        public byte Type;
        public byte Subtype;
        [StructSize]
        public UInt16 Length;
        [EfiDevicePathProtocolDataParser]
        public object Data;

        public override string ToString()
        {
            string res = null;

            switch ((Type, Subtype))
            {
                case (Constants.EFIDP_HARDWARE_TYPE, _):
                    throw new NotImplementedException();
                case (Constants.EFIDP_ACPI_TYPE, _):
                    throw new NotImplementedException();
                case (Constants.EFIDP_MESSAGE_TYPE, _):
                    throw new NotImplementedException();
                case (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FILE):
                    return Data.ToString();
                case (Constants.EFIDP_MEDIA_TYPE, _):
                    return Utils.FormatDeviceNode(Constants.EFIDP_MEDIA_TYPE, Subtype, Data.ToString());
                case (Constants.EFIDP_BIOS_BOOT_TYPE, _):
                    throw new NotImplementedException();
                case (Constants.EFIDP_END_TYPE, 0x01):
                    return ",";
                case (Constants.EFIDP_END_TYPE, 0xFF):
                    return null;
            }

            Data.ToString();


            return res;
        }
    }
}
