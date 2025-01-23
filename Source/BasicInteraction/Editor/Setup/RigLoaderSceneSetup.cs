using UnityEngine;
using VRBuilder.BasicInteraction.RigSetup;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Editor.Setup
{
    /// <summary>
    /// Setups the rig loader, cleans up the scene and creates a dummy user. 
    /// </summary>
    public class RigLoaderSceneSetup : SceneSetup
    {
        /// <inheritdoc />
        public override int Priority { get; } = 10;

        /// <inheritdoc />
        public override string Key { get; } = "InteractionFrameworkSetup";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            RemoveMainCamera();

            InteractionRigSetup setup = Object.FindFirstObjectByType<InteractionRigSetup>();
            if (setup == null)
            {
                SetupPrefab("INTERACTION_RIG_LOADER", configuration.ParentObjectsHierarchy);
                setup = Object.FindFirstObjectByType<InteractionRigSetup>();
                setup.UpdateRigList();
            }

            UserSceneObject user = Object.FindFirstObjectByType<UserSceneObject>();
            if (user == null)
            {
                SetupPrefab("USER_DUMMY", configuration.ParentObjectsHierarchy);
                setup.DummyUser = GameObject.Find("USER_DUMMY");
            }
        }

        /// <summary>
        /// Removes current MainCamera.
        /// </summary>
        private void RemoveMainCamera()
        {
            if (Camera.main != null && Camera.main.transform.parent == null && Camera.main.gameObject.name != "USER_DUMMY")
            {
                Object.DestroyImmediate(Camera.main.gameObject);
            }
        }
    }
}