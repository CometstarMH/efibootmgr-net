using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using System.Linq;

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
            uint rc = 0;
            var buffer = IntPtr.Zero;
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

        static ushort[] read_order(UefiLoadOptionType loadOptionType)
        {
            (IntPtr buffer, uint size) = read_dyn(Enum.GetName(typeof(UefiLoadOptionType), loadOptionType) + "Order", sizeof(ushort) * 10);
            var result = new ushort[size / sizeof(ushort)];
            Copy(buffer, result, 0, size / sizeof(ushort));
            //Marshal.FreeCoTaskMem(buffer);
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        static void Main(string[] args)
        {
            // Parse options
            new Mono.Options.OptionSet
            {
                { "y|sysprep", "Operate on SysPrep variables, not Boot Variables.", n => VariableType = UefiLoadOptionType.SysPrep },
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

            ErrorHandling.HandleWarning(() => Console.WriteLine($"{Enum.GetName(typeof(UefiLoadOptionType), VariableType)}Order: {string.Join(",", read_order(VariableType).Select(y => $"{y:X4}"))}"),
                VariableType == UefiLoadOptionType.Boot ? "No BootOrder is set; firmware will attempt recovery" : $"No { Enum.GetName(typeof(UefiLoadOptionType), VariableType)}Order is set", 
                verbosity: 2);

            // Windows does not provide any function to iterate over all available firmware variables. Oh well.
            for (int i = 0; i <= 0xFFFF; i++)
            {
                var variableName = Enum.GetName(typeof(UefiLoadOptionType), VariableType) + $"{i:X4}";
                var buffer = IntPtr.Zero; 
                uint size = 0;
                try
                {
                    (buffer, size) = read_dyn(variableName);
                    var loadOption = EfiLoadOption.MarshalFromNative(buffer);
                    Console.WriteLine(variableName + ((loadOption.Attributes & EfiLoadOption.LOAD_OPTION_ACTIVE) != 0 ? "* " : "  ") + loadOption.Description);
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

            //show_mirror??
#if DEBUG
            Console.ReadLine();
#endif
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

    struct EfiLoadOption
    {
        public uint Attributes;
        public ushort FilePathListLength;
        public string Description;
        //public EFI_DEVICE_PATH_PROTOCOL FilePathList;
        public byte[] OptionalData;

        public const int LOAD_OPTION_ACTIVE = 0x00000001;
        public const int LOAD_OPTION_FORCE_RECONNECT = 0x00000002;
        public const int LOAD_OPTION_HIDDEN = 0x00000008;
        public const int LOAD_OPTION_CATEGORY = 0x00001F00;
        public const int LOAD_OPTION_CATEGORY_BOOT = 0x00000000;
        public const int LOAD_OPTION_CATEGORY_APP = 0x00000100;

        public unsafe static EfiLoadOption MarshalFromNative(IntPtr data)
        {
            var result = new EfiLoadOption();
            result.Attributes = *(uint*)data;
            result.FilePathListLength = *(ushort*)(data + 4);
            result.Description = Marshal.PtrToStringUni(data + 6);

            return result;
        }

        //public static IntPtr MarshalToNative(EFILoadOption data)
        //{

        //}
    }

}
