using NUnit.Framework;
using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Conditions
{
    public class CompareTextConditionTests : CompareValuesConditionTests<string>
    {
        protected override ICondition CreateDefaultCondition()
        {
            return new CompareValuesCondition<string>(Guid.Empty, Guid.Empty, "blah", "some text", true, true, new NotEqualToOperation<string>());
        }

        protected override IDataProperty<string> CreateValueProperty(string name, string value)
        {
            GameObject propertyObject = new GameObject(name);
            IDataProperty<string> property = propertyObject.AddComponent<TextDataProperty>();
            property.SetValue(value);
            return property;
        }

        protected static TestCaseData[] CompareValuesTestCases = new TestCaseData[]
        {
            new TestCaseData("asdfg", "asdfg", true, true, new EqualToOperation<string>()).Returns(null),
            new TestCaseData("asdfg", "qwerty", true, true, new NotEqualToOperation<string>()).Returns(null),
            new TestCaseData("asdfg", "asdfg", false, false, new EqualToOperation<string>()).Returns(null),
            new TestCaseData("asdfg", "qwerty", false, false, new NotEqualToOperation<string>()).Returns(null),
        };
    }
}