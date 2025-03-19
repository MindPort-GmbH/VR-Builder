using System.Collections.Generic;

namespace VRBuilder.Core.Configuration.Permissions
{
    public interface IUserPermissions
    {
        IEnumerable<string> UserGroups { get; }

        void AddUserGroup(string groupId);
    }
}