using BinaryCoder;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EfiBootMgr
{
    internal class FilePathListParserAttribute : CustomParserAttribute
    {
        public override (object, int) Parse(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals)
        {
            var result = new List<EfiDevicePathProtocol>();
            var pos = 0;

            source = source.Slice(0, (int)readFieldVals[nameof(EfiLoadOption.FilePathListLength)]);

            while (true)
            {
                var (node, size) = BytesReader.ReadObject<EfiDevicePathProtocol>(source.Slice(pos));

                result.Add(node);
                pos += size;

                if (node.Type == Constants.EFIDP_END_TYPE && node.Subtype == 0xFF)
                {
                    if (size > 4)
                    {
                        throw new Exception("invalid end node, too long");
                    }

                    break;
                }

                if (node.Type == Constants.EFIDP_END_TYPE && node.Subtype == 0x01)
                {
                    if (size > 4)
                    {
                        throw new Exception("invalid end node, too long");
                    }
                }
            }

            return (result.ToArray(), pos);
        }
    }
}
