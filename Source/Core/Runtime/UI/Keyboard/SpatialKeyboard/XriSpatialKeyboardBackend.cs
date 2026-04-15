using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

namespace VRBuilder.Netcode.UI.Keyboard
{
    [DisallowMultipleComponent]
    public class XriSpatialKeyboardBackend : MonoBehaviour, IKeyboardBackend
    {
        private static readonly FieldInfo CaretFieldInfo = typeof(XRKeyboard).GetField("m_CaretPosition", BindingFlags.Instance | BindingFlags.NonPublic);

        [SerializeField]
        private XRKeyboard keyboard;

        [SerializeField]
        private bool autoFindKeyboard = true;

        [SerializeField]
        private bool preferGlobalKeyboardManager = true;

        [SerializeField]
        private bool logWarnings = true;

        [SerializeField]
        private bool repositionWithoutGlobalManager = true;

        [SerializeField]
        private Vector3 fallbackKeyboardOffset = new Vector3(0f, -0.3f, 0.55f);

        private GlobalNonNativeKeyboard globalKeyboardManager;
        private bool subscribedToKeyboardEvents;
        private bool missingKeyboardWarningShown;
        private KeyboardTextState currentState = KeyboardTextState.FromRaw(string.Empty, 0, 0);

        public bool IsAvailable => ResolveKeyboard(false) != null;
        public bool IsOpen => keyboard != null && keyboard.isOpen;

        public event Action<KeyboardTextState> StateUpdated;
        public event Action<string> Submitted;
        public event Action Closed;

        private void Awake()
        {
            ResolveKeyboard(false);
        }

        private void OnEnable()
        {
            EnsureKeyboardEventSubscription();
        }

        private void OnDisable()
        {
            UnsubscribeKeyboardEvents();
        }

        public void Open(KeyboardTextState state)
        {
            currentState = state.Normalized();

            XRKeyboard resolvedKeyboard = ResolveKeyboard();
            if (resolvedKeyboard == null)
            {
                return;
            }

            EnsureKeyboardEventSubscription();

            ResolveGlobalKeyboardManager();
            if (globalKeyboardManager != null && globalKeyboardManager.keyboard != null)
            {
                keyboard = globalKeyboardManager.keyboard;
                globalKeyboardManager.ShowKeyboard(currentState.Text);
                resolvedKeyboard = keyboard;
            }
            else
            {
                resolvedKeyboard.Open(currentState.Text);
                if (repositionWithoutGlobalManager)
                {
                    PositionKeyboardFallback(resolvedKeyboard);
                }
            }

            SetKeyboardCaret(resolvedKeyboard, currentState.CursorIndex);
        }

        public void SyncState(KeyboardTextState state)
        {
            currentState = state.Normalized();

            XRKeyboard resolvedKeyboard = ResolveKeyboard(false);
            if (resolvedKeyboard == null || !resolvedKeyboard.isOpen)
            {
                return;
            }

            ApplyStateToKeyboard(resolvedKeyboard, currentState);
        }

        public void Close()
        {
            XRKeyboard resolvedKeyboard = ResolveKeyboard(false);
            if (resolvedKeyboard == null)
            {
                return;
            }

            if (globalKeyboardManager != null)
            {
                globalKeyboardManager.HideKeyboard();
                return;
            }

            if (resolvedKeyboard.isOpen)
            {
                resolvedKeyboard.Close();
            }
        }

        private XRKeyboard ResolveKeyboard(bool showWarningIfMissing = true)
        {
            ResolveGlobalKeyboardManager();
            if (globalKeyboardManager != null && globalKeyboardManager.keyboard != null)
            {
                keyboard = globalKeyboardManager.keyboard;
                return keyboard;
            }

            if (keyboard != null)
            {
                return keyboard;
            }

            if (autoFindKeyboard)
            {
#if UNITY_2023_1_OR_NEWER
                keyboard = FindFirstObjectByType<XRKeyboard>(FindObjectsInactive.Include);
#else
                keyboard = FindObjectOfType<XRKeyboard>(true);
#endif
            }

            if (keyboard == null && showWarningIfMissing && logWarnings && !missingKeyboardWarningShown)
            {
                missingKeyboardWarningShown = true;
                Debug.LogWarning("XriSpatialKeyboardBackend could not find an XRKeyboard. UI Toolkit input will use hardware input only.", this);
            }

            return keyboard;
        }

        private void ResolveGlobalKeyboardManager()
        {
            if (!preferGlobalKeyboardManager)
            {
                return;
            }

            if (globalKeyboardManager != null)
            {
                return;
            }

            globalKeyboardManager = GlobalNonNativeKeyboard.instance ?? FindFirstObjectByType<GlobalNonNativeKeyboard>();
        }

        private void EnsureKeyboardEventSubscription()
        {
            XRKeyboard resolvedKeyboard = ResolveKeyboard(false);
            if (resolvedKeyboard == null || subscribedToKeyboardEvents)
            {
                return;
            }

            resolvedKeyboard.onKeyPressed.AddListener(OnKeyboardKeyPressed);
            resolvedKeyboard.onTextSubmitted.AddListener(OnKeyboardTextSubmitted);
            resolvedKeyboard.onClosed.AddListener(OnKeyboardClosed);
            subscribedToKeyboardEvents = true;
        }

