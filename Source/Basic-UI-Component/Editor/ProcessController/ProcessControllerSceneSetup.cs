using VRBuilder.Unity;
using VRBuilder.UX;
using UnityEngine;

namespace VRBuilder.Editor.UX
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
        public override void Setup()
        {
            GameObject processController = SetupPrefab("PROCESS_CONTROLLER");
            if (processController != null)
            {
                processController.GetOrAddComponent<ProcessControllerSetup>().ResetToDefault();
            }
        }
    }
}