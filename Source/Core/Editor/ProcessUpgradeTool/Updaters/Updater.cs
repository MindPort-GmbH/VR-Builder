using System;
using System.Reflection;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class Updater<T> : IUpdater
    {
        public Type UpdatedType => typeof(T);

        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
