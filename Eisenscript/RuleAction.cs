namespace Eisenscript
{
    public class RuleAction
    {
        #region Private variables
        public string? RuleName { get; }
        private readonly List<TransformationLoop>? _loops;
        private SetAction? _setAction;
        #endregion

        #region Properties
        public List<TransformationLoop>? Loops => _loops;
        public TokenType Type { get; } = TokenType.End;
        #endregion

        #region Constructors
        internal RuleAction(string ruleName, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            RuleName = ruleName;
            _setAction = setAction;
            _loops = loops;
        }

        internal RuleAction(TokenType tt, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            Type = tt;
            _setAction = setAction;
            _loops = loops;
        }
        #endregion
    }
}
