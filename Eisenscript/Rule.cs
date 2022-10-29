namespace Eisenscript
{
    public class Rule
    {
        private readonly List<RuleAction> _actions = new();

        internal List<RuleAction> Actions => _actions;

        public string? MaxDepthNext { get; init; }
        public double Weight { get; set; }

        public string? Name { get; }

        public int MaxDepth { get; init; }

        internal Rule(string? name, double weight = 1.0)
        {
            Name = name;
            Weight = weight;
        }

        internal void AddAction(RuleAction action)
        {
            _actions.Add(action);
        }
    }
}
