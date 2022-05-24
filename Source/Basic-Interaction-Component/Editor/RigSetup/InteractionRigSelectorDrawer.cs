using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.BasicInteraction.RigSetup;

namespace VRBuilder.Editor.BasicInteraction.RigSetup
{

    [CustomEditor(typeof(InteractionRigSelector))]
    public class InteractionRigSelectorDrawer : UnityEditor.Editor
    {
        private List<GameObject> rigPrefabs = new List<GameObject>();

        int selectedIndex = 0;

        private InteractionRigSelector rigSetup;

        private void OnEnable()
        {
            rigSetup = (InteractionRigSelector)target;

            if (Application.isPlaying == false)
            {
                rigPrefabs = rigSetup.RigPrefabs.ToList();

                foreach (GameObject prefab in rigPrefabs)
                {
                    Debug.Log(prefab.name);
                }
            }

        }

        public override void OnInspectorGUI()
        {
            selectedIndex = EditorGUILayout.Popup(selectedIndex, rigPrefabs.Select(prefab => prefab.name).ToArray());

            if (GUILayout.Button("Load in scene"))
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;

                if (rigSetup.SpawnedRig != null)
                {
                    position = rigSetup.SpawnedRig.transform.position;
                    rotation = rigSetup.SpawnedRig.transform.rotation;

                    DestroyImmediate(rigSetup.SpawnedRig);
                }

                rigSetup.SpawnedRig = Instantiate(rigPrefabs[selectedIndex]);
                rigSetup.SpawnedRig.transform.position = position;
                rigSetup.SpawnedRig.transform.rotation = rotation;
                rigSetup.SpawnedRig.name = rigSetup.SpawnedRig.name.Replace("(Clone)", string.Empty);
            }

            base.OnInspectorGUI();
        }
    }
}