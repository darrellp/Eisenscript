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

                if (!ParseSet(rules) && !ParseRule(rules))
                {
                    ParseStartingRule(rules);
                }
            }
            return rules;
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
            ParseRuleBody(rule);
            _scan.Consume(TokenType.CloseBrace);

            return true;
        }

        private void ParseRuleBody(Rule rule)
        {

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
