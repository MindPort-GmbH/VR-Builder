using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using VRBuilder.Core.Utils;

namespace VRBuilder.UI.Console
{
    /// <summary>
    /// Console implementation for an in-world UI using UI Toolkit rendered in world space.
    /// </summary>
    public class LogConsole : MonoBehaviour, ILogConsole
    {
        [SerializeField]
        private VisualTreeAsset logItemTemplate = null;

        [SerializeField]
        private UIDocument document = null;

        [SerializeField]
        private VRBConsolePlacer placer = null;

        [SerializeField]
        [Tooltip("Objects toggled when the console is shown or hidden. The root object stays active so queued messages keep being processed.")]
        private GameObject[] visuals = new GameObject[0];

        [SerializeField]
        [Tooltip("Scroll speed in pixels per second when scrolling the hovered area with a controller thumbstick.")]
        private float thumbstickScrollSpeed = 1600f;

        private List<LogMessage> logs = new List<LogMessage>();
        private ListView listView;

        private VisualElement detailPane;
        private Label detailIcon;
        private Label detailTitle;
        private Label detailMessage;
        private Label detailStack;

        private ScrollView hoveredScroll;
        private static readonly List<InputDevice> xrControllers = new List<InputDevice>();

        private bool isDirty = false;

        /// <summary>
        /// True if the console window is currently visible.
        /// </summary>
        public bool IsVisible => visuals.Length > 0 && visuals[0].activeSelf;

        /// <summary>
        /// The in-world console only operates in the Editor and development builds. In release
        /// players it stays inert: it never renders and never accumulates logged messages.
        /// </summary>
        private static bool IsEnabled => Debug.isDebugBuild;

        private void Awake()
        {
            if (document == null)
            {
                document = GetComponentInChildren<UIDocument>(true);
            }

            if (placer == null)
            {
                placer = GetComponent<VRBConsolePlacer>();
            }
        }

        private void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                VRBConsole.Refresh();
            }

