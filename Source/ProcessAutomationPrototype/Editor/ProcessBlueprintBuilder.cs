using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>
    /// Converts a <see cref="ProcessBlueprint"/> into a real VR Builder <see cref="IProcess"/>,
    /// saves it as the native process asset, and opens it in the Process Editor.
    /// </summary>
    public static class ProcessBlueprintBuilder
    {
        private const float ColumnSpacing = 350f;
        private const float RowSpacing = 250f;
        private const int StepsPerRow = 5;

        /// <summary>
        /// Builds and saves the process. Returns its (possibly auto-renamed) name.
        /// </summary>
        public static string BuildAndSave(ProcessBlueprint blueprint, MenuCatalog catalog)
        {
            ProcessBlueprintSanitizer.Sanitize(blueprint);

            List<List<IStep>> stepsByChapter = new List<List<IStep>>();
            foreach (ChapterBlueprint chapterBlueprint in blueprint.Chapters)
            {
                List<IStep> steps = new List<IStep>();
                for (int s = 0; s < chapterBlueprint.Steps.Count; s++)
                {
                    StepBlueprint stepBlueprint = chapterBlueprint.Steps[s];
                    IStep step = new Step(stepBlueprint.Name);
                    step.StepMetadata.Position = LayoutPosition(s);
                    steps.Add(step);
                }

                stepsByChapter.Add(steps);
            }

            for (int c = 0; c < blueprint.Chapters.Count; c++)
            {
                ChapterBlueprint chapterBlueprint = blueprint.Chapters[c];
                for (int s = 0; s < chapterBlueprint.Steps.Count; s++)
                {
                    StepBlueprint stepBlueprint = chapterBlueprint.Steps[s];
                    IStep step = stepsByChapter[c][s];

                    foreach (string behaviorPath in stepBlueprint.BehaviorPaths)
                    {
                        IBehavior behavior = catalog.CreateBehavior(behaviorPath);
                        if (behavior != null)
                        {
                            step.Data.Behaviors.Data.Behaviors.Add(behavior);
                        }
                    }

                    foreach (TransitionBlueprint transitionBlueprint in stepBlueprint.Transitions)
                    {
                        ITransition transition = new Transition();

                        foreach (string conditionPath in transitionBlueprint.ConditionPaths)
                        {
                            ICondition condition = catalog.CreateCondition(conditionPath);
                            if (condition != null)
                            {
                                transition.Data.Conditions.Add(condition);
                            }
                        }

                        transition.Data.TargetStep = ResolveTarget(transitionBlueprint.Target, stepsByChapter, c);
                        step.Data.Transitions.Data.Transitions.Add(transition);
                    }
                }
            }

            List<IChapter> chapters = new List<IChapter>();
            for (int c = 0; c < blueprint.Chapters.Count; c++)
            {
                List<IStep> steps = stepsByChapter[c];
                IChapter chapter = new Chapter(blueprint.Chapters[c].Name, null);
                chapter.Data.FirstStep = steps.Count > 0 ? steps[0] : null;
                foreach (IStep step in steps)
                {
                    chapter.Data.Steps.Add(step);
                }

                chapter.ChapterMetadata.EntryNodePosition = new Vector2(50f, 100f);
                chapters.Add(chapter);
            }

            IProcess process = new Process(blueprint.ProcessName, chapters);

            ProcessAssetManager.Import(process);
            AssetDatabase.Refresh();

            return process.Data.Name;
        }

        /// <summary>Saves the process and opens it in the Process Editor.</summary>
        public static void BuildSaveAndOpen(ProcessBlueprint blueprint, MenuCatalog catalog)
        {
            string processName = BuildAndSave(blueprint, catalog);
            GlobalEditorHandler.SetCurrentProcess(processName);
            GlobalEditorHandler.StartEditingProcess();
        }

        private static IStep ResolveTarget(StepRef? target, List<List<IStep>> stepsByChapter, int ownerChapterIndex)
        {
            if (target.HasValue == false)
            {
                return null;
            }

            StepRef reference = target.Value;
            if (reference.Chapter != ownerChapterIndex)
            {
                return null;
            }

            if (reference.Chapter < 0 || reference.Chapter >= stepsByChapter.Count)
            {
                return null;
            }

            List<IStep> steps = stepsByChapter[reference.Chapter];
            if (reference.Step < 0 || reference.Step >= steps.Count)
            {
                return null;
            }

            return steps[reference.Step];
        }

        private static Vector2 LayoutPosition(int stepIndex)
        {
            int column = stepIndex % StepsPerRow;
            int row = stepIndex / StepsPerRow;
            return new Vector2(ColumnSpacing + (column * ColumnSpacing), 100f + (row * RowSpacing));
        }
    }
}
