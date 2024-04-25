using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.TestTools;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Tests.Conditions
{
    public abstract class CompareValuesConditionTests<T> : ConditionTests where T : IEquatable<T>, IComparable<T>
    {
        protected abstract IDataProperty<T> CreateValueProperty(string name, T value);

        [UnityTest]
        [TestCaseSource("CompareValuesTestCases")]
        public IEnumerator CreateByReference(T leftValue, T rightValue, bool isLeftConst, bool isRightConst, IOperationCommand<T, bool> operationType)
        {
            // Given the necessary parameters,
            IDataProperty<T> leftProperty = CreateValueProperty("Left Property Object", leftValue);
            IDataProperty<T> rightProperty = CreateValueProperty("Left Property Object", rightValue);

            // When we create the condition passing process objects by reference,
            CompareValuesCondition<T> condition = new CompareValuesCondition<T>(leftProperty, rightProperty, leftValue, rightValue, isLeftConst, isRightConst, operationType);

            // Then all properties of the condition are properly assigned.
            Assert.AreEqual(leftProperty, condition.Data.LeftProperty.Value);
            Assert.AreEqual(rightProperty, condition.Data.RightProperty.Value);
            Assert.AreEqual(leftValue, condition.Data.LeftValue);
            Assert.AreEqual(rightValue, condition.Data.RightValue);
            Assert.AreEqual(isLeftConst, condition.Data.IsLeftConst);
            Assert.AreEqual(isRightConst, condition.Data.IsRightConst);
            Assert.AreEqual(operationType, condition.Data.Operation);

            yield break;
        }

        [UnityTest]
        [TestCaseSource("CompareValuesTestCases")]
        public IEnumerator CreateByName(T leftValue, T rightValue, bool isLeftConst, bool isRightConst, IOperationCommand<T, bool> operationType)
        {
            // Given the necessary parameters,
            IDataProperty<T> leftProperty = CreateValueProperty("Left Property Object", leftValue);
            IDataProperty<T> rightProperty = CreateValueProperty("Left Property Object", rightValue);
            Guid leftPropertyId = leftProperty.SceneObject.Guid;
            Guid rightPropertyId = rightProperty.SceneObject.Guid;

            // When we create the behavior passing process objects by name,
            CompareValuesCondition<T> condition = new CompareValuesCondition<T>(leftPropertyId, rightPropertyId, leftValue, rightValue, isLeftConst, isRightConst, operationType);

            // Then all properties of the behavior are properly assigned.
            Assert.AreEqual(leftProperty, condition.Data.LeftProperty.Value);
            Assert.AreEqual(rightProperty, condition.Data.RightProperty.Value);
            Assert.AreEqual(leftValue, condition.Data.LeftValue);
            Assert.AreEqual(rightValue, condition.Data.RightValue);
            Assert.AreEqual(isLeftConst, condition.Data.IsLeftConst);
            Assert.AreEqual(isRightConst, condition.Data.IsRightConst);
            Assert.AreEqual(operationType, condition.Data.Operation);

            yield break;
        }
        [UnityTest]
        [TestCaseSource("CompareValuesTestCases")]
        public IEnumerator ConditionIsFulfilledWhenExpected(T leftValue, T rightValue, bool isLeftConst, bool isRightConst, IOperationCommand<T, bool> operationType)
        {
            // Given a condition,
            IDataProperty<T> leftProperty = CreateValueProperty("Left Property Object", leftValue);
            IDataProperty<T> rightProperty = CreateValueProperty("Left Property Object", rightValue);
            CompareValuesCondition<T> condition = new CompareValuesCondition<T>(leftProperty, rightProperty, leftValue, rightValue, isLeftConst, isRightConst, operationType);

            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When the condition is fulfilled,

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Then the condition completes.
            Assert.IsTrue(condition.IsCompleted);
        }
    }
}