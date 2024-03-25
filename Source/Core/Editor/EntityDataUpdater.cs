using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.Utils
{
    public class EntityDataUpdater : IEntityDataUpdater<IDataOwner>
    {
        public IDataOwner GetUpdatedData(IDataOwner dataOwner)
        {
            // Get all properties of type ProcessSceneReferenceBase which are null or empty.
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllFieldsAndProperties(dataOwner.Data);

            foreach (MemberInfo property in properties)
            {
                if (property.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)property;

                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(ProcessSceneReferenceBase)))
                    {
                        ProcessSceneReferenceBase processSceneReference = propertyInfo.GetValue(dataOwner.Data) as ProcessSceneReferenceBase;
                        if (processSceneReference == null || processSceneReference.IsEmpty())
                        {
                            AttemptToUpdateProperty(propertyInfo, dataOwner);
                        }
                    }
                }
            }
            return dataOwner;
        }

        private bool AttemptToUpdateProperty(PropertyInfo propertyInfo, IDataOwner dataOwner)
        {
            // Check if there is a non-null obsolete reference available (e.g. use LegacyProperty attribute).
            MemberInfo legacyProperty = EditorReflectionUtils.GetAllFieldsAndProperties(dataOwner.Data)
                .FirstOrDefault(property => property.GetCustomAttribute<LegacyPropertyAttribute>() != null
                    && property.GetCustomAttribute<LegacyPropertyAttribute>().NewPropertyName == propertyInfo.Name);

            if (legacyProperty == null)
            {
                return false;
            }

            PropertyInfo legacyPropertyInfo = legacyProperty as PropertyInfo;

            if (legacyPropertyInfo == null)
            {
                return false;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            UniqueNameReference legacyPropertyValue = legacyPropertyInfo.GetValue(dataOwner.Data) as UniqueNameReference;
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

            ProcessSceneReferenceBase processSceneReference = propertyInfo.GetValue(dataOwner.Data) as ProcessSceneReferenceBase;
            processSceneReference.AddGuid(referencedObject.Guid);

            // If found, create a process scene reference of the appropriate type and assign it to the null property.
            return true;
        }
    }
}
