using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eisenscript
{
    internal class WeightedRule
    {
        private readonly List<Rule> _rules = new();
        private readonly Random _rnd = new();
        private bool _isNormalized = false;

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

        internal Rule Pick()
        {
            if (!_isNormalized)
            {
                Normalize();
                _isNormalized = true;
            }

            var weight = _rnd.NextDouble();
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
