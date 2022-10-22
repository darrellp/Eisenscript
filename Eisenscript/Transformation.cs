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
}