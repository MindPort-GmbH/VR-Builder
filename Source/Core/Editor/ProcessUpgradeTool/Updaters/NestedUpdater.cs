using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters
{
    /// <summary>
    /// Generic <see cref="IUpdater"/> that iterates through the fields and properties of the provided object and
    /// tries to update them, provided it finds a suitable updater.
    /// </summary>    
    public abstract class NestedUpdater<T> : Updater<T>
    {
        /// <inheritdoc/>        
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
