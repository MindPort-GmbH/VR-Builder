using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.Utils
{
    public class EntityDataUpdater : IEntityDataUpdater<IData>
    {
        public IData GetUpdatedData(IData data)
        {
            // Get all properties of type ProcessSceneReferenceBase which are null or empty.
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllFieldsAndProperties(data);

            foreach (MemberInfo property in properties)
            {
                if (property.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)property;

                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(ProcessSceneReferenceBase)))
                    {
                        ProcessSceneReferenceBase processSceneReference = propertyInfo.GetValue(data) as ProcessSceneReferenceBase;
                        if (processSceneReference == null || processSceneReference.IsEmpty())
                        {
                            AttemptToUpdateProperty(propertyInfo, ref data);
                        }
                    }
                }
            }
            return null;
        }

        private void AttemptToUpdateProperty(PropertyInfo propertyInfo, ref IData data)
        {
            // Check if there is a non-null obsolete reference available (e.g. use LegacyProperty attribute).

            // Attempt to find an object with the given unique name in the scene.

            // If found, create a process scene reference of the appropriate type and assign it to the null property.            
        }
    }
}
