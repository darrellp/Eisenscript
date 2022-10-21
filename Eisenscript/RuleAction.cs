using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eisenscript
{
    internal class RuleAction
    {
        #region Private variables
        private List<TransformationLoop> _loops = new();
        private Rule _rule;     // The rule that will be called after all the transformations
        private SetAction _setAction;
        #endregion

        public RuleAction(Rule rule, SetAction setAction)
        {
            _rule = rule;
            _setAction = setAction;
        }
    }
}
