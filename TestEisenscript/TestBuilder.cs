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
box"[2..];
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
            var testScript = @"
{x 2 y 3} box"[2..];
            Matrix4x4? mtx = null;
            var tt = TokenType.Error;
            var callCount = 0;

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
rule r1 {3 * {x 2 y 3} box}"[2..];
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

        [TestMethod]
        public void TestNestedLoopCalls()
        {
            var testScript = @"
r1
rule r1 {2 * {x 2} 2 * {y 3} box}"[2..];
            var matrices = new Matrix4x4?[4];
            var tt = new TokenType?[4];
            var callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt[callCount] = t;
                matrices[callCount++] = m;
            });
            builder.Build(tr);
            Assert.AreEqual(4, callCount);
            Assert.IsTrue(tt.All(t => t == TokenType.Box));
            Assert.AreEqual(2, matrices[0]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[0]!.Value.Translation.Y);
            Assert.AreEqual(2, matrices[1]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[1]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[2]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[2]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[3]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[3]!.Value.Translation.Y);
        }

        [TestMethod]
        public void TestMatrixInheritance()
        {
            var testScript = @"
r1
rule r1 {2 * {x 2} 2 * {y 3} r2}
rule r2 {box}"[2..];
            var matrices = new Matrix4x4?[4];
            var tt = new TokenType?[4];
            var callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt[callCount] = t;
                matrices[callCount++] = m;
            });
            builder.Build(tr);
            Assert.AreEqual(4, callCount);
            Assert.IsTrue(tt.All(t => t == TokenType.Box));
            Assert.AreEqual(2, matrices[0]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[0]!.Value.Translation.Y);
            Assert.AreEqual(2, matrices[1]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[1]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[2]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[2]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[3]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[3]!.Value.Translation.Y);
        }

        [TestMethod]
        public void TestBuilderRuleCall()
        {
            var testScript = @"
r1
rule r1 {2 * {x 2} 2 * {y 3} r2}
rule r2 {box}"[2..];
            var matrices = new Matrix4x4?[4];
            var tt = new TokenType?[4];
            var callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt[callCount] = t;
                matrices[callCount++] = m;
            });
            builder.Build(tr);
            Assert.AreEqual(4, callCount);
            Assert.IsTrue(tt.All(t => t == TokenType.Box));
            Assert.AreEqual(2, matrices[0]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[0]!.Value.Translation.Y);
            Assert.AreEqual(2, matrices[1]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[1]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[2]!.Value.Translation.X);
            Assert.AreEqual(3, matrices[2]!.Value.Translation.Y);
            Assert.AreEqual(4, matrices[3]!.Value.Translation.X);
            Assert.AreEqual(6, matrices[3]!.Value.Translation.Y);
        }

        [TestMethod]
        public void TestRecursion()
        {
            var testScript = @"
set maxdepth 6
r1
rule r1 { {x 2} r1 box}"[2..];
            var matrices = new Matrix4x4?[4];
            var tt = new TokenType?[4];
            var callCount = 0;

            var tr = new StringReader(testScript);
            var builder = new SSBuilder((t, m) =>
            {
                tt[callCount] = t;
                matrices[callCount++] = m;
            });
            builder.Build(tr);
            Assert.AreEqual(4, callCount);
            Assert.IsTrue(tt.All(t => t == TokenType.Box));
            // This is the proper behavior of Structure Synth.  I thought it would start at 2 as
            // {4 * {x 2} box}
            // would have, but in that case the loop's action is to draw a box.  In our
            // case the loop's action is to invoke a rule so the next action, box, is
            // executed with the matrix reset to the input matrix which starts at 0 so
            // the loop applies to the invoked rule rather than the box draw.
            Assert.AreEqual(6, matrices[0]!.Value.Translation.X);
            Assert.AreEqual(4, matrices[1]!.Value.Translation.X);
            Assert.AreEqual(2, matrices[2]!.Value.Translation.X);
            Assert.AreEqual(0, matrices[3]!.Value.Translation.X);
        }
    }
}