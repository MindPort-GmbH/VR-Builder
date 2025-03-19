using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VRBuilder.Core.Configuration.Permissions
{
    [Serializable]
    public class UserPermissions : IUserPermissions
    {
        [DataMember]
        public IEnumerable<string> UserGroups { get; protected set; }

        public void AddUserGroup(string groupId)
        {
            if (UserGroups.Contains(groupId) == false)
            {
                UserGroups.Append(groupId);
            }
        }
    }
}