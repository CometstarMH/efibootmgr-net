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
	}
}
