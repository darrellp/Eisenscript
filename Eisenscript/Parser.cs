// ReSharper disable once IdentifierTypo
namespace Eisenscript
{
    public class Parser
    {
        #region Private Variables
        private readonly Scan _scan;
        // ReSharper disable once NotAccessedField.Local
        private List<ParserException> _exceptions;
        #endregion

        #region Constructor
        internal Parser(TextReader input)
        {
            _scan = new Scan(input);
            _exceptions = _scan.Exceptions;
        }
        #endregion

        #region Parsing
        public Rules Rules()
        {
            return ParseProgram();
        }

        private Rules ParseProgram()
        {
            var rules = new Rules();

            while (!_scan.Done)
            {

                if (!ParseSet(rules) && !Rule.ParseRule(rules, _scan) && !ParseDefine())
                {
                    Rule.ParseStartingRule(rules, _scan);
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

                case TokenType.ColorPool:
                    rules.Pool = ColorPool.FromScan(_scan);
                    break;

                default:
                    throw new ParserException("Unexpected token after \"set\"", token.Line);
            }

            return true;
        }
        #endregion
    }
}
