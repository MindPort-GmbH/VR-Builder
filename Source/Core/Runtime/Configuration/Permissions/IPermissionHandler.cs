using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration.Permissions
{
    public interface IPermissionHandler
    {
        void SetUserPermissions(IUserIdentifier user, IUserPermissions permissions);

        bool UserHasGroup(IUserIdentifier user, IUserGroup group);
    }
}