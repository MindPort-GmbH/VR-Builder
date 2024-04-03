using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class PropertyUpdater<TNew, TOld> : IUpdater where TNew : class where TOld : class
    {
        public Type UpdatedType => typeof(TNew);

        protected abstract bool ShouldBeUpdated(TNew property);

        protected abstract bool PerformUpgrade(ref TNew newProperty, ref TOld oldProperty);

        public void Update(MemberInfo memberInfo, object owner)
        {
            TNew propertyValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;

            if (ShouldBeUpdated(propertyValue) == false)
            {
                string ownerName = owner is INamedData ? ((INamedData)owner).Name : owner.ToString();
                Debug.Log($"Skipped <i>{memberInfo.Name}</i> in <i>{ownerName}</i>: does not need updating.");
                return;
            }

            if (AttemptToUpdateProperty(memberInfo, owner))
            {
                string ownerName = owner is INamedData ? ((INamedData)owner).Name : owner.ToString();
                TNew updatedValue = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as TNew;
                Debug.Log($"Successfully updated <i>{memberInfo.Name}</i> to <b>{updatedValue}</b> in <i>{ownerName}</i>.");
            }
            else
            {
                string ownerName = owner is INamedData ? ((INamedData)owner).Name : owner.ToString();
                Debug.LogWarning($"Failed to update <i>{memberInfo.Name}</i> in <i>{ownerName}</i>.");
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

            return PerformUpgrade(ref propertyValue, ref legacyPropertyValue);
        }
    }
}
