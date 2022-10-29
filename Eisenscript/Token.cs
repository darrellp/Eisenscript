using Eisenscript.Data_Structures;

namespace Eisenscript
{
    public enum TokenType
    {
        // ReSharper disable UnusedMember.Global
        White,
        Comment,
        Error,
        Mult,
        Greater,
        Define,
        OpenBrace,
        CloseBrace,
        Number,
        Rgba,
        Variable,
        Set,
        Rule,
        MaxDepth,
        MaxObjects,
        MinSize,
        MaxSize,
        Seed,
        Initial,
        Background,
        Weight,
        X,
        Y,
        Z,
        Rx,
        Ry,
        Rz,
        S,
        M,
        Fx,
        Fy,
        Fz,
        Hue,
        Sat,
        Brightness,
        Alpha,
        Color,
        Blend,
        Random,
        ColorPool,
        Box,
        Grid,
        Sphere,
        Line,
        Point,
        Triangle,
        Mesh,
        Cylinder,
        Tube,
        End
        // ReSharper restore UnusedMember.Global
    }

    internal readonly struct Token
    {
        internal static bool IsObject(Token token)
        {
            return token.Type is 
                TokenType.Box or 
                TokenType.Grid or 
                TokenType.Sphere or 
                TokenType.Line or 
                TokenType.Point or 
                TokenType.Triangle or 
                TokenType.Mesh or 
                TokenType.Cylinder or 
                TokenType.Tube;
        }

        // ReSharper disable RedundantDefaultMemberInitializer
        private readonly double _value = 0;
        private readonly string? _name = null;
        private readonly RGBA _rgba = new();
        private readonly int _line;
        // ReSharper restore RedundantDefaultMemberInitializer

        internal static readonly Trie<TokenType> Trie = new();

        internal int Line => _line;
        internal double Value
        {
            get
            {
                if (Type != TokenType.Number)
                {
                    throw new ParserException("Internal: Trying to get value from non-numeric token", _line);
                }
                return _value;
            }
        }

        internal RGBA Rgba
        {
            get
            {
                if (Type != TokenType.Rgba)
                {
                    throw new ParserException("Internal: Trying to get value from non-numeric token", _line);
                }
                return _rgba;
            }
        }

        internal string? Name
        {
            get
            {
                if (Type != TokenType.Variable)
                {
                    throw new ParserException("Internal: Trying to get name from non-variable token", _line);
                }
                return _name;
            }
        }

        static Token()
        {
            // Tokens whose name isn't the same as the string
            Trie.Insert("*", TokenType.Mult);
            Trie.Insert(">", TokenType.Greater);
            Trie.Insert("#define", TokenType.Define);
            Trie.Insert("{", TokenType.OpenBrace);
            Trie.Insert("}", TokenType.CloseBrace);

            // Abbreviations
            Trie.Insert("md", TokenType.MaxDepth);
            Trie.Insert("w", TokenType.Weight);
            Trie.Insert("b", TokenType.Brightness);
            Trie.Insert("a", TokenType.Alpha);

            for (var i = (int)TokenType.Variable + 1; i < (int)TokenType.End; i++)
            {
                TokenType tt = (TokenType)i;
#pragma warning disable CS8602
                Trie.Insert(Enum.GetName(tt).ToLower(), tt);
#pragma warning restore CS8602
            }
        }

        internal TokenType Type { get; }

        internal Token(TokenType type, int line)
        {
            Type = type;
            _line = line;
        }

        internal Token(double value, int line)
        {
            Type = TokenType.Number;
            _value = value;
            _line = line;
        }

        internal Token(RGBA rgba, int line)
        {
            Type = TokenType.Rgba;
            _rgba = rgba;
            _line = line;
        }
        internal Token(string name, int line)
        {
            Type = TokenType.Variable;
            _name = name;
            _line = line;
        }
    }
}
