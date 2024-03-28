using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Utils;

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
                if (behaviorList[i] is SetObjectsWithTagEnabledBehavior)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    SetObjectsWithTagEnabledBehavior oldBehavior = behaviorList[i] as SetObjectsWithTagEnabledBehavior;
#pragma warning restore CS0618 // Type or member is obsolete
                    SetObjectsEnabledBehavior newBehavior = new SetObjectsEnabledBehavior();
                    newBehavior.Data.SetEnabled = oldBehavior.Data.SetEnabled;
                    newBehavior.Data.TargetObjects = oldBehavior.Data.TargetObjects;
                    newBehavior.Data.RevertOnDeactivation = oldBehavior.Data.RevertOnDeactivation;

                    behaviorList[i] = newBehavior;
                }
            }
        }
    }
}
