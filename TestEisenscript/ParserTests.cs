using Eisenscript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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
        public void TestBasicRule()
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
            Assert.AreEqual(null, rule.Actions[0].Loops);
        }

        [TestMethod]
        public void TestRuleTransform()
        {
            var scriptSets = @"
rule bx {{x -2 y 1} box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(1, rule.Actions.Count);
            Assert.AreEqual(TokenType.Box, rule.Actions[0].Type);
#pragma warning disable CS8602
            Assert.AreEqual(-2, rule.Actions[0].Loops[0].Transform.Mtx.Translation.X);
            Assert.AreEqual(1, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Y);
            Assert.AreEqual(0, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Z);
#pragma warning restore CS8602
        }

        [TestMethod]
        public void TestRuleSetAction()
        {
            var scriptSets = @"
rule bx {set md 10 set seed initial box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(3, rule.Actions.Count);
            Assert.AreEqual(TokenType.MaxDepth, rule.Actions[0].Set!.SetTarget);
            Assert.IsTrue(rule.Actions[1].Set!.IsInitSeed);
            Assert.AreEqual(TokenType.Box, rule.Actions[2].Type);

        }

        [TestMethod]
        public void TestRuleTransformReps()
        {
            var scriptSets = @"
rule bx {3 * {x -2 y 1} box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(1, rule.Actions.Count);
            Assert.AreEqual(TokenType.Box, rule.Actions[0].Type);
#pragma warning disable CS8602
            Assert.AreEqual(3, rule.Actions[0].Loops[0].Reps);
            Assert.AreEqual(-2, rule.Actions[0].Loops[0].Transform.Mtx.Translation.X);
            Assert.AreEqual(1, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Y);
            Assert.AreEqual(0, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Z);
#pragma warning restore CS8602
        }

        [TestMethod]
        public void TestRuleMaxDepth()
        {
            var scriptSets = @"
rule bx md 100 > bx2 {3 * {x -2 y 1} box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(1, rule.Actions.Count);
            Assert.AreEqual(100, rule.MaxDepth);
            Assert.AreEqual("bx2", rule.MaxDepthNext);
            Assert.AreEqual(TokenType.Box, rule.Actions[0].Type);
#pragma warning disable CS8602
            Assert.AreEqual(3, rule.Actions[0].Loops[0].Reps);
            Assert.AreEqual(-2, rule.Actions[0].Loops[0].Transform.Mtx.Translation.X);
            Assert.AreEqual(1, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Y);
            Assert.AreEqual(0, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Z);
#pragma warning restore CS8602
        }

        [TestMethod]
        public void TestRuleWeight()
        {
            var scriptSets = @"
rule bx w 30 {3 * {x -2 y 1} box}"[2..];
            var tr = new StringReader(scriptSets);
            var parser = new Parser(tr);
            var rules = parser.Rules();
            Assert.AreEqual(1, rules.RuleCount);

            // Picking the rule normalizes the weight to 1.
            // All this stuff makes the testing of weights a bit difficult
            var rule = rules.PickRule("bx", 0);
            Assert.AreEqual(1, rule.Weight);

            Assert.AreEqual(1, rule.Actions.Count);
            Assert.AreEqual(TokenType.Box, rule.Actions[0].Type);
#pragma warning disable CS8602
            Assert.AreEqual(3, rule.Actions[0].Loops[0].Reps);
            Assert.AreEqual(-2, rule.Actions[0].Loops[0].Transform.Mtx.Translation.X);
            Assert.AreEqual(1, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Y);
            Assert.AreEqual(0, rule.Actions[0].Loops[0].Transform.Mtx.Translation.Z);
#pragma warning restore CS8602
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
