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
    public double ScaleV { get; }
    public double ScaleAlpha { get; }
    public bool IsAbsoluteColor { get; }

    // For color blends
    public RGBA BlendColor { get; } = new();
    public double Strength { get; } = 0.0;
    // ReSharper restore UnusedMember.Global
#pragma warning restore CS0414

    internal Transformation(Matrix4x4 mtx,
        double deltaH = 0.0,
        double scaleS = 1.0,
        double scaleV = 1.0,
        double scaleAlpha = 1.0,
        bool isAbsoluteColor = false)
    {
        Mtx = mtx;
        DeltaH = deltaH;
        ScaleS = scaleS;
        ScaleV = scaleV;
        ScaleAlpha = scaleAlpha;
        IsAbsoluteColor = isAbsoluteColor;
    }
}