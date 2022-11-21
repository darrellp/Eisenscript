namespace Eisenscript
{
    public class WeightedRule
    {
        internal List<Rule> RulesList { get; } = new();
        private bool _isNormalized;

        internal void AddRule(Rule rule)
        {
            RulesList.Add(rule);
        }

        private void Normalize()
        {
            var total = RulesList.Select(r => r.Weight).Sum();
            foreach (var rule in RulesList)
            {
                rule.Weight /= total;
            }
        }

        public Rule Pick(Random rnd)
        {
            if (!_isNormalized)
            {
                Normalize();
                _isNormalized = true;
            }

            var weight = rnd.NextDouble();
            var total = 0.0;
            var iSelect = -1;

            while (total < weight)
            {
                total += RulesList[++iSelect].Weight;
            }

            return RulesList[iSelect];
        }

        internal int Count => RulesList.Count;
    }
}
