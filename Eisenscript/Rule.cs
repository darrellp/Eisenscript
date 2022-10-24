namespace Eisenscript
{
    internal class Rule
    {
        private readonly string _name;
        private double _weight;
        private int _maxDepth;
        private List<RuleAction> _actions = new();

        internal string Name
        {
            get => _name;
            set => throw new NotImplementedException();
        }

        internal int MaxDepth
        {
            get => _maxDepth;
            set => _maxDepth = value;
        }

        public Rule(string name, double weight)
        {
            _name = name;
            _weight = weight;
        }
    }
}
