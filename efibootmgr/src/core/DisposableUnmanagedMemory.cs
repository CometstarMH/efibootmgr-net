using System;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    internal class DisposableUnmanagedMemory : IDisposable
    {
        public static implicit operator IntPtr(DisposableUnmanagedMemory a) => a.Handle;

        public IntPtr Handle {  get; private set; }
        public int Size { get; private set; }

        public DisposableUnmanagedMemory(int size)
        {
            Handle = Marshal.AllocHGlobal(size);
            Size = size;
        }

        public DisposableUnmanagedMemory(IntPtr handle, int size)
        {
            Handle = handle;
            Size = size;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero) Marshal.FreeHGlobal(Handle);
            Handle = IntPtr.Zero;
        }
    }
}
