using Eisencript;
using System.Numerics;
using Eisenscript;

namespace Builder
{
    // ReSharper disable once InconsistentNaming
    public class SSBuilder
    {
        #region Private variables
        internal Action<Matrix4x4>? SetMatrix { get; }
        internal Action<Matrix4x4>? MulMatrix { get; }
        internal Action<TokenType, Matrix4x4> Draw { get; }
        internal Rules? CurrentRules { get; private set; }
        internal Stack<State> StateStack { get; } = new();
        #endregion

        public SSBuilder(Action<TokenType, Matrix4x4> draw,
            Action<Matrix4x4> setMatrix = null,
            Action<Matrix4x4> mulMatrix = null)
        {
            if (SetMatrix == null ^ MulMatrix == null)
            {
                // Both should be null or non-null.  Makes no sense to have one without the other.
                throw new ArgumentException("Both MulMatrix and SetMatrix must be null or non-null in SSBuilder constructor");
            }
            SetMatrix = setMatrix;
            MulMatrix = mulMatrix;
            Draw = draw;
        }

        public void Build(TextReader input)
        {
            CurrentRules = new Parser(input).Rules();
            foreach (var rule in CurrentRules.InitRules)
            {
                StateStack.Push(new State(rule));
                Execute();
            }
        }

        private void Execute()
        {
            while (StateStack.Count > 0)
            {
                var state = StateStack.Peek();
                state.Execute(this);
            }
        }
    }
}