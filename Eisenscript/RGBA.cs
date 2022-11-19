// ReSharper disable once IdentifierTypo
using System.Drawing;

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

    internal (double hue, double sat, double val) HsvFromRgb()
    {
        var clr = Color.FromArgb(A, R, G, B);

        var hue = clr.GetHue();
        var sat = clr.GetSaturation();
        var val = clr.GetBrightness();
        return (hue, sat, val);
    }

    public static (double hue, double sat, double val) LerpHSV(
        double h1, double s1, double v1,
        double h2, double s2, double v2, double t)
    {
        // Hue interpolation
        double h = 0;
        if (s2 == 0 || v2 is 0 or 1)
        {
            // For black/white, hue is undefined
            h2 = h1;
        }
        double d = h2 - h1;
        if (h1 > h2)
        {
            (h1, h2) = (h2, h1);
            d = -d;
            t = 1 - t;
        }

        if (d > 0.5)
        {
            h1 = h1 + 360;
            h = (h1 + t * (h2 - h1)) % 360;
        }
        else
        {
            h = h1 + t * d;
        }

        // Interpolates the rest
        return 
        (
            h,                  // H
            s1 + t * (s2-s1),   // S
            v1 + t * (v2-v1)    // V
        );
    }

    public override string ToString()
    {
        return $"({R}, {G}, {B}, {A})";
    }

    public static RGBA RgbFromHsv(double h, double s, double b)
    {
        if (h is < 0f or > 360f) throw new ArgumentOutOfRangeException("h", h, "Invalid Hue");
        if (s is < 0f or > 1f) throw new ArgumentOutOfRangeException("s", s, "Invalid Saturation");
        if (b is < 0f or > 1f) throw new ArgumentOutOfRangeException("b", b, "Invalid Brightness");
        if (0 == s)
            return new RGBA(Convert.ToByte(b * 255), Convert.ToByte(b * 255), Convert.ToByte(b * 255));
        double fMax, fMid, fMin;
        if (0.5 < b)
        {
            fMax = b - b * s + s;
            fMin = b + b * s - s;
        }
        else
        {
            fMax = b + b * s;
            fMin = b - b * s;
        }

        var iSextant = (int) Math.Floor(h / 60);
        if (300 <= h)
        {
            h -= 360;
        }
        h /= 60;
        h -= 2 * Math.Floor((iSextant + 1) % 6 / 2.0);
        if (0 == iSextant % 2)
            fMid = h * (fMax - fMin) + fMin;
        else
            fMid = fMin - h * (fMax - fMin);
        var iMax = Convert.ToByte(fMax * 255);
        var iMid = Convert.ToByte(fMid * 255);
        var iMin = Convert.ToByte(fMin * 255);
        switch (iSextant)
        {
            case 1: return new RGBA(iMid, iMax, iMin);
            case 2: return new RGBA(iMin, iMax, iMid);
            case 3: return new RGBA(iMin, iMid, iMax);
            case 4: return new RGBA(iMid, iMin, iMax);
            case 5: return new RGBA(iMax, iMin, iMid);
            default: return new RGBA(iMax, iMid, iMin);
        }
    }
}