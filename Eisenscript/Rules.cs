using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eisenscript
{
    internal class Rules
    {
        private List<Rule> InitRules = new();
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
    }
}
