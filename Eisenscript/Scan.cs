using System.Diagnostics;
using System.Reflection;
using System.Text;
using static System.Globalization.NumberStyles;

// ReSharper disable once IdentifierTypo
namespace Eisenscript
{
    internal class Scan
    {
        #region Statics

        private static readonly Dictionary<string, RGBA> InternetColors = new();
        #endregion

        #region Private Variables
        private int _ich;
        private bool _isScanned;
        private readonly List<Token> _tokens = new();
        private readonly Dictionary<string, Object> _defines = new();

        // TODO: Probably implement the following list as a list of runs for efficiency and easier color coding
        private readonly List<TokenType> _mapCharToTokenType = new();

        private readonly string _canonicalText;
        private bool _inMultilineComment;
        private int _iToken;
        private readonly List<ParserException> _exceptions = new();
        private int _iLine;
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

        static Scan()
        {
            using var stmColors = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Eisenscript.Data.Internet Colors.csv");
            Debug.Assert(stmColors != null, nameof(stmColors) + " != null");
            var trColors = new StreamReader(stmColors);
            while (trColors.ReadLine() is { } line)
            {
                if (line[0] == '/' && line[1] == '/')
                {
                    continue;
                }
                var posComma = line.IndexOf(',');
                var name = DeSpace(line[..posComma]);
                var val = line[(posComma + 2)..];
                var r = byte.Parse(val[..2], HexNumber);
                var g = byte.Parse(val[2..4], HexNumber);
                var b = byte.Parse(val[4..], HexNumber);
                InternetColors[name] = new RGBA(r, g, b);
            }
        }

        static string DeSpace(string str)
        {
            StringBuilder sb = new();

            foreach (var ch in str.Where(c => char.IsAscii(c) && !char.IsWhiteSpace(c)))
            {
                sb.Append(char.ToLower(ch));
            }

            return sb.ToString();
        }

        internal Scan(TextReader input)
        {
            _canonicalText = Canonize(input);
            EnsureScan();
        }

        private static string Canonize(TextReader input)
        {
            var sb = new StringBuilder();
            while (input.ReadLine() is { } line)
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
                        // TODO: This could skip over a MLC which continues to the next line.  Fix this.
                        // Not something I'm WILDLY concerned about but it means we'll be in the middle
                        // of a MLC on the next line expecting it to be normal code which could cause
                        // a cascade of false positives for errors.
                        AdvanceScan(TokenType.Error);
                    }

                    AdvanceScan(TokenType.White);
                }

