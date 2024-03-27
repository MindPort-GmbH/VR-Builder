using System;
using System.Reflection;

namespace VRBuilder.Editor.Utils
{
    public abstract class EntityDataUpdater<T> : IUpdater where T : class
    {
        public Type SupportedType => typeof(T);

        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
