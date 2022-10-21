using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eisenscript
{
    internal class Rule
    {
        private string _name;
        private double _weight;
        private List<RuleAction> _actions = new();

        public Rule(string name, double weight)
        {
            _name = name;
            _weight = weight;
        }
    }
}
