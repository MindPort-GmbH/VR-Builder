using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public abstract class SetValueBehaviorTests<T> : BehaviorTests
    {
        protected abstract IDataProperty<T> CreatePropertyObject();

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator CreateByReference(T value)
        {
            // Given the necessary parameters,
            IDataProperty<T> property = CreatePropertyObject();

            // When we create the behavior passing process objects by reference,
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            // Then all properties of the behavior are properly assigned.
            Assert.AreEqual(property, behavior.Data.DataProperties.Values.First());
            Assert.AreEqual(value, behavior.Data.NewValue);

            yield break;
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator CreateByName(T value)
        {
            // Given the necessary parameters,
            IDataProperty<T> property = CreatePropertyObject();
            Guid propertyName = property.SceneObject.Guid;

            // When we create the behavior passing process objects by name,
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(propertyName, value);

            // Then all properties of the behavior are properly assigned.
            Assert.AreEqual(property, behavior.Data.DataProperties.Values.First());
            Assert.AreEqual(value, behavior.Data.NewValue);

            yield break;
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator ValueIsSet(T value)
        {
            //Given a set value behavior,
            IDataProperty<T> property = CreatePropertyObject();
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            //When it is activated,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            //Then the value is set
            Assert.AreEqual(value, property.GetValue());
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt(T value)
        {
            //Given a set value behavior,
            IDataProperty<T> property = CreatePropertyObject();
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(value, property.GetValue());

            yield break;
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt(T value)
        {
            //Given a set value behavior,
            IDataProperty<T> property = CreatePropertyObject();
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            // When we mark it to fast-forward, activate and immediately deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.AreEqual(value, property.GetValue());
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator FastForwardActivatingBehavior(T value)
        {
            //Given a set value behavior,
            IDataProperty<T> property = CreatePropertyObject();
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(value, property.GetValue());
        }

        [UnityTest]
        [TestCaseSource("SetValueTestCases")]
        public IEnumerator FastForwardDeactivatingBehavior(T value)
        {
            //Given a set value behavior,
            IDataProperty<T> property = CreatePropertyObject();
            SetValueBehavior<T> behavior = new SetValueBehavior<T>(property, value);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            while (behavior.LifeCycle.Stage != Stage.Deactivating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.AreEqual(value, property.GetValue());
        }
    }
}