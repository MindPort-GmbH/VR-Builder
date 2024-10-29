using System;
using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters
{
    /// <summary>
    /// Iterates through a collection of <see cref="LockablePropertyReference"/>s and updates them if a suitable updater exists.
    /// </summary>
    public class LockablePropertyReferenceUpdater : Updater<IEnumerable<LockablePropertyReference>>
    {
        /// <inheritdoc/>
        public override void Update(MemberInfo memberInfo, object owner)
        {
            IEnumerable<LockablePropertyReference> lockableProperties = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IEnumerable<LockablePropertyReference>;

            foreach (LockablePropertyReference lockablePropertyReference in lockableProperties)
            {
                IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllDataMembers(lockablePropertyReference);

                foreach (MemberInfo propertyMemberInfo in properties)
                {
                    Type type = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(propertyMemberInfo);
                    IUpdater updater = ProcessUpgradeTool.GetUpdaterForType(type);

                    if (updater != null)
                    {
                        updater.Update(propertyMemberInfo, lockablePropertyReference);
                    }
                }
            }
        }
    }
}
