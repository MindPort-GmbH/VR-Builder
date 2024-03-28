using System;
using System.Collections.Generic;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Editor.Utils
{
    public class BehaviorCollectionUpdater : IUpdater
    {
        public Type SupportedType => typeof(IEnumerable<IBehavior>);

        public void Update(MemberInfo memberInfo, object owner)
        {
            IEnumerable<IBehavior> updatedObject = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IEnumerable<IBehavior>;

            Debug.Log("Updating behavior list");

            foreach (IBehavior behavior )

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
