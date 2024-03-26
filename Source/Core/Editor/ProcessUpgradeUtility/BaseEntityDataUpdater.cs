using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;
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
                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo);
                    IPropertyUpdater updater = ProcessUpdater.GetPropertyUpdater(type);

                    if (updater != null)
                    {
                        updater.UpdateProperty(memberInfo, dataOwner.Data);
                    }
                }
            }
        }
    }
}
