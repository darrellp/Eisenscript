using System.Numerics;
using System.Text.RegularExpressions;
using Eisenscript;

namespace Builder
{
    // ReSharper disable once InconsistentNaming
    public class SSBuilder
    {
        #region Private variables
        internal Action<Matrix4x4>? SetMatrix { get; }
        internal Action<Matrix4x4>? MulMatrix { get; }
        internal Action<TokenType, Matrix4x4, RGBA> Draw { get; }
        internal Rules? CurrentRules { get; private set; }
        internal Stack<State> StateStack { get; } = new();
        internal int RecurseDepth => StateStack.Count;
        internal bool AtStackLimit => StateStack.Count >= CurrentRules!.MaxDepth - 1;
        #endregion

        public SSBuilder(Action<TokenType, Matrix4x4, RGBA> draw,
            Action<Matrix4x4>? setMatrix = null,
            Action<Matrix4x4>? mulMatrix = null)
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

        public List<ParserException> Build(TextReader input)
        {
            var parser = new Parser(input);
            CurrentRules = parser.Rules();
            if (parser.Exceptions.Count > 0)
            {
                return parser.Exceptions;
            }


            foreach (var rule in CurrentRules.InitRules)
            {
                StateStack.Push(new State(this, rule, CurrentRules));
                Execute();
            }

            return new List<ParserException>();
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