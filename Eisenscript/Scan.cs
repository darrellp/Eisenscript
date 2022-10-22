using System.Text;

namespace Eisenscript
{
    internal class Scan
    {
        #region Private Variables
        private int _ich;
        private bool _isScanned;
        private readonly List<Token> _tokens = new();
        private readonly List<TokenType> _mapCharToTokenType = new();
        private readonly string _canonicalText;
        private bool _inMultilineComment;
        private int _iToken = 0;
        private List<ParserException> _exceptions = new();
        private int _iLine = 0;
        #endregion

        #region Properties
        internal bool Done => _iToken == _tokens.Count;
        private char Cur => _canonicalText[_ich];
        private char ScanPeek => _ich < _canonicalText.Length - 1 ? _canonicalText[_ich + 1] : ' ';
        private bool FinishedLine => _ich >= _canonicalText.Length || _canonicalText[_ich] == '\n';
        internal List<ParserException> Exceptions => _exceptions;
        internal List<Token> Tokens
        {
            get
            {
                EnsureScan();
                return _tokens;
            }
        }

        internal List<TokenType> MapCharTo
        {
            get
            {
                EnsureScan();
                return _mapCharToTokenType;
            }
        }
        #endregion

        #region Constructor
        internal Scan(TextReader input)
        {
            _canonicalText = Canonize(input);
            EnsureScan();
        }

        private static string Canonize(TextReader input)
        {
            var sb = new StringBuilder();
            string? line;
            while ((line = input.ReadLine()) != null)
            {
                sb.Append(line);
                sb.Append('\n');
            }

            return sb.ToString();
        }
        #endregion

        #region Scanning
        #region High Level Loop
        private void EnsureScan()
        {
            if (_isScanned)
            {
                return;
            }

            _isScanned = true;

            while (_ich < _canonicalText.Length)
            {
                try
                {
                    ScanLine();
                }
                catch (ParserException e)
                {
                    _exceptions.Add(e);
                    while (!FinishedLine)
                    {
                        AdvanceScan(TokenType.Error);
                    }

                    AdvanceScan(TokenType.White);
                }

                _iLine++;
            }
        }

        private void AdvanceScan(TokenType type)
        {
            _mapCharToTokenType.Add(type);
            _ich++;
        }

        private void AdvanceScan(int newPosition, TokenType type)
        {
            var count = newPosition - _ich;
            _mapCharToTokenType.AddRange(Enumerable.Repeat(type, count));
            _ich = newPosition;
        }

        private void ScanLine()
        {
            while (true)
            {
                if (_inMultilineComment)
                {
                    if (FindMlcEnd())
                    {
                        // Could be in the middle of the line so keep scanning
                        continue;
                    }
                    // EOL without the MLC ending so go to next line
                    break;
                }

                CheckComment();

                while (!FinishedLine && char.IsWhiteSpace(Cur))
                {
                    AdvanceScan(TokenType.White);
                }

                if (FinishedLine)
                {
                    break;
                }

                // Not comments or white space - start doing real stuff
                if (IsNumber())
                {
                    continue;
                }

                IsKeywordOrVariable();
            }

            // Skip final '\n' of line (or add a space if we're at EOF which is fine)
            AdvanceScan(TokenType.White);
        }
        #endregion

        #region Keywords/variables
        private bool IsKeywordOrVariable()
        {
            if (Cur == '/')
            {
                // Forget about comments
                return false;
            }
            var ichReadAhead = _ich;
            Token.Trie.ResetSearch();

            while (true)
            {
                var (tokenType, fStop, fFound) = Token.Trie.Search(_canonicalText[ichReadAhead++]);
                if (fFound)
                {
                    // We found a keyword.  If it's not the prefix of something longer,
                    // report it.  Note that due to post-incrementing, ichReadAhead is
                    // pointing one char after the last letter in the keyword.
                    if (ichReadAhead == _canonicalText.Length ||
                        !char.IsLetterOrDigit(_canonicalText[ichReadAhead]))
                    {
                        AdvanceScan(ichReadAhead, tokenType);
                        _tokens.Add(new Token(tokenType, _iLine));
                        return true;
                    }

                    continue;
                }

                if (fStop)
                {
                    // Not a keyword.  Must be a variable - go till we run out of alphanumeric chars
                    // or reach the end of the text

                    // Variables better start with a letter or underscore
                    if (!char.IsLetter(Cur) && Cur != '_')
                    {
                        throw new ParserException("Variables must start with letters or underscores", _iLine);
                    }

                    while ((char.IsLetterOrDigit(_canonicalText[ichReadAhead]) || _canonicalText[ichReadAhead] == '_') &&
                           ichReadAhead != _canonicalText.Length)
                    {
                        ichReadAhead++;
                    }

                    // We're now one beyond the end of the variable name
                    var name = _canonicalText[_ich..ichReadAhead];
                    AdvanceScan(ichReadAhead, TokenType.Variable);
                    _tokens.Add(new Token(name, _iLine));
                    return true;
                }
            }
        }
        #endregion

