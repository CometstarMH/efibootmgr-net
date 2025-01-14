using BinaryCoder;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EfiBootMgr
{
    internal class GuidParserAttribute : CustomParserAttribute
    {
        [StructLayout(LayoutKind.Sequential)]
        private class MSGuid
        {
            public UInt32 TimeLow;
            public UInt16 TimeMid;
            public UInt16 TimeHighAndVersion;
            [ArraySize(8)]
            public byte[] FamilyAndNode;
        }

        public override (object, int) Parse(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals)
        {
            var (x, _) = BytesReader.ReadObject<MSGuid>(source.Slice(0, 16));
            var val = new Guid(x.TimeLow, x.TimeMid, x.TimeHighAndVersion, x.FamilyAndNode[0], x.FamilyAndNode[1], x.FamilyAndNode[2], x.FamilyAndNode[3], x.FamilyAndNode[4], x.FamilyAndNode[5], x.FamilyAndNode[6], x.FamilyAndNode[7]);

            return (val, 16);
        }
    }
}
