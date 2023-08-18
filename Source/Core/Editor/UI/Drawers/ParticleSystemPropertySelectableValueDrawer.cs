using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(PropertyReferenceOrTagSelectableValue<IParticleSystemProperty>))]
    public class ParticleSystemPropertySelectableValueDrawer : SelectableValueDrawer<ScenePropertyReference<IParticleSystemProperty>, SceneObjectTag<IParticleSystemProperty>>
    {
    }
}
