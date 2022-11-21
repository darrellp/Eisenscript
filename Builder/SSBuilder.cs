using System.Numerics;
using System.Text.RegularExpressions;
using Eisenscript;

namespace Builder
{
    // ReSharper disable once InconsistentNaming
    public class SSBuilder
    {
        #region Private variables
        internal Rules? CurrentRules { get; private set; }
        internal Stack<State> StateStack { get; } = new();
        internal int RecurseDepth => StateStack.Count;
        internal bool AtStackLimit => StateStack.Count >= CurrentRules!.MaxDepth - 1;
        #endregion

        #region Public variables
        public event DrawEventHandler? DrawEvent;
        public event RgbaEventHandler? BackgroundEvent;
        #endregion

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

        internal virtual void OnRaiseDrawEvent(TokenType type, Matrix4x4 mtx, RGBA rgba)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var raiseEvent = DrawEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                var args = new DrawArgs(type, mtx, rgba);

                // Call to raise the event.
                raiseEvent(this, args);
            }
        }

        internal virtual void OnRaiseBackgroundEvent(Matrix4x4 mtx, RGBA rgba)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var raiseEvent = BackgroundEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                var args = new RgbaArgs(rgba);

                // Call to raise the event.
                raiseEvent(this, args);
            }
        }

    }
}