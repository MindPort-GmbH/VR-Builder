using NUnit.Framework;
using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Conditions
{
    public class CompareBooleansConditionTests : CompareValuesConditionTests<bool>
    {
        protected override ICondition CreateDefaultCondition()
        {
            return new CompareValuesCondition<bool>(Guid.Empty, Guid.Empty, true, false, true, true, new OrOperation());
        }

        protected override IDataProperty<bool> CreateValueProperty(string name, bool value)
        {
            GameObject propertyObject = new GameObject(name);
            IDataProperty<bool> property = propertyObject.AddComponent<BooleanDataProperty>();
            property.SetValue(value);
            return property;
        }

        protected static TestCaseData[] CompareValuesTestCases = new TestCaseData[]
        {
            new TestCaseData(true, true, true, true, new EqualToOperation<bool>()).Returns(null),
            new TestCaseData(false, true, true, true, new NotEqualToOperation<bool>()).Returns(null),
            new TestCaseData(true, true, true, true, new AndOperation()).Returns(null),
            new TestCaseData(false, true, true, true, new OrOperation()).Returns(null),
            new TestCaseData(true, true, false, false, new EqualToOperation<bool>()).Returns(null),
            new TestCaseData(false, true, false, false, new NotEqualToOperation<bool>()).Returns(null),
            new TestCaseData(true, true, false, false, new AndOperation()).Returns(null),
            new TestCaseData(false, true, false, false, new OrOperation()).Returns(null),
        };
    }
}