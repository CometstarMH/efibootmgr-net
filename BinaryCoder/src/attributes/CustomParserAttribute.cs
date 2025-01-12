using System.Reflection;

namespace BinaryCoder;

[AttributeUsage(AttributeTargets.Field)]
public abstract class CustomParserAttribute : Attribute
{
    public abstract (object, int) Parse(ReadOnlySpan<byte> source, FieldInfo field, Dictionary<string, dynamic> readFieldVals);
}
