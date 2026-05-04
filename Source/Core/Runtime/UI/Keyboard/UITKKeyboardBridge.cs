using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// Glue MonoBehaviour that connects UIToolkit <see cref="TextField"/>s on a <see cref="UIDocument"/>
    /// to an <see cref="IKeyboardBackend"/> (typically a spatial XR keyboard).
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>Finds the configured fields in the document (by name) and registers UIToolkit callbacks on them.</item>
    ///   <item>Opens the backend when a field is focused/clicked and syncs field → backend on user edits.</item>
    ///   <item>Listens to backend events and mirrors keyboard → field without re-emitting UIToolkit change events (to avoid feedback loops).</item>
    ///   <item>Handles lifecycle edge cases where the UIDocument's <c>rootVisualElement</c> isn't built yet when this component enables.</item>
    /// </list>
    /// A single bridge can serve multiple fields, but only the most recently interacted-with field is the
    /// active editing target at any time.
    /// </summary>
    [DisallowMultipleComponent]
    public class UITKKeyboardBridge : MonoBehaviour
    {
        // Number of frames to keep retrying EnsureInitialized() when the UIDocument's
        // rootVisualElement is not yet available (e.g., UIDocument.OnEnable hasn't run,
        // or DefaultConnectionUI rebuilds the tree after the bridge's Start).
        private const int InitRetryFrameBudget = 300; // ~5s @ 60 FPS

        [SerializeField]
        private bool enableSpatialKeyboardBridge = true;

        [SerializeField]
        private MonoBehaviour keyboardBackendBehaviour;

        [SerializeField]
        private bool closeKeyboardOnFocusOut = false;

        [SerializeField]
        private bool closeKeyboardOnSubmit = true;

        [SerializeField]
        private bool logWarnings = true;

        [SerializeField]
        private List<string> textFieldNames = new List<string>();

        private UIDocument uiDocument;
        private readonly Dictionary<TextField, UIToolkitTextFieldAdapter> adapters = new Dictionary<TextField, UIToolkitTextFieldAdapter>();
        private IKeyboardBackend runtimeBackend;
        private IKeyboardBackend resolvedBackend;
        private UIToolkitTextFieldAdapter activeAdapter;
        private bool suppressFieldCallbacks;
        private bool backendEventsSubscribed;
        private bool missingBackendWarningShown;
        private bool initialized;
        private Coroutine initializationRoutine;

        /// <summary>
        /// Master switch. When false the bridge registers fields but doesn't open the backend or sync state —
        /// the field behaves like a plain UIToolkit TextField with hardware input only.
        /// </summary>
        public bool EnableSpatialKeyboardBridge
        {
            get => enableSpatialKeyboardBridge;
            set => enableSpatialKeyboardBridge = value;
        }

        /// <summary>
        /// If true, losing focus on a registered field closes the spatial keyboard. If false (default),
        /// the keyboard stays open so the user can keep typing on it after the field loses focus — needed
        /// because clicking the XR keyboard itself momentarily steals focus from the field.
        /// </summary>
        public bool CloseKeyboardOnFocusOut
        {
            get => closeKeyboardOnFocusOut;
            set => closeKeyboardOnFocusOut = value;
        }

        /// <summary>If true (default), the bridge closes the keyboard after the backend reports a submit.</summary>
        public bool CloseKeyboardOnSubmit
        {
            get => closeKeyboardOnSubmit;
            set => closeKeyboardOnSubmit = value;
        }

        /// <summary>The <see cref="UIDocument"/> the bridge is currently driving, or null if none has been resolved yet.</summary>
        public UIDocument TargetUIDocument => uiDocument;

        private void Awake()
        {
            ResolveUIDocument();
            ResolveBackend();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            SubscribeBackendEvents();

            // If the UIDocument hasn't built its rootVisualElement yet, retry on a coroutine
            // instead of per-frame Update — cleaner lifecycle, stops as soon as init succeeds,
            // and self-terminates with a warning if the budget runs out.
            if (!initialized && initializationRoutine == null && isActiveAndEnabled)
            {
                initializationRoutine = StartCoroutine(WaitForInitialization());
            }
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private IEnumerator WaitForInitialization()
        {
            for (int frame = 0; frame < InitRetryFrameBudget && !initialized; frame++)
            {
                yield return null;
                EnsureInitialized();
            }

            initializationRoutine = null;

            if (initialized || !logWarnings)
            {
                yield break;
            }

            if (uiDocument == null)
            {
                Debug.LogWarning("UITKKeyboardBridge: no UIDocument is assigned and none was found in the scene. " +
                    "Assign one in the inspector or add a UIDocument component somewhere in the scene; the spatial keyboard bridge is inactive for this session.", this);
            }
            else
            {
                Debug.LogWarning($"UITKKeyboardBridge: UIDocument.rootVisualElement was not available within {InitRetryFrameBudget} frames; the spatial keyboard bridge is inactive for this session.", this);
            }
        }

        private void OnDisable()
        {
            if (initializationRoutine != null)
            {
                StopCoroutine(initializationRoutine);
                initializationRoutine = null;
            }

            UnsubscribeBackendEvents();
            UnregisterAllFields();
            initialized = false;
            activeAdapter = null;
        }

        /// <summary>
        /// Replace the backend at runtime. Unsubscribes from the previous one and re-subscribes to the
        /// new one on the next <see cref="OnEnable"/> (or immediately if the component is already active).
        /// </summary>
        public void SetBackend(IKeyboardBackend backend)
        {
            UnsubscribeBackendEvents();

            runtimeBackend = backend;
            ResolveBackend();

            if (isActiveAndEnabled)
            {
                SubscribeBackendEvents();
            }
        }

        /// <summary>
        /// Convenience overload for inspector wiring: stores the reference as a <see cref="MonoBehaviour"/>
        /// and forwards to <see cref="SetBackend"/>. Passing something that doesn't implement
        /// <see cref="IKeyboardBackend"/> simply clears the backend.
        /// </summary>
        public void SetBackendBehaviour(MonoBehaviour backendBehaviour)
        {
            keyboardBackendBehaviour = backendBehaviour;
            SetBackend(backendBehaviour as IKeyboardBackend);
        }

        /// <summary>
        /// Re-points the bridge at a different <see cref="UIDocument"/> at runtime. Unregisters TextField
        /// callbacks from the previous document, resets initialization, then re-resolves the configured
        /// <c>textFieldNames</c> against the new document. Idempotent — passing the document the bridge
        /// is already driving is a no-op. Pass <c>null</c> to clear the assignment (the bridge will then
        /// fall back to <c>autoDiscoverUIDocument</c> behavior on the next initialization pass).
        /// </summary>
        public void SetUIDocument(UIDocument document)
        {
            if (uiDocument == document)
            {
                return;
            }

            UnregisterAllFields();
            initialized = false;
            activeAdapter = null;
            uiDocument = document;

            if (initializationRoutine != null)
            {
                StopCoroutine(initializationRoutine);
                initializationRoutine = null;
            }

            EnsureInitialized();

            if (!initialized && isActiveAndEnabled)
            {
                initializationRoutine = StartCoroutine(WaitForInitialization());
            }
        }

        /// <summary>
        /// Adds a field name to the list of TextFields the bridge will look for in the UIDocument.
        /// If the bridge has already initialized the field is looked up and registered immediately;
        /// otherwise the name is remembered and picked up during initialization.
        /// Duplicate or empty names are ignored.
        /// </summary>
        public void RegisterTextFieldName(string textFieldName)
        {
            if (string.IsNullOrWhiteSpace(textFieldName))
            {
                return;
            }

            if (textFieldNames.Contains(textFieldName))
            {
                return;
            }

            textFieldNames.Add(textFieldName);
            if (!initialized)
            {
                return;
            }

            VisualElement root = uiDocument != null ? uiDocument.rootVisualElement : null;
            TextField textField = root?.Q<TextField>(textFieldName);
            if (textField != null)
            {
                RegisterTextField(textField);
            }
        }

        /// <summary>
        /// Registers a <see cref="TextField"/> with the bridge directly (bypassing name lookup).
        /// Hooks up the UIToolkit callbacks (focus/pointer/key/navigation) and makes sure the field
        /// will receive XR pointer events.
        /// </summary>
        /// <returns>True if the field was newly registered; false if it was null or already registered.</returns>
        public bool RegisterTextField(TextField textField)
        {
            Debug.Log("Registering TextField with UITKKeyboardBridge: " + textField.name);
            if (textField == null || adapters.ContainsKey(textField))
            {
                return false;
            }

            var adapter = new UIToolkitTextFieldAdapter(textField);
            adapters[textField] = adapter;

            // Ensure the wrapper receives XR pointer events and that Focus() routes to the inner text input.
            textField.focusable = true;
            textField.delegatesFocus = true;
            if (textField.pickingMode == PickingMode.Ignore)
            {
                textField.pickingMode = PickingMode.Position;
            }

            // PointerDown must use TrickleDown — the inner TextElement otherwise consumes it before the wrapper sees it.
            textField.RegisterCallback<PointerDownEvent>(OnTextFieldPointerDownActivity, TrickleDown.TrickleDown);
            textField.RegisterCallback<FocusInEvent>(OnTextFieldFocusIn);
            textField.RegisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
            textField.RegisterCallback<ChangeEvent<string>>(OnTextFieldValueChanged);
            textField.RegisterCallback<KeyUpEvent>(OnTextFieldInputActivity);
            textField.RegisterCallback<PointerUpEvent>(OnTextFieldPointerUpActivity);
            textField.RegisterCallback<NavigationMoveEvent>(OnTextFieldNavigationActivity);
            return true;
        }

        /// <summary>
        /// Opens the spatial keyboard for the given field, registering it first if necessary.
        /// After this call the field becomes the active editing target.
        /// </summary>
        public void OpenKeyboardForField(TextField textField)
        {
            if (textField == null)
            {
                return;
            }

            if (!adapters.TryGetValue(textField, out UIToolkitTextFieldAdapter adapter))
            {
                if (!RegisterTextField(textField))
                {
                    return;
                }

                adapter = adapters[textField];
            }

            activeAdapter = adapter;
            OpenKeyboardForActiveAdapter();
        }

        /// <summary>
        /// One-shot configuration helper used by bootstrap code. Sets the enabled flag, assigns the
        /// backend behaviour (if provided), registers the field name, and forces an initialization pass
        /// so the field is picked up as soon as the UIDocument is ready.
        /// </summary>
        public void ConfigureFieldAndBackend(string textFieldName, MonoBehaviour backendBehaviour, bool enabled = true)
        {
            enableSpatialKeyboardBridge = enabled;
            if (backendBehaviour != null)
            {
                SetBackendBehaviour(backendBehaviour);
            }

            RegisterTextFieldName(textFieldName);
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            ResolveUIDocument();
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                return;
            }

            ResolveBackend();
            foreach (string fieldName in textFieldNames.Distinct())
            {
                TextField textField = uiDocument.rootVisualElement.Q<TextField>(fieldName);
                if (textField == null)
                {
                    continue;
                }

                RegisterTextField(textField);
            }

            initialized = true;
        }

        private void ResolveUIDocument()
        {
            if (uiDocument != null)
            {
                return;
            }


#if UNITY_2023_1_OR_NEWER
            uiDocument = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
#else
            uiDocument = Object.FindObjectOfType<UIDocument>(includeInactive: true);
#endif
        }

        private void ResolveBackend()
        {
            if (runtimeBackend != null)
            {
                resolvedBackend = runtimeBackend;
                return;
            }

            if (keyboardBackendBehaviour is IKeyboardBackend serializedBackend)
            {
                resolvedBackend = serializedBackend;
                return;
            }

            resolvedBackend = GetComponents<MonoBehaviour>().OfType<IKeyboardBackend>().FirstOrDefault();
        }

        private void OpenKeyboardForActiveAdapter()
        {
            Debug.Log("Attempting to open spatial keyboard for active adapter. Adapter: " + activeAdapter?.TextField.name);
            if (!enableSpatialKeyboardBridge)
            {
                return;
            }

            ResolveBackend();
            if (resolvedBackend == null || !resolvedBackend.IsAvailable)
            {
                if (logWarnings && !missingBackendWarningShown)
                {
                    missingBackendWarningShown = true;
                    Debug.LogWarning("UITKKeyboardBridge could not find an available keyboard backend. Falling back to hardware input only.", this);
                }

                return;
            }

            missingBackendWarningShown = false;
            resolvedBackend.Open(activeAdapter.GetState());
        }

        private void SyncAdapterStateToBackend(UIToolkitTextFieldAdapter adapter)
        {
            if (!enableSpatialKeyboardBridge || suppressFieldCallbacks || adapter == null)
            {
                return;
            }

            ResolveBackend();
            if (resolvedBackend == null || !resolvedBackend.IsOpen)
            {
                return;
            }

            resolvedBackend.SyncState(adapter.GetState());
        }

        private void OnTextFieldFocusIn(FocusInEvent evt)
        {
            if (!enableSpatialKeyboardBridge)
            {
                return;
            }

            TextField textField = ResolveTargetTextField(evt.target);
            if (textField == null || !adapters.TryGetValue(textField, out UIToolkitTextFieldAdapter adapter))
            {
                return;
            }

            activeAdapter = adapter;
            OpenKeyboardForActiveAdapter();
        }

        private void OnTextFieldFocusOut(FocusOutEvent evt)
        {
            TextField textField = ResolveTargetTextField(evt.target);
            if (textField == null || !adapters.TryGetValue(textField, out UIToolkitTextFieldAdapter adapter))
            {
                return;
            }

            SyncAdapterStateToBackend(adapter);
            bool wasActiveAdapter = ReferenceEquals(activeAdapter, adapter);
            if (wasActiveAdapter && closeKeyboardOnFocusOut)
            {
                activeAdapter = null;
            }

            if (!closeKeyboardOnFocusOut)
            {
                if (wasActiveAdapter)
                {
                    // Keep editing session alive while user interacts with the spatial keyboard.
                    activeAdapter = adapter;
                }

                return;
            }

            ResolveBackend();
            resolvedBackend?.Close();
        }

        private void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            if (suppressFieldCallbacks)
            {
                return;
            }

            TextField textField = ResolveTargetTextField(evt.target);
            if (textField == null || !adapters.TryGetValue(textField, out UIToolkitTextFieldAdapter adapter))
            {
                return;
            }

            SyncAdapterStateToBackend(adapter);
        }

        private void OnTextFieldInputActivity(KeyUpEvent evt)
        {
            HandleTextFieldActivityEvent(evt);
        }

        private void OnTextFieldPointerUpActivity(PointerUpEvent evt)
        {
            HandleTextFieldActivityEvent(evt);
        }

        private void OnTextFieldPointerDownActivity(PointerDownEvent evt)
        {
            HandleTextFieldActivityEvent(evt);
        }

        private void OnTextFieldNavigationActivity(NavigationMoveEvent evt)
        {
            HandleTextFieldActivityEvent(evt);
        }

        private void HandleTextFieldActivityEvent(EventBase evt)
        {
            if (suppressFieldCallbacks)
            {
                return;
            }

            TextField textField = ResolveTargetTextField(evt.target);
            if (textField == null || !adapters.TryGetValue(textField, out UIToolkitTextFieldAdapter adapter))
            {
                return;
            }

            bool changedActiveAdapter = !ReferenceEquals(activeAdapter, adapter);
            if (changedActiveAdapter)
            {
                activeAdapter = adapter;
            }

            // On pointer-down, make sure UITK focus routes to the inner text input so FocusInEvent,
            // caret placement, and selection handling behave consistently with desktop input.
            if (evt is PointerDownEvent)
            {
                textField.Focus();
            }

            if (enableSpatialKeyboardBridge)
            {
                Debug.Log("Received TextField activity event: " + evt.GetType().Name);
                ResolveBackend();
                Debug.Log("Resolved backend: " + (resolvedBackend != null ? resolvedBackend.GetType().Name : "null") + ", IsAvailable: " + (resolvedBackend != null ? resolvedBackend.IsAvailable.ToString() : "N/A") + ", IsOpen: " + (resolvedBackend != null ? resolvedBackend.IsOpen.ToString() : "N/A"));
                if (resolvedBackend != null && resolvedBackend.IsAvailable && !resolvedBackend.IsOpen)
                {
                    OpenKeyboardForActiveAdapter();
                }
            }

            SyncAdapterStateToBackend(adapter);
        }

        private static TextField ResolveTargetTextField(IEventHandler target)
        {
            if (target is TextField directField)
            {
                return directField;
            }

            return (target as VisualElement)?.GetFirstAncestorOfType<TextField>();
        }

        private void SubscribeBackendEvents()
        {
            if (backendEventsSubscribed)
            {
                return;
            }

            ResolveBackend();
            if (resolvedBackend == null)
            {
                return;
            }

            resolvedBackend.StateUpdated += OnBackendStateUpdated;
            resolvedBackend.Submitted += OnBackendSubmitted;
            resolvedBackend.Closed += OnBackendClosed;
            backendEventsSubscribed = true;
        }

        private void UnsubscribeBackendEvents()
        {
            if (!backendEventsSubscribed || resolvedBackend == null)
            {
                backendEventsSubscribed = false;
                return;
            }

            resolvedBackend.StateUpdated -= OnBackendStateUpdated;
            resolvedBackend.Submitted -= OnBackendSubmitted;
            resolvedBackend.Closed -= OnBackendClosed;
            backendEventsSubscribed = false;
        }

        private void OnBackendStateUpdated(KeyboardTextState state)
        {
            if (!enableSpatialKeyboardBridge || activeAdapter == null)
            {
                return;
            }

            suppressFieldCallbacks = true;
            activeAdapter.SetState(state, notify: false);
            suppressFieldCallbacks = false;
        }

        private void OnBackendSubmitted(string submittedText)
        {
            if (!enableSpatialKeyboardBridge || activeAdapter == null)
            {
                return;
            }

            KeyboardTextState state = activeAdapter.GetState();
            string submittedValue = submittedText ?? string.Empty;
            int caret = Mathf.Clamp(state.CursorIndex, 0, submittedValue.Length);
            KeyboardTextState committed = KeyboardTextState.FromRaw(submittedValue, caret, caret, state.MaxLength);

            suppressFieldCallbacks = true;
            activeAdapter.SetState(committed, notify: false);
            suppressFieldCallbacks = false;

            if (!closeKeyboardOnSubmit)
            {
                return;
            }

            ResolveBackend();
            resolvedBackend?.Close();
        }

        private void OnBackendClosed()
        {
            activeAdapter = null;
        }

        private void UnregisterAllFields()
        {
            foreach (KeyValuePair<TextField, UIToolkitTextFieldAdapter> pair in adapters)
            {
                TextField textField = pair.Key;
                textField.UnregisterCallback<PointerDownEvent>(OnTextFieldPointerDownActivity, TrickleDown.TrickleDown);
                textField.UnregisterCallback<FocusInEvent>(OnTextFieldFocusIn);
                textField.UnregisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
                textField.UnregisterCallback<ChangeEvent<string>>(OnTextFieldValueChanged);
                textField.UnregisterCallback<KeyUpEvent>(OnTextFieldInputActivity);
                textField.UnregisterCallback<PointerUpEvent>(OnTextFieldPointerUpActivity);
                textField.UnregisterCallback<NavigationMoveEvent>(OnTextFieldNavigationActivity);
            }

            adapters.Clear();
        }
    }
}
