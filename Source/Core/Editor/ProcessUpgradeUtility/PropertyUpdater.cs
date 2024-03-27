using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public abstract class PropertyUpdater<TNew, TOld> : IUpdater where TNew : class where TOld : class
    {
        public Type SupportedType => typeof(TNew);

        protected abstract bool ShouldBeUpdated(TNew property);

        protected abstract bool PerformUpgrade(TNew newProperty, TOld oldProperty);

        public void Update(MemberInfo memberInfo, object owner)
        {
            TNew propertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;
            if (ShouldBeUpdated(propertyValue) == false)
            {
                return;
            }

            if (AttemptToUpdateProperty(memberInfo, owner))
            {
                Debug.Log($"Successfully updated {memberInfo.Name} to '{propertyValue}' in {owner}");
            }
            else
            {
                Debug.Log($"Failed to update {memberInfo.Name} in {owner}");
            }
        }

        protected bool AttemptToUpdateProperty(MemberInfo memberInfo, object owner)
        {
            // Check if there is a non-null obsolete reference available (e.g. use LegacyProperty attribute).
            MemberInfo legacyPropertyInfo = EditorReflectionUtils.GetAllFieldsAndProperties(owner)
                .FirstOrDefault(property => property.GetCustomAttribute<LegacyPropertyAttribute>() != null
                    && property.GetCustomAttribute<LegacyPropertyAttribute>().NewPropertyName == memberInfo.Name);

            if (legacyPropertyInfo == null)
            {
                return false;
            }

            TOld legacyPropertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, legacyPropertyInfo) as TOld;

            if (legacyPropertyValue == null)
            {
                return false;
            }

            TNew propertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;

            return PerformUpgrade(propertyValue, legacyPropertyValue);
        }
    }
}
