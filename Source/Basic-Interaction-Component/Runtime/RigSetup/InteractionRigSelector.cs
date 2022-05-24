using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.RigSetup
{
    public class InteractionRigSelector : MonoBehaviour
    {
        private IEnumerable<GameObject> rigPrefabs;

        [SerializeField, Tooltip("Currently spawned rig")]
        public GameObject SpawnedRig;

        public IEnumerable<GameObject> RigPrefabs
        {
            get
            {
                if (rigPrefabs == null || rigPrefabs.Count() == 0)
                {
                    rigPrefabs = RefreshRigPrefabs();
                }

                return rigPrefabs;
            }
        }

        private IEnumerable<GameObject> RefreshRigPrefabs()
        {
            return AssetDatabase.FindAssets("t: GameObject")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .Where(asset => asset.GetComponentInChildren<UserSceneObject>() != null);

            //return AssetDatabase.FindAssets("l:XRRig")
            //        .Select(AssetDatabase.GUIDToAssetPath)
            //        .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            //        .Where(asset => asset != null);                    
        }
    }
}