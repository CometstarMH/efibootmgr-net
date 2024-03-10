using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    public static class Utils
    {
        public static string UCS2BufferToString(IntPtr data)
        {
            // TODO: replace with proper UCS-2 filtering, i.e. no UTF-16 surrogate pairs
            return Marshal.PtrToStringUni(data);
        }

        public static string FormatDeviceNode(byte deviceNodeType, byte deviceNodeSubtype, params string[] optionParams)
        {
            var fallbackOptionNames = new Dictionary<byte, string>
            {
                [Constants.EFIDP_HARDWARE_TYPE] = "HardwarePath",
                [Constants.EFIDP_ACPI_TYPE] = "AcpiPath",
                [Constants.EFIDP_MESSAGE_TYPE] = "Msg",
                [Constants.EFIDP_MEDIA_TYPE] = "MediaPath",
                [Constants.EFIDP_BIOS_BOOT_TYPE] = "BbsPath",
            };
            var optionName = (deviceNodeType, deviceNodeSubtype) switch
            {
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_PCI) => "Pci",
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_PCCARD) => "PcCard",
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_MMIO) => "MemoryMapped",
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_VENDOR) => "VenHw",
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_CONTROLLER) => "Ctrl",
                (Constants.EFIDP_HARDWARE_TYPE, Constants.EFIDP_HW_BMC) => "BMC",
                (Constants.EFIDP_HARDWARE_TYPE, _) => fallbackOptionNames[deviceNodeType],

                (Constants.EFIDP_ACPI_TYPE, Constants.EFIDP_ACPI_HID) => throw new Exception("ACPI device path needs special handling"),
                (Constants.EFIDP_ACPI_TYPE, Constants.EFIDP_ACPI_HID_EX) => throw new Exception("ACPI device path needs special handling"),
                (Constants.EFIDP_ACPI_TYPE, Constants.EFIDP_ACPI_ADR) => "AcpiAdr",
                (Constants.EFIDP_ACPI_TYPE, _) => fallbackOptionNames[deviceNodeType],

                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_ATAPI) => "Ata",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_SCSI) => "Scsi",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_FIBRECHANNEL) => "Fibre",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_FIBRECHANNELEX) => "FibreEx",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_1394) => "I1394",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_USB) => "USB",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_I2O) => "I2O",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_INFINIBAND) => "Infiniband",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_VENDOR) => throw new Exception("Message device path Vendor subtype needs special handling"),
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_MAC_ADDR) => "MAC",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_IPv4) => "IPv4",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_IPv6) => "IPv6",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_UART) => "Uart",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_USB_CLASS) => throw new Exception("Message device path USB class subtype needs special handling"),
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_USB_WWID) => "UsbWwid",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_LUN) => "Unit",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_SATA) => "Sata",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_ISCSI) => "iSCSI",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_VLAN) => "Vlan",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_SAS_EX) => "SasEx",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_NVME) => "NVMe",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_URI) => "Uri",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_UFS) => "UFS",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_SD) => "SD",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_BT) => "Bluetooth",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_WIFI) => "Wi-Fi",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_EMMC) => "eMMC",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_BTLE) => "BluetoothLE",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_DNS) => "Dns",
                (Constants.EFIDP_MESSAGE_TYPE, Constants.EFIDP_MSG_REST) => "RestService",
                (Constants.EFIDP_MESSAGE_TYPE, _) => fallbackOptionNames[deviceNodeType],

                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_HD) => "HD",
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_CDROM) => "CDROM",
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_VENDOR) => "VenMedia",
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FILE) => throw new Exception("File path should not be handled here"),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_PROTOCOL) => "Media",
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FIRMWARE_FILE) => throw new Exception("Contents are defined in the UEFI PI Specification."),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_FIRMWARE_VOLUME) => throw new Exception("Contents are defined in the UEFI PI Specification."),
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_RELATIVE_OFFSET) => "Offset",
                (Constants.EFIDP_MEDIA_TYPE, Constants.EFIDP_MEDIA_RAMDISK) => throw new Exception("Ram disk needs special handling"),
                (Constants.EFIDP_MEDIA_TYPE, _) => fallbackOptionNames[deviceNodeType],

                (Constants.EFIDP_BIOS_BOOT_TYPE, Constants.EFIDP_BIOS_BOOT) => "BBS",
                (Constants.EFIDP_BIOS_BOOT_TYPE, _) => fallbackOptionNames[deviceNodeType],

                _ => "Path",
            };
            var optionParamString = String.Join(",", optionParams);

            if (optionName == "Path")
            {
                return $"Path({deviceNodeType},{deviceNodeSubtype},{optionParamString})";
            }
            else if(fallbackOptionNames.ContainsValue(optionName))
            {
                return $"{optionName}({deviceNodeSubtype},{optionParamString})";
            }
            else
            {
                return $"{optionName}({optionParamString})";
            }
        }
	}
}
