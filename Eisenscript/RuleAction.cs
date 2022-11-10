// ReSharper disable once IdentifierTypo
namespace Eisenscript
{
    public class RuleAction
    {
        #region Private variables
        public string? PostRule { get; }
        public SetAction? Set { get; }

        #endregion

        #region Properties
        public List<TransformationLoop>? Loops { get; }

        public TokenType Type { get; } = TokenType.End;
        #endregion

        #region Constructors
        internal RuleAction(string postRule, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            PostRule = postRule;
            Set = setAction;
            Loops = loops;
        }

        internal RuleAction(TokenType tt, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            Type = tt;
            Set = setAction;
            Loops = loops;
        }

        internal RuleAction(SetAction setAction)
        {
            Set = setAction;
        }
        #endregion

        #region Parsing
        internal static RuleAction? ParseAction(Scan scan)
        {
            List<TransformationLoop>? loops = null;
            SetAction? setAction = null;

            while (true)
            {
                if (scan.Done)
                {
                    return null;
                }
                var token = scan.Peek();

                if (Token.IsObject(token))
                {
                    scan.Consume(token.Type);
                    return new RuleAction(token.Type, loops, setAction);
                }

                switch (token.Type)
                {
                    case TokenType.Variable:
                        var ruleName = scan.Consume(TokenType.Variable).Name!;
                        return new RuleAction(ruleName, loops, setAction);

                    case TokenType.Number:
                        var reps = scan.NextInt();
                        scan.Consume(TokenType.Mult);
                        if (loops == null)
                        {
                            loops = new List<TransformationLoop>();
                        }
                        loops.Add(new TransformationLoop(reps, Transformation.ParseTransform(scan)));
                        break;

                    case TokenType.OpenBrace:
                        if (loops == null)
                        {
                            loops = new List<TransformationLoop>();
                        }
                        loops.Add(new TransformationLoop(1, Transformation.ParseTransform(scan)));
                        break;

                    case TokenType.Set:
                        scan.Advance();
                        var tokenType = scan.Next();
                        var value = 0.0;
                        var fIsInitSeed = false;

                        // There is nothing to set RGBA value in rules I don't believe

                        switch (tokenType.Type)
                        {
                            case TokenType.MaxDepth:
                                value =scan.NextInt();
                                break;

                            case TokenType.MaxObjects:
                                value = scan.NextInt();
                                break;

                            case TokenType.MinSize:
                                value = scan.NextDouble();
                                break;

                            case TokenType.MaxSize:
                                value = scan.NextDouble();
                                break;

                            case TokenType.Seed:
                                var tokenSeed = scan.Peek();
                                if (tokenSeed.Type == TokenType.Initial)
                                {
                                    scan.Advance();
                                    fIsInitSeed = true;
                                    break;
                                }
                                value  = scan.NextInt();
                                break;

                            case TokenType.ColorPool:
                                var pool = ColorPool.FromScan(scan);
                                return new RuleAction(new SetAction(TokenType.ColorPool, pool));

                            default:
                                throw new ParserException("Unexpected token after \"set\"", token.Line);
                        }

                        return new RuleAction(fIsInitSeed
                            ? SetAction.InitSeed()
                            : new SetAction(tokenType.Type, value));

                    default:
                        return null;
                }
            }
        }
        #endregion
    }
}
