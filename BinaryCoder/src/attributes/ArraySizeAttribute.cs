namespace BinaryCoder;

[AttributeUsage(AttributeTargets.Field)]
public class ArraySizeAttribute : Attribute
{
    public int? ConstantSize { get; } = null;
    public string? SizeField { get; }

    public ArraySizeAttribute(int constantSize)
    {
        ConstantSize = constantSize;
    }

    public ArraySizeAttribute(string sizeField)
    {
        SizeField = sizeField;
    }
}