using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class NestedUpdater<T> : Updater<T>
    {
        public override void Update(MemberInfo memberInfo, object owner)
        {
            object updatedObject = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllDataMembers(updatedObject);

            foreach (MemberInfo propertyMemberInfo in properties)
            {
                Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(propertyMemberInfo);
                IUpdater updater = ProcessUpgradeTool.GetUpdaterForType(type);

                if (updater != null)
                {
                    updater.Update(propertyMemberInfo, updatedObject);
                }
            }

            if (memberInfo is PropertyInfo propertyInfo && propertyInfo.GetSetMethod() != null)
            {
                ReflectionUtils.SetValueToPropertyOrField(owner, memberInfo, updatedObject);
            }
        }
    }
}
