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

        /* 0x01 hardware device subtypes */
        public const byte EFIDP_HW_PCI = 0x01;
        public const byte EFIDP_HW_PCCARD = 0x02;
        public const byte EFIDP_HW_MMIO = 0x03;
        public const byte EFIDP_HW_VENDOR = 0x04;
        public const byte EFIDP_HW_CONTROLLER = 0x05;
        public const byte EFIDP_HW_BMC = 0x06;

        /* 0x02 acpi device subtypes */
        public const byte EFIDP_ACPI_HID = 0x01;
        public const byte EFIDP_ACPI_HID_EX = 0x02;
        public const byte EFIDP_ACPI_ADR = 0x03;

        /* 0x03 message device subtypes */
        public const byte EFIDP_MSG_ATAPI = 0x01;
        public const byte EFIDP_MSG_SCSI = 0x02;
        public const byte EFIDP_MSG_FIBRECHANNEL = 0x03;
        public const byte EFIDP_MSG_1394 = 0x04;
        public const byte EFIDP_MSG_USB = 0x05;
        public const byte EFIDP_MSG_I2O = 0x06;
        public const byte EFIDP_MSG_INFINIBAND = 0x09;
        public const byte EFIDP_MSG_VENDOR = 0x0a;
        public const byte EFIDP_MSG_MAC_ADDR = 0x0b;
        public const byte EFIDP_MSG_IPv4 = 0x0c;
        public const byte EFIDP_MSG_IPv6 = 0x0d;
        public const byte EFIDP_MSG_UART = 0x0e;
        public const byte EFIDP_MSG_USB_CLASS = 0x0f;
        public const byte EFIDP_MSG_USB_WWID = 0x10;
        public const byte EFIDP_MSG_LUN = 0x11;
        public const byte EFIDP_MSG_SATA = 0x12;
        public const byte EFIDP_MSG_ISCSI = 0x13;
        public const byte EFIDP_MSG_VLAN = 0x14;
        public const byte EFIDP_MSG_FIBRECHANNELEX = 0x15;
        public const byte EFIDP_MSG_SAS_EX = 0x16;
        public const byte EFIDP_MSG_NVME = 0x17;
        public const byte EFIDP_MSG_URI = 0x18;
        public const byte EFIDP_MSG_UFS = 0x19;
        public const byte EFIDP_MSG_SD = 0x1a;
        public const byte EFIDP_MSG_BT = 0x1b;
        public const byte EFIDP_MSG_WIFI = 0x1c;
        public const byte EFIDP_MSG_EMMC = 0x1d;
        public const byte EFIDP_MSG_BTLE = 0x1e;
        public const byte EFIDP_MSG_DNS = 0x1f;
        public const byte EFIDP_MSG_REST = 0x20;

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

        /* 0x05 bbs subtypes */
        public const byte EFIDP_BIOS_BOOT = 0x01;
    }
}