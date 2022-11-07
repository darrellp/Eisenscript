using System.IO;
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
rule r1 {{x 2 y 3} box}".Substring(2);
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
    }
}