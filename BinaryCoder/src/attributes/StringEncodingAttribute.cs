namespace BinaryCoder;

[AttributeUsage(AttributeTargets.Field)]
public class StringEncodingAttribute : Attribute
{
    public StringEncoding Encoding { get; }
    public string? SizeField { get; }

    public StringEncodingAttribute(StringEncoding encoding) {
        Encoding = encoding;
    }

    public StringEncodingAttribute(StringEncoding encoding, string sizeField) {
        Encoding = encoding;
        SizeField = sizeField;
    }
}
