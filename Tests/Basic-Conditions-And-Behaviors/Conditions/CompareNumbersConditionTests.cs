using NUnit.Framework;
using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Conditions
{
    public class CompareNumbersConditionTests : CompareValuesConditionTests<float>
    {
        protected override ICondition CreateDefaultCondition()
        {
            return new CompareValuesCondition<float>(Guid.Empty, Guid.Empty, 5f, -6.3f, true, true, new GreaterThanOperation<float>());
        }

        protected override IDataProperty<float> CreateValueProperty(string name, float value)
        {
            GameObject propertyObject = new GameObject(name);
            IDataProperty<float> property = propertyObject.AddComponent<NumberDataProperty>();
            property.SetValue(value);
            return property;
        }

        protected static TestCaseData[] CompareValuesTestCases = new TestCaseData[]
        {
            new TestCaseData(44f, -32f, true, true, new GreaterThanOperation<float>()).Returns(null),
            new TestCaseData(44f, -32f, true, true, new GreaterOrEqualOperation<float>()).Returns(null),
            new TestCaseData(-32f, -32f, true, true, new GreaterOrEqualOperation<float>()).Returns(null),
            new TestCaseData(44f, 44f, true, true, new EqualToOperation<float>()).Returns(null),
            new TestCaseData(44f, -32f, true, true, new NotEqualToOperation<float>()).Returns(null),
            new TestCaseData(15.4f, 23.65f, true, true, new LessThanOperation<float>()).Returns(null),
            new TestCaseData(15.4f, 15.4f, true, true, new LessThanOrEqualOperation<float>()).Returns(null),
            new TestCaseData(-44f, -32f, true, true, new LessThanOrEqualOperation<float>()).Returns(null),
            new TestCaseData(44f, -32f, false, false, new GreaterThanOperation<float>()).Returns(null),
            new TestCaseData(44f, -32f, false, false, new GreaterOrEqualOperation<float>()).Returns(null),
            new TestCaseData(-32f, -32f, false, false, new GreaterOrEqualOperation<float>()).Returns(null),
            new TestCaseData(44f, 44f, false, false, new EqualToOperation<float>()).Returns(null),
            new TestCaseData(44f, -32f, false, false, new NotEqualToOperation<float>()).Returns(null),
            new TestCaseData(15.4f, 23.65f, false, false, new LessThanOperation<float>()).Returns(null),
            new TestCaseData(15.4f, 15.4f, false, false, new LessThanOrEqualOperation<float>()).Returns(null),
            new TestCaseData(-44f, -32f, false, false, new LessThanOrEqualOperation<float>()).Returns(null),
        };
    }
}