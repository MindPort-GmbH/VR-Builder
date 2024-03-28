using System;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public class SetEnabledByTagBehaviorUpdater : IUpdater
    {
#pragma warning disable CS0618 // Type or member is obsolete
        public Type SupportedType => typeof(SetObjectsWithTagEnabledBehavior);
#pragma warning restore CS0618 // Type or member is obsolete

        public void Update(MemberInfo memberInfo, object owner)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            SetObjectsWithTagEnabledBehavior oldBehavior = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as SetObjectsWithTagEnabledBehavior;
#pragma warning restore CS0618 // Type or member is obsolete
            SetObjectsEnabledBehavior newBehavior = new SetObjectsEnabledBehavior();
            newBehavior.Data.SetEnabled = oldBehavior.Data.SetEnabled;
            newBehavior.Data.TargetObjects = oldBehavior.Data.TargetObjects;
            newBehavior.Data.RevertOnDeactivation = oldBehavior.Data.RevertOnDeactivation;

            ReflectionUtils.SetValueToPropertyOrField(owner, memberInfo, newBehavior);

            Debug.Log("Updated behavior");
        }
    }
}
