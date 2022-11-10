// #define VERBOSE
using System.Numerics;
using Eisenscript;

namespace Builder
{
    internal class State
    {
        internal Rule CurrentRule { get; }
        internal int ActionIndex { get; set; }
        internal Matrix4x4[] LoopMatrices;
        internal int[] LoopIndices;
        private readonly Matrix4x4 _mtxInput;
        private SSBuilder _builder;

        internal RuleAction Action => CurrentRule.Actions[ActionIndex];

#pragma warning disable CS8618
        public State(SSBuilder builder, Rule currentRule, Matrix4x4? mtxInput = null)
        {
            CurrentRule = currentRule;
            _mtxInput = mtxInput ?? Matrix4x4.Identity;
            _builder = builder;
            SetActionIndex(0);
        }
#pragma warning restore CS8618

        internal void NextAction()
        {
            SetActionIndex(ActionIndex + 1);
        }

        internal void SetActionIndex(int index)
        {
            ActionIndex = index;
            if (index == CurrentRule.Actions.Count || Action.Loops == null)
            {
                return;
            }

            var loopCount = Action.Loops.Count;
            LoopMatrices = new Matrix4x4[loopCount];
            LoopIndices = new int[loopCount];
            var curMatrix = _mtxInput;
            for (var i = 0; i < loopCount; i++)
            {
                LoopIndices[i] = 0;
                curMatrix *= Action.Loops[i].Transform.Mtx;
                LoopMatrices[i] = curMatrix;
            }
        }

        // The state is positioned to be executed as is.  After effecting the execution the state/stack should be
        // manipulated to reflect the actions to be taken AFTER this execution.
        public void Execute(SSBuilder builder)
        {
#if VERBOSE
            Console.WriteLine(PadString(""));
            Console.WriteLine(PadString(this.ToString()));
#endif
            if (ActionIndex == CurrentRule.Actions.Count)
            {
#if VERBOSE
                Console.WriteLine(PadString("Popping ourselves off the stack"));
#endif
                builder.StateStack.Pop();
                return;
            }

            if (Action.Loops == null )
            {
                if (Action.PostRule != null)
                {
                    if (builder.AtStackLimit)
                    {
                        NextAction();
                        return;
                    }
#if VERBOSE
                    Console.WriteLine(PadString($"Invoking rule {Action.PostRule}"));
#endif
                    var newRule = builder.CurrentRules!.PickRule(Action.PostRule);
                    builder.StateStack.Push(new State(_builder, newRule));
                    NextAction();
                }
                else
                {
#if VERBOSE
                    Console.WriteLine(PadString($"Drawing a {Action.Type}"));
#endif
                    builder.Draw(Action.Type, _mtxInput);
                    NextAction();
                }

                return;
            }

            // Execute loops
            // Current loops values are present when we enter this routine
            var fContinue = false;
            var cLoops = Action.Loops.Count;
            var mtxExecution = LoopMatrices[cLoops - 1];

            // Arrange for the invoked rule to be called
            // We ALWAYS are in the position to invoke the rule at this point.  If at the end we discover that we have
            // completed all the loops, we'll arrange for the next call to be positioned on the next action.
            if (Action.PostRule != null)
            {
                if (builder.AtStackLimit)
                {
#if VERBOSE
                    Console.WriteLine(PadString("Breaking out due to stack limit"));
#endif
                    // We can never call the post rule so don't bother going through all the loops
                    NextAction();
                    return;
                }
#if VERBOSE
                Console.WriteLine(PadString($"Invoking rule {Action.PostRule}"));
#endif
                var newRule = builder.CurrentRules!.PickRule(Action.PostRule);
                builder.StateStack.Push(new State(_builder, newRule, mtxExecution));
            }
            else
            {
#if VERBOSE
                Console.WriteLine(PadString($"Drawing a {Action.Type}"));
#endif
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

#if VERBOSE
                Console.WriteLine(PadString($"Incrementing loop {index}"));
#endif
                // We've found the index that will be incremented
                fContinue = true;
                var prevMatrix = Action.Loops[index].Transform.Mtx * LoopMatrices[index];
                LoopMatrices[index] = prevMatrix;

                for (var iIndex = index + 1; iIndex < cLoops; iIndex++)
                {
                    prevMatrix *= Action.Loops[iIndex].Transform.Mtx;
                    LoopMatrices[iIndex] = prevMatrix;
                }

                break;
            }

            if (!fContinue)
            {
                // Done with this action, move on to the next
                NextAction();
            }
        }

        public override string ToString()
        {
            if (_builder == null)
            {
                return "<NULL BUILDER>";
            }
            var name = CurrentRule.Name ?? "<INIT>";
            var xlat = _mtxInput.Translation;

            if (ActionIndex == CurrentRule.Actions.Count)
            {
                return $"{name} finished";
            }
            return $"{name}:{ActionIndex} Indices: {LoopIndicesToString()} TR:({xlat.X}, {xlat.Y}, {xlat.Z})";
        }

        private string LoopIndicesToString()
        {
            if (Action.Loops == null)
            {
                return "NA";
            }

            return string.Join(" ", LoopIndices.Select(v => v.ToString()));
        }

        private string PadString(string str)
        {
            var padding = new string(' ', 4 * (_builder.RecurseDepth - 1));
            return padding + str;
        }
    }
}
