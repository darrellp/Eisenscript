namespace Eisenscript
{
    internal class Rules
    {
        private List<Rule> _initRules = new();
        public int MaxDepth { get; set; } = 1000;
        public int MaxObjects { get; set; } = -1;
        public double MinSize { get; set; } = int.MaxValue;
        public double MaxSize { get; set; } = int.MinValue;
        public int SeedInit { get; set; } = -1;
        public RGBA Background { get; set; } = new RGBA();

        private List<Rule> _rules = new();

        internal void AddRule(Rule rule)
        {
            _rules.Add(rule);
        }

        internal int RuleCount => _rules.Count;
    }
}
