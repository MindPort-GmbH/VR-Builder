using System;
using System.Reflection;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters
{
    /// <summary>
    /// Updates the value in a field or property according to a newer standard.
    /// </summary>
    public interface IUpdater
    {
        /// <summary>
        /// Base type compatible with this updater.
        /// </summary>
        Type UpdatedType { get; }

        /// <summary>
        /// Updates the provided field or property.
        /// </summary>
        void Update(MemberInfo memberInfo, object owner);
    }
}
