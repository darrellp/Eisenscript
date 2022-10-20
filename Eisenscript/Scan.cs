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
        #endregion

        #region Properties
        internal bool Done => _iToken == _tokens.Count;
        private char Cur => _canonicalText[_ich];
        private char ScanPeek => _ich < _canonicalText.Length - 1 ? _canonicalText[_ich + 1] : ' ';
        private bool FinishedLine => _ich >= _canonicalText.Length || _canonicalText[_ich] == '\n';
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
                sb.Append("\n");
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
                ScanLine();
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
                    FindMlcEnd();
                }
                else
                {
                    CheckComment();
                }

                if (FinishedLine)
                {
                    break;
                }

                if (char.IsWhiteSpace(Cur))
                {
                    AdvanceScan(TokenType.White);
                }

                // Okay - not in comments, not at the end of the line,
                // not white space - time to do real work

                else if (char.IsDigit(Cur) || Cur == '.')
                {
                    var valInt = 0.0;
                    var valFrac = 0.0;

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
                    _tokens.Add(new Token(valInt + valFrac));
                }
                else
                {
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
                                _tokens.Add(new Token(tokenType));
                                break;
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
                                throw new ParserException("Variables have to start with letters");
                            }

                            while ((char.IsLetterOrDigit(_canonicalText[ichReadAhead]) || _canonicalText[ichReadAhead] == '_') &&
                                   ichReadAhead != _canonicalText.Length)
                            {
                                ichReadAhead++;
                            }
                            // We're now one beyond the end of the variable name
                            var name = _canonicalText[_ich..ichReadAhead];
                            AdvanceScan(ichReadAhead, TokenType.Variable);
                            _tokens.Add(new Token(name));
                            break;
                        }
                    }

                    if (FinishedLine)
                    {
                        break;
                    }
                }
            }

            // Skip final '\n' of line (or add a space if we're at EOF which is fine)
            AdvanceScan(TokenType.White);
        }
        #endregion

        #region Comments
        private void FindMlcEnd()
        {
            while (true)
            {
                if (Cur == '*' && ScanPeek == '/')
                {
                    AdvanceScan(TokenType.Comment);      // over '*'
                    AdvanceScan(TokenType.Comment);      // over '/'
                    _inMultilineComment = false;
                    break;
                }
                else if (FinishedLine)
                {
                    break;
                }
                AdvanceScan(TokenType.Comment);
            }
        }

        private void CheckComment()
        {
            if (Cur != '/')
            {
                return;
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
                FindMlcEnd();
            }
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
                    throw new ParserException("Expected Integer");
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
                throw new ParserException("Expected Float");
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
                throw new ParserException("Unexpected Token");
            }
        }

        internal Token Peek()
        {
            return _tokens[_iToken];
        }
        #endregion
    }
}