using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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

            Debug.Log("Updating behavior list");

            for (int i = 0; i < behaviorList.Count; i++)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (behaviorList[i] is SetObjectsWithTagEnabledBehavior)
                {
#pragma warning restore CS0618 // Type or member is obsolete

                    IEntityConverter converter = new SetObjectEnabledBehaviorConverter() as IEntityConverter;
                    behaviorList[i] = (IBehavior)converter.Convert(behaviorList[i]);
                }
            }
        }
    }
}
