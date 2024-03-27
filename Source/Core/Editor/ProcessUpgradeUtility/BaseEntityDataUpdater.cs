using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public class BaseEntityDataUpdater : EntityDataUpdater<IData>
    {
        public override void Update(MemberInfo memberInfo, object owner)
        {
            object updatedObject = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo);
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllDataMembers(updatedObject);

            foreach (MemberInfo propertyMemberInfo in properties)
            {
                Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(propertyMemberInfo);
                IUpdater updater = ProcessUpdater.GetUpdaterForType(type);

                if (updater != null)
                {
                    updater.Update(propertyMemberInfo, updatedObject);
                }
            }
        }
    }
}
