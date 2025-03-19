namespace VRBuilder.Core.Configuration.Permissions
{
    /// <summary>
    /// A configurable user group.
    /// </summary>
    public interface IUserGroup
    {
        string Name { get; }

        string Id { get; }

        void SetPermission(string id, bool value);

        bool HasPermission(string id);
    }
}