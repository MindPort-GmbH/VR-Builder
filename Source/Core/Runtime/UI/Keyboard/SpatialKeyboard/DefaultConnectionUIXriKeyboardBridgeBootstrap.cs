using UnityEngine;
using VRBuilder.Netcode.UI;

namespace VRBuilder.Netcode.UI.Keyboard
{
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
                UITKKeyboardBridge bridge = gameObject.GetComponent<UITKKeyboardBridge>();
                if (bridge == null)
                {
                    bridge = gameObject.AddComponent<UITKKeyboardBridge>();
                }

                XriSpatialKeyboardBackend backend = gameObject.GetComponent<XriSpatialKeyboardBackend>();
                if (backend == null)
                {
                    backend = gameObject.AddComponent<XriSpatialKeyboardBackend>();
                }

                bridge.ConfigureFieldAndBackend(ServerIpFieldName, backend, enabled: true);
                bridge.CloseKeyboardOnFocusOut = false;
                bridge.CloseKeyboardOnSubmit = true;
            }
        }
    }
}
