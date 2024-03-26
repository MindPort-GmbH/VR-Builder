using System;
using System.Reflection;

namespace VRBuilder.Editor.Utils
{
    public interface IPropertyUpdater
    {
        Type SupportedType { get; }

        void UpdateProperty(MemberInfo memberInfo, object owner);
    }
}
