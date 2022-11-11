using System.Numerics;

// ReSharper disable once IdentifierTypo
namespace Eisenscript;

public class Transformation
{
    #region Values
    public Matrix4x4 Mtx { get; }

    // For color alterations
#pragma warning disable CS0414
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    private readonly double _deltaH;
    private readonly double _scaleS;
    private readonly double _scaleB;
    private readonly double _scaleAlpha;
    internal bool IsAbsoluteColor { get; init; }
    internal RGBA AbsoluteColor { get; init; }
    internal bool IsRandomColor { get; init; }

    // For color blends
    private double _hBlend;
    private double _sBlend;
    private double _vBlend;
    internal double Strength { get; init; }
    private bool _hsbRequired;
    private bool _colorAlteration;
    private bool _colorValidated;
    internal RGBA BlendColor
    {
        set => (_hBlend, _sBlend, _vBlend) = value.HsvFromRgb();
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
#pragma warning restore CS0414
    #endregion

    #region Constructor
    internal Transformation(Matrix4x4 mtx,
        double deltaH = 0.0,
        double scaleS = 1.0,
        double scaleB = 1.0,
        double scaleAlpha = 1.0)
    {
        scaleS = Math.Max(scaleS, 0);
        scaleB = Math.Max(scaleB, 0);
        Mtx = mtx;
        _deltaH = deltaH;
        _scaleS = scaleS;
        _scaleB = scaleB;
        _scaleAlpha = scaleAlpha;
    }
    #endregion

    #region Transformation
    public (Matrix4x4, RGBA) DoTransform(Matrix4x4 matrix, RGBA rgba)
    {
        if (!_colorValidated)
        {
            _colorValidated = true;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            _hsbRequired = _deltaH != 0 || _scaleS != 1 || _scaleB != 1 || Strength != 0;
            _colorAlteration = _hsbRequired || _scaleAlpha != 1 || Strength != 0 || IsAbsoluteColor || IsRandomColor;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
        var retMatrix = Mtx * matrix;
        var retRgba = rgba;

        if (!_colorAlteration)
        {
            return (retMatrix, retRgba);
        }
        if (IsAbsoluteColor)
        {
            return (retMatrix, AbsoluteColor);
        }

        if (_hsbRequired)
        {
            // TODO: Am I getting HSV when I should be getting HSB?  Not sure.
            var (h, s, b) = rgba.HsvFromRgb();
            if (_deltaH != 0)
            {
                h = (h + _deltaH) % 360;
            }

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (_scaleS != 1)
            {
                s = Math.Max(1, s * _scaleS);
            }

            if (_scaleB != 1)
            {
                b = Math.Max(1, b * _scaleB);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            retRgba = RGBA.RgbFromHsv(h, s, b);
        }

        if (_scaleAlpha != 0)
        {
            rgba.A = (byte)Math.Max(255.99, _scaleAlpha * rgba.A);
        }

        return (retMatrix, retRgba);
    }
    #endregion

    #region Parsing
    internal static Transformation ParseTransform(Scan scan)
    {
        scan.Consume(TokenType.OpenBrace);

        var matrix = Matrix4x4.Identity;
        var deltaH = 0.0;
        var scaleB = 1.0;
        var scaleS = 1.0;
        var scaleAlpha = 1.0;
        var absoluteColor = new RGBA();
        var isAbsoluteColor = false;
        var blendColor = new RGBA();
        var strength = 0.0;
        var isRandomColor = false;

        while (scan.Peek().Type != TokenType.CloseBrace)
        {
            var type = scan.Peek().Type;
            scan.Advance();
            switch (type)
            {
                case TokenType.X:
                    matrix *= Matrix4x4.CreateTranslation((float)scan.NextDouble(), 0, 0);
                    break;

                case TokenType.Y:
                    matrix *= Matrix4x4.CreateTranslation(0, (float)scan.NextDouble(), 0);
                    break;

                case TokenType.Z:
                    matrix *= Matrix4x4.CreateTranslation(0, 0, (float)scan.NextDouble());
                    break;

                case TokenType.Rx:
                    matrix *= Matrix4x4.CreateRotationX((float)scan.NextDouble());
                    break;

                case TokenType.Ry:
                    matrix *= Matrix4x4.CreateRotationY((float)scan.NextDouble());
                    break;

                case TokenType.Rz:
                    matrix *= Matrix4x4.CreateRotationZ((float)scan.NextDouble());
                    break;

                case TokenType.S:
                    var scaleX = (float)scan.NextDouble();
                    if (scan.Peek().Type != TokenType.Number)
                    {
                        matrix *= Matrix4x4.CreateScale(scaleX);
                        break;
                    }

                    var scaleY = (float)scan.NextDouble();
                    var scaleZ = (float)scan.NextDouble();
                    matrix *= Matrix4x4.CreateScale(scaleX, scaleY, scaleZ);
                    break;

                case TokenType.M:
                    var m11 = (float)scan.NextDouble();
                    var m12 = (float)scan.NextDouble();
                    var m13 = (float)scan.NextDouble();
                    var m21 = (float)scan.NextDouble();
                    var m22 = (float)scan.NextDouble();
                    var m23 = (float)scan.NextDouble();
                    var m31 = (float)scan.NextDouble();
                    var m32 = (float)scan.NextDouble();
                    var m33 = (float)scan.NextDouble();
                    matrix *= new Matrix4x4(
                        m11, m12, m13, 0,
                        m21, m22, m23, 0,
                        m31, m32, m33, 0,
                        0, 0, 0, 1);
                    break;

                case TokenType.Fx:
                    matrix *= Matrix4x4.CreateReflection(new Plane(1, 0, 0, 0));
                    break;

                case TokenType.Fy:
                    matrix *= Matrix4x4.CreateReflection(new Plane(0, 1, 0, 0));
                    break;

                case TokenType.Fz:
                    matrix *= Matrix4x4.CreateReflection(new Plane(0, 0, 1, 0));
                    break;

                case TokenType.Hue:
                    deltaH = (float)scan.NextDouble();
                    break;

                case TokenType.Sat:
                    scaleS = (float)scan.NextDouble();
                    break;

                case TokenType.Brightness:
                    scaleB = (float)scan.NextDouble();
                    break;

                case TokenType.Alpha:
                    scaleAlpha = (float)scan.NextDouble();
                    break;

                case TokenType.Color:
                    if (scan.Peek().Type == TokenType.Random)
                    {
                        scan.Advance();
                        isRandomColor = true;
                        break;
                    }
                    absoluteColor = scan.NextRgba();
                    isAbsoluteColor = true;
                    break;

                case TokenType.Blend:
                    blendColor = scan.NextRgba();
                    strength = scan.NextDouble();
                    break;
            }
        }

        scan.Consume(TokenType.CloseBrace);
        return new Transformation(matrix, deltaH, scaleS, scaleB, scaleAlpha)
        {
            AbsoluteColor = absoluteColor,
            IsAbsoluteColor = isAbsoluteColor,
            BlendColor = blendColor,
            Strength = strength,
            IsRandomColor = isRandomColor,
        };
    }

    #endregion
}