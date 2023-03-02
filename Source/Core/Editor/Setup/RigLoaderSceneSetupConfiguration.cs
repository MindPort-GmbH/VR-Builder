using System.Collections.Generic;
using VRBuilder.UX;

namespace VRBuilder.Editor.Setup
{
    public class RigLoaderSceneSetupConfiguration : ISceneSetupConfiguration
    {
        public int Priority => 128;

        public string Name => "Single user - Rig loader";

        public string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        public string Description => "Similar to the default configuration, except there is no rig in the editor scene. The rig is spawned at runtime by" +
            "the INTERACTION_RIG_LOADER object at the DUMMY_USER position. This can be useful for advanced use cases requiring to switch rig at runtime," +
            "but it makes it harder to customize the rig.";

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
