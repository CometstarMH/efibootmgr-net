using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace EfiBootMgr
{
    // https://stackoverflow.com/questions/1887288/marshal-allochglobal-vs-marshal-alloccotaskmem-marshal-sizeof-vs-sizeof
    // https://stackoverflow.com/questions/36420692/cotaskmemalloc-v-malloc-v-allochglobal
    // AllocHGlobal and AllocCoTaskMem behaviour has changed since Windows 8(?)/VS2012

    class Program
    {
        public static readonly Guid EFI_GLOBAL_GUID = new Guid("8BE4DF61-93CA-11D2-AA0D-00E098032B8C");

        public static UefiLoadOptionType VariableType { get; set; } = UefiLoadOptionType.Boot;

        static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        unsafe static void Copy(IntPtr source, ushort[] destination, uint startIndex, uint length)
        {
            var sourcePtr = (ushort*)source;
            for (uint i = startIndex; i < startIndex + length; ++i)
            {
                destination[i] = *sourcePtr++;
            }
        }

        unsafe static ushort read_u16(string name)
        {
            ushort value = 0;
            //uint32_t attributes = 0;
            var rc = Natives.GetFirmwareEnvironmentVariable(name, EFI_GLOBAL_GUID.ToString("B"), new IntPtr(&value), sizeof(ushort));

            if (rc == 0)
            {
                //int error = Marshal.GetLastWin32Error(); // https://stackoverflow.com/a/17918729
                throw new Win32Exception();
            }
            else if (rc != sizeof(ushort))
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        static (IntPtr buffer, uint size) read_dyn(string name, int initsize = 1024)
        {
            int size = initsize;
            uint rc;
            IntPtr buffer;
            while (true)
            {
                //buffer = Marshal.AllocCoTaskMem(size);
                buffer = Marshal.AllocHGlobal(size);
                rc = Natives.GetFirmwareEnvironmentVariable(name, EFI_GLOBAL_GUID.ToString("B"), buffer, (uint)size);
                if (rc == 0)
                {
                    var lastError = Marshal.GetLastWin32Error();
                    if (lastError == Natives.ERROR_INSUFFICIENT_BUFFER) // initial size not enough, reallocate and try again
                    {
                        //Marshal.FreeCoTaskMem(buffer);
                        Marshal.FreeHGlobal(buffer);
                        size *= 2;
                        continue;
                    }
                    throw new Win32Exception(lastError);
                }
                break;
            }

            return (buffer, rc);
        }

        static ushort[] ReadLoadOptionOrder(UefiLoadOptionType loadOptionType)
        {
            (IntPtr buffer, uint size) = read_dyn(Enum.GetName(typeof(UefiLoadOptionType), loadOptionType) + "Order", sizeof(ushort) * 10);
            var result = new ushort[size / sizeof(ushort)];
            Copy(buffer, result, 0, size / sizeof(ushort));
            //Marshal.FreeCoTaskMem(buffer);
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        static List<string> ReadAllBootEntryNames()
        {
            var result = new List<string>();
            uint size = 0;
            var buffer = IntPtr.Zero;
            var status = Natives.NtEnumerateBootEntries(buffer, ref size);
            if (status != NtStatus.BufferTooSmall)
            {
                if (status >= 0)
                {
                    // Somehow there are no boot entries in NVRAM.
                    Console.WriteLine("Somehow there are no boot entries in NVRAM.");
                    return result;
                }
                else
                {
                    // An unexpected error occurred
                    Console.WriteLine("An unexpected error occurred.");
                    throw new Exception();
                }
            }

            buffer = Marshal.AllocHGlobal((int)size);
            status = Natives.NtEnumerateBootEntries(buffer, ref size);

            if (status < 0)
            {
                // An unexpected error occurred
                Console.WriteLine("An unexpected error occurred. 2");
                Marshal.FreeHGlobal(buffer);
                throw new Exception();
            }

            var currPtr = buffer;

            for (var x = NtEfiBootEntryList.FromNative(currPtr); ; x = NtEfiBootEntryList.FromNative(currPtr += (int)x.NextEntryOffset))
            {
                var id = NtEfiBootEntry.FromNative(x.BootEntry).Id;
                result.Add($"Boot{id.ToString("X4")}");

                if (x.NextEntryOffset == 0)
                {
                    break;
                }
            }

            Marshal.FreeHGlobal(buffer);
            return result;
        }

        static void Main(string[] args)
        {
            // Parse options
            new Mono.Options.OptionSet
            {
                // Windows only directly supports Driver and Boot variables
                //{ "y|sysprep", "Operate on SysPrep variables, not Boot Variables.", n => VariableType = UefiLoadOptionType.SysPrep },
                { "r|driver", "Operate on Driver variables, not Boot Variables.", n => VariableType = UefiLoadOptionType.Driver },
                { "v|verbose", "print additional information", n => ErrorHandling.Verbosity++ },
            }.Parse(args);


            if (!IsElevated)
            {
                Console.WriteLine("Administrative privileges are needed.");
                return;
            }
            Privileges.EnablePrivilege(Privileges.SecurityEntity.SE_SYSTEM_ENVIRONMENT_NAME);

            if (VariableType == UefiLoadOptionType.Boot)
            {
                ErrorHandling.HandleWarning(() => Console.WriteLine($"BootNext: {read_u16("BootNext")}"), "Could not read variable 'BootNext'", verbosity: 2);
                ErrorHandling.HandleWarning(() => Console.WriteLine($"BootCurrent: {read_u16("BootCurrent"):X4}"), "Could not read variable 'BootCurrent'", verbosity: 2);
                ErrorHandling.HandleWarning(() => Console.WriteLine($"Timeout : {read_u16("Timeout")}"), "Could not read variable 'Timeout'", verbosity: 2);
            }

            ErrorHandling.HandleWarning(() => Console.WriteLine($"{Enum.GetName(typeof(UefiLoadOptionType), VariableType)}Order: {string.Join(",", ReadLoadOptionOrder(VariableType).Select(y => $"{y:X4}"))}"),
                VariableType == UefiLoadOptionType.Boot ? "No BootOrder is set; firmware will attempt recovery" : $"No { Enum.GetName(typeof(UefiLoadOptionType), VariableType)}Order is set",
                verbosity: 2);

            // Windows does not provide any documented function to iterate over all available firmware variables
            // An undocumented function NtEnumerateBootEntries ntdll.dll returns all boot options in Windows specific struct
            // It internally calls HalEnumerateEnvironmentVariablesEx from hal.dll, which actually returns all variables,
            // filters them to only show Boot#### variables, and do additional parsing for convenience and Windows specific options. 

            var BootEntryNames = ReadAllBootEntryNames();

            foreach (var variableName in BootEntryNames)
            {
                var buffer = IntPtr.Zero;
                uint size = 0;
                try
                {
                    (buffer, size) = read_dyn(variableName);
                    var loadOption = EfiLoadOption.FromNative(buffer, size);
                    Console.Write(variableName + ((loadOption.Attributes & Constants.LOAD_OPTION_ACTIVE) != 0 ? "* " : "  ") + loadOption.Description);
                    Console.Write("\t");
                    Console.WriteLine(loadOption.FilePathList.ToString());
                }
                catch (Win32Exception win32exeption)
                {
                    if (win32exeption.NativeErrorCode == 203) //not exist, skip
                    {
                        continue;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

#if DEBUG
            Console.ReadLine();
#endif
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
    }

    enum EFI_MEMORY_TYPE
    {
        EfiReservedMemoryType,
        EfiLoaderCode,
        EfiLoaderData,
        EfiBootServicesCode,
        EfiBootServicesData,
        EfiRuntimeServicesCode,
        EfiRuntimeServicesData,
        EfiConventionalMemory,
        EfiUnusableMemory,
        EfiACPIReclaimMemory,
        EfiACPIMemoryNVS,
        EfiMemoryMappedIO,
        EfiMemoryMappedIOPortSpace,
        EfiPalCode,
        EfiPersistentMemory,
        EfiMaxMemoryType
    }

    enum UefiLoadOptionType
    {
		Boot,
		Driver,
		SysPrep,
    }

    // EFI_LOAD_OPTION
    struct EfiLoadOption
    {
        public uint Attributes;
        public ushort FilePathListLength;
        public string Description;
        public EfiDevicePath FilePathList;
        //public byte[] OptionalData;

        private byte[] data;

        public unsafe static EfiLoadOption FromNative(IntPtr data, uint size)
        {
            var result = new EfiLoadOption();
            result.Attributes = *(uint*)data;
            result.FilePathListLength = *(ushort*)(data + 4);
            result.Description = Utils.UCS2BufferToString(data + 6);
            result.data = new byte[size];

            Marshal.Copy(data, result.data, 0, (int)size);

            var startFilePathList = 6 + System.Text.Encoding.Unicode.GetByteCount(result.Description) + 2;

            if (size - startFilePathList >= result.FilePathListLength)
            {
                result.FilePathList = EfiDevicePath.FromNative(data + startFilePathList, result.FilePathListLength);
            }
            else
            {
                throw new Exception($"EFI load option `{result.Description}` has invalid FilePathListLength");
            }


            return result;
        }
    }

    struct EfiDevicePath
    {
        public List<ValueType> Nodes;

        public override string ToString()
        {
            return string.Join("/", Nodes);
        }

        public unsafe static EfiDevicePath FromNative(IntPtr data, uint size)
        {
            var result = new EfiDevicePath() { Nodes = new List<ValueType>() };
            IntPtr start = data;
            var endAllReached = false;
            
            while (start.ToInt64() < (data.ToInt64() + size))
            {
                var header = Marshal.PtrToStructure<EfiDpNodeHeader>(start);

                if ((start + header.Length).ToInt64() > (data.ToInt64() + size))
                {
                    throw new Exception("device path node length overruns buffer");
                }

                switch (header.Type)
                {
                    case Constants.EFIDP_HARDWARE_TYPE:
                        switch (header.Subtype)
                        {
                            default:
                                {
                                    var node = Marshal.PtrToStructure<EfiDpNodeUnknown>(start);

                                    result.Nodes.Add(node);
                                    //Console.WriteLine(header.Type);
                                    //throw new Exception("invalid device path node type");
                                    break;
                                }
                        }

                        break;
                    case Constants.EFIDP_ACPI_TYPE:
                        /*
                        if (n.Length > 1024)
                        {
                            throw new Exception("invalid ACPI node");
                        }
                        */
                        switch (header.Subtype)
                        {
                            default:
                                {
                                    var node = Marshal.PtrToStructure<EfiDpNodeUnknown>(start);
                                    result.Nodes.Add(node);
                                    break;
                                }
                        }
                        break;
                    case Constants.EFIDP_MESSAGE_TYPE:
                        /*
                        if (n.SubType != 0x0a && n.Length > 1024)
                        {
                            throw new Exception("invalid message node");
                        }
                        */
                        switch (header.Subtype)
                        {
                            default:
                                {
                                    var node = Marshal.PtrToStructure<EfiDpNodeUnknown>(start);
                                    result.Nodes.Add(node);
                                    break;
                                }
                        }
                        break;
                    case Constants.EFIDP_MEDIA_TYPE:
                        switch (header.Subtype)
                        {
                            case Constants.EFIDP_MEDIA_HD:
                                var hdNode = Marshal.PtrToStructure<EfiDpNodeHd>(start);
                                result.Nodes.Add(hdNode);

                                break;
                            case Constants.EFIDP_MEDIA_FILE:
                                var filePath = Utils.UCS2BufferToString(start + sizeof(EfiDpNodeHeader));
                                result.Nodes.Add(new EfiDpNodeFile() { PathName = filePath, Header = header });

                                break;
                            default:
                                throw new Exception("invalid media node");
                        }
                        break;
                    case Constants.EFIDP_BIOS_BOOT_TYPE:
                        switch (header.Subtype)
                        {
                            default:
                                {
                                    var node = Marshal.PtrToStructure<EfiDpNodeUnknown>(start);
                                    result.Nodes.Add(node);
                                    break;
                                }
                        }
                        break;
                    case Constants.EFIDP_END_TYPE:
                        if (header.Length > 4)
                        {
                            throw new Exception("invalid end node, too long");
                        }

                        if (header.Subtype == 0x01)
                        {
                            result.Nodes.Add(new EfiDpInstanceEndNode());
                        }
                        else if (header.Subtype == 0xFF)
                        {
                            endAllReached = true;
                        }
                        else
                        {
                            throw new Exception("invalid end node");
                        }

                        break;
                    default:
                        {
                            var node = Marshal.PtrToStructure<EfiDpNodeUnknown>(start);

                            result.Nodes.Add(node);
                            //Console.WriteLine(header.Type);
                            //throw new Exception("invalid device path node type");
                            break;
                        }

                }

                start += header.Length;

                if (endAllReached)
                {
                    break;
                }
            }

            if (!endAllReached)
            {
                throw new Exception("device path missing end node");
            }

            return result;
        }
    }
}
