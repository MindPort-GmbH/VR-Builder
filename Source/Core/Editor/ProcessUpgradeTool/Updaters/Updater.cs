using System;
using System.Reflection;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Generic implementation of <see cref="IUpdater"/>.
    /// </summary>
    public abstract class Updater<T> : IUpdater
    {
        /// <inheritdoc/>        
        public Type UpdatedType => typeof(T);

        /// <inheritdoc/>        
        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
