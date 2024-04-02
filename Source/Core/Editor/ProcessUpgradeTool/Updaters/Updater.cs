using System;
using System.Reflection;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class Updater<T> : IUpdater where T : class
    {
        public Type UpdatedType => typeof(T);

        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
