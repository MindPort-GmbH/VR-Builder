using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public abstract class PropertyUpdater<TNew, TOld> : IPropertyUpdater where TNew : class where TOld : class
    {
        public Type SupportedType => typeof(TNew);

        protected abstract bool ShouldBeUpdated(TNew property);

        protected abstract bool PerformUpgrade(TNew newProperty, TOld oldProperty);

        public void UpdateProperty(MemberInfo memberInfo, object owner)
        {
            TNew propertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;
            if (ShouldBeUpdated(propertyValue) == false)
            {
                return;
            }

            if (AttemptToUpdateProperty(memberInfo, owner))
            {
                object updatedValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);
                Debug.Log($"Successfully updated {memberInfo.Name} to '{updatedValue}' in {owner}");
            }
            else
            {
                Debug.Log($"Failed to update {memberInfo.Name} in {owner}");
            }
        }

        protected bool AttemptToUpdateProperty(MemberInfo memberInfo, object owner)
        {
            // Check if there is a non-null obsolete reference available (e.g. use LegacyProperty attribute).
            MemberInfo legacyProperty = EditorReflectionUtils.GetAllFieldsAndProperties(owner)
                .FirstOrDefault(property => property.GetCustomAttribute<LegacyPropertyAttribute>() != null
                    && property.GetCustomAttribute<LegacyPropertyAttribute>().NewPropertyName == memberInfo.Name);

            if (legacyProperty == null)
            {
                return false;
            }

            TOld legacyPropertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, legacyProperty) as TOld;

            if (legacyPropertyValue == null)
            {
                return false;
            }

            TNew processSceneReference = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;

            return PerformUpgrade(processSceneReference, legacyPropertyValue);
        }
    }
}
