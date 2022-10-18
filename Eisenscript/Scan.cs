using System.Text;
using Eisenscript.Data_Structures;

namespace Eisenscript
{
    internal class Scan
    {
        #region Private Variables
        private int _ich;
        private bool _isScanned;
        private readonly List<Token> _tokens = new List<Token>();
        private readonly List<TokenType> _mapCharToTokenType = new List<TokenType>();
        private readonly string _canonicalText;
        private bool _inMultilineComment;
        #endregion

        #region Properties
        private char Cur => _canonicalText[_ich];
        private char Peek => _ich < _canonicalText.Length - 1 ? _canonicalText[_ich + 1] : ' ';
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

        private void Advance(TokenType type)
        {
            _mapCharToTokenType.Add(type);
            _ich++;
        }

        private void Advance(int newPosition, TokenType type)
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
                    Advance(TokenType.White);
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
                        Advance(TokenType.Number);
                    }

                    if (Cur == '.')
                    {
                        Advance(TokenType.Number);
                        var decimalVal = 0.1;

                        while (char.IsDigit(Cur))
                        {
                            valFrac += (Cur - '0') * decimalVal;
                            decimalVal /= 10.0;
                            Advance(TokenType.Number);
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
                                Advance(ichReadAhead, tokenType);
                                _tokens.Add(new Token(tokenType));
                                break;
                            }

                            continue;
                        }
                        if (fStop)
                        {
                            // Not a keyword.  Must be a variable - go till we run out of alphanumeric chars
                            // or reach the end of the text

                            // Variables better start with a letter
                            if (!char.IsLetter(Cur))
                            {
                                throw new InvalidOperationException("Variables have to start with letters");
                            }

                            while (char.IsLetterOrDigit(_canonicalText[ichReadAhead]) &&
                                   ichReadAhead != _canonicalText.Length)
                            {
                                ichReadAhead++;
                            }
                            // We're now one beyond the end of the variable name
                            var count = ichReadAhead - _ich;
                            var name = _canonicalText[_ich..ichReadAhead];
                            Advance(ichReadAhead, TokenType.Variable);
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
            Advance(TokenType.White);
        }
        #endregion

        #region Comments
        private void FindMlcEnd()
        {
            while (true)
            {
                if (Cur == '*' && Peek == '/')
                {
                    Advance(TokenType.Comment);      // over '*'
                    Advance(TokenType.Comment);      // over '/'
                    _inMultilineComment = false;
                    break;
                }
                else if (FinishedLine)
                {
                    break;
                }
                Advance(TokenType.Comment);
            }
        }

        private void CheckComment()
        {
            if (Cur != '/')
            {
                return;
            }

            if (Peek == '/')
            {
                Advance(TokenType.Comment);      // over '/'
                Advance(TokenType.Comment);      // over '/'

                while (!FinishedLine)
                {
                    Advance(TokenType.Comment);
                }
            }
            else if (Peek == '*')
            {
                Advance(TokenType.Comment);      // over '/'
                Advance(TokenType.Comment);      // over '*'

                _inMultilineComment = true;
                FindMlcEnd();
            }
        }
        #endregion
        #endregion
    }
}