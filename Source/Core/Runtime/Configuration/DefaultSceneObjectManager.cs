using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    public class DefaultSceneObjectManager : ISceneObjectManager
    {
        public void SetSceneObjectActive(SceneObjectReference sceneObject, bool isActive)
        {
            sceneObject.Value.GameObject.SetActive(isActive);
        }
    }
}
