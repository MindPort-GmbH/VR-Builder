using System.Collections.Generic;

namespace VRBuilder.Editor.Setup
{
    public class RigLoaderSceneSetupConfiguration : ISceneSetupConfiguration
    {
        public int Priority => 128;

        public string Name => "Single user (rig loader)";

        public IEnumerable<string> GetSetupNames()
        {
            return new string[]
            {
                "VRBuilder.Editor.RuntimeConfigurationSetup",
                "VRBuilder.Editor.BasicInteraction.RigSetup.RigLoaderSceneSetup",
                "VRBuilder.Editor.UX.ProcessControllerSceneSetup",
                "VRBuilder.Editor.XRInteraction.XRInteractionSceneSetup"
            };
        }
    }
}
