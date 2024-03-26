using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public class BaseEntityDataUpdater : EntityDataUpdater<IDataOwner>
    {
        protected override void Update(IDataOwner dataOwner)
        {
            // Get all properties of type ProcessSceneReferenceBase which are null or empty.
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllFieldsAndProperties(dataOwner.Data);

            foreach (MemberInfo memberInfo in properties)
            {
                Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo);
                if (type.IsSubclassOf(typeof(ProcessSceneReferenceBase)))
                {
                    ProcessSceneReferenceBase processSceneReference = ReflectionUtils.GetValueFromPropertyOrField(dataOwner.Data, memberInfo) as ProcessSceneReferenceBase;
                    if (processSceneReference == null || processSceneReference.IsEmpty())
                    {
                        UpdateProperty(memberInfo, dataOwner);
                    }
                }
            }
        }
    }
}
