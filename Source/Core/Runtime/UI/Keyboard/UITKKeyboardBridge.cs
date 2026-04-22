using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Netcode.UI.Keyboard
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
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
        private List<string> textFieldNames = new List<string> { "ServerIpInput" };

        private readonly Dictionary<TextField, UIToolkitTextFieldAdapter> adapters = new Dictionary<TextField, UIToolkitTextFieldAdapter>();
        private UIDocument uiDocument;
        private IKeyboardBackend runtimeBackend;
        private IKeyboardBackend resolvedBackend;
        private UIToolkitTextFieldAdapter activeAdapter;
        private bool suppressFieldCallbacks;
        private bool backendEventsSubscribed;
        private bool missingBackendWarningShown;
        private bool initialized;
        private Coroutine initializationRoutine;

        public bool EnableSpatialKeyboardBridge
        {
            get => enableSpatialKeyboardBridge;
            set => enableSpatialKeyboardBridge = value;
        }

        public bool CloseKeyboardOnFocusOut
        {
            get => closeKeyboardOnFocusOut;
            set => closeKeyboardOnFocusOut = value;
        }

        public bool CloseKeyboardOnSubmit
        {
            get => closeKeyboardOnSubmit;
            set => closeKeyboardOnSubmit = value;
        }

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
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

            if (!initialized && logWarnings)
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

        public void SetBackendBehaviour(MonoBehaviour backendBehaviour)
        {
            keyboardBackendBehaviour = backendBehaviour;
            SetBackend(backendBehaviour as IKeyboardBackend);
        }

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

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

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
