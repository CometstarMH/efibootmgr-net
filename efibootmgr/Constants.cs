namespace EfiBootMgr
{
    public static class Constants
    {
        /* load option attr flags */
        public const int LOAD_OPTION_ACTIVE = 0x00000001;
        public const int LOAD_OPTION_FORCE_RECONNECT = 0x00000002;
        public const int LOAD_OPTION_HIDDEN = 0x00000008;
        public const int LOAD_OPTION_CATEGORY = 0x00001F00;
        public const int LOAD_OPTION_CATEGORY_BOOT = 0x00000000;
        public const int LOAD_OPTION_CATEGORY_APP = 0x00000100;

        /* device path */
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