using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using BinaryCoder;
using System.Data;
using System.Runtime.Versioning;

namespace EfiBootMgr
{
    // https://stackoverflow.com/questions/1887288/marshal-allochglobal-vs-marshal-alloccotaskmem-marshal-sizeof-vs-sizeof
    // https://stackoverflow.com/questions/36420692/cotaskmemalloc-v-malloc-v-allochglobal
    // AllocHGlobal and AllocCoTaskMem behaviour has changed since Windows 8(?)/VS2012

    class Program
    {
        public static readonly Guid EFI_GLOBAL_GUID = new Guid("8BE4DF61-93CA-11D2-AA0D-00E098032B8C");

        public static UefiLoadOptionType VariableType { get; set; } = UefiLoadOptionType.Boot;

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

        static DisposableUnmanagedMemory GetFwEnvVariableBuffer(string name, int initsize = 1024)
        {
            int size = initsize <= 0 ? 1 : initsize;
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

            return new DisposableUnmanagedMemory(buffer, (int)rc);
        }

        static ushort[] ReadLoadOptionOrder(UefiLoadOptionType loadOptionType)
        {
            using (var buffer = GetFwEnvVariableBuffer(Enum.GetName(typeof(UefiLoadOptionType), loadOptionType) + "Order", sizeof(ushort) * 10))
            {
                unsafe
                {
                    return new Span<ushort>(buffer.Handle.ToPointer(), buffer.Size / sizeof(ushort)).ToArray();
                }
            }
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
                    Console.WriteLine("An unexpected error occurred when calulating buffer needed for boot entries. Error code: {status}");
                    throw new Win32Exception((int)status);
                }
            }

            using (var buf = new DisposableUnmanagedMemory((int)size))
            {
                status = Natives.NtEnumerateBootEntries(buf, ref size);

                if (status < 0)
                {
                    // An unexpected error occurred
                    Console.WriteLine($"An unexpected error occurred when reading boot entries. Error code: {status}");
                    throw new Win32Exception((int)status);
                }

                int offset = 0;
                NtExApiBootEntryList x;
                ReadOnlySpan<byte> wholeSpan;

                unsafe
                {
                    wholeSpan = new ReadOnlySpan<byte>(buf.Handle.ToPointer(), buf.Size);
                }

                do
                {
                    (x, _) = BytesReader.ReadObject<NtExApiBootEntryList>(wholeSpan.Slice(offset));

                    var id = x.BootEntry.Id;
                    result.Add($"Boot{id:X4}");

                    offset += (int)x.NextEntryOffset;
                } while (x.NextEntryOffset != 0 && offset < size);
            }

            return result;
        }

        static void Main(string[] args)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new Exception("This is for Windows only");
            }

            // Parse options
            new Mono.Options.OptionSet
            {
                // Windows only directly supports Driver and Boot variables
                //{ "y|sysprep", "Operate on SysPrep variables, not Boot Variables.", n => VariableType = UefiLoadOptionType.SysPrep },
                { "r|driver", "Operate on Driver variables, not Boot Variables.", n => VariableType = UefiLoadOptionType.Driver },
                { "v|verbose", "print additional information", n => ErrorHandling.Verbosity++ },
            }.Parse(args);

            if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)))
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
                int size = 0;
                EfiLoadOption loadOption;

                try
                {
                    using (var buf = GetFwEnvVariableBuffer(variableName))
                    {
                        ReadOnlySpan<byte> wholeSpan;

                        unsafe
                        {
                            wholeSpan = new ReadOnlySpan<byte>(buf.Handle.ToPointer(), buf.Size);
                        }

                        (loadOption, size) = BytesReader.ReadObject<EfiLoadOption>(wholeSpan);
                    }

                    var filePathStr = loadOption.FilePathList.Select((ele) => ele.ToString()).TakeWhile(x => x != null);

                    Console.Write(variableName + ((loadOption.Attributes & Constants.LOAD_OPTION_ACTIVE) != 0 ? "* " : "  ") + loadOption.Description);
                    Console.Write("\t");
                    Console.Write(string.Join("/", filePathStr));
                    Console.Write("\n");
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
        }

        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
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
}
