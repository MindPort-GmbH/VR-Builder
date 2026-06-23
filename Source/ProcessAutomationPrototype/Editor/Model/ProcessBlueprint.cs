using System.Collections.Generic;

namespace VRBuilder.ProcessAutomationPrototype.Editor.Model
{
    /// <summary>
    /// In-memory process description produced by the wizard and converted by <see cref="ProcessBlueprintBuilder"/>.
    /// </summary>
    public class ProcessBlueprint
    {
        public string ProcessName = "New Process";
        public List<ChapterBlueprint> Chapters = new List<ChapterBlueprint>();
    }

    public class ChapterBlueprint
    {
        public string Name;
        public List<StepBlueprint> Steps = new List<StepBlueprint>();
    }

    public class StepBlueprint
    {
        public string Name;

        /// <summary>Displayed menu names of the chosen behaviors (e.g. "Animation/Move Object").</summary>
        public List<string> BehaviorPaths = new List<string>();

        public List<TransitionBlueprint> Transitions = new List<TransitionBlueprint>();
    }

    public class TransitionBlueprint
    {
        /// <summary>Displayed menu names of the chosen conditions (e.g. "Interaction/Grab Object").</summary>
        public List<string> ConditionPaths = new List<string>();

        /// <summary>Target step slot. <c>null</c> means the transition ends the chapter.</summary>
        public StepRef? Target;
    }

    /// <summary>Stable reference to a step slot by its position in the blueprint.</summary>
    public struct StepRef
    {
        public int Chapter;
        public int Step;

        public StepRef(int chapter, int step)
        {
            Chapter = chapter;
            Step = step;
        }
    }
}
