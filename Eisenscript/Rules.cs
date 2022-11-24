using System.Diagnostics;

namespace Eisenscript
{
    public class Rules
    {
        private readonly List<Rule> _initRules = new();
        private readonly Dictionary<string, WeightedRule> _weightedRules = new();
        private int _seedInit = -1;

        public int MaxDepth { get; set; } = 1000;
        public int MaxObjects { get; set; } = int.MaxValue;
        public double MinSize { get; set; } = int.MaxValue;
        public double MaxSize { get; set; } = int.MinValue;
        public CameraInfo CamInfo { get; } = new();

        public int SeedInit
        {
            get => _seedInit;

            set
            {
                SetSeed(value);
                _seedInit = value;
            }
        }
        public RGBA Background { get; set; } = new();
        public ColorPool Pool { get; set; } = new();

        public List<Rule> InitRules => _initRules;

        internal void AddInitRule(Rule rule)
        {
            _initRules.Add(rule);
        }

        internal Random RndGeometry = new Random();
        internal Random RndColors = new Random();

        public void SetSeed(int seed)
        {
            SetSeedGeometry(seed);
            SetSeedColors(seed + 1);
        }

        public void SetSeedGeometry(int seed)
        {
            RndGeometry = new Random(seed);
        }

        public void SetSeedColors(int seed)
        {
            RndColors = new Random(seed);
        }

        internal void AddRule(Rule rule)
        {
            Debug.Assert(rule.Name != null, "rule.Name != null");
            if (!_weightedRules.ContainsKey(rule.Name))
            {
                _weightedRules[rule.Name] = new WeightedRule();
            }
            _weightedRules[rule.Name].AddRule(rule);
        }

        public Rule PickRule(string name, int line = -1)
        {
            if (!_weightedRules.ContainsKey(name))
            {
                throw new ParserException("Trying to pick a non-existent rule", line);
            }

            return _weightedRules[name].Pick(RndGeometry);
        }

        public int RuleCount => _weightedRules.Values.Select(wr => wr.Count).Sum();

        public void CheckValidity()
        {
            foreach (var rule in InitRules)
            {
                foreach (var ruleAction in rule.Actions)
                {
                    if (ruleAction.PostRule != null && !_weightedRules.ContainsKey(ruleAction.PostRule))
                    {
                        throw new ParserException($"Undefined rule: {ruleAction.PostRule}", -1);
                    }
                }
            }
            foreach (var wrule in _weightedRules.Values)
            {
                foreach (var rule in wrule.RulesList)
                {
                    foreach (var ruleAction in rule.Actions)
                    {
                        if (ruleAction.PostRule != null && !_weightedRules.ContainsKey(ruleAction.PostRule))
                        {
                            throw new ParserException($"Undefined rule: {ruleAction.PostRule}", -1);
                        }
                    }
                }
            }
        }
    }
}
