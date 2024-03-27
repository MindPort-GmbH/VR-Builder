using System;
using System.Reflection;
using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public abstract class EntityDataUpdater<T> : IUpdater where T : class, IData
    {
        public Type SupportedType => typeof(T);

        public abstract void Update(MemberInfo memberInfo, object owner);
    }
}
