using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration.Permissions
{
    public interface IPermissionHandler
    {
        bool UserHasRole(IUserIdentifier user, IUserRole role);
    }
}