using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Object that handles scene object operations, e.g. enabling/disabling them.
    /// </summary>
    public interface ISceneObjectManager
    {
        /// <summary>
        /// Set the specified scene object enabled or disabled.
        /// </summary>
        void SetSceneObjectActive(ISceneObject sceneObject, bool isActive);

        /// <summary>
        /// Sets all components of a given type on a specified scene object enabled or disabled.
        /// </summary>
        void SetComponentActive(ISceneObject sceneObject, string componentTypeName, bool isActive);
    }
}
