// ReSharper disable once IdentifierTypo
namespace Eisenscript;

// ReSharper disable once InconsistentNaming
public struct RGBA
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public RGBA()
    {
        R = G = B = A = 255;
    }

    public RGBA(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public RGBA(byte r, byte g, byte b) : this(r, g, b, 255) {}
}