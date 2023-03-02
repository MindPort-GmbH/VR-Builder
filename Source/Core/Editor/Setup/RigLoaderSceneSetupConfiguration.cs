using System.Collections.Generic;
using VRBuilder.UX;

namespace VRBuilder.Editor.Setup
{
    public class RigLoaderSceneSetupConfiguration : ISceneSetupConfiguration
    {
        public int Priority => 128;

        public string Name => "Single user (rig loader)";

        public string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

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
