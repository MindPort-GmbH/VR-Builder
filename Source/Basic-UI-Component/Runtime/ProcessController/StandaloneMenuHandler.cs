using TMPro;
using UnityEngine;
using UnityEngine.XR;
using System;
using System.Collections.Generic;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;

namespace VRBuilder.UX
{
    /// <summary>
    /// Shows and hides the StandaloneProcessController prefab.
    /// </summary>
    public class StandaloneMenuHandler : MonoBehaviour
    {
        [Tooltip("Initial distance between this controller and the user.")] [SerializeField]
        protected float appearanceDistance = 2f;

        [SerializeField, HideInInspector] 
        private string buttonTypeName = "bool";
        
        [SerializeField, HideInInspector] 
        private string buttonName = "MenuButton";

        private Canvas canvas;
        private Type buttonType;
        private Transform user;
        private bool lastMenuState;
        private float defaultPressThreshold = 0.1f;
        private readonly List<InputDevice> controllers = new List<InputDevice>();
        private readonly List<TMP_Dropdown> dropdownsList = new List<TMP_Dropdown>();
        
        private void OnValidate()
        {
            // MenuButton does not exist in OpenVR, so it is switched to PrimaryButton (sandwich button).
            if (Application.isPlaying && buttonName == "MenuButton")
            {
                string deviceName = XRSettings.loadedDeviceName;

                if (string.IsNullOrEmpty(deviceName) == false && deviceName.ToLower().Contains("openvr"))
                {
                    buttonName = "PrimaryButton";
                }
            }

            buttonType = Type.GetType(buttonTypeName);
        }

        private void Start()
        {
            try
            {
                user = RuntimeConfigurator.Configuration.User.GameObject.transform;
                canvas = GetComponentInChildren<Canvas>();
                canvas.worldCamera = Camera.main;

                canvas.enabled = ProcessRunner.Current != null;
            }
            catch (Exception exception)
            {
                Debug.LogError($"{exception.GetType().Name} while initializing {GetType().Name}.\n{exception.StackTrace}", gameObject);
            }

            Vector3 position = user.position + (user.forward * appearanceDistance);
            Quaternion rotation = new Quaternion(0.0f, user.rotation.y, 0.0f, user.rotation.w);
            position.y = 1f;

            transform.SetPositionAndRotation(position, rotation);
            dropdownsList.AddRange(GetComponentsInChildren<TMP_Dropdown>());
            
#if ENABLE_INPUT_SYSTEM && XRIT_0_10_OR_NEWER
            UnityEngine.InputSystem.UI.InputSystemUIInputModule inputSystem = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            if (inputSystem)
            {
                Destroy(inputSystem);
            }
#endif

            OnValidate();
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected += RegisterDevice;

            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevices(devices);

            foreach (InputDevice device in devices)
            {
                RegisterDevice(device);
            }
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= RegisterDevice;
        }

        private void Update()
        {
            if (IsActionButtonPressDown())
            {
                ToggleProcessControllerMenu();
            }
        }

        private void RegisterDevice(InputDevice connectedDevice)
        {
            if (connectedDevice.isValid)
            {
                controllers.Add(connectedDevice);
            }
        }

        private void ToggleProcessControllerMenu()
        {
            canvas.enabled = !canvas.enabled;

            if (canvas.enabled)
            {
                Vector3 position = user.position + (user.forward * appearanceDistance);
                Quaternion rotation = new Quaternion(0.0f, user.rotation.y, 0.0f, user.rotation.w);
                position.y = user.position.y;

                transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                HideDropdowns();
            }
        }

        private void HideDropdowns()
        {
            foreach (TMP_Dropdown dropdown in dropdownsList)
            {
                if (dropdown.IsExpanded)
                {
                    dropdown.Hide();
                }
            }
        }

        private bool IsActionButtonPressDown(float pressThreshold = -1.0f)
        {
            IsActionButtonPress(out bool isButtonPress);
            bool wasPressed = lastMenuState == false && isButtonPress;

            lastMenuState = isButtonPress;

            return wasPressed;
        }

        private bool IsActionButtonPress(out bool isPressed, float pressThreshold = -1.0f)
        {
            foreach (InputDevice controller in controllers)
            {
                if (controller.isValid == false)
                {
                    return isPressed = false;
                }

                if (buttonType == typeof(bool))
                {
                    if (controller.TryGetFeatureValue(new InputFeatureUsage<bool>(buttonName), out bool value))
                    {
                        isPressed = value;
                        return true;
                    }
                }
                else if (buttonType == typeof(float))
                {
                    if (controller.TryGetFeatureValue(new InputFeatureUsage<float>(buttonName), out float value))
                    {
                        float threshold = (pressThreshold >= 0.0f) ? pressThreshold : defaultPressThreshold;
                        isPressed = value >= threshold;
                        return true;
                    }
                }
                else if (buttonType == typeof(Vector2))
                {
                    if (controller.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonName), out Vector2 value))
                    {
                        float threshold = (pressThreshold >= 0.0f) ? pressThreshold : defaultPressThreshold;
                        isPressed = value.x >= threshold;
                        return true;
                    }
                }
            }

            return isPressed = false;
        }
    }
}
