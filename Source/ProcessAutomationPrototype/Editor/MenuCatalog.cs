using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;
using VRBuilder.ProcessAutomationPrototype.Editor.Questions;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>
    /// Exposes VR Builder behavior and condition menu entries for use in the wizard.
    /// </summary>
    public class MenuCatalog
    {
        private readonly Dictionary<string, MenuItem<IBehavior>> behaviorsByPath = new Dictionary<string, MenuItem<IBehavior>>();
        private readonly Dictionary<string, MenuItem<ICondition>> conditionsByPath = new Dictionary<string, MenuItem<ICondition>>();

        /// <summary>Behaviors as menu options whose path is the hierarchical displayed name.</summary>
        public IReadOnlyList<MenuPickOption> BehaviorOptions { get; }

        /// <summary>Conditions as menu options whose path is the hierarchical displayed name.</summary>
        public IReadOnlyList<MenuPickOption> ConditionOptions { get; }

        public MenuCatalog()
        {
            BehaviorOptions = Index(EditorConfigurator.Instance.BehaviorsMenuContent, behaviorsByPath);
            ConditionOptions = Index(EditorConfigurator.Instance.ConditionsMenuContent, conditionsByPath);
        }

        /// <summary>Displayed behavior menu paths.</summary>
        public IEnumerable<string> BehaviorPaths => behaviorsByPath.Keys;

        /// <summary>Displayed condition menu paths.</summary>
        public IEnumerable<string> ConditionPaths => conditionsByPath.Keys;

        public IBehavior CreateBehavior(string displayedName)
        {
            return behaviorsByPath.TryGetValue(displayedName, out MenuItem<IBehavior> item) ? item.GetNewItem() : null;
        }

        public ICondition CreateCondition(string displayedName)
        {
            return conditionsByPath.TryGetValue(displayedName, out MenuItem<ICondition> item) ? item.GetNewItem() : null;
        }

        /// <summary>Returns the canonical behavior path for the given name, or null if unknown.</summary>
        public string ResolveBehavior(string name)
        {
            return Resolve(behaviorsByPath.Keys, name);
        }

        /// <summary>Returns the canonical condition path for the given name, or null if unknown.</summary>
        public string ResolveCondition(string name)
        {
            return Resolve(conditionsByPath.Keys, name);
        }

        private static string Resolve(IEnumerable<string> paths, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (string path in paths)
            {
                if (string.Equals(path, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return null;
        }

        private static IReadOnlyList<MenuPickOption> Index<T>(IEnumerable<MenuOption<T>> options, Dictionary<string, MenuItem<T>> byPath)
        {
            List<MenuPickOption> result = new List<MenuPickOption>();

            foreach (MenuOption<T> option in options)
            {
                if (!(option is MenuItem<T> item) || string.IsNullOrEmpty(item.DisplayedName))
                {
                    continue;
                }

                string path = item.DisplayedName;
                byPath[path] = item;
                result.Add(new MenuPickOption(path, path, path));
            }

            return result
                .OrderBy(option => option.MenuPath)
                .ToList();
        }
    }
}
