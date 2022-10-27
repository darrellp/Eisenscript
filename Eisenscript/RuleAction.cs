namespace Eisenscript
{
    internal class RuleAction
    {
        #region Private variables
        private string? _ruleName;     // The rule that will be called after all the transformations
        private List<TransformationLoop>? _loops;
        private SetAction? _setAction;
        private TokenType _tt = TokenType.End;
        #endregion

        public RuleAction(string ruleName, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            _ruleName = ruleName;
            _setAction = setAction;
            _loops = loops;
        }
        public RuleAction(TokenType tt, List<TransformationLoop>? loops = null, SetAction? setAction = null)
        {
            _tt = tt;
            _setAction = setAction;
            _loops = loops;
        }
    }
}
