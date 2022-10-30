using System.Numerics;

// ReSharper disable once IdentifierTypo
namespace Eisenscript;

public class Transformation
{
    public Matrix4x4 Mtx { get; }

    // For color alterations
#pragma warning disable CS0414
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public double DeltaH { get; }
    public double ScaleS { get; }
    public double ScaleB { get; }
    public double ScaleAlpha { get; }
    public bool IsAbsoluteColor { get; init;  }
    public bool IsRandomColor { get; init; }
    public RGBA AbsoluteColor { get; init; }
    // For color blends
    public RGBA BlendColor { get; init; }
    public double Strength { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
#pragma warning restore CS0414

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