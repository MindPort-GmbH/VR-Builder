using System;
using System.Reflection;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public interface IUpdater
    {
        Type SupportedType { get; }

        void Update(MemberInfo memberInfo, object owner);
    }
}