        private void UnsubscribeKeyboardEvents()
        {
            if (!subscribedToKeyboardEvents || keyboard == null)
            {
                subscribedToKeyboardEvents = false;
                return;
            }

            keyboard.onKeyPressed.RemoveListener(OnKeyboardKeyPressed);
            keyboard.onTextSubmitted.RemoveListener(OnKeyboardTextSubmitted);
            keyboard.onClosed.RemoveListener(OnKeyboardClosed);
            subscribedToKeyboardEvents = false;
        }

        private void OnKeyboardKeyPressed(KeyboardKeyEventArgs args)
        {
            if (args?.key == null || keyboard == null)
            {
                return;
            }

            KeyboardEditCommand command = MapKeyToCommand(args.key);
            switch (command.Action)
            {
                case KeyboardEditAction.None:
                    return;
                case KeyboardEditAction.Submit:
                    return;
                case KeyboardEditAction.Close:
                    Close();
                    return;
            }

            KeyboardTextState next = KeyboardTextEditing.Apply(currentState, command);
            if (next.Equals(currentState))
            {
                return;
            }

            currentState = next;
            ApplyStateToKeyboard(keyboard, currentState);
            StateUpdated?.Invoke(currentState);
        }

        private void OnKeyboardTextSubmitted(KeyboardTextEventArgs args)
        {
            string submittedText = args?.keyboardText ?? currentState.Text;
            int caret = keyboard != null ? Mathf.Clamp(keyboard.caretPosition, 0, submittedText.Length) : submittedText.Length;
            currentState = KeyboardTextState.FromRaw(submittedText, caret, caret, currentState.MaxLength);

            StateUpdated?.Invoke(currentState);
            Submitted?.Invoke(currentState.Text);
        }

        private void OnKeyboardClosed(KeyboardTextEventArgs _)
        {
            Closed?.Invoke();
        }

        private void ApplyStateToKeyboard(XRKeyboard targetKeyboard, KeyboardTextState state)
        {
            if (targetKeyboard == null)
            {
                return;
            }

            if (!targetKeyboard.isOpen || targetKeyboard.text != state.Text)
            {
                targetKeyboard.Open(state.Text);
            }

            SetKeyboardCaret(targetKeyboard, state.CursorIndex);
        }

        private static void SetKeyboardCaret(XRKeyboard targetKeyboard, int caret)
        {
            if (targetKeyboard == null || CaretFieldInfo == null)
            {
                return;
            }

            int clampedCaret = Mathf.Clamp(caret, 0, targetKeyboard.text.Length);
            CaretFieldInfo.SetValue(targetKeyboard, clampedCaret);
        }

        private void PositionKeyboardFallback(XRKeyboard targetKeyboard)
        {
            if (targetKeyboard == null)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Transform cameraTransform = mainCamera.transform;
            Vector3 worldOffset = cameraTransform.right * fallbackKeyboardOffset.x +
                                  Vector3.up * fallbackKeyboardOffset.y +
                                  cameraTransform.forward * fallbackKeyboardOffset.z;
            targetKeyboard.transform.position = cameraTransform.position + worldOffset;
            Vector3 toCamera = (cameraTransform.position - targetKeyboard.transform.position).normalized;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                targetKeyboard.transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
            }
        }

        private KeyboardEditCommand MapKeyToCommand(XRKeyboardKey key)
        {
            if (key == null)
            {
                return KeyboardEditCommand.None;
            }

            switch (key.keyCode)
            {
                case KeyCode.Backspace:
                    return KeyboardEditCommand.Backspace;
                case KeyCode.Delete:
                    return KeyboardEditCommand.Delete;
                case KeyCode.Clear:
                    return KeyboardEditCommand.Clear;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    return keyboard != null && keyboard.submitOnEnter ? KeyboardEditCommand.Submit : KeyboardEditCommand.Insert("\n");
                case KeyCode.Space:
                    return KeyboardEditCommand.Insert(" ");
            }

            string effectiveCharacter = key.GetEffectiveCharacter();
            if (string.IsNullOrEmpty(effectiveCharacter))
            {
                return KeyboardEditCommand.None;
            }

            switch (effectiveCharacter)
            {
                case "\\b":
                    return KeyboardEditCommand.Backspace;
                case "\\cl":
                    return KeyboardEditCommand.Clear;
                case "\\h":
                    return KeyboardEditCommand.Close;
                case "\\r":
                    return keyboard != null && keyboard.submitOnEnter ? KeyboardEditCommand.Submit : KeyboardEditCommand.Insert("\n");
                case "\\s":
                case "\\caps":
                case "\\c":
                    return KeyboardEditCommand.None;
                default:
                    return effectiveCharacter.StartsWith("\\", StringComparison.Ordinal) ? KeyboardEditCommand.None : KeyboardEditCommand.Insert(effectiveCharacter);
            }
        }
    }
}
