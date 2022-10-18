﻿using Eisenscript.Data_Structures;

namespace Eisenscript
{
    internal enum TokenType
    {
        // ReSharper disable UnusedMember.Global
        White,
        Comment,
        Mult,
        Minus,
        Plus,
        Div,
        Define,
        OpenBrace,
        CloseBrace,
        Number,
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

    internal struct Token
    {
        private readonly double _value = 0;
        private string? _name = null;

        internal static readonly Trie<TokenType> Trie = new();

        internal double Value
        {
            get
            {
                if (Type != TokenType.Number)
                {
                    throw new InvalidOperationException("Trying to get value from non-numeric token");
                }
                return _value;
            }

            private init => _value = value;
        }

        internal string? Name
        {
            get
            {
                if (Type != TokenType.Variable)
                {
                    throw new InvalidOperationException("Trying to get name from non-variable token");
                }
                return _name;
            }

            private set => _name = value;
        }

        static Token()
        {
            // Tokens whose name isn't the same as the string
            Trie.Insert("*", TokenType.Mult);
            Trie.Insert("/", TokenType.Div);
            Trie.Insert("-", TokenType.Minus);
            Trie.Insert("+", TokenType.Plus);
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

        internal Token(TokenType type)
        {
            Type = type;
        }

        internal Token(double value)
        {
            Type = TokenType.Number;
            _value = value;
        }
        internal Token(string name)
        {
            Type = TokenType.Variable;
            _name = name;
        }
    }
}
