using BinaryCoder;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    internal class EfiDevicePathMediaFileData
    {
        [StringEncoding(StringEncoding.UTF16)]
        public string PathName;

        public override string ToString()
        {
            return $"File({PathName})";
        }
    }
}
