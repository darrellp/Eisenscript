using System.Numerics;
using Eisenscript;

namespace Builder
{
    internal class State
    {
        public const int MaxNestedTransforms = 10;
        internal Rule CurrentRule { get; }
        internal int ActionIndex { get; set; }
        internal Matrix4x4[] _loopMatrices;
        internal int[] LoopIndices;
        readonly Matrix4x4 _mtxInput = Matrix4x4.Identity;

        internal RuleAction Action => CurrentRule.Actions[ActionIndex];

        public State(Rule currentRule, Matrix4x4? mtxInput = null)
        {
            CurrentRule = currentRule;
            int actionCount = currentRule.Actions.Count;
            _mtxInput = mtxInput ?? Matrix4x4.Identity;
            SetActionIndex(0);
        }

        internal void NextAction()
        {
            SetActionIndex(ActionIndex + 1);
        }

        internal void SetActionIndex(int index)
        {
            ActionIndex = index;
            if (index == CurrentRule.Actions.Count)
            {
                return;
            }
            if (Action.Loops != null)
            {
                var loopCount = Action.Loops.Count;
                _loopMatrices = new Matrix4x4[loopCount];
                LoopIndices = new int[loopCount];
                var curMatrix = _mtxInput;
                for (var i = 0; i < loopCount; i++)
                {
                    LoopIndices[i] = 0;
                    curMatrix *= Action.Loops[i].Transform.Mtx;
                    _loopMatrices[i] = curMatrix;
                }
            }
        }

        // The state is positioned to be executed as is.  During the execution the state/stack should be
        // manipulated to reflect the actions to be taken AFTER this execution.
        public void Execute(SSBuilder builder)
        {
            if (ActionIndex == CurrentRule.Actions.Count)
            {
                builder.StateStack.Pop();
                return;
            }

            if (Action.Loops == null )
            {
                if (Action.PostRule != null)
                {
                    var newRule = builder.CurrentRules.PickRule(Action.PostRule);
                    builder.StateStack.Push(new State(newRule));
                    NextAction();
                }
                else
                {
                    builder.Draw(Action.Type, _mtxInput);
                    NextAction();
                }

                return;
            }

            // Execute loops
            // Current loops values are present when we enter this routine
            var fContinue = false;
            var cLoops = Action.Loops.Count;
            var mtxExecution = _loopMatrices[cLoops - 1];

            // Arrange for the invoked rule to be called
            // We ALWAYS are in the position to invoke the rule at this point.  If at the end we discover that we have
            // completed all the loops, we'll arrange for the next call to be positioned on the next action.
            if (Action.PostRule != null)
            {
                var newRule = builder.CurrentRules!.PickRule(Action.PostRule);
                builder.StateStack.Push(new State(newRule, mtxExecution));
            }
            else
            {
                builder.Draw(Action.Type, mtxExecution);
            }

            // ...and adjust indices/matrices for next step in the loops
            for (var index = cLoops - 1; index >= 0; index--)
            {
                if (++LoopIndices[index] == Action.Loops[index].Reps)
                {
                    // This index loops back to the beginning
                    LoopIndices[index] = 0;

                    // We'll adjust matrices after we've located the advancing index
                    continue;
                }

                // We've found the index that will be incremented
                fContinue = true;
                var prevMatrix = Action.Loops[index].Transform.Mtx * _loopMatrices[index];
                _loopMatrices[index] = prevMatrix;

                for (var iIndex = index + 1; iIndex < cLoops; iIndex++)
                {
                    prevMatrix *= Action.Loops[iIndex].Transform.Mtx;
                    _loopMatrices[iIndex] = prevMatrix;
                }
            }

            if (!fContinue)
            {
                // Done with this action, move on to the next
                NextAction();
            }
        }
    }
}
