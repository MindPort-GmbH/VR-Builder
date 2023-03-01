using System.Collections.Generic;
using VRBuilder.Editor.Netcode;

namespace VRBuilder.Editor.Setup
{
    public class DefaultSceneSetupConfiguration : ISceneSetupConfiguration
    {
        public int Priority => 256;

        public string Name => "Single user (default rig)";

        public IEnumerable<SceneSetup> GetSceneSetups()
        {
            return new SceneSetup[]
            {
                new RuntimeConfigurationSetup(),
                new DefaultRigSceneSetup(),
                new InteractionFrameworkSceneSetup(),
                new ProcessControllerSceneSetup(),
                new XRInteractionObjectsSceneSetup(),
            };
        }
    }
}
