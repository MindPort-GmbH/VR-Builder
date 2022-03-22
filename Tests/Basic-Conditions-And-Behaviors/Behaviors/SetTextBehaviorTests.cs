using NUnit.Framework;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class SetTextBehaviorTests : SetValueBehaviorTests<string>
    {
        protected static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData("some text").Returns(null),
            new TestCaseData("").Returns(null),
            new TestCaseData(null).Returns(null),
        };

        protected override IBehavior CreateDefaultBehavior()
        {
            IDataProperty<string> property = CreatePropertyObject();
            return new SetValueBehavior<string>(property, "blah blah prrr");
        }

        protected override IDataProperty<string> CreatePropertyObject()
        {
            GameObject propertyObject = new GameObject("Value Property Object");
            return propertyObject.AddComponent<TextDataProperty>();
        }
    }
}