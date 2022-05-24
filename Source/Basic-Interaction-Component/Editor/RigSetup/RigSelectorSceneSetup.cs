using VRBuilder.BasicInteraction.RigSetup;
using VRBuilder.Core.Properties;
using UnityEngine;

namespace VRBuilder.Editor.BasicInteraction.RigSetup
{
    /// <summary>
    /// Setups the rig selector, cleans up the scene and creates a dummy user. 
    /// </summary>
    public class RigSelectorSceneSetup : SceneSetup
    {
        /// <inheritdoc />
        public override int Priority { get; } = 10;

        /// <inheritdoc />
        public override string Key { get; } = "InteractionFrameworkSetup";

        /// <inheritdoc/>
        public override void Setup()
        {
            RemoveMainCamera();

            InteractionRigSelector setup = Object.FindObjectOfType<InteractionRigSelector>();
            if (setup == null)
            {
                SetupPrefab("[INTERACTION_RIG_SELECTOR]");
                setup = Object.FindObjectOfType<InteractionRigSelector>();
            }

            UserSceneObject user = Object.FindObjectOfType<UserSceneObject>();
            if (user == null)
            {
                SetupPrefab("[USER]");
                setup.SpawnedRig = GameObject.Find("[USER]");
            }
        }

        /// <summary>
        /// Removes current MainCamera.
        /// </summary>
        private void RemoveMainCamera()
        {
            if (Camera.main != null && Camera.main.transform.parent == null && Camera.main.gameObject.name != "[USER]")
            {
                Object.DestroyImmediate(Camera.main.gameObject);
            }
        }
    }
}