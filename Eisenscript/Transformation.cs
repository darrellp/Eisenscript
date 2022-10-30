using System.Numerics;

// ReSharper disable once IdentifierTypo
namespace Eisenscript;

public class Transformation
{
    public Matrix4x4 Mtx { get; }

    // For color alterations
#pragma warning disable CS0414
    // ReSharper disable UnusedMember.Global
    public double DeltaH { get; }
    public double ScaleS { get; }
    public double ScaleB { get; }
    public double ScaleAlpha { get; }
    public bool IsAbsoluteColor { get; init;  }
    public RGBA AbsoluteColor { get; init; }
    // For color blends
    public RGBA BlendColor { get; init; }
    public double Strength { get; init; }
    // ReSharper restore UnusedMember.Global
#pragma warning restore CS0414

    private static readonly RGBA DefaultRgba = new RGBA();
    internal Transformation(Matrix4x4 mtx, double deltaH = 0.0,
        double scaleS = 1.0,
        double scaleB = 1.0,
        double scaleAlpha = 1.0)
    {
        Mtx = mtx;
        DeltaH = deltaH;
        ScaleS = scaleS;
        ScaleB = scaleB;
        ScaleAlpha = scaleAlpha;
    }
}