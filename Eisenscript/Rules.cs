namespace Eisenscript
{
    public class Rules
    {
        private readonly List<Rule> _initRules = new();
        private readonly Dictionary<string, WeightedRule> _weightedRules = new();

        public int MaxDepth { get; set; } = 1000;
        public int MaxObjects { get; set; } = -1;
        public double MinSize { get; set; } = int.MaxValue;
        public double MaxSize { get; set; } = int.MinValue;
        public int SeedInit { get; set; } = -1;
        public RGBA Background { get; set; } = new RGBA();

        public List<Rule> InitRules => _initRules;

        internal void AddInitRule(Rule rule)
        {
            _initRules.Add(rule);
        }

        internal void AddRule(Rule rule)
        {
            if (!_weightedRules.ContainsKey(rule.Name))
            {
                _weightedRules[rule.Name] = new WeightedRule();
            }
            _weightedRules[rule.Name].AddRule(rule);
        }

        public Rule PickRule(string name, int line)
        {
            if (!_weightedRules.ContainsKey(name))
            {
                throw new ParserException("Trying to pick a non-existent rule", line);
            }

            return _weightedRules[name].Pick();
        }

        public int RuleCount => _weightedRules.Values.Select(wr => wr.Count).Sum();
    }
}
