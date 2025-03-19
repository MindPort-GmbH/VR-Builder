using System;

namespace VRBuilder.Core.Configuration.Permissions
{
    [Serializable]
    public class UserGroup : IUserGroup
    {
        public string Name { get; private set; }

        public string Id { get; private set; }

        public bool HasPermission(string id)
        {
            throw new NotImplementedException();
        }

        public void SetPermission(string id, bool value)
        {
            throw new NotImplementedException();
        }
    }
}