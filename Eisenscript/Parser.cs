using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eisenscript
{
    internal class Parser
    {
        private readonly Scan _scan;

        internal Parser(TextReader input)
        {
            _scan = new Scan(input);
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
                if (!ParseSet(rules))
                {
                    ParseRule(rules);
                }
            }
            return rules;
        }

        private bool ParseSet(Rules rules)
        {
            if (_scan.Peek().Type != TokenType.Set)
            {
                return false;
            }

            _scan.Advance();

            switch (_scan.Next().Type)
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

                default:
                    throw new ParserException("Unexpected token after \"set\"");
            }

            return true;
        }

        private void ParseRule(Rules rules)
        {

        }
    }
}
