using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>Collapsible sidebar listing persisted wizard sessions.</summary>
    public sealed class WizardHistoryPanel
    {
        private readonly VisualElement root;
        private readonly ScrollView list;
        private readonly Label emptyLabel;
        private readonly Button filterAll;
        private readonly Button filterGuided;
        private readonly Button filterPrompt;

        private WizardHistoryFilter activeFilter = WizardHistoryFilter.All;
        private string selectedId;

        private readonly Action<WizardSessionRecord> onSessionSelected;

        public WizardHistoryPanel(Action<WizardSessionRecord> onSessionSelected, Action<WizardHistoryFilter> onFilterChanged)
        {
            this.onSessionSelected = onSessionSelected ?? (_ => { });
            root = new VisualElement();
            root.AddToClassList("pw-history");

            Label heading = new Label("History");
            heading.AddToClassList("pw-history-heading");
            root.Add(heading);

            VisualElement filters = new VisualElement();
            filters.AddToClassList("pw-history-filters");

            filterAll = CreateFilterButton("All", WizardHistoryFilter.All, onFilterChanged);
            filterGuided = CreateFilterButton("Guided", WizardHistoryFilter.Guided, onFilterChanged);
            filterPrompt = CreateFilterButton("Prompt", WizardHistoryFilter.Prompt, onFilterChanged);
            filters.Add(filterAll);
            filters.Add(filterGuided);
            filters.Add(filterPrompt);
            root.Add(filters);

            list = new ScrollView(ScrollViewMode.Vertical);
            list.AddToClassList("pw-history-list");
            root.Add(list);

            emptyLabel = new Label("No saved sessions yet.");
            emptyLabel.AddToClassList("pw-history-empty");
            root.Add(emptyLabel);
        }

        public VisualElement Root => root;

        public void SetCollapsed(bool collapsed)
        {
            root.EnableInClassList("pw-history--collapsed", collapsed);
            root.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetFilter(WizardHistoryFilter filter)
        {
            activeFilter = filter;
            filterAll.EnableInClassList("pw-history-filter--active", filter == WizardHistoryFilter.All);
            filterGuided.EnableInClassList("pw-history-filter--active", filter == WizardHistoryFilter.Guided);
            filterPrompt.EnableInClassList("pw-history-filter--active", filter == WizardHistoryFilter.Prompt);
        }

        public void Refresh(IReadOnlyList<WizardSessionRecord> sessions, WizardHistoryFilter filter, string selectedSessionId)
        {
            activeFilter = filter;
            selectedId = selectedSessionId;
            SetFilter(filter);

            list.Clear();
            IEnumerable<WizardSessionRecord> filtered = sessions ?? Array.Empty<WizardSessionRecord>();
            if (filter == WizardHistoryFilter.Guided)
            {
                filtered = filtered.Where(session => session.Mode == WizardMode.Guided);
            }
            else if (filter == WizardHistoryFilter.Prompt)
            {
                filtered = filtered.Where(session => session.Mode == WizardMode.Prompt);
            }

            List<WizardSessionRecord> items = filtered.ToList();
            emptyLabel.text = GetEmptyMessage(filter);
            emptyLabel.style.display = items.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            foreach (WizardSessionRecord session in items)
            {
                list.Add(BuildItem(session));
            }
        }

        private VisualElement BuildItem(WizardSessionRecord session)
        {
            Button item = new Button(() => onSessionSelected?.Invoke(session));
            item.AddToClassList("pw-history-item");
            item.EnableInClassList("pw-history-item--active", session.Id == selectedId);

            Label title = new Label(WizardSessionArchive.BuildTitle(session));
            title.AddToClassList("pw-history-item-title");
            item.Add(title);

            VisualElement meta = new VisualElement();
            meta.AddToClassList("pw-history-item-meta");

            Label mode = new Label(session.Mode == WizardMode.Guided ? "Guided" : "AI Prompt");
            mode.AddToClassList("pw-history-item-mode");
            meta.Add(mode);

            Label date = new Label(FormatHistoryDate(session.UpdatedAtUtc));
            date.AddToClassList("pw-history-item-date");
            meta.Add(date);

            item.Add(meta);
            return item;
        }

        private static Button CreateFilterButton(string label, WizardHistoryFilter filter, Action<WizardHistoryFilter> onFilterChanged)
        {
            Button button = new Button(() => onFilterChanged?.Invoke(filter)) { text = label };
            button.AddToClassList("pw-history-filter");
            return button;
        }

        private static string GetEmptyMessage(WizardHistoryFilter filter)
        {
            switch (filter)
            {
                case WizardHistoryFilter.Guided:
                    return "No guided sessions saved yet. Complete or start over a guided flow to archive it here.";
                case WizardHistoryFilter.Prompt:
                    return "No AI prompt sessions saved yet. Generate from a prompt or start a new session to save it here.";
                default:
                    return "No saved sessions yet. Finish, start over, or close the wizard to archive a session here.";
            }
        }

        private static string FormatHistoryDate(string isoUtc)
        {
            if (!DateTime.TryParse(isoUtc, null, DateTimeStyles.RoundtripKind, out DateTime utc))
            {
                return string.Empty;
            }

            DateTime local = utc.ToLocalTime();
            if (local.Date == DateTime.Today)
            {
                return local.ToString("HH:mm", CultureInfo.CurrentCulture);
            }

            if (local.Date == DateTime.Today.AddDays(-1))
            {
                return "Yesterday";
            }

            return local.ToString("MMM d", CultureInfo.CurrentCulture);
        }
    }
}
