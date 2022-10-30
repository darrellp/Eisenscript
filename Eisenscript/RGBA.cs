// ReSharper disable once IdentifierTypo
namespace Eisenscript;

// ReSharper disable once InconsistentNaming
public struct RGBA
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public byte A;

    public RGBA()
    {
        R = G = B = A = 255;
    }

    public RGBA(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    internal static RGBA RgbFromHsv(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = Convert.ToByte(value);
        var p = Convert.ToByte(value * (1 - saturation));
        var q = Convert.ToByte(value * (1 - f * saturation));
        var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => new RGBA(v, t, p),
            1 => new RGBA(q, v, p),
            2 => new RGBA(p, v, t),
            3 => new RGBA(p, q, v),
            4 => new RGBA(t, p, v),
            _ => new RGBA(v, p, q)
        };
    }

    internal (double hue, double sat, double val) HsvFromRgb()
    {
        int max = Math.Max(R, Math.Max(G, B));
        int min = Math.Min(R, Math.Min(G, B));

        var hue = GetHue();
        var sat = (max == 0) ? 0 : 1d - (1d * min / max);
        var val = max / 255d;
        return (hue, sat, val);
    }

    private double GetHue()
    {
        double hue;

        var max = Math.Max(R, Math.Max(G, B));
        var min = Math.Min(R, Math.Min(G, B));
        var delta = (double)(max - min);
        if (delta == 0)
        {
            return 0.0;
        }

        if (max == R)
        {
            hue = (G - B) / delta;
        }
        else if (max == B)
        {
            hue = 2.0 + (B - R) / delta;
        }
        else
        {
            hue = 4 + (R - G) / delta;
        }

        hue *= 60;
        if (hue < 0)
        {
            hue += 360;
        }

        return hue;
    }


}