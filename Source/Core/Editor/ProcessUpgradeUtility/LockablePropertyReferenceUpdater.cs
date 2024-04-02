using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.ProcessUpdater
{
    public class LockablePropertyReferenceUpdater : Updater<IEnumerable<LockablePropertyReference>>
    {
        public override void Update(MemberInfo memberInfo, object owner)
        {
            IEnumerable<LockablePropertyReference> lockableProperties = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IEnumerable<LockablePropertyReference>;

            foreach (LockablePropertyReference lockablePropertyReference in lockableProperties)
            {
                IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllDataMembers(lockablePropertyReference);

                foreach (MemberInfo propertyMemberInfo in properties)
                {
                    Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(propertyMemberInfo);
                    IUpdater updater = ProcessUpdater.GetUpdaterForType(type);

                    if (updater != null)
                    {
                        updater.Update(propertyMemberInfo, lockablePropertyReference);
                    }
                }
            }
        }
    }
}
