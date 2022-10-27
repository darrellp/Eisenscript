using System.Collections.Concurrent;
using System.Numerics;

namespace Eisenscript
{
    internal class Parser
    {
        private readonly Scan _scan;
        private List<ParserException> _exceptions;

        internal Parser(TextReader input)
        {
            _scan = new Scan(input);
            _exceptions = _scan.Exceptions;
        }

        internal Rules Rules()
        {
            return ParseProgram();
        }

        private Rules ParseProgram()
        {
            var rules = new Rules();

            while (!_scan.Done)
            {

                if (!ParseSet(rules) && !ParseRule(rules) && !ParseDefine(rules))
                {
                    ParseStartingRule(rules);
                }
            }
            return rules;
        }

        private bool ParseDefine(Rules rules)
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
            throw new NotImplementedException();
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
            while ((action = ParseAction(rule)) != null)
            {
                rule.AddAction(action);
            }

            return true;
        }

        private RuleAction? ParseAction(Rule rule)
        {
            List<TransformationLoop>? loops = new();
            SetAction? setAction = null;

            while (true)
            {
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

                    default:
                        return null;
                }
            }
        }

        private Transformation ParseTransform()
        {
            _scan.Consume(TokenType.OpenBrace);
            Matrix4x4 matrix = Matrix4x4.Identity;

            while (_scan.Peek().Type != TokenType.CloseBrace)
            {
                switch (_scan.Peek().Type)
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
                }
            }

            _scan.Consume(TokenType.CloseBrace);
            return new Transformation(matrix);
        }

        private Rule ParseRuleHeader()
        {
            var ruleVariable = _scan.Consume(TokenType.Variable);
            var weight = 100.0;
            var maxDepth = -1;
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
                            continue;
                    }
                }

                rule = new Rule(ruleVariable.Name, weight)
                {
                    MaxDepth = maxDepth
                };

                break;
            }

            return rule;
        }
    }
}
