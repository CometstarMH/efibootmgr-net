using BinaryCoder;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    public class EfiLoadOption
    {
        public uint Attributes;
        public ushort FilePathListLength;
        [StringEncoding(StringEncoding.UTF16)]
        public string Description;
        [FilePathListParser]
        public EfiDevicePathProtocol[] FilePathList;
    }
}
