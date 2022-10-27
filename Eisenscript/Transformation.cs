using System.Numerics;

namespace Eisenscript;

internal class Transformation
{
    private Matrix4x4 _mtx = Matrix4x4.Identity;

    // For color alterations
#pragma warning disable CS0414
    private double _deltaH = 0.0;
    private double _scaleS = 1.0;
    private double _scaleV = 1.0;
    private double _scaleAlpha = 1.0;
    private bool _isAbsoluteColor = false;

    // For color blends
    private RGBA _blendColor = new();
    private double _strength = 0.0;
#pragma warning restore CS0414

    internal Transformation(Matrix4x4 mtx,
        double deltaH = 0.0,
        double scaleS = 1.0,
        double scaleV = 1.0,
        double scaleAlpha = 1.0,
        bool isAbsoluteColor = false)
    {
        _mtx = mtx;

    }
}