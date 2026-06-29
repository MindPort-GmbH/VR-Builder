using UnityEngine;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>Fixes blueprint transition targets before building a VR Builder process.</summary>
    public static class ProcessBlueprintSanitizer
    {
        /// <summary>
        /// Ensures transition targets only reference valid steps in the same chapter.
        /// Cross-chapter targets are cleared because the process graph editor links nodes per chapter.
        /// </summary>
        public static void Sanitize(ProcessBlueprint blueprint)
        {
            if (blueprint?.Chapters == null)
            {
                return;
            }

            for (int chapterIndex = 0; chapterIndex < blueprint.Chapters.Count; chapterIndex++)
            {
                ChapterBlueprint chapter = blueprint.Chapters[chapterIndex];
                if (chapter.Steps == null)
                {
                    continue;
                }

                int stepCount = chapter.Steps.Count;
                for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
                {
                    StepBlueprint step = chapter.Steps[stepIndex];
                    if (step.Transitions == null)
                    {
                        continue;
                    }

                    foreach (TransitionBlueprint transition in step.Transitions)
                    {
                        if (IsValidIntraChapterTarget(transition.Target, chapterIndex, blueprint.Chapters))
                        {
                            continue;
                        }

                        if (transition.Target.HasValue)
                        {
                            StepRef reference = transition.Target.Value;
                            Debug.LogWarning(
                                $"Process Wizard: removed invalid transition target Ch {reference.Chapter + 1} Step {reference.Step + 1} " +
                                $"from Ch {chapterIndex + 1} Step {stepIndex + 1}. The graph editor only links steps within the same chapter.");
                        }

                        transition.Target = null;
                    }
                }
            }
        }

        private static bool IsValidIntraChapterTarget(StepRef? target, int ownerChapterIndex, System.Collections.Generic.List<ChapterBlueprint> chapters)
        {
            if (!target.HasValue)
            {
                return true;
            }

            StepRef reference = target.Value;
            if (reference.Chapter != ownerChapterIndex)
            {
                return false;
            }

            if (reference.Chapter < 0 || reference.Chapter >= chapters.Count)
            {
                return false;
            }

            int targetStepCount = chapters[reference.Chapter].Steps?.Count ?? 0;
            return reference.Step >= 0 && reference.Step < targetStepCount;
        }
    }
}
