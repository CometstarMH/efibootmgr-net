// See https://aka.ms/new-console-template for more information
using BinaryCoder;
using System.Runtime.InteropServices;

Console.WriteLine("start");

byte[] a = new byte[] {
    5,
    3, 2, 7, 11, // int
    3, // array length
    6, 2, 4, // array
    72, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 33, 0, // Hello world!
    // nested struct
    42,
    13, 5, 11, 3, // int
    // array of nested struct
    2, // array length
    42,
    13, 5, 11, 3, // int
    42,
    13, 5, 11, 3, // int
    1, 2, 3, 4, 5, 6,
};
var (x, pos) = BytesReader.ReadObject<X>(new Span<byte>(a));

Console.WriteLine(x.a);
Console.WriteLine(x.b);
Console.WriteLine(x.c);
Console.WriteLine(x.d[0]);
Console.WriteLine(x.d[1]);
Console.WriteLine(x.d[2]);
Console.WriteLine(x.e);
Console.WriteLine(x.f.a);
Console.WriteLine(x.f.b);
Console.WriteLine(x.h[0].a);
Console.WriteLine(x.h[1].b);
Console.WriteLine(String.Join(", ", x.i));

[StructLayout(LayoutKind.Sequential)]
public class X
{
    public byte a;
    public int b;
    public byte c;

    [ArraySize("c")]
    public byte[] d;

    [StringEncoding(StringEncoding.ASCII)]
    public string e;

    public Y f;
    public byte g;
    [ArraySize(nameof(g))]
    public Y[] h;
    [RemainingBytes]
    public byte[] i;
}

[StructLayout(LayoutKind.Sequential)]
public class Y
{
    public byte a;
    public int b;
}

