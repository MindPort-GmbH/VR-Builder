using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class PropertyUpdater<TOld, TNew> : IUpdater where TNew : class where TOld : class
    {
        public Type UpdatedType => typeof(TNew);

        protected abstract bool ShouldBeUpdated(TNew property);

        protected abstract bool PerformUpgrade(TNew newProperty, TOld oldProperty);

        public void Update(MemberInfo memberInfo, object owner)
        {
            TNew propertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;
            string ownerName = owner is INamedData ? ((INamedData)owner).Name : owner.ToString();

            if (ShouldBeUpdated(propertyValue) == false)
            {
                Debug.Log($"Skipped updating {memberInfo.Name} in {ownerName}");
                return;
            }

            if (AttemptToUpdateProperty(memberInfo, owner))
            {
                TNew updatedValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;
                Debug.Log($"Successfully updated {memberInfo.Name} to {updatedValue} in {ownerName}");
            }
            else
            {
                Debug.LogWarning($"Failed to update {memberInfo.Name} in {ownerName}");
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

            if (propertyValue == null)
            {
                propertyValue = ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo)) as TNew;
                ReflectionUtils.SetValueToPropertyOrField(owner, memberInfo, propertyValue);
            }

            return PerformUpgrade(propertyValue, legacyPropertyValue);
        }
    }
}
