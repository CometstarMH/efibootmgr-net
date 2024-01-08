namespace EfiBootMgr
{
    public static class Constants
    {
        /* top-level types */
        public const byte EFIDP_HARDWARE_TYPE = 0x01;
        public const byte EFIDP_ACPI_TYPE = 0x02;
        public const byte EFIDP_MESSAGE_TYPE = 0x03;
        public const byte EFIDP_MEDIA_TYPE = 0x04;
        public const byte EFIDP_BIOS_BOOT_TYPE = 0x05;
        public const byte EFIDP_END_TYPE = 0x7f;

        /* 0x04 media subtypes */
        public const byte EFIDP_MEDIA_HD = 0x01;
        public const byte EFIDP_MEDIA_CDROM = 0x02;
        public const byte EFIDP_MEDIA_VENDOR = 0x03;
        public const byte EFIDP_MEDIA_FILE = 0x04;
        public const byte EFIDP_MEDIA_PROTOCOL = 0x05;
        public const byte EFIDP_MEDIA_FIRMWARE_FILE = 0x06;
        public const byte EFIDP_MEDIA_FIRMWARE_VOLUME = 0x07;
        public const byte EFIDP_MEDIA_RELATIVE_OFFSET = 0x08;
        public const byte EFIDP_MEDIA_RAMDISK = 0x09;

        /* 0x04 0x01 hd signature types */
        public const byte EFIDP_HD_SIGNATURE_NONE = 0x00;
        public const byte EFIDP_HD_SIGNATURE_MBR = 0x01;
        public const byte EFIDP_HD_SIGNATURE_GUID = 0x02;
    }
}