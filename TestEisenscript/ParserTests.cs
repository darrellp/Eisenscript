using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eisenscript;

namespace TestEisenscript
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestSetParse()
        {
            var scriptSets = @"
set maxdepth 100
set maxobjects 1000
set minsize 0.1
set maxsize 20.1
set background red
set seed 500"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(100, rules.MaxDepth);
            Assert.AreEqual(1000, rules.MaxObjects);
            Assert.AreEqual(500, rules.SeedInit);
            Assert.AreEqual(new RGBA(0xff, 0, 0), rules.Background);
            Assert.IsTrue(Math.Abs(rules.MinSize - 0.1) < 0.001);
            Assert.IsTrue(Math.Abs(rules.MaxSize - 20.1) < 0.001);
        }

        [TestMethod]
        public void TestDefinesParse()
        {
            var scriptSets = @"
#define depth 100
#define bgnd #fed
set maxdepth depth
set background bgnd"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(100, rules.MaxDepth);
            Assert.AreEqual(new RGBA(0xff, 0xee, 0xdd), rules.Background);
        }

        [TestMethod]
        public void TestRulesParse1()
        {
            var scriptSets = @"
rule bx {box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(1, rule.Actions.Count);
            Assert.AreEqual(TokenType.Box, rule.Actions[0].Type);
        }

        [TestMethod]
        public void TestInitRules()
        {
            var scriptSets = @"
box"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.InitRules.Count);
            Assert.AreEqual(TokenType.Box, rules.InitRules[0].Actions[0].Type);
        }

    }
}
