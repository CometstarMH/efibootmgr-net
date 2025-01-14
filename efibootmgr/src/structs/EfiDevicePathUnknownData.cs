using BinaryCoder;
using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    [StructLayout(LayoutKind.Sequential)]
    internal class EfiDevicePathUnknownData
    {
        [RemainingBytes]
        public byte[] Dump;

        public override string ToString()
        {
            return BitConverter.ToString(Dump).Replace("-", "");
        }
    }
}
