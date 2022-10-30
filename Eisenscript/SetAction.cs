// ReSharper disable once IdentifierTypo
namespace Eisenscript;

public class SetAction
{
    public TokenType SetTarget { get; }
    public double NumericValue { get; }
    public ColorPool? Pool { get; }

    public bool IsInitSeed;

    public SetAction(TokenType type, double value)
    {
        SetTarget = type;
        NumericValue = value;
    }
    public SetAction(TokenType type, ColorPool pool)
    {
        SetTarget = type;
        Pool = pool;
    }

    private SetAction() { }

    internal static SetAction InitSeed()
    {
        return new SetAction() { IsInitSeed = true };
    }
}