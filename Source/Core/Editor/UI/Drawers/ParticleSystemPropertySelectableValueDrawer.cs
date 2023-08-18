using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Concrete implementation of <see cref="SelectableValue{TFirst, TSecond}"/> for <see cref="IParticleSystemProperty"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(PropertyReferenceOrTagSelectableValue<IParticleSystemProperty>))]
    public class ParticleSystemPropertySelectableValueDrawer : SelectableValueDrawer<ScenePropertyReference<IParticleSystemProperty>, SceneObjectTag<IParticleSystemProperty>>
    {
    }
}