            ApplyThumbstickScroll();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            logs.Clear();
            CloseDetail();
            RefreshList();
        }

        /// <inheritdoc/>
        public void Show()
        {
            if (IsEnabled == false)
            {
                return;
            }

            if (IsVisible == false)
            {
                placer?.PlaceInFrontOfUser();
            }

            foreach (GameObject visual in visuals)
            {
                visual.SetActive(true);
            }

            BindDocument();
            RefreshList();
        }

        /// <inheritdoc/>
        public void Hide()
        {
            foreach (GameObject visual in visuals)
            {
                visual.SetActive(false);
            }

            listView = null;
            detailPane = null;
        }

        /// <inheritdoc/>
        public void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <inheritdoc/>
        public void LogMessage(string message, string details, LogSeverity severity)
        {
            if (IsEnabled == false)
            {
                return;
            }

            logs.Add(new LogMessage(message, details, severity));
            RefreshList();
        }

        /// <inheritdoc/>
        public void SetDirty()
        {
            isDirty = true;
        }

        private void BindDocument()
        {
            VisualElement root = document != null ? document.rootVisualElement : null;

            if (root == null)
            {
                return;
            }

            listView = root.Q<ListView>("LogList");

            if (listView != null)
            {
                listView.makeItem = () => logItemTemplate.CloneTree();
                listView.bindItem = BindItem;
                listView.itemsSource = logs;
                listView.selectionChanged -= OnLogSelected;
                listView.selectionChanged += OnLogSelected;
            }

            detailPane = root.Q<VisualElement>("DetailPane");
            detailIcon = root.Q<Label>("DetailIcon");
            detailTitle = root.Q<Label>("DetailTitle");
            detailMessage = root.Q<Label>("DetailMessage");
            detailStack = root.Q<Label>("DetailStack");
            CloseDetail();

            hoveredScroll = null;
            RegisterScrollHover(listView?.Q<ScrollView>());
            RegisterScrollHover(root.Q<ScrollView>("DetailScroll"));

            Button closeButton = root.Q<Button>("CloseButton");

            if (closeButton != null)
            {
                closeButton.clicked -= Hide;
                closeButton.clicked += Hide;
            }

            Button clearButton = root.Q<Button>("ClearButton");

            if (clearButton != null)
            {
                clearButton.clicked -= Clear;
                clearButton.clicked += Clear;
            }

            Button detailBack = root.Q<Button>("DetailBack");

            if (detailBack != null)
            {
                detailBack.clicked -= CloseDetail;
                detailBack.clicked += CloseDetail;
            }
        }

        private void BindItem(VisualElement element, int index)
        {
            LogMessage log = logs[index];

            ApplyRowSeverity(element, log.Severity);

            Label message = element.Q<Label>("Message");

            if (message != null)
            {
                SplitMessage(log, out string summary, out _);
                message.text = ToSingleLine(summary);
            }

            Label icon = element.Q<Label>("Icon");

            if (icon != null)
            {
                ApplyIcon(icon, log.Severity);
            }
        }

        private void OnLogSelected(IEnumerable<object> selection)
        {
            if (listView == null)
            {
                return;
            }

            int index = listView.selectedIndex;

            if (index < 0 || index >= logs.Count)
            {
                CloseDetail();
                return;
            }

            ShowDetail(logs[index]);
        }

        private void ShowDetail(LogMessage log)
        {
            if (detailPane == null)
            {
                return;
            }

            SplitMessage(log, out string summary, out string details);

            if (detailMessage != null)
            {
                detailMessage.text = summary;
            }

            if (detailStack != null)
            {
                detailStack.text = details;
                detailStack.style.display = string.IsNullOrEmpty(details) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (detailTitle != null)
            {
                detailTitle.text = log.Severity.ToString();
            }

            if (detailIcon != null)
            {
                ApplyIcon(detailIcon, log.Severity);
            }

            detailPane.RemoveFromClassList("console-detail--hidden");
        }

        private void CloseDetail()
        {
            detailPane?.AddToClassList("console-detail--hidden");

            if (listView != null && listView.selectedIndex >= 0)
            {
                listView.ClearSelection();
            }
        }

        private void RegisterScrollHover(ScrollView scrollView)
        {
            if (scrollView == null)
            {
                return;
            }

            scrollView.UnregisterCallback<PointerMoveEvent>(OnScrollPointerMove);
            scrollView.RegisterCallback<PointerMoveEvent>(OnScrollPointerMove);
            scrollView.UnregisterCallback<PointerLeaveEvent>(OnScrollPointerLeave);
            scrollView.RegisterCallback<PointerLeaveEvent>(OnScrollPointerLeave);
        }

        private void OnScrollPointerMove(PointerMoveEvent evt)
        {
            hoveredScroll = evt.currentTarget as ScrollView;
        }

        private void OnScrollPointerLeave(PointerLeaveEvent evt)
        {
            if (ReferenceEquals(hoveredScroll, evt.currentTarget))
            {
                hoveredScroll = null;
            }
        }

        private void ApplyThumbstickScroll()
        {
            if (IsVisible == false || hoveredScroll == null)
            {
                return;
            }

            float input = ReadThumbstickY();

            if (Mathf.Abs(input) < 0.15f)
            {
                return;
            }

            Vector2 offset = hoveredScroll.scrollOffset;
            offset.y -= input * thumbstickScrollSpeed * Time.deltaTime;
            hoveredScroll.scrollOffset = offset;
        }

        private static float ReadThumbstickY()
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, xrControllers);

            foreach (InputDevice controller in xrControllers)
            {
                if (controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis) && Mathf.Abs(axis.y) > 0.15f)
                {
                    return axis.y;
                }
            }

            return 0f;
        }

        private static void ApplyIcon(Label icon, LogSeverity severity)
        {
            bool isError = severity == LogSeverity.Error || severity == LogSeverity.Exception || severity == LogSeverity.Assert;
            icon.EnableInClassList("console-icon--log", severity == LogSeverity.Log);
            icon.EnableInClassList("console-icon--warning", severity == LogSeverity.Warning);
            icon.EnableInClassList("console-icon--error", isError);
            icon.text = severity == LogSeverity.Warning ? "!" : severity == LogSeverity.Log ? "i" : "✕";
        }

        private static void ApplyRowSeverity(VisualElement row, LogSeverity severity)
        {
            bool isError = severity == LogSeverity.Error || severity == LogSeverity.Exception || severity == LogSeverity.Assert;
            row.EnableInClassList("console-row--log", severity == LogSeverity.Log);
            row.EnableInClassList("console-row--warning", severity == LogSeverity.Warning);
            row.EnableInClassList("console-row--error", isError);
        }

        private static string ToSingleLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Splits a log into a one-line summary and the remaining detail text. Callers sometimes pack a
        /// whole exception (with its stack trace) into the message, so the first line becomes the summary
        /// shown in the row and detail header, and everything after it joins the details for the stack pane.
        /// </summary>
        private static void SplitMessage(LogMessage log, out string summary, out string details)
        {
            string message = log.Message ?? string.Empty;
            int firstBreak = message.IndexOf('\n');

            if (firstBreak >= 0)
            {
                summary = message.Substring(0, firstBreak).TrimEnd();
                string remainder = message.Substring(firstBreak + 1).Trim();
                details = string.IsNullOrEmpty(log.Details) ? remainder : $"{remainder}\n{log.Details}".Trim();
            }
            else
            {
                summary = message;
                details = log.Details ?? string.Empty;
            }
        }

        private void RefreshList()
        {
            if (IsVisible == false || listView == null)
            {
                return;
            }

            listView.RefreshItems();

            if (logs.Count > 0)
            {
                listView.ScrollToItem(-1);
            }
        }
    }
}
