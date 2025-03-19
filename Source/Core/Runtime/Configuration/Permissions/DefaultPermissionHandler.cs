using VRBuilder.Core.SceneObjects;
namespace VRBuilder.Core.Configuration.Permissions
{
    public class DefaultPermissionHandler : IPermissionHandler
    {
        public void SetUserPermissions(IUserIdentifier user, IUserPermissions permissions)
        {
        }

        public bool UserHasGroup(IUserIdentifier user, IUserGroup role)
        {
            return true;
        }
    }
}