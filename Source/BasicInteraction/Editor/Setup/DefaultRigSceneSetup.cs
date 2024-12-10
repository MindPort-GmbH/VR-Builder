using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.Core.Setup;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Editor.Setup
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
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            RemoveMainCamera();

            IEnumerable<Type> interactionComponents = ReflectionUtils.GetConcreteImplementationsOf<IInteractionComponentConfiguration>();

            if (interactionComponents.Count() == 0)
            {
                Debug.LogError("No interaction component is enabled in the project, therefore no user rig has been placed in the scene. You can enable the default interaction component in the Project Settings.");
                return;
            }

            if (interactionComponents.Count() > 1)
            {
                Debug.LogWarning("Multiple interaction components are enabled in the project. Unable to choose a default rig. Please ensure this is intended and verify the correct user rig has been placed in the scene.");
            }

            IInteractionComponentConfiguration interactionConfiguration = ReflectionUtils.CreateInstanceOfType(interactionComponents.First()) as IInteractionComponentConfiguration;
            GameObject rig = SetupPrefab(interactionConfiguration.DefaultRigPrefab);

            foreach (ILayerConfigurator layerConfigurator in rig.GetComponentsInChildren<ILayerConfigurator>())
            {
                switch (layerConfigurator.LayerSet)
                {
                    case LayerSet.Teleportation:
                        layerConfigurator.ConfigureLayers("Teleport", "Teleport");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Removes current MainCamera.
        /// </summary>
        private void RemoveMainCamera()
        {
            if (Camera.main != null && Camera.main.transform.parent == null && Camera.main.gameObject.name != "USER_DUMMY")
            {
                UnityEngine.Object.DestroyImmediate(Camera.main.gameObject);
            }
        }
    }
}
