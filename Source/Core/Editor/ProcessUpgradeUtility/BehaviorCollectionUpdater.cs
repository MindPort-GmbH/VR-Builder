using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.ProcessUpdater;

namespace VRBuilder.Editor.Utils
{
    public class BehaviorCollectionUpdater : IUpdater
    {
        public Type SupportedType => typeof(IList<IBehavior>);

        public void Update(MemberInfo memberInfo, object owner)
        {
            IList<IBehavior> behaviorList = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IList<IBehavior>;

            for (int i = 0; i < behaviorList.Count; i++)
            {
                IEntityConverter converter = ProcessUpdater.ProcessUpdater.EntityConverters.FirstOrDefault(converter => converter.ConvertedType == behaviorList[i].GetType());

                if (converter != null)
                {
                    behaviorList[i] = (IBehavior)converter.Convert(behaviorList[i]);
                }
            }
        }
    }
}
