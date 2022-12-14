using System.IO;
using Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestEisenscript
{
    [TestClass]
    public class ErrorTests
    {
        [TestMethod]
        public void TestMissingRule()
        {
            string testScript = @"
set colorpool randomrgb
set seed 1
errorRule
r1
rule r1 {{color random x 2} box}"[2..];

            var builder = new SSBuilder();
            builder.DrawEvent += ((s, a) => { });
            var errors = builder.Build(new StringReader(testScript));
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Undefined rule: errorRule", errors[0].Message);
        }

        private void Builder_DrawEvent(object sender, Eisenscript.DrawArgs args)
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void TestMissingRuleKeyword()
        {
            string testScript = @"
seed
r1
rule r1 {{color random x 2} box}"[2..];
            var builder = new SSBuilder();
            builder.DrawEvent += ((s, a) => { });
            var errors = builder.Build(new StringReader(testScript));
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Invalid/empty rule body", errors[0].Message);
            Assert.AreEqual(0, errors[0].Line);
        }

        [TestMethod]
        public void TestMissingBraces()
        {
            string testScript = @"
seed
r1
rule r1 {{color random x 2} box"[2..];
            var builder = new SSBuilder();
            builder.DrawEvent += ((s, a) => { });
            var errors = builder.Build(new StringReader(testScript));
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Invalid/empty rule body", errors[0].Message);
            Assert.AreEqual(0, errors[0].Line);
        }
    }
}
