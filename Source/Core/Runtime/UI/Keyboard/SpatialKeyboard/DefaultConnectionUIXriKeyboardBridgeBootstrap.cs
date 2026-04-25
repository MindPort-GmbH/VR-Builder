using UnityEngine;
using VRBuilder.Netcode.UI;

namespace VRBuilder.Netcode.UI.Keyboard
{
    /// <summary>
    /// Runtime auto-installer that wires the XR spatial keyboard onto every <see cref="DefaultConnectionUI"/>
    /// in the active scene. Runs once after scene load so that scenes shipped with only a stock
    /// <c>DefaultConnectionUI</c> automatically get a working spatial keyboard on the <c>ServerIpInput</c>
    /// field without any manual prefab wiring.
    /// <para>
    /// Adds the backend component <i>before</i> the bridge so that <see cref="UITKKeyboardBridge.Awake"/>
    /// can find the backend through its <c>GetComponents&lt;IKeyboardBackend&gt;</c> fallback.
    /// </para>
    /// </summary>
    public static class DefaultConnectionUIXriKeyboardBridgeBootstrap
    {
        private const string ServerIpFieldName = "ServerIpInput";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
#if UNITY_2023_1_OR_NEWER
            DefaultConnectionUI[] connectionUis = Object.FindObjectsByType<DefaultConnectionUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            DefaultConnectionUI[] connectionUis = Object.FindObjectsOfType<DefaultConnectionUI>();
#endif
            foreach (DefaultConnectionUI connectionUi in connectionUis)
            {
                if (connectionUi == null)
                {
                    continue;
                }

                GameObject gameObject = connectionUi.gameObject;

                // Add the backend BEFORE the bridge so that UITKKeyboardBridge.Awake() can resolve it
                // via the GetComponents<IKeyboardBackend>() fallback and subscribe to events correctly.
                XriSpatialKeyboardBackend backend = gameObject.GetComponent<XriSpatialKeyboardBackend>();
                if (backend == null)
                {
                    backend = gameObject.AddComponent<XriSpatialKeyboardBackend>();
                }

                UITKKeyboardBridge bridge = gameObject.GetComponent<UITKKeyboardBridge>();
                if (bridge == null)
                {
                    bridge = gameObject.AddComponent<UITKKeyboardBridge>();
                }

                bridge.ConfigureFieldAndBackend(ServerIpFieldName, backend, enabled: true);
                bridge.CloseKeyboardOnFocusOut = false;
                bridge.CloseKeyboardOnSubmit = true;
            }
        }
    }
}
