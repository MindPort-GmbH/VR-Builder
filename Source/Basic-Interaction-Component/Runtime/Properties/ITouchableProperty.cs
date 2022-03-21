using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ITouchableProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Is object currently touched.
        /// </summary>
        bool IsBeingTouched { get; }
        
        /// <summary>
        /// Instantaneously simulate that the object was touched.
        /// </summary>
        void FastForwardTouch();
    }
}