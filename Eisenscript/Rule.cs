namespace Eisenscript
{
    public class Rule
    {
        private readonly List<RuleAction> _actions = new();

        public List<RuleAction> Actions => _actions;

        public string? MaxDepthNext { get; init; }
        public double Weight { get; set; }

        public string? Name { get; }

        public int MaxDepth { get; init; }

        internal Rule(string? name, double weight = 1.0)
        {
            Name = name;
            Weight = weight;
        }

        internal void AddAction(RuleAction action)
        {
            _actions.Add(action);
        }

        #region Parsing
        internal static bool ParseRule(Rules rules, Scan scan)
        {
            if (scan.Peek().Type != TokenType.Rule)
            {
                return false;
            }

            scan.Consume(TokenType.Rule);

            var rule = ParseRuleHeader(scan);

            scan.Consume(TokenType.OpenBrace);
            var line = scan.Peek().Line;
            if (!ParseRuleBody(rule, scan))
            {
                throw new ParserException("Expected rule body but didn't find one", line);
            }
            scan.Consume(TokenType.CloseBrace);

            rules.AddRule(rule);
            return true;
        }

        internal static bool ParseRuleBody(Rule rule, Scan scan)
        {
            var fActionAdded = false;

            while (RuleAction.ParseAction(scan) is { } action)
            {
                rule.AddAction(action);
                fActionAdded = true;
            }

            if (!fActionAdded)
            {
                throw new ParserException("Invalid/empty rule body", scan.Peek().Line);
            }
            return true;
        }

        internal static Rule ParseRuleHeader(Scan scan)
        {
            var ruleVariable = scan.Consume(TokenType.Variable);
            var weight = 100.0;
            var maxDepth = -1;
            string? maxDepthNext = null;
            Rule rule;

            while (true)
            {
                var next = scan.Peek();
                if (next.Type is TokenType.Weight or TokenType.MaxDepth)
                {
                    switch (next.Type)
                    {
                        case TokenType.Weight:
                            scan.Advance();
                            weight = scan.NextDouble();
                            continue;

                        case TokenType.MaxDepth:
                            scan.Advance();
                            maxDepth = scan.NextInt();
                            if (scan.Peek().Type is TokenType.Greater)
                            {
                                scan.Advance();
                                maxDepthNext = scan.Consume(TokenType.Variable).Name;
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

        internal static void ParseStartingRule(Rules rules, Scan scan)
        {
            var rule = new Rule(null);
            if (!Rule.ParseRuleBody(rule, scan))
            {
                var line = scan.Next().Line;
                throw new ParserException("Expected rule body", line);
            }

            rules.AddInitRule(rule);
        }
        #endregion
    }
}
