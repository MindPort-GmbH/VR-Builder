using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using VRBuilder.Core.Editor.UI.GraphView.Windows;

namespace VRBuilder.Core.Editor.UI.GraphView
{
    /// <summary>Integrates live process steps with Unity Search.</summary>
    internal static class ProcessStepSearchProvider
    {
        private const string ProviderId = "vrbuilder-steps";

        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            var provider = new SearchProvider(ProviderId, "VR Builder Steps")
            {
                filterId = "vrb:",
                isExplicitProvider = true,
                fetchItems = FetchItems
            };
            provider.actions.Add(new SearchAction(ProviderId, "open", null, "Open step") { handler = Open, enabled = items => items.Count == 1 });
            return provider;
        }

        [MenuItem("Tools/VR Builder/Search Process...", false, 16)]
        [MenuItem("Window/VR Builder/Search Process", false, 16)]
        internal static void Show()
        {
            if (!CanSearch()) return;
            SearchService.ShowWindow(SearchService.CreateContext(new[] { CreateProvider() }, string.Empty),
                topic: "Process steps", multiselect: false);
        }

        [MenuItem("Tools/VR Builder/Search Process...", true, 16)]
        [MenuItem("Window/VR Builder/Search Process", true, 16)]
        private static bool CanSearch()
        {
            return GlobalEditorHandler.GetCurrentProcess() != null;
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, List<SearchItem> items, SearchProvider provider)
        {
            IProcess process = GlobalEditorHandler.GetCurrentProcess();
            foreach (var result in ProcessStepSearch.Find(process, context.searchQuery))
                yield return provider.CreateItem(context, result.Step.Id.ToString(), result.Score,
                    result.Step.Data.Name, result.Path + " — " + result.Match, null, new Selection { Process = process, Step = result.Step });
        }

        private static void Open(SearchItem item)
        {
            if (item.data is Selection selection)
                ProcessGraphViewWindow.NavigateToStep(selection.Process, selection.Step);
        }

        private sealed class Selection
        {
            internal IProcess Process;
            internal IStep Step;
        }
    }
}
