using System.Linq;
using System.Reflection;
using System.Text;

namespace BinaryCoder;

public static class BytesReader
{
    private static int SizeOf(Type type)
    {
        if (type == typeof(sbyte))
        {
            return sizeof(sbyte);
        }
        else if (type == typeof(byte))
        {
            return sizeof(byte);
        }
        else if (type == typeof(short))
        {
            return sizeof(short);
        }
        else if (type == typeof(ushort))
        {
            return sizeof(ushort);
        }
        else if (type == typeof(int))
        {
            return sizeof(int);
        }
        else if (type == typeof(uint))
        {
            return sizeof(uint);
        }
        else if (type == typeof(long))
        {
            return sizeof(long);
        }
        else if (type == typeof(ulong))
        {
            return sizeof(ulong);
        }
        else if (type == typeof(char))
        {
            return sizeof(char);
        }
        else if (type == typeof(float))
        {
            return sizeof(float);
        }
        else if (type == typeof(double))
        {
            return sizeof(double);
        }
        else if (type == typeof(decimal))
        {
            return sizeof(decimal);
        }
        else if (type == typeof(bool))
        {
            return sizeof(bool);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static dynamic ToPrimitive(ReadOnlySpan<byte> source, Type type, bool isLittleEndian = true)
    {
        var isMachineLittleEndian = BitConverter.IsLittleEndian;
        var bytes = source;

        if (isLittleEndian != isMachineLittleEndian)
        {
            var size = SizeOf(type);

            if (size > 1 && bytes.Length != size)
            {
                throw new NotImplementedException();
            }

            bytes = new ReadOnlySpan<byte>(source.ToArray().Reverse().ToArray());
        }

        if (type == typeof(sbyte))
        {
            return (sbyte)bytes[0];
        }
        else if (type == typeof(byte))
        {
            return bytes[0];
        }
        else if (type == typeof(short))
        {
            return BitConverter.ToInt16(bytes);
        }
        else if (type == typeof(ushort))
        {
            return BitConverter.ToUInt16(bytes);
        }
        else if (type == typeof(int))
        {
            return BitConverter.ToInt32(bytes);
        }
        else if (type == typeof(uint))
        {
            return BitConverter.ToUInt32(bytes);
        }
        else if (type == typeof(long))
        {
            return BitConverter.ToInt64(bytes);
        }
        else if (type == typeof(ulong))
        {
            return BitConverter.ToUInt64(bytes);
        }
        else if (type == typeof(char))
        {
            return BitConverter.ToChar(bytes);
        }
        else if (type == typeof(float))
        {
            return BitConverter.ToSingle(bytes);
        }
        else if (type == typeof(double))
        {
            return BitConverter.ToDouble(bytes);
        }
        else if (type == typeof(bool))
        {
            return BitConverter.ToBoolean(bytes);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static bool IsNumericType(object o)
    {
        switch (Type.GetTypeCode(o.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    private static (dynamic, int) ReadValue(ReadOnlySpan<byte> source, FieldInfo field)
    {
        var elementType = field.FieldType.IsArray ? field.FieldType.GetElementType()! : field.FieldType;
        var type = elementType.IsEnum ? Enum.GetUnderlyingType(elementType) : elementType;
        var size = SizeOf(type);
        var slice = source.Slice(0, size);
        var isBigEndian = field.GetCustomAttribute<BigEndianAttribute>(true) is not null;
        return (ToPrimitive(slice, type, !isBigEndian), size);
    }

    private static (string, int) ReadString(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals)
    {
        var attr = field.GetCustomAttribute<StringEncodingAttribute>(true);
        var encoding = attr is null ? StringEncoding.ASCII : attr.Encoding;
        ReadOnlySpan<byte> actualSlice;
        byte[]? nullPattern = null;
        string result = "";

        if (attr is null || attr.SizeField is null)
        {
            // null terminated. size of a null character depends on encoding
            nullPattern = (encoding == StringEncoding.UTF16 || encoding == StringEncoding.UTF16BE) ? new byte[] { 0, 0 } : new byte[] { 0 };
            var endIndex = source.IndexOf(new ReadOnlySpan<byte>(nullPattern));
            if (endIndex == -1)
            {
                throw new InvalidOperationException($"Null-terminated string field {field.Name} is specified but null character is not found in bytes source");
            }

            actualSlice = source.Slice(0, endIndex);
        }
        else
        {
            // SizeField is specified, try to see if field is read
            if (readFieldVals.TryGetValue(attr.SizeField, out dynamic? fieldVal) && IsNumericType(fieldVal))
            {
                // byte size
                actualSlice = source.Slice(0, (int)fieldVal);
            }
            else
            {
                throw new InvalidOperationException($"String field {field.Name} does not have a valid length specified");
            }
        }

        switch (encoding)
        {
            case StringEncoding.UTF16:
                result = Encoding.Unicode.GetString(actualSlice);
                break;
            case StringEncoding.UTF16BE:
                result = Encoding.BigEndianUnicode.GetString(actualSlice);
                break;
            case StringEncoding.UTF8:
                result = Encoding.UTF8.GetString(actualSlice);
                break;
            case StringEncoding.ASCII:
                result = Encoding.ASCII.GetString(actualSlice);
                break;
        }

        return (result, actualSlice.Length + (nullPattern is null ? 0 : nullPattern.Length));
    }

    private static (Array, int) ReadArray(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals)
    {
        var sizeAttr = field.GetCustomAttribute<ArraySizeAttribute>(true);

        if (sizeAttr is null)
        {
            throw new InvalidOperationException($"Array attribute {field.Name} does not have valid length specified");
        }

        Array result;
        var type = field.FieldType.GetElementType()!;
        var position = 0;
        int length;

        if (sizeAttr.SizeField is not null && readFieldVals.TryGetValue(sizeAttr.SizeField, out dynamic? fieldVal))
        {
            if (!IsNumericType(fieldVal))
            {
                throw new InvalidOperationException($"Array attribute {field.Name} does not have valid length specified");
            }

            length = fieldVal;
            result = Array.CreateInstance(type, length);
        }
        else if (sizeAttr.ConstantSize is not null)
        {
            length = (int)sizeAttr.ConstantSize;
            result = Array.CreateInstance(type, length);
        }
        else
        {
            throw new InvalidOperationException($"Array attribute {field.Name} does not have valid length specified");
        }

        if (type.IsPrimitive || type.IsEnum)
        {
            for (int i = 0; i < length; i++)
            {
                var (val, size) = ReadValue(source.Slice(position), field);
                result.SetValue(val, i);
                position += size;
            }
        }
        else if (type == typeof(string))
        {
            for (int i = 0; i < length; i++)
            {
                var (val, size) = ReadString(source.Slice(position), field, readFieldVals);
                result.SetValue(val, i);
                position += size;
            }
        }
        else if (type.IsArray)
        {
            // TODO: nested array?
            throw new NotImplementedException();
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                var (val, size) = ReadObject(source.Slice(position), type);
                result.SetValue(val, i);
                position += size;
            }
        }

        return (result, position);
    }

    private static void CheckFieldLayoutAuto(Type type)
    {
        // https://learn.microsoft.com/en-us/dotnet/api/system.type.structlayoutattribute?view=net-7.0#remarks
        // https://github.com/dotnet/docs/issues/42850
        // StructLayoutAttribute is a pseudo custom attribute
        // var typeLayoutAttr = type.StructLayoutAttribute; // may throw
        var layout = type.Attributes & TypeAttributes.LayoutMask;

        if (!type.IsClass || layout == TypeAttributes.AutoLayout)
        {
            throw new InvalidTargetClassException("Target T must be a class which has a StructLayoutAttribute with its value (LayoutKind) not Auto");
        }
    }

    private static int ReadObjectInternal(ReadOnlySpan<byte> source, dynamic obj, Type type)
    {
        var position = 0;
        Dictionary<string, dynamic> readFieldVals = new Dictionary<string, dynamic>();
        var bindingAttr = BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.SetField;
        var fields = type.GetFields(bindingAttr).OrderBy(f => System.Runtime.InteropServices.Marshal.OffsetOf(type, f.Name).ToInt32());
        var fieldIdx = -1;

        foreach (var field in fields)
        {
            fieldIdx++;
            var fieldType = field.FieldType;
            dynamic? val = null;
            int size = 0;

            var customParserAttr = field.GetCustomAttribute<CustomParserAttribute>(true);

            if (customParserAttr != null)
            {
                (val, size) = customParserAttr.Parse(source.Slice(position), field, readFieldVals);
            }
            else if (fieldType.IsPrimitive || fieldType.IsEnum)
            {
                (val, size) = ReadValue(source.Slice(position), field);
                readFieldVals.Add(field.Name, val);

                var structSizeAttr = field.GetCustomAttribute<StructSizeAttribute>(true);

                if (structSizeAttr != null)
                {
                    if (!IsNumericType(val))
                    {
                        throw new InvalidOperationException($"Field {field.Name} has {nameof(StructSizeAttribute)} specified but it is not numeric");
                    }

                    source = source.Slice(0, (int)val);
                }
            }
            else if (fieldType == typeof(string))
            {
                (val, size) = ReadString(source.Slice(position), field, readFieldVals);
            }
            else if (fieldType.IsArray)
            {
                var remaingingBytesAttr = field.GetCustomAttribute<RemainingBytesAttribute>(true);
                if (remaingingBytesAttr != null)
                {
                    if (type.GetFields(bindingAttr).Length - 1 != fieldIdx)
                    {
                        throw new InvalidOperationException($"A field with {nameof(RemainingBytesAttribute)} must be the last field.");
                    }

                    val = source.Slice(position).ToArray();
                    size = val.Length;
                }
                else
                {
                    (val, size) = ReadArray(source.Slice(position), field, readFieldVals);
                }
            }
            else
            {
                CheckFieldLayoutAuto(fieldType);
                // TODO: ExplicitLayout?
                (val, size) = ReadObject(source.Slice(position), fieldType);
            }

            field.SetValue(obj, val);
            position += size;

            if (position >= source.Length)
            {
                break;
            }
        }

        return position;
    }

    public static (object, int) ReadObject(ReadOnlySpan<byte> source, Type type)
    {
        object result = Activator.CreateInstance(type)!;
        var length = ReadObjectInternal(source, result, type);

        return (result, length);
    }

    public static (T, int) ReadObject<T>(ReadOnlySpan<byte> source) where T : new()
    {
        var type = typeof(T);

        CheckFieldLayoutAuto(type);

        var layout = type.Attributes & TypeAttributes.LayoutMask;
        if (layout == TypeAttributes.ExplicitLayout)
        {
            // TODO: ExplicitLayout?
            throw new NotImplementedException();
        }

        T result = Activator.CreateInstance<T>()!;
        var length = ReadObjectInternal(source, result, type);

        return (result, length);
    }
}