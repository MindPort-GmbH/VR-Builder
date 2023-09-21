//using UnityEditor;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
//using VRBuilder.Core.Properties;

//namespace VRBuilder.Editor.XRInteraction
//{
//    /// <summary>
//    /// Custom inspector for <see cref="TeleportationProperty"/>, adding a button to automatically configure <see cref="VRBuilder.XRInteraction.TeleportationAnchor"/>s.
//    /// </summary>
//    [CustomEditor(typeof(TeleportationProperty)), CanEditMultipleObjects]
//    internal class TeleportationPropertyEditor : UnityEditor.Editor
//    {
//        private const string ReticlePrefab = "TeleportReticle";
//        private const string TeleportLayerName = "XR Teleport";
//        private InteractionLayerMask teleportLayer;
//        private LayerMask teleportRaycastLayer;
//        private bool isSetup;

//        private void OnEnable()
//        {
//            TeleportationProperty teleportationProperty = target as TeleportationProperty;
//            TeleportationAnchor teleportAnchor = teleportationProperty.GetComponent<TeleportationAnchor>();

//            if (teleportationProperty.transform.childCount != 0 && teleportAnchor.teleportAnchorTransform.name == AnchorPrefabName)
//            {
//                isSetup = true;
//            }
//        }

//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();
//            EditorGUILayout.Space();

//            ShowConfigurationButton();
//        }

//        private void ShowConfigurationButton()
//        {

//        }

//    }
//}
