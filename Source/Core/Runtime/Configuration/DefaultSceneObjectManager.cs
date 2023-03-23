using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Default single-user implementation of <see cref="ISceneObjectManager"/>.
    /// </summary>
    public class DefaultSceneObjectManager : ISceneObjectManager
    {
        /// <inheritdoc/>
        public void SetSceneObjectActive(ISceneObject sceneObject, bool isActive)
        {
            sceneObject.GameObject.SetActive(isActive);
        }
    }
}
