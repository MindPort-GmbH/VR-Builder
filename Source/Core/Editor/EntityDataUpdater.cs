using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public class EntityDataUpdater : IEntityDataUpdater<IData>
    {
        public IData GetUpdatedData(IData data)
        {
            // Get all properties of type ProcessSceneReferenceBase which are null or empty.
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllFieldsAndProperties(data);

            // Check if there is a non-null obsolete reference available (e.g. use attributes).

            // Attempt to find an object with the given unique name in the scene.

            // If found, create a process scene reference of the appropriate type and assign it to the null property.            
            return null;
        }
    }
}
