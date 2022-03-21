using NUnit.Framework;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class SetBoolBehaviorTests : SetValueBehaviorTests<bool>
    {
        protected static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(true).Returns(null),
            new TestCaseData(false).Returns(null),
        };

        protected override IBehavior CreateDefaultBehavior()
        {
            IDataProperty<bool> property = CreatePropertyObject();
            return new SetValueBehavior<bool>(property, true);
        }

        protected override IDataProperty<bool> CreatePropertyObject()
        {
            GameObject propertyObject = new GameObject("Value Property Object");
            return propertyObject.AddComponent<BooleanDataProperty>();
        }
    }
}