// ReSharper disable once IdentifierTypo
namespace Eisenscript;

public class SetAction
{
    public TokenType SetTarget { get; }
    public double NumericValue { get; }
    public RGBA RgbaValue { get; }

    public bool IsInitSeed;

    public SetAction(TokenType type, double value)
    {
        SetTarget = type;
        NumericValue = value;
    }
    public SetAction(TokenType type, RGBA value)
    {
        SetTarget = type;
        RgbaValue = value;
    }

    private SetAction() {}

    internal static SetAction InitSeed()
    {
        return new SetAction() {IsInitSeed = true};
    }
}