namespace Eisenscript
{
    public class WeightedRule
    {
        private readonly List<Rule> _rules = new();
        private bool _isNormalized;

        internal void AddRule(Rule rule)
        {
            _rules.Add(rule);
        }

        private void Normalize()
        {
            var total = _rules.Select(r => r.Weight).Sum();
            foreach (var rule in _rules)
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
                total += _rules[++iSelect].Weight;
            }

            return _rules[iSelect];
        }

        internal int Count => _rules.Count;
    }
}
