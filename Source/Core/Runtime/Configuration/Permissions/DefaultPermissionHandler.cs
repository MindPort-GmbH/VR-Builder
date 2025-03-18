using VRBuilder.Core.SceneObjects;
namespace VRBuilder.Core.Configuration.Permissions
{
    public class DefaultPermissionHandler : IPermissionHandler
    {
        public bool UserHasRole(IUserIdentifier user, IUserRole role)
        {
            return true;
        }
    }
}