using System.Numerics;
using System.Text.RegularExpressions;
using Eisenscript;

namespace Builder
{
    // ReSharper disable once InconsistentNaming
    public sealed class SSBuilder
    {
        #region Private variables
        internal Rules? CurrentRules { get; private set; }
        internal Stack<State> StateStack { get; } = new();
        internal int RecurseDepth => StateStack.Count;
        internal bool AtStackLimit => StateStack.Count >= CurrentRules!.MaxDepth - 1;
        internal int objCount;
        #endregion

        #region Public variables
        public event DrawEventHandler? DrawEvent;
        public event RgbaEventHandler? BackgroundEvent;
        public event CameraInfoHandler? CameraInfoEvent;
        #endregion

        public List<ParserException> Build(TextReader input)
        {
            CurrentRules = null;
            StateStack.Clear();
            objCount = 0;

            var parser = new Parser(input);
            CurrentRules = parser.Rules();
            if (parser.Exceptions.Count > 0)
            {
                return parser.Exceptions;
            }

            OnRaiseBackgroundEvent(CurrentRules.Background);
            OnRaiseCameraInfoEvent(CurrentRules.CamInfo);

            foreach (var rule in CurrentRules.InitRules)
            {
                StateStack.Push(new State(this, rule, CurrentRules));
                Execute();
                if (objCount >= CurrentRules!.MaxObjects)
                {
                    break;
                }
            }

            return new List<ParserException>();
        }

        private void Execute()
        {
            while (StateStack.Count > 0 && objCount < CurrentRules!.MaxObjects)
            {
                var state = StateStack.Peek();
                state.Execute(this);
            }
        }

        internal void OnRaiseDrawEvent(TokenType type, Matrix4x4 mtx, RGBA rgba)
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

            ++objCount;
        }

        private void OnRaiseBackgroundEvent(RGBA rgba)
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

        private void OnRaiseCameraInfoEvent(CameraInfo camInfo)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var raiseEvent = CameraInfoEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                var args = new CameraInfoArgs(camInfo);

                // Call to raise the event.
                raiseEvent(this, args);
            }
        }

    }
}