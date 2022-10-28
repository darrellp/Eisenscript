using System.Numerics;

namespace Eisenscript;

public class Transformation
{
    public Matrix4x4 Mtx { get; } = Matrix4x4.Identity;

    // For color alterations
#pragma warning disable CS0414
    public double DeltaH { get; } = 0.0;
    public double ScaleS { get; } = 1.0;
    public double ScaleV { get; } = 1.0;
    public double ScaleAlpha { get; } = 1.0;
    public bool IsAbsoluteColor { get; } = false;

    // For color blends
    public RGBA BlendColor { get; } = new();
    public double Strength { get; } = 0.0;
#pragma warning restore CS0414

    internal Transformation(Matrix4x4 mtx,
        double deltaH = 0.0,
        double scaleS = 1.0,
        double scaleV = 1.0,
        double scaleAlpha = 1.0,
        bool isAbsoluteColor = false)
    {
        Mtx = mtx;

    }
}