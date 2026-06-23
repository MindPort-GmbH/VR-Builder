using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>
    /// Builds a process blueprint from plain text using keyword matching.
    /// </summary>
    public static class HeuristicProcessGenerator
    {
        private static readonly (string[] Keywords, string Path)[] ConditionRules =
        {
            (new[] { "grab", "grabs", "pick up", "picks up", "pick", "take", "takes", "hold", "grasp" }, "Interaction/Grab Object"),
            (new[] { "release", "releases", "let go", "drop", "drops", "put down" }, "Interaction/Release Object"),
            (new[] { "press", "presses", "push", "pushes", "poke", "pokes", "button" }, "Interaction/Poke Object"),
            (new[] { "snap", "snaps", "insert", "inserts", "attach", "plug", "place", "places" }, "Interaction/Snap Object"),
            (new[] { "touch", "touches" }, "Interaction/Touch Object"),
            (new[] { "use", "uses", "operate", "turn the", "pull the trigger" }, "Interaction/Use Object"),
            (new[] { "teleport", "walk to", "move to", "go to", "navigate", "stand on" }, "Interaction/Teleport"),
            (new[] { "wait", "waits", "pause", "delay", "seconds", "timeout" }, "Utility/Timeout"),
        };

        private static readonly (string[] Keywords, string Path)[] BehaviorRules =
        {
            (new[] { "highlight", "glow", "blink", "indicate", "point to", "show the" }, "Guidance/Highlight Object"),
            (new[] { "play audio", "play a sound", "narration", "voice", "announce", "speak", "say", "says", "hear" }, "Guidance/Play Audio File"),
            (new[] { "animate", "slide", "move the", "move object" }, "Animation/Move Object"),
            (new[] { "scale", "grow", "shrink", "resize" }, "Animation/Scale Object"),
            (new[] { "enable", "activate", "turn on", "switch on", "spawn", "appear" }, "Utility/Enable Objects"),
            (new[] { "disable", "deactivate", "turn off", "switch off", "hide", "remove" }, "Utility/Disable Objects"),
        };

        public static ProcessBlueprint Generate(MenuCatalog catalog, string request)
        {
            List<string> clauses = SplitIntoClauses(request);

            ChapterBlueprint chapter = new ChapterBlueprint { Name = "Chapter 1" };

            for (int i = 0; i < clauses.Count; i++)
            {
                string clause = clauses[i];
                StepBlueprint step = new StepBlueprint { Name = StepName(clause, i) };

                string behaviorPath = Match(BehaviorRules, clause, catalog.ResolveBehavior);
                if (behaviorPath != null)
                {
                    step.BehaviorPaths.Add(behaviorPath);
                }

                TransitionBlueprint transition = new TransitionBlueprint();
                string conditionPath = Match(ConditionRules, clause, catalog.ResolveCondition);
                if (conditionPath != null)
                {
                    transition.ConditionPaths.Add(conditionPath);
                }

                if (i < clauses.Count - 1)
                {
                    transition.Target = new StepRef(0, i + 1);
                }

                step.Transitions.Add(transition);
                chapter.Steps.Add(step);
            }

            if (chapter.Steps.Count == 0)
            {
                chapter.Steps.Add(new StepBlueprint { Name = "Step 1" });
            }

            return new ProcessBlueprint
            {
                ProcessName = ProcessName(request),
                Chapters = new List<ChapterBlueprint> { chapter },
            };
        }

        private static List<string> SplitIntoClauses(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            string[] parts = Regex.Split(text, @"\b(?:then|next|afterwards?|after that|and then|finally|first|second|third)\b|[\.;,\n\r]+", RegexOptions.IgnoreCase);

            return parts
                .Select(part => part.Trim())
                .Where(part => part.Length >= 3)
                .Take(24)
                .ToList();
        }

        private static string Match((string[] Keywords, string Path)[] rules, string clause, Func<string, string> resolve)
        {
            string lower = clause.ToLowerInvariant();
            foreach ((string[] keywords, string path) in rules)
            {
                if (keywords.Any(keyword => lower.Contains(keyword)))
                {
                    string resolved = resolve(path);
                    if (resolved != null)
                    {
                        return resolved;
                    }
                }
            }

            return null;
        }

        private static string StepName(string clause, int index)
        {
            string trimmed = clause.Trim();
            if (trimmed.Length == 0)
            {
                return $"Step {index + 1}";
            }

            string[] words = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string shortened = string.Join(" ", words.Take(6));
            return Capitalize(shortened);
        }

        private static string ProcessName(string request)
        {
            string[] words = request.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return "New Process";
            }

            return Capitalize(string.Join(" ", words.Take(5)));
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpper(value[0], CultureInfo.InvariantCulture) + value.Substring(1);
        }
    }
}
