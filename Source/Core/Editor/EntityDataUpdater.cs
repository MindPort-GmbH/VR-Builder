using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Editor.Utils
{
    public abstract class EntityDataUpdater<T> : IEntityDataUpdater where T : class, IDataOwner
    {
        public Type SupportedType => typeof(T);

        protected abstract void Update(T dataOwner);

        public void UpdateData(IDataOwner dataOwner)
        {
            T castDataOwner = dataOwner as T;

            if (castDataOwner != null)
            {
                Update(castDataOwner);
            }
            else
            {
                Debug.Log("Invalid data.");
            }
        }

        protected void UpdateProperty(MemberInfo memberInfo, IDataOwner dataOwner)
        {
            string dataOwnerString = dataOwner.GetType().Name;

            if (dataOwner.Data is INamedData)
            {
                INamedData namedData = dataOwner.Data as INamedData;
                dataOwnerString = namedData.Name;
            }

            if (AttemptToUpdateProperty(memberInfo, dataOwner))
            {
                object updatedValue = ReflectionUtils.GetValueFromPropertyOrField(dataOwner.Data, memberInfo);
                Debug.Log($"Successfully updated {memberInfo.Name} to '{updatedValue}' in {dataOwnerString}");
            }
            else
            {
                Debug.Log($"Failed to update {memberInfo.Name} in {dataOwnerString}");
            }
        }

        protected bool AttemptToUpdateProperty(MemberInfo memberInfo, IDataOwner dataOwner)
        {
            // Check if there is a non-null obsolete reference available (e.g. use LegacyProperty attribute).
            MemberInfo legacyProperty = EditorReflectionUtils.GetAllFieldsAndProperties(dataOwner.Data)
                .FirstOrDefault(property => property.GetCustomAttribute<LegacyPropertyAttribute>() != null
                    && property.GetCustomAttribute<LegacyPropertyAttribute>().NewPropertyName == memberInfo.Name);

            if (legacyProperty == null)
            {
                return false;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            UniqueNameReference legacyPropertyValue = ReflectionUtils.GetValueFromPropertyOrField(dataOwner.Data, legacyProperty) as UniqueNameReference;
#pragma warning restore CS0618 // Type or member is obsolete

            if (legacyPropertyValue == null)
            {
                return false;
            }

            // Attempt to find an object with the given unique name in the scene.
#pragma warning disable CS0618 // Type or member is obsolete
            ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == legacyPropertyValue.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            if (referencedObject == null)
            {
                return false;
            }

            ProcessSceneReferenceBase processSceneReference = ReflectionUtils.GetValueFromPropertyOrField(dataOwner.Data, memberInfo) as ProcessSceneReferenceBase;
            processSceneReference.ResetGuids(new List<Guid> { referencedObject.Guid });

            // If found, create a process scene reference of the appropriate type and assign it to the null property.
            return true;
        }
    }
}
