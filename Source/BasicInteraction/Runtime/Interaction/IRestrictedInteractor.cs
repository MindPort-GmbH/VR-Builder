using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Interaction
{
    public interface IRestrictedInteractor
    {
        IUserIdentifier OwningUser { get; }
    }
}