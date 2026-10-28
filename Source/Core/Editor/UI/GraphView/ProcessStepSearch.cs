using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;

namespace VRBuilder.Core.Editor.UI.GraphView
{
    /// <summary>Searches the live authoring model and retains the route to each owning step.</summary>
    public static class ProcessStepSearch
    {
        /// <summary>A matching step and its containing chapter route.</summary>
        public sealed class Result
        {
            /// <summary>The owning step.</summary>
            public IStep Step { get; internal set; }
            /// <summary>Root-to-containing-chapter route.</summary>
            public IChapter[] Chapters { get; internal set; }
            /// <summary>Matching content excerpt.</summary>
            public string Match { get; internal set; }
            /// <summary>Lower values rank first.</summary>
            public int Score { get; internal set; }
            /// <summary>Human-readable location.</summary>
            public string Path => string.Join(" > ", Chapters.Select(chapter => chapter.Data.Name));
        }

        /// <summary>Finds steps using all query words, including nested step groups.</summary>
        public static IEnumerable<Result> Find(IProcess process, string query)
        {
            if (process == null) yield break;
            string[] words = (query ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (IChapter chapter in process.Data.Chapters)
            {
                foreach (Result result in Walk(chapter, new List<IChapter>(), new HashSet<IChapter>()))
                {
                    string name = result.Step.Data.Name ?? string.Empty;
                    string[] fields = Text(result.Step).Where(value => !string.IsNullOrEmpty(value)).ToArray();
                    string content = string.Join(" | ", fields);
                    if (!words.All(word => content.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                    string match = fields.FirstOrDefault(value => words.Any(word => value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)) ?? name;
                    result.Match = match.Length > 180 ? match.Substring(0, 180) + "…" : match;
                    result.Score = words.All(word => name.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) ? 0 : 1;
                    yield return result;
                }
            }
        }

        private static IEnumerable<Result> Walk(IChapter chapter, List<IChapter> route, HashSet<IChapter> ancestors)
        {
            if (chapter == null || !ancestors.Add(chapter)) yield break;
            route.Add(chapter);
            foreach (IStep step in chapter.Data.Steps)
            {
                if (step == null) continue;
                yield return new Result { Step = step, Chapters = route.ToArray() };
                foreach (ExecuteChapterBehavior group in step.Data.Behaviors.Data.Behaviors.OfType<ExecuteChapterBehavior>())
                    foreach (Result child in Walk(group.Data.Chapter, route, ancestors)) yield return child;
                foreach (ExecuteChaptersBehavior parallel in step.Data.Behaviors.Data.Behaviors.OfType<ExecuteChaptersBehavior>())
                    foreach (SubChapter subChapter in parallel.Data.SubChapters)
                        foreach (Result child in Walk(subChapter?.Chapter, route, ancestors)) yield return child;
            }
            route.RemoveAt(route.Count - 1);
            ancestors.Remove(chapter);
        }

        private static IEnumerable<string> Text(IStep step)
        {
            yield return step.Data.Name;
            yield return step.Data.Description;
            foreach (var behavior in step.Data.Behaviors.Data.Behaviors)
            {
                yield return EntityNaming.ResolveTitle(behavior);
                yield return behavior.GetType().Name;
            }
            foreach (var transition in step.Data.Transitions.Data.Transitions)
            {
                yield return EntityNaming.ResolveTitle(transition);
                foreach (var condition in transition.Data.Conditions)
                {
                    yield return EntityNaming.ResolveTitle(condition);
                    yield return condition.GetType().Name;
                }
            }
        }
    }
}
