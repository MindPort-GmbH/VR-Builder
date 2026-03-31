using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Common data contract for color-based highlight behaviors.
    /// </summary>
    /// <typeparam name="TProperty">Referenced property type.</typeparam>
    public interface IColorHighlightBehaviorData<TProperty> : IBehaviorData
        where TProperty : class, ISceneObjectProperty
    {
        /// <summary>
        /// Mode parameter of the configured highlight color.
        /// </summary>
        ModeParameter<Color> CustomColor { get; set; }

        /// <summary>
        /// Highlight color in the step inspector.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Target scene objects.
        /// </summary>
        MultipleScenePropertyReference<TProperty> TargetObjects { get; set; }
    }
}
