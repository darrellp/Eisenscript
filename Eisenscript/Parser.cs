using System.Numerics;

// ReSharper disable once IdentifierTypo
namespace Eisenscript
{
    public class Parser
    {
        private readonly Scan _scan;
        // ReSharper disable once NotAccessedField.Local
        private List<ParserException> _exceptions;

        internal Parser(TextReader input)
        {
            _scan = new Scan(input);
            _exceptions = _scan.Exceptions;
        }

        public Rules Rules()
        {
            return ParseProgram();
        }

        private Rules ParseProgram()
        {
            var rules = new Rules();

            while (!_scan.Done)
            {

                if (!ParseSet(rules) && !ParseRule(rules) && !ParseDefine())
                {
                    ParseStartingRule(rules);
                }
            }
            return rules;
        }

        private bool ParseDefine()
        {
            if (_scan.Peek().Type != TokenType.Define)
            {
                return false;
            }

            _scan.Consume(TokenType.Define);

            var name = _scan.Consume(TokenType.Variable).Name!;
            var token = _scan.Peek();

            if (token.Type == TokenType.Number)
            {
                _scan.Define(name, token.Value);
                _scan.Consume(TokenType.Number);
            }
            else if (token.Type == TokenType.Rgba)
            {
                _scan.Define(name, token.Rgba);
                _scan.Consume(TokenType.Rgba);
            }
            else
            {
                throw new ParserException("Expecting int, float or RGBA", token.Line);
            }

            return true;
        }

        private void ParseStartingRule(Rules rules)
        {
            var rule = new Rule(null);
            if (!ParseRuleBody(rule))
            {
                var line = _scan.Next().Line;
                throw new ParserException("Expected rule body", line);
            }

            rules.AddInitRule(rule);
        }

        private bool ParseSet(Rules rules)
        {
            if (_scan.Peek().Type != TokenType.Set)
            {
                return false;
            }

            _scan.Advance();
            var token = _scan.Next();
            switch (token.Type)
            {
                case TokenType.MaxDepth:
                    rules.MaxDepth = _scan.NextInt();
                    break;

                case TokenType.MaxObjects:
                    rules.MaxObjects = _scan.NextInt();
                    break;

                case TokenType.MinSize:
                    rules.MinSize = _scan.NextDouble();
                    break;

                case TokenType.MaxSize:
                    rules.MaxSize = _scan.NextDouble();
                    break;

                case TokenType.Seed:
                    rules.SeedInit  = _scan.NextInt();
                    break;

                case TokenType.Background:
                    rules.Background = _scan.NextRgba();
                    break;

                default:
                    throw new ParserException("Unexpected token after \"set\"", token.Line);
            }

            return true;
        }

        private bool ParseRule(Rules rules)
        {
            if (_scan.Peek().Type != TokenType.Rule)
            {
                return false;
            }

            _scan.Consume(TokenType.Rule);

            var rule = ParseRuleHeader();

            _scan.Consume(TokenType.OpenBrace);
            var line = _scan.Peek().Line;
            if (!ParseRuleBody(rule))
            {
                throw new ParserException("Expected rule body but didn't find one", line);
            }
            _scan.Consume(TokenType.CloseBrace);

            rules.AddRule(rule);
            return true;
        }


        private bool ParseRuleBody(Rule rule)
        {
            RuleAction? action;
            while ((action = ParseAction()) != null)
            {
                rule.AddAction(action);
            }

            return true;
        }