                _iLine++;
            }
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

                if (IsRgba())
                {
                    continue;
                }

                IsKeywordOrVariable();
            }

            // Skip final '\n' of line (or add a space if we're at EOF which is fine)
            AdvanceScan(TokenType.White);
        }
        #endregion

        #region Scanning utilities
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

        private string PeekWord(bool fOnlyAlpha = false)
        {
            var ichReadAhead = _ich;
            if (fOnlyAlpha)
            {
                while (char.IsLetter(_canonicalText[ichReadAhead++]) && !FinishedLine) {}
            }
            else
            {
                while (!char.IsWhiteSpace(_canonicalText[ichReadAhead++]) && !FinishedLine) { }
            }

            // ichReadAhead is now one char beyond the end of our word
            return _canonicalText[_ich..(ichReadAhead - 1)];
        }
        #endregion

        #region Keywords/variables
        private void IsKeywordOrVariable()
        {
            if (Cur == '/')
            {
                // Forget about comments
                return;
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
                    if (ichReadAhead == _canonicalText.Length || !char.IsLetter(Cur) ||
                        !char.IsLetterOrDigit(_canonicalText[ichReadAhead]))
                    {
                        AdvanceScan(ichReadAhead, tokenType);
                        _tokens.Add(new Token(tokenType, _iLine));
                        return;
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
                    return;
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

        #region RGBA
        private bool IsRgba()
        {
            var word = PeekWord();
            var ret = false;

            if (Cur == '#')
            {
                byte r;
                byte g;
                byte b;
                byte a = 0xff;

                if (word == "#define")
                {
                    // special exception for #define
                    return false;
                }

                word = StripTrailingNonHex(word);
                switch (word.Length)
                {
                    case 4:
                        r = byte.Parse(word[1..2], HexNumber);
                        // Turn 'f' into 'ff' for instance
                        r = (byte)(17 * r);
                        g = byte.Parse(word[2..3], HexNumber);
                        g = (byte)(17 * g);
                        b = byte.Parse(word[3..], HexNumber);
                        b = (byte)(17 * b);
                        break;

                    case 7:
                        r = byte.Parse(word[1..3], HexNumber);
                        g = byte.Parse(word[3..5], HexNumber);
                        b = byte.Parse(word[5..], HexNumber);
                        break;

                    case 9:
                        a = byte.Parse(word[1..3], HexNumber);
                        r = byte.Parse(word[3..5], HexNumber);
                        g = byte.Parse(word[5..7], HexNumber);
                        b = byte.Parse(word[7..], HexNumber);
                        break;

                    case 10:
                        r = byte.Parse(word[1..3], HexNumber);
                        g = byte.Parse(word[4..6], HexNumber);
                        b = byte.Parse(word[7..9], HexNumber);
                        break;

                    case 13:
                        r = byte.Parse(word[1..3], HexNumber);
                        g = byte.Parse(word[5..7], HexNumber);
                        b = byte.Parse(word[9..11], HexNumber);
                        break;

                    default:
                        throw new ParserException("Invalid format for #color", _iLine);
                }
                _tokens.Add(new Token(new RGBA(r, g, b, a), _iLine));
                ret = true;
            }
            else
            {
                word = StripTrailingJunk(word);
                if (InternetColors.ContainsKey(word))
                {
                    _tokens.Add(new Token(InternetColors[word], _iLine));
                    ret = true;
                }
            }

            if (ret)
            {
                for (var i = 0; i < word.Length; i++)
                {
                    AdvanceScan(TokenType.Rgba);
                }
            }
            return ret;
        }

        private string StripTrailingNonHex(string str)
        {
            // Skip the leading '#'
            for (var iLast = 1; iLast < str.Length; iLast++)
            {
                var chCur = char.ToLower(str[iLast]);
                if (char.IsDigit(chCur) || chCur is <= 'f' and >= 'a')
                {
                    continue;
                }

                return str[..iLast];
            }

            return str;
        }

        private string StripTrailingJunk(string str)
        {
            if (!char.IsLetter(str[0]))
            {
                return "";
            }

            for (var iLast = 1; iLast < str.Length; iLast++)
            {
                var chCur = char.ToLower(str[iLast]);
                if (char.IsLetterOrDigit(chCur))
                {
                    continue;
                }

                return str[..iLast];
            }

            return str;
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

                if (FinishedLine)
                {
                    return false;
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

        internal void Define(string name, Object definition)
        {
            _defines[name] = definition;
        }

        //internal int? TryNextInt()
        //{
        //    if (Peek().Type != TokenType.Number)
        //    {
        //        return null;
        //    }

        //    return (int)Math.Round(Consume(TokenType.Number).Value);
        //}

        internal int NextInt()
        {
            var token = Peek();
            var line = token.Line;

            if (token.Type == TokenType.Variable)
            {
                var name = token.Name!;
                if (!_defines.ContainsKey(name) || _defines[name] is not double)
                {
                    throw new ParserException("Expected Integer", line);
                }

                Advance();
                var val = (double)_defines[name];
                return (int)Math.Round(val);
            }

            if (token.Type != TokenType.Number)
            {
                throw new ParserException("Expected Integer", line);
            }

            return (int)Math.Round(Consume(TokenType.Number).Value);
        }

        internal RGBA NextRgba()
        {
            var token = Peek();
            var line = token.Line;

            if (token.Type == TokenType.Variable)
            {
                var name = token.Name!;
                if (!_defines.ContainsKey(name) || _defines[name] is not RGBA)
                {
                    throw new ParserException("Expected Rgba", line);
                }

                Advance();
                return (RGBA)_defines[name];
            }

            if (token.Type != TokenType.Rgba)
            {
                throw new ParserException("Expected RGBA", line);
            }

            return Consume(TokenType.Rgba).Rgba;
        }

        //internal double? TryNextDouble()
        //{
        //    if (Peek().Type != TokenType.Number)
        //    {
        //        return null;
        //    }

        //    return Consume(TokenType.Number).Value;
        //}

        internal double NextDouble()
        {
            var token = Peek();
            var line = token.Line;

            if (token.Type == TokenType.Variable)
            {
                var name = token.Name!;
                if (!_defines.ContainsKey(name) || _defines[name] is not double)
                {
                    throw new ParserException("Expected Integer", line);
                }

                Advance();
                return (double)_defines[name];
            }

            if (token.Type != TokenType.Number)
            {
                throw new ParserException("Expected Float", line);
            }

            Advance();
            return token.Value;
        }

        internal Token Consume(TokenType tt)
        {
            var ret = _tokens[_iToken++];
            if (ret.Type != tt)
            {
                throw new ParserException("Unexpected Token", ret.Line);
            }
            return ret;
        }

        internal Token Peek()
        {
            return _tokens[_iToken];
        }
        #endregion
    }
}