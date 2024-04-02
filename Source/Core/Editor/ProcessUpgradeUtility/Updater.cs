using System;
using System.Reflection;

namespace VRBuilder.Editor.ProcessUpdater
{
    public abstract class Updater<T> : IUpdater where T : class
    {
        public Type SupportedType => typeof(T);

        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
