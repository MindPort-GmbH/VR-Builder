using System.Collections.Generic;
using VRBuilder.UX;

namespace VRBuilder.Editor.Setup
{
    public class DefaultSceneSetupConfiguration : ISceneSetupConfiguration
    {
        public int Priority => 256;

        public string Name => "Single user - Default";

        public string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        public string Description => "The default VR Builder scene. This configuration includes the default rig for the active interaction component, " +
            "a single VR Builder process and the default process controller.";

        public IEnumerable<string> GetSetupNames()
        {
            return new string[]
            {
                "VRBuilder.Editor.RuntimeConfigurationSetup",
                "VRBuilder.Editor.BasicInteraction.RigSetup.DefaultRigSceneSetup",
                "VRBuilder.Editor.UX.ProcessControllerSceneSetup",
                "VRBuilder.Editor.XRInteraction.XRInteractionSceneSetup"
            };
        }
    }
}
