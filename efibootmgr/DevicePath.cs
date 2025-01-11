using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace EfiBootMgr.DevicePath
{
    abstract class EfiDevicePathProtocol
    {
        public readonly Guid InterfaceGuid = new Guid("8BE4DF61-93CA-11D2-AA0D-00E098032B8C");

        public byte Type
        {
            get
            {
                return Data[0];
            }
            protected set
            {
                Data[0] = value;
            }
        }

        public byte SubType
        {
            get
            {
                return Data[1];
            }
            protected set
            {
                Data[1] = value;
            }
        }

        public ushort Length
        {
            get
            {
                return BitConverter.ToUInt16(Data, 2);
            }
            protected set
            {
                // UEFI uses little-endian, last time I checked Windows doesn't even support big-endian
                var temp = BitConverter.GetBytes(value);
                Data[2] = temp[0];
                Data[3] = temp[1];
            }
        }

        /// <summary>
        /// Stores ALL bytes for the structure, not just data for each specific type.
        /// </summary>
        protected byte[] Data { get; set; } = new byte[0];

        public abstract string Format();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">ALL bytes for the structure</param>
        protected EfiDevicePathProtocol(byte[] data)
        {
            Debug.Assert(data != null);
            if (data.Length > ushort.MaxValue || data.Length < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Device Path data has invalid length.");
            }

            Data = data;
        }

        public static unsafe EfiDevicePathProtocol FromBuffer(IntPtr buffer, ulong maxLength = ulong.MaxValue, ulong offset = 0)
        {
            while (offset > int.MaxValue)
            {
                buffer += int.MaxValue;
                offset -= int.MaxValue;
            }

            buffer += (int)offset;

            void* ptr = buffer.ToPointer();
            byte type = *(byte*)ptr; // offset 0
            byte subtype = *((byte*)ptr + 1); // offset 1
            ushort length = *(ushort*)((byte*)ptr + 2); // offset 2, length of whole structure = 4 + n bytes
            if (offset + length > maxLength)
            {
                throw new ArgumentException(nameof(offset), $"Buffer at offset {offset} does not contain enough data as indicated at offset + 2.");
            }

            byte[] temp = new byte[length];
            Marshal.Copy(buffer, temp, 0, length);

            switch (type)
            {
                case 0x7F:
                    return new DevicePathEnd(temp);
                default:
                    return new DevicePathUnknown(temp);
            }
        }
    }

    class DevicePathUnknown : EfiDevicePathProtocol
    {
        public override string Format()
        {
            throw new NotImplementedException();
        }

        public DevicePathUnknown(byte[] data) : base(data)
        {
        }
    }

    class DevicePathEnd : EfiDevicePathProtocol
    {
        public bool IsEntirePathEnd
        {
            get
            {
                return SubType == 0xFF;
            }
        }

        public override string Format()
        {
            if (!IsEntirePathEnd) // instance end only => output a comma
            {
                return ",";
            }

            return "";
        }

        public DevicePathEnd(byte[] data) : base(data)
        {
            if (Length != 4) throw new ArgumentOutOfRangeException(nameof(data), "End of Hardware Device Path node has invalid length field, which must be 4.");
        }
    }

    class DevicePathPci : EfiDevicePathProtocol
    {
        public byte Function
        {
            get
            {
                return Data[4];
            }
            set
            {
                Data[4] = value;
            }
        }

        public byte Device
        {
            get
            {
                return Data[5];
            }
            set
            {
                Data[5] = value;
            }
        }

        public DevicePathPci(byte[] data) : base(data)
        {
            if (Length != 6) throw new ArgumentOutOfRangeException(nameof(data), "PCI Device Path node has invalid length field, which must be 4.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathPccard : EfiDevicePathProtocol
    {
        public byte FunctionNumber
        {
            get
            {
                return Data[4];
            }
            set
            {
                Data[4] = value;
            }
        }

        public DevicePathPccard(byte[] data) : base(data)
        {
            if (Length != 6) throw new ArgumentOutOfRangeException(nameof(data), "PCCARD Device Path node has invalid length field, which must be 6.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathMemoryMapped : EfiDevicePathProtocol
    {
        public EFI_MEMORY_TYPE MemoryType
        {
            // 0x70000000..0x7FFFFFFF are reserved for OEM use
            // 0x80000000..0xFFFFFFFF are reserved for use by UEFI OS loaders that are provided by operating system vendors
            get
            {
                return (EFI_MEMORY_TYPE)BitConverter.ToInt32(Data, 4);
            }
            set
            {
                Array.Copy(BitConverter.GetBytes((int)value), 0, Data, 4, sizeof(int));
            }
        }

        public ulong StartAddress
        {
            get
            {
                return BitConverter.ToUInt64(Data, 8);
            }
            set
            {
                Array.Copy(BitConverter.GetBytes(value), 0, Data, 8, sizeof(ulong));
            }
        }

        public ulong EndAddress
        {
            get
            {
                return BitConverter.ToUInt64(Data, 16);
            }
            set
            {
                Array.Copy(BitConverter.GetBytes(value), 0, Data, 16, sizeof(ulong));
            }
        }

        public DevicePathMemoryMapped(byte[] data) : base(data)
        {
            if (Length != 24) throw new ArgumentOutOfRangeException(nameof(data), "Memory Mapped Device Path node has invalid length field, which must be 24.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathVendor : EfiDevicePathProtocol
    {
        public Guid VendorGuid
        {
            /*
             *  typedef struct {
                UINT32 Data1 ;
                UINT16 Data2 ;
                UINT16 Data3 ;
                UINT8 Data4[8] ;
                } EFI_GUID;
             */

            get
            {
                // Although Microsoft use the same byte ordering (without explicitly documented) as defined in EFU_GUID struct, do it in a safe way none the less
                var temp = Data.Skip(4 + sizeof(uint) + sizeof(ushort) + sizeof(ushort)).Take(8).ToArray();
                return new Guid(BitConverter.ToUInt32(Data, 4), BitConverter.ToUInt16(Data, 4 + sizeof(uint)), BitConverter.ToUInt16(Data, 4 + sizeof(uint) + sizeof(ushort)), temp[0], temp[1], temp[2], temp[3], temp[4], temp[5], temp[6], temp[7]);
            }
            set
            {
                // TODO:
                Array.Copy(value.ToByteArray(), 0, Data, 4, 128 / 8);
            }
        }

        public byte[] VendorDefinedData
        {
            get
            {
                return Data.Skip(20).ToArray();
            }
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }

        public DevicePathVendor(byte[] data) : base(data)
        {
            if (Length < 20) throw new ArgumentOutOfRangeException(nameof(data), "Vendor Device Path node has invalid length field, which must be at least 20.");
        }
    }

    class DevicePathController : EfiDevicePathProtocol
    {
        public uint ControllerNumber
        {
            get
            {
                return BitConverter.ToUInt32(Data, 4);
            }
            set
            {
                Array.Copy(BitConverter.GetBytes(value), 0, Data, 4, sizeof(uint));
            }
        }

        public DevicePathController(byte[] data) : base(data)
        {
            if (Length != 8) throw new ArgumentOutOfRangeException(nameof(data), "Controller Device Path node has invalid length field, which must be 8.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathBmc : EfiDevicePathProtocol
    {
        public enum BmcInterfaceType
        {
            Unknown = 0x00,
            KCS = 0x01,
            SMIC = 0x02,
            BT = 0x03
        }
        public BmcInterfaceType InterfaceType
        {
            get
            {
                return (BmcInterfaceType)Data[4];
            }
            set
            {
                Data[4] = (byte)value;
            }
        }

        public ulong BaseAddress
        {
            get
            {
                return BitConverter.ToUInt64(Data, 8);
            }
            set
            {
                Array.Copy(BitConverter.GetBytes(value), 0, Data, 8, sizeof(ulong));
            }
        }

        public DevicePathBmc(byte[] data) : base(data)
        {
            if (Length != 13) throw new ArgumentOutOfRangeException(nameof(data), "BMC Device Path node has invalid length field, which must be 13.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathAcpi : EfiDevicePathProtocol
    {
        /// <summary>
        /// a 32-bit compressed EISA-type IDs
        /// </summary>
        public uint _HID
        {
            get
            {
                return BitConverter.ToUInt32(Data, 4);
            }
        }

        /// <summary>
        /// a 32-bit compressed EISA-type IDs
        /// </summary>
        public uint _UID
        {
            get
            {
                return BitConverter.ToUInt32(Data, 8);
            }
        }

        public DevicePathAcpi(byte[] data) : base(data)
        {
            if (Length != 12) throw new ArgumentOutOfRangeException(nameof(data), "ACPI Device Path node has invalid length field, which must be 12.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathAcpiExpanded : EfiDevicePathProtocol
    {
        /// <summary>
        /// a 32-bit compressed EISA-type ID
        /// </summary>
        public uint _HID
        {
            get
            {
                return BitConverter.ToUInt32(Data, 4);
            }
        }

        /// <summary>
        /// a 32-bit compressed EISA-type ID
        /// </summary>
        public uint _UID
        {
            get
            {
                return BitConverter.ToUInt32(Data, 8);
            }
        }

        /// <summary>
        /// a 32-bit compressed EISA-type ID
        /// </summary>
        public uint _CID
        {
            get
            {
                return BitConverter.ToUInt32(Data, 12);
            }
        }

        public unsafe string _HIDSTR
        {
            get
            {
                // Took a hint from Marshal.PtrToStringAnsi() reference source
                byte[] s = Data.Skip(16).TakeWhile(x => x != 0).ToArray();
                if (s.Length == 0) return "";
                fixed (byte* ps = s)
                {
                    return new string((sbyte*)ps, 0, s.Length, System.Text.Encoding.ASCII);
                }
            }
        }

        public unsafe string _UIDSTR
        {
            get
            {
                int offset = Data.Skip(16).TakeWhile(x => x != 0).Count() + 1; // +1 for the last null
                byte[] s = Data.Skip(16 + offset).TakeWhile(x => x != 0).ToArray();
                if (s.Length == 0) return "";
                fixed (byte* ps = s)
                {
                    return new string((sbyte*)ps, 0, s.Length, System.Text.Encoding.ASCII);
                }
            }
        }

        public unsafe string _CIDSTR
        {
            get
            {
                int offset = 16;
                offset += Data.Skip(offset).TakeWhile(x => x != 0).Count() + 1; // +1 for the last null
                offset += Data.Skip(offset).TakeWhile(x => x != 0).Count() + 1; // +1 for the last null
                byte[] s = Data.Skip(offset).TakeWhile(x => x != 0).ToArray();
                if (s.Length == 0) return "";
                fixed (byte* ps = s)
                {
                    return new string((sbyte*)ps, 0, s.Length, System.Text.Encoding.ASCII);
                }
            }
        }

        public DevicePathAcpiExpanded(byte[] data) : base(data)
        {
            if (Length < 19) throw new ArgumentOutOfRangeException(nameof(data), "Expanded ACPI Device Path node has invalid length field, which must be at least 19.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }

    class DevicePathAcpiAdr : EfiDevicePathProtocol
    {
        public uint _ADR
        {
            get
            {
                return BitConverter.ToUInt32(Data, 4);
            }
        }

        public uint[] Additional_ADR
        {
            get
            {
                int i = 8;
                var temp = new System.Collections.Generic.List<uint>();
                while (Length - i > 0)
                {
                    temp.Add(BitConverter.ToUInt32(Data, i));
                    i += sizeof(uint);
                }

                return temp.ToArray();
            }
        }

        public DevicePathAcpiAdr(byte[] data) : base(data)
        {
            if (Length < 8) throw new ArgumentOutOfRangeException(nameof(data), "ACPI _ADR Device Path node has invalid length field, which must be at least 8.");
        }

        public override string Format()
        {
            throw new NotImplementedException();
        }
    }
}