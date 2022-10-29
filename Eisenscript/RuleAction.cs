namespace Eisenscript
{
    public class RuleAction
    {
        #region Private variables
        public string? RuleName { get; }
        public SetAction? Set { get; }

        #endregion

        #region Properties
        public List<TransformationLoop>? Loops { get; }

        public TokenType Type { get; } = TokenType.End;
        #endregion

        #region Constructors
        internal RuleAction(string ruleName, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            RuleName = ruleName;
            Set = setAction;
            Loops = loops;
        }

        internal RuleAction(TokenType tt, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            Type = tt;
            Set = setAction;
            Loops = loops;
        }

        internal RuleAction(SetAction setAction)
        {
            Set = setAction;
        }
        #endregion
    }
}
