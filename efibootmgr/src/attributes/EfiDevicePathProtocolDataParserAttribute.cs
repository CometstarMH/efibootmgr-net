using BinaryCoder;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EfiBootMgr
{
    internal class EfiDevicePathProtocolDataParserAttribute : CustomParserAttribute
    {
        public override (object, int) Parse(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals)
        {
            return (readFieldVals[nameof(EfiDevicePathProtocol.Type)], readFieldVals[nameof(EfiDevicePathProtocol.Subtype)]) switch
            {
                // (Constants.EFIDP_END_TYPE, 0x01) => (throw new NotImplementedException()),
                // (Constants.EFIDP_END_TYPE, 0xFF) => (throw new NotImplementedException()),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_HD) => BytesReader.ReadObject<EfiDevicePathMediaHdData>(source),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_CDROM) => BytesReader.ReadObject<EfiDevicePathMediaCdRomData>(source),
                //(Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_VENDOR) => BytesReader.ReadObject<EfiDevicePathMediaVendorData>(source),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FILE) => BytesReader.ReadObject<EfiDevicePathMediaFileData>(source),
                // (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_PROTOCOL) => BytesReader.ReadObject<EfiDevicePathMediaProtocolData>(source),
                // (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FIRMWARE_FILE) => BytesReader.ReadObject<EfiDevicePathMediaFirmwareFileData>(source),
                // (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FIRMWARE_VOLUME) => BytesReader.ReadObject<EfiDevicePathMediaFirmwareVolumeData>(source),
                // (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_RELATIVE_OFFSET) => BytesReader.ReadObject<EfiDevicePathMediaRelativeOffsetData>(source),
                // (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_RAMDISK) => BytesReader.ReadObject<EfiDevicePathMediaRamdiskData>(source),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
