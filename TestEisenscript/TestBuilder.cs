using System.IO;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Builder;
using Eisenscript;

// ReSharper disable once IdentifierTypo
namespace TestEisenscript
{
    [TestClass]
    public class BuilderTests
    {
        [TestMethod]
        public void TestSingleDraw()
        {
            string testScript = @"
box".Substring(2);
            Matrix4x4? mtx = null;
            TokenType tt = TokenType.Error;
            int callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt = t;
                mtx = m;
                callCount++;
            });
            builder.Build(tr);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(TokenType.Box, tt);
            Assert.AreEqual(Matrix4x4.Identity, mtx);
        }

        [TestMethod]
        public void TestTransformDraw()
        {
            string testScript = @"
{x 2 y 3} box".Substring(2);
            Matrix4x4? mtx = null;
            TokenType tt = TokenType.Error;
            int callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt = t;
                mtx = m;
                callCount++;
            });
            builder.Build(tr);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(TokenType.Box, tt);
            Assert.AreEqual(2, mtx!.Value.Translation.X);
            Assert.AreEqual(3, mtx!.Value.Translation.Y);
        }


        [TestMethod]
        public void TestRuleCall()
        {
            string testScript = @"
r1
rule r1 {3 * {x 2 y 3} box}".Substring(2);
            Matrix4x4?[] matrices = new Matrix4x4?[3];
            TokenType?[] tt = new TokenType?[3];
            int callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt[callCount] = t;
                matrices[callCount++] = m;
            });
            builder.Build(tr);
            Assert.AreEqual(3, callCount);
            Assert.IsTrue(tt.All(t => t == TokenType.Box));
            Assert.AreEqual(2, matrices[0]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[0]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[1]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[1]!.Value.Translation.Y);
            Assert.AreEqual(6, matrices[2]!.Value.Translation.X);
            Assert.AreEqual(9, matrices[2]!.Value.Translation.Y);
        }
    }
}