using System;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration.Permissions
{
    public interface IPermissionHandler
    {
        void SetUserPermissions(IUserIdentifier user, IUserPermissions permissions);

        [Obsolete]
        bool UserHasGroup(IUserIdentifier user, IUserGroup group);

        bool UserHasPermission(IUserIdentifier user, string permissionId);
    }
}