using UnityEngine;
using VRBuilder.ProcessController;
using VRBuilder.Unity;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Will be called on OnSceneSetup to add the process controller menu.
    /// </summary>
    public class ProcessControllerSceneSetup : SceneSetup
    {
        /// <inheritdoc />
        public override int Priority { get; } = 20;

        /// <inheritdoc />
        public override string Key { get; } = "ProcessControllerSetup";

        /// <inheritdoc />
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            GameObject processController = SetupPrefab("PROCESS_CONTROLLER", configuration.ParentObjectsHierarchy);
            if (processController != null)
            {
                ProcessControllerSetup processControllerSetup = processController.GetOrAddComponent<ProcessControllerSetup>();
                processControllerSetup.ResetToDefault();
                processControllerSetup.SetProcessControllerQualifiedName(configuration.DefaultProcessController);
            }
        }
    }
}
