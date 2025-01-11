namespace BinaryCoder;

[AttributeUsage(AttributeTargets.Field)]
public class RemainingBytesAttribute : Attribute
{
    // If specified for a field, must be the last field
    // If length is not specified when reading from a bytes source, either this or StructLengthAttribute must be specified in the struct
}
