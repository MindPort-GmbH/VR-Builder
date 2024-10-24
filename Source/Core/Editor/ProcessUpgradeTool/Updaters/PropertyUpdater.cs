using System;
using System.Linq;
using System.Reflection;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters
{
    /// <summary>
    /// Generic implementation of <see cref="IUpdater"/> specific for properties that supersede obsolete properties of a different type.
    /// </summary>
    /// <typeparam name="TNew">Type of the property whose value has to be updated.</typeparam>
    /// <typeparam name="TOld">Type of the obsolete property which should be read to get the value.</typeparam>
    public abstract class PropertyUpdater<TNew, TOld> : IUpdater
    {
        /// <inheritdoc/>        
        public Type UpdatedType => typeof(TNew);

        /// <summary>
        /// True if it is necessary to update the provided property.
        /// </summary>
        protected abstract bool ShouldBeUpdated(TNew property);

        /// <summary>
        /// Reads the value from the old property and assigns it to the new property.
        /// </summary>
        protected abstract bool PerformUpgrade(ref TNew newProperty, ref TOld oldProperty);

        /// <inheritdoc/>        
        public void Update(MemberInfo memberInfo, object owner)
        {
            TNew propertyValue = (TNew)ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);
            string ownerName = owner is INamedData ? ((INamedData)owner).Name : owner.ToString();

            if (ShouldBeUpdated(propertyValue) == false)
            {
                UnityEngine.Debug.Log($"Skipped <i>{memberInfo.Name}</i> in <i>{ownerName}</i>: does not need updating.");
                return;
            }

            if (AttemptToUpdateProperty(memberInfo, owner))
            {
                TNew updatedValue = (TNew)ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);
                UnityEngine.Debug.Log($"Successfully updated <i>{memberInfo.Name}</i> to <b>{updatedValue}</b> in <i>{ownerName}</i>.");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Failed to update <i>{memberInfo.Name}</i> in <i>{ownerName}</i>.");
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

            TOld legacyPropertyValue = (TOld)ReflectionUtils.GetValueFromPropertyOrField(owner, legacyPropertyInfo);

            if (legacyPropertyValue == null)
            {
                return true;
            }

            TNew propertyValue = (TNew)ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);

            if (propertyValue == null)
            {
                propertyValue = (TNew)ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo));
                ReflectionUtils.SetValueToPropertyOrField(owner, memberInfo, propertyValue);
            }

            return PerformUpgrade(ref propertyValue, ref legacyPropertyValue);
        }
    }
}
