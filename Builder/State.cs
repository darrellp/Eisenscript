// #define VERBOSE

using System.Diagnostics;
using System.Numerics;
using Eisenscript;

namespace Builder
{
    internal class State
    {
        #region Variables
        internal Rule CurrentRule { get; }
        internal int ActionIndex { get; set; }
        internal Matrix4x4[] LoopMatrices;
        internal RGBA[] LoopRgbas;
        internal int[] LoopIndices;
        private readonly Matrix4x4 _mtxInput;
        private readonly SSBuilder _builder;
        // Solid red ala Structure Synth
        private readonly RGBA _rgbaInput = new(255, 0, 0);
        internal RuleAction Action => CurrentRule.Actions[ActionIndex];
        #endregion

        #region Constructor
#pragma warning disable CS8618
        public State(SSBuilder builder, Rule currentRule, Matrix4x4? mtxInput = null)
        {
            CurrentRule = currentRule;
            _mtxInput = mtxInput ?? Matrix4x4.Identity;
            _builder = builder;
            SetActionIndex(0);
        }

        public State(SSBuilder builder, Rule currentRule, RGBA rgbaInput, Matrix4x4? mtxInput = null) : this(builder, currentRule, mtxInput)
        {
            _rgbaInput = rgbaInput;
        }

#pragma warning restore CS8618
        #endregion

        #region Building
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
            LoopRgbas = new RGBA[loopCount];
            LoopIndices = new int[loopCount];
            var curMatrix = _mtxInput;
            var curRgba = _rgbaInput;
            for (var i = 0; i < loopCount; i++)
            {
                LoopIndices[i] = 0;
                (curMatrix, curRgba, _) = Action.Loops[i].Transform.DoTransform(curMatrix, curRgba);
                LoopMatrices[i] = curMatrix;
                LoopRgbas[i] = curRgba;
            }
        }

        // The state is positioned to be executed as is.  After effecting the execution the state/stack should be
        // manipulated to reflect the actions to be taken AFTER this execution.
        public void Execute(SSBuilder builder)
        {
            VerboseMsg("");
            VerboseMsg(this.ToString());
            if (ActionIndex == CurrentRule.Actions.Count)
            {
                VerboseMsg("Popping ourselves off the stack");
                builder.StateStack.Pop();
                return;
            }

            if (Action.Loops == null )
            {
                if (Action.PostRule != null)
                {
                    if (builder.AtStackLimit)
                    {
                        VerboseMsg("Stack limit reached");
                        NextAction();
                        return;
                    }
                    VerboseMsg($"Invoking rule {Action.PostRule}");
                    var newRule = builder.CurrentRules!.PickRule(Action.PostRule);
                    builder.StateStack.Push(new State(_builder, newRule));
                    NextAction();
                }
                else
                {
                    VerboseMsg($"Drawing a {Action.Type}");
                    builder.Draw(Action.Type, _mtxInput, _rgbaInput);
                    NextAction();
                }

                return;
            }

            // Execute loops
            // Current loops values are present when we enter this routine
            var fContinue = false;
            var cLoops = Action.Loops.Count;
            var mtxExecution = LoopMatrices[cLoops - 1];
            var rgbaExecution = LoopRgbas[cLoops - 1];

            // Arrange for the invoked rule to be called
            // We ALWAYS are in the position to invoke the rule at this point.  If at the end we discover that we have
            // completed all the loops, we'll arrange for the next call to be positioned on the next action.
            if (Action.PostRule != null)
            {
                if (builder.AtStackLimit)
                {
                    VerboseMsg("Breaking out due to stack limit");

                    // We can never call the post rule so don't bother going through all the loops
                    NextAction();
                    return;
                }
                VerboseMsg($"Invoking rule {Action.PostRule}");
                var newRule = builder.CurrentRules!.PickRule(Action.PostRule);
                builder.StateStack.Push(new State(_builder, newRule, rgbaExecution, mtxExecution));
            }
            else
            {
                VerboseMsg($"Drawing a {Action.Type}");
                builder.Draw(Action.Type, mtxExecution, rgbaExecution);
            }

            // ...and adjust indices/matrices for next step in the loops
            for (var index = cLoops - 1; index >= 0; index--)
            {
                if (++LoopIndices[index] == Action.Loops[index].Reps)
                {
                    // This index loops back to the beginning
                    LoopIndices[index] = 0;

                    // We'll adjust matrices/colors after we've located the advancing index
                    continue;
                }

                VerboseMsg($"Incrementing loop {index}");
                // We've found the index that will be incremented
                fContinue = true;
                // TODO: Use ColorChanged?
                var (prevMatrix, prevRgba, _) =
                    Action.Loops[index].Transform.DoTransform(LoopMatrices[index], LoopRgbas[index]);

                LoopMatrices[index] = prevMatrix;
                LoopRgbas[index] = prevRgba;

                for (var iIndex = index + 1; iIndex < cLoops; iIndex++)
                {
                    (prevMatrix, prevRgba, _) = Action.Loops[iIndex].Transform.DoTransform(prevMatrix, prevRgba);
                    LoopMatrices[iIndex] = prevMatrix;
                    LoopRgbas[iIndex] = prevRgba;
                }

                break;
            }

            if (!fContinue)
            {
                // Done with this action, move on to the next
                NextAction();
            }
        }
        #endregion

        #region Debug
        public override string ToString()
        {
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

        [Conditional("VERBOSE")]
        private void VerboseMsg(string msg)
        {
            var padding = new string(' ', 4 * (_builder.RecurseDepth - 1));

            Console.WriteLine($"{padding}{msg}");
        }
        #endregion
    }
}
