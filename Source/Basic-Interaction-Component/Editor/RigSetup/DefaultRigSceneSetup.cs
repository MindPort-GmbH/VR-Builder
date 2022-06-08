using UnityEngine;
using System.Collections.Generic;
using System;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Configuration;
using System.Linq;

namespace VRBuilder.Editor.BasicInteraction.RigSetup
{
    /// <summary>
    /// Setups the default rig for the active interaction component.
    /// </summary>
    public class DefaultRigSceneSetup : SceneSetup
    {
        /// <inheritdoc />
        public override int Priority { get; } = 10;

        /// <inheritdoc />
        public override string Key { get; } = "InteractionFrameworkSetup";

        /// <inheritdoc/>
        public override void Setup()
        {
            RemoveMainCamera();

            IEnumerable<Type> interactionComponents = ReflectionUtils.GetConcreteImplementationsOf<IInteractionComponentConfiguration>();

            if(interactionComponents.Count() == 0)
            {
                Debug.LogError("No interaction component is activated in the project, therefore no user rig has been placed in the scene.");
                return;
            }

            IInteractionComponentConfiguration configuration = ReflectionUtils.CreateInstanceOfType(interactionComponents.First()) as IInteractionComponentConfiguration;
            SetupPrefab(configuration.DefaultRigPrefab);

            //InteractionRigSetup setup = Object.FindObjectOfType<InteractionRigSetup>();
            //if (setup == null)
            //{
            //    SetupPrefab("[INTERACTION_RIG_LOADER]");
            //    setup = Object.FindObjectOfType<InteractionRigSetup>();
            //    setup.UpdateRigList();
            //}

            //UserSceneObject user = Object.FindObjectOfType<UserSceneObject>();
            //if (user == null)
            //{
            //    SetupPrefab("[USER]");
            //    setup.DummyUser = GameObject.Find("[USER]");
            //}
        }

        /// <summary>
        /// Removes current MainCamera.
        /// </summary>
        private void RemoveMainCamera()
        {
            if (Camera.main != null && Camera.main.transform.parent == null && Camera.main.gameObject.name != "[USER]")
            {
                UnityEngine.Object.DestroyImmediate(Camera.main.gameObject);
            }
        }
    }
}