namespace Eisenscript
{
    public class Rule
    {
        private readonly string _name;
        private double _weight;
        private int _maxDepth;
        private readonly List<RuleAction> _actions = new();

        internal List<RuleAction> Actions => _actions;

        public string? MaxDepthNext { get; set; }
        public double Weight
        {
            get => _weight;
            set => _weight = value;
        }

        public string Name
        {
            get => _name;
            set => throw new NotImplementedException();
        }

        public int MaxDepth
        {
            get => _maxDepth;
            set => _maxDepth = value;
        }

        internal Rule(string name, double weight)
        {
            _name = name;
            _weight = weight;
        }

        internal void AddAction(RuleAction action)
        {
            _actions.Add(action);
        }
    }
}
