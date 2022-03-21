using NUnit.Framework;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class SetNumberBehaviorTests : SetValueBehaviorTests<float>
    {
        protected static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(33124.4313f).Returns(null),
            new TestCaseData(-31f).Returns(null),
            new TestCaseData(0f).Returns(null),
            new TestCaseData(float.MinValue).Returns(null),
            new TestCaseData(float.NaN).Returns(null),
            new TestCaseData(float.MaxValue).Returns(null),
        };

        protected override IBehavior CreateDefaultBehavior()
        {
            IDataProperty<float> property = CreatePropertyObject();
            return new SetValueBehavior<float>(property, 6.4f);
        }

        protected override IDataProperty<float> CreatePropertyObject()
        {
            GameObject propertyObject = new GameObject("Value Property Object");
            return propertyObject.AddComponent<NumberDataProperty>();
        }
    }
}