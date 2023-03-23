using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    public interface ISceneObjectManager
    {
        void SetSceneObjectActive(SceneObjectReference sceneObject, bool isActive);
    }
}
