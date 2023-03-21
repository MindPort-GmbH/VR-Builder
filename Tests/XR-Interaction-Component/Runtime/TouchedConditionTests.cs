using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Tests.Utils;
using VRBuilder.XRInteraction.Properties;
using VRBuilder.BasicInteraction.Conditions;

namespace VRBuilder.XRInteraction.Tests.Conditions
{
    public class TouchedConditionTests : RuntimeTests
    {
        public class TouchablePropertyMock : TouchableProperty
        {
            private bool isTouched;
            public override bool IsBeingTouched
            {
                get
                {
                    return isTouched;
                }
            }

            public void SetTouched(bool value)
            {
                isTouched = value;
            }

            public void EmitIsTouched()
            {
                EmitTouched();
            }
        }

        [UnityTest]
        public IEnumerator CompleteWhenTouched()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            TouchablePropertyMock mockedProperty = obj.AddComponent<TouchablePropertyMock>();

            yield return new WaitForFixedUpdate();

            TouchedCondition condition = new TouchedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When the object is touched
            mockedProperty.SetTouched(true);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenItIsDoneOnStart()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            TouchablePropertyMock mockedProperty = obj.AddComponent<TouchablePropertyMock>();
            mockedProperty.SetTouched(true);

            yield return new WaitForFixedUpdate();

            TouchedCondition condition = new TouchedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed due IsGrabbed being true
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator ConditionNotCompleted()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            TouchablePropertyMock mockedProperty = obj.AddComponent<TouchablePropertyMock>();
            TouchedCondition condition = new TouchedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a touched condition,
            GameObject obj = new GameObject("T1");
            TouchablePropertyMock mockedProperty = obj.AddComponent<TouchablePropertyMock>();

            yield return new WaitForFixedUpdate();

            TouchedCondition condition = new TouchedCondition(mockedProperty);

            bool wasTouched = false;
            bool wasUntouched = false;
            mockedProperty.OnTouched.AddListener((args) =>
            {
                wasTouched = true;
                mockedProperty.OnUntouched.AddListener((unargs) => wasUntouched = true);
            });

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then it is completed.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(wasTouched);
            Assert.IsTrue(wasUntouched);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a touched condition,
            GameObject obj = new GameObject("T1");
            TouchablePropertyMock mockedProperty = obj.AddComponent<TouchablePropertyMock>();

            yield return new WaitForFixedUpdate();

            TouchedCondition condition = new TouchedCondition(mockedProperty);

            bool wasTouched = false;
            bool wasUntouched = false;
            mockedProperty.OnTouched.AddListener((args) =>
            {
                wasTouched = true;
                mockedProperty.OnUntouched.AddListener((unargs) => wasUntouched = true);
            });

            // When you activate it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you fast-forward it
            condition.LifeCycle.MarkToFastForward();

            // Then nothing happens.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsFalse(condition.IsCompleted);
            Assert.IsFalse(wasUntouched);
            Assert.IsFalse(wasTouched);
        }
    }
}