        #region Numbers
        private bool IsNumber()
        {
            var sign = 1.0;

            if (!char.IsDigit(Cur) && Cur != '.' && Cur != '-')
            {
                return false;
            }
            var valInt = 0.0;
            var valFrac = 0.0;

            if (Cur == '-')
            {
                AdvanceScan(TokenType.Number);
                sign = -1;
            }
            while (char.IsDigit(Cur))
            {
                valInt = valInt * 10 + Cur - '0';
                AdvanceScan(TokenType.Number);
            }

            if (Cur == '.')
            {
                AdvanceScan(TokenType.Number);
                var decimalVal = 0.1;

                while (char.IsDigit(Cur))
                {
                    valFrac += (Cur - '0') * decimalVal;
                    decimalVal /= 10.0;
                    AdvanceScan(TokenType.Number);
                }
            }

            _tokens.Add(new Token(sign * (valInt + valFrac), _iLine));
            return true;
        }
        #endregion

        #region Comments
        private bool FindMlcEnd()
        {
            while (true)
            {
                if (Cur == '*' && ScanPeek == '/')
                {
                    AdvanceScan(TokenType.Comment);      // over '*'
                    AdvanceScan(TokenType.Comment);      // over '/'
                    _inMultilineComment = false;
                    return true;
                }
                else if (FinishedLine)
                {
                    return false;
                }
                AdvanceScan(TokenType.Comment);
            }
        }

        private bool CheckComment()
        {
            if (Cur != '/')
            {
                return false;
            }

            if (ScanPeek == '/')
            {
                AdvanceScan(TokenType.Comment);      // over '/'
                AdvanceScan(TokenType.Comment);      // over '/'

                while (!FinishedLine)
                {
                    AdvanceScan(TokenType.Comment);
                }
            }
            else if (ScanPeek == '*')
            {
                AdvanceScan(TokenType.Comment);      // over '/'
                AdvanceScan(TokenType.Comment);      // over '*'

                _inMultilineComment = true;
                if (FindMlcEnd())
                {
                    // We haven't advanced to end of line so return false
                    return false;
                }
            }

            return true;
        }
        #endregion
        #endregion

        #region Retrieval
        internal void Advance()
        {
            _iToken++;
        }

        internal Token Next()
        {
            return _tokens[_iToken++];
        }

        internal int? TryNextInt()
        {
            if (Peek().Type != TokenType.Number)
            {
                return null;
            }

            return (int)Math.Round(Consume(TokenType.Number).Value);
        }
        internal int NextInt()
        {
            if (Peek().Type != TokenType.Number)
            {
                var line = Peek().Line;
                Advance();
                throw new ParserException("Expected Integer", line);
            }

            return (int)Math.Round(Consume(TokenType.Number).Value);
        }

        internal double? TryNextDouble()
        {
            if (Peek().Type != TokenType.Number)
            {
                return null;
            }

            return Consume(TokenType.Number).Value;
        }

        internal double NextDouble()
        {
            if (Peek().Type != TokenType.Number)
            {
                var line = Peek().Line;
                Advance();
                throw new ParserException("Expected Float", line);
            }

            return Consume(TokenType.Number).Value;
        }

        internal Token Consume(TokenType tt)
        {
            var ret = _tokens[_iToken++];
            if (ret.Type == tt)
            {
                return ret;
            }
            else
            {
                throw new ParserException("Unexpected Token", ret.Line);
            }
        }

        internal Token Peek()
        {
            return _tokens[_iToken];
        }
        #endregion
    }
}