        private RuleAction? ParseAction()
        {
            List<TransformationLoop> loops = new();
            SetAction? setAction = null;

            while (true)
            {
                if (_scan.Done)
                {
                    return null;
                }
                var token = _scan.Peek();

                if (Token.IsObject(token))
                {
                    _scan.Consume(token.Type);
                    return new RuleAction(token.Type, loops, setAction);
                }

                switch (token.Type)
                {
                    case TokenType.Variable:
                        var ruleName = _scan.Consume(TokenType.Variable).Name!;
                        return new RuleAction(ruleName, loops, setAction);

                    case TokenType.Number:
                        var reps = _scan.NextInt();
                        _scan.Consume(TokenType.Mult);
                        loops.Add(new TransformationLoop(reps, ParseTransform()));
                        break;

                    case TokenType.OpenBrace:
                        loops.Add(new TransformationLoop(1, ParseTransform()));
                        break;

                    case TokenType.Set:
                        _scan.Advance();
                        var tokenType = _scan.Next();
                        double value = 0.0;
                        bool fIsInitSeed = false;

                        // There is nothing to set RGBA value in rules I don't believe

                        switch (tokenType.Type)
                        {
                            case TokenType.MaxDepth:
                                value =_scan.NextInt();
                                break;

                            case TokenType.MaxObjects:
                                value = _scan.NextInt();
                                break;

                            case TokenType.MinSize:
                                value = _scan.NextDouble();
                                break;

                            case TokenType.MaxSize:
                                value = _scan.NextDouble();
                                break;

                            case TokenType.Seed:
                                var tokenSeed = _scan.Peek();
                                if (tokenSeed.Type == TokenType.Initial)
                                {
                                    _scan.Advance();
                                    fIsInitSeed = true;
                                    break;
                                }
                                value  = _scan.NextInt();
                                break;

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

        private Transformation ParseTransform()
        {
            _scan.Consume(TokenType.OpenBrace);

            var matrix = Matrix4x4.Identity;
            var deltaH = 0.0;
            var scaleB = 1.0;
            var scaleS = 1.0;
            var scaleAlpha = 1.0;
            var absoluteColor = new RGBA();
            var isAbsoluteColor = false;
            var blendColor = new RGBA();
            var strength = 0.0;

            while (_scan.Peek().Type != TokenType.CloseBrace)
            {
                var type = _scan.Peek().Type;
                _scan.Advance();
                switch (type)
                {
                    case TokenType.X:
                        matrix *= Matrix4x4.CreateTranslation((float)_scan.NextDouble(), 0, 0);
                        break;

                    case TokenType.Y:
                        matrix *= Matrix4x4.CreateTranslation(0, (float)_scan.NextDouble(), 0);
                        break;

                    case TokenType.Z:
                        matrix *= Matrix4x4.CreateTranslation(0, 0, (float)_scan.NextDouble());
                        break;

                    case TokenType.Rx:
                        matrix *= Matrix4x4.CreateRotationX((float)_scan.NextDouble());
                        break;

                    case TokenType.Ry:
                        matrix *= Matrix4x4.CreateRotationY((float)_scan.NextDouble());
                        break;

                    case TokenType.Rz:
                        matrix *= Matrix4x4.CreateRotationZ((float)_scan.NextDouble());
                        break;

                    case TokenType.S:
                        var scaleX = (float)_scan.NextDouble();
                        if (_scan.Peek().Type != TokenType.Number)
                        {
                            matrix *= Matrix4x4.CreateScale(scaleX);
                            break;
                        }

                        var scaleY = (float)_scan.NextDouble();
                        var scaleZ = (float)_scan.NextDouble();
                        matrix *= Matrix4x4.CreateScale(scaleX, scaleY, scaleZ);
                        break;

                    case TokenType.M:
                        var m11 = (float)_scan.NextDouble();
                        var m12 = (float)_scan.NextDouble();
                        var m13 = (float)_scan.NextDouble();
                        var m21 = (float)_scan.NextDouble();
                        var m22 = (float)_scan.NextDouble();
                        var m23 = (float)_scan.NextDouble();
                        var m31 = (float)_scan.NextDouble();
                        var m32 = (float)_scan.NextDouble();
                        var m33 = (float)_scan.NextDouble();
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
                        deltaH = (float)_scan.NextDouble();
                        break;

                    case TokenType.Sat:
                        scaleS = (float)_scan.NextDouble();
                        break;

                    case TokenType.Brightness:
                        scaleB = (float)_scan.NextDouble();
                        break;

                    case TokenType.Alpha:
                        scaleAlpha = (float)_scan.NextDouble();
                        break;

                    case TokenType.Color:
                        absoluteColor = _scan.NextRgba();
                        isAbsoluteColor = true;
                        break;

                    case TokenType.Blend:
                        blendColor = _scan.NextRgba();
                        strength = _scan.NextDouble();
                        break;
                }
            }

            _scan.Consume(TokenType.CloseBrace);
            return new Transformation(matrix, deltaH, scaleS, scaleB, scaleAlpha)
            {
                AbsoluteColor = absoluteColor,
                IsAbsoluteColor = isAbsoluteColor,
                BlendColor = blendColor,
                Strength = strength,
            };
        }

        private Rule ParseRuleHeader()
        {
            var ruleVariable = _scan.Consume(TokenType.Variable);
            var weight = 100.0;
            var maxDepth = -1;
            string? maxDepthNext = null;
            Rule rule;

            while (true)
            {
                var next = _scan.Peek();
                if (next.Type is TokenType.Weight or TokenType.MaxDepth)
                {
                    switch (next.Type)
                    {
                        case TokenType.Weight:
                            _scan.Advance();
                            weight = _scan.NextDouble();
                            continue;

                        case TokenType.MaxDepth:
                            _scan.Advance();
                            maxDepth = _scan.NextInt();
                            if (_scan.Peek().Type is TokenType.Greater)
                            {
                                _scan.Advance();
                                maxDepthNext = _scan.Consume(TokenType.Variable).Name;
                            }
                            continue;
                    }
                }

                rule = new Rule(ruleVariable.Name, weight)
                {
                    MaxDepth = maxDepth,
                    MaxDepthNext = maxDepthNext
                };

                break;
            }

            return rule;
        }
    }
}
