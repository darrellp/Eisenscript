using Eisenscript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

// TODO: Real error handling instead of just throws
namespace TestEisenscript
{
    [TestClass]

    public class ScanTests
    {
        private const bool T = true;
        private const bool F = false;

        [TestMethod]
        public void TestNumberScan()
        {
            string testScriptNumbers = @"
123 456/* 1
2 */789  
1 2 3 4
3.141592654 -.12".Substring(2);
            TextReader tr = new StringReader(testScriptNumbers);
            var scanner = new Scan(tr);
            var charTypes = scanner.MapCharTo;
            var tokens = scanner.Tokens;
            Assert.AreEqual(9, tokens.Count);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue(tokens[0].Type == TokenType.Number && tokens[0].Value == 123);
            Assert.IsTrue(tokens[1].Type == TokenType.Number && tokens[1].Value == 456);
            Assert.IsTrue(tokens[2].Type == TokenType.Number && tokens[2].Value == 789);
            Assert.IsTrue(tokens[3].Type == TokenType.Number && tokens[3].Value == 1);
            Assert.IsTrue(tokens[4].Type == TokenType.Number && tokens[4].Value == 2);
            Assert.IsTrue(tokens[5].Type == TokenType.Number && tokens[5].Value == 3);
            Assert.IsTrue(tokens[6].Type == TokenType.Number && tokens[6].Value == 4);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            Assert.IsTrue(tokens[7].Type == TokenType.Number && Math.Abs(tokens[7].Value - 3.141592654) < 0.00001);
            Assert.IsTrue(tokens[8].Type == TokenType.Number && Math.Abs(tokens[8].Value + 0.12) < 0.001);
        }

        [TestMethod]
        public void TestMLComments()
        {
            string testScriptMlc = @"
/*234567*/
  /*567
90123*//*13*/   ".Substring(2);

            bool[] IsComment =
            {
                T, T, T, T, T, T, T, T, T, T, F,
                F, F, T, T, T, T, T, F,
                T, T, T, T, T, T, T, T, T, T, T, T, T, F, F, F,
                F
            };
            TextReader tr = new StringReader(testScriptMlc);
            var scanner = new Scan(tr);
            var charTypes = scanner.MapCharTo;
            Assert.IsTrue(charTypes.Zip(IsComment).All(p => !((p.First == TokenType.Comment) ^ p.Second)));
        }

        [TestMethod]
        public void TestSLComments()
        {
            string testScriptSlc = @"
//1234
   //1234".Substring(2);

            bool[] IsComment =
            {
                T, T, T, T, T, T, F,
                F, F, F, T, T, T, T, T, T, F
            };
            TextReader tr = new StringReader(testScriptSlc);
            var scanner = new Scan(tr);
            var charTypes = scanner.MapCharTo;
            Assert.IsTrue(charTypes.Zip(IsComment).All(p => !((p.First == TokenType.Comment) ^ p.Second)));
        }

        [TestMethod]
        public void TestRgba()
        {
            string testRgbaSrc = @"
maizeyellow
#fff
#010203
#ee010203
#010020030
#010002000300".Substring(2);

            TextReader tr = new StringReader(testRgbaSrc);
            var scanner = new Scan(tr);
            var charTypes = scanner.MapCharTo.ToArray();
            var tokens = scanner.Tokens.ToArray();
            Assert.IsTrue(charTypes[..11].All(t => t == TokenType.Rgba));
            Assert.IsTrue(charTypes[12..16].All(t => t == TokenType.Rgba));
            Assert.IsTrue(charTypes[17..24].All(t => t == TokenType.Rgba));
            Assert.IsTrue(charTypes[25..34].All(t => t == TokenType.Rgba));
            Assert.IsTrue(charTypes[35..45].All(t => t == TokenType.Rgba));
            Assert.IsTrue(charTypes[46..59].All(t => t == TokenType.Rgba));
            Assert.IsTrue(tokens[..].All(tk => tk.Type == TokenType.Rgba));
            Assert.AreEqual(tokens[0], new Token(new RGBA(0xE4, 0xA0, 0x10), 0));
            Assert.AreEqual(tokens[1], new Token(new RGBA(0xFF, 0xFF, 0xFF), 1));
            Assert.AreEqual(tokens[2], new Token(new RGBA(0x01, 0x02, 0x03), 2));
            Assert.AreEqual(tokens[3], new Token(new RGBA(0x01, 0x02, 0x03, 0xEE), 3));
            Assert.AreEqual(tokens[4], new Token(new RGBA(0x01, 0x02, 0x03), 4));
            Assert.AreEqual(tokens[5], new Token(new RGBA(0x01, 0x02, 0x03), 5));
        }

        [TestMethod]
        public void TestDefines()
        {
        }

        [TestMethod]
        public void TestKeywordsVariables()
        {
            string testScriptKeywords = @"
* #define {} md w b a set rule maxdepth
maxobjects minsize maxsize seed initial background weight
x y z rx ry rz s m fx fy fz hue sat brightness alpha color
blend random colorpool box grid sphere line point triangle
mesh cylinder tube

// Now some variables
white24
base
rules
zzxxy
_dog_cat
squirrel @nog
rabbit
".Substring(2);
            TextReader tr = new StringReader(testScriptKeywords);
            var scanner = new Scan(tr);
            var charTypes = scanner.MapCharTo;
            var tokens = scanner.Tokens;
            Assert.AreEqual(TokenType.Mult, tokens[0].Type);
            Assert.AreEqual(TokenType.Define, tokens[1].Type);
            Assert.AreEqual(TokenType.OpenBrace, tokens[2].Type);
            Assert.AreEqual(TokenType.CloseBrace, tokens[3].Type);
            Assert.AreEqual(TokenType.MaxDepth, tokens[4].Type);
            Assert.AreEqual(TokenType.Weight, tokens[5].Type);
            Assert.AreEqual(TokenType.Brightness, tokens[6].Type);
            Assert.AreEqual(TokenType.Alpha, tokens[7].Type);

            var offset = 8 - (int)TokenType.Set;

            for (var i = (int)TokenType.Set; i < (int)TokenType.End; i++)
            {
                Assert.AreEqual((TokenType)i, tokens[i + offset].Type);
            }

            var iCur = (int)TokenType.End + offset;
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("white24", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("base", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("rules", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("zzxxy", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("_dog_cat", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("squirrel", tokens[iCur++].Name);
            Assert.AreEqual(TokenType.Variable, tokens[iCur].Type);
            Assert.AreEqual("rabbit", tokens[iCur++].Name);
            Assert.AreEqual(1, scanner.Exceptions.Count);
            Assert.AreEqual(12, scanner.Exceptions[0].Line);
        }
    }
}