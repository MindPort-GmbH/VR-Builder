using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Mocks;
using VRBuilder.Editor.Utils;

namespace VRBuilder.Core.Tests
{
    public class EntityDataUpdaterTests : RuntimeTests
    {
        public class ObsoleteProcessSceneObject : ProcessSceneObject
        {
            public void SetUniqueName(string uniqueName)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                this.uniqueName = uniqueName;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private ProcessSceneObject CreateObsoleteGameObject(string name)
        {
            GameObject obsoleteObject = new GameObject(name);
            ObsoleteProcessSceneObject processSceneObject = obsoleteObject.AddComponent<ObsoleteProcessSceneObject>();
            processSceneObject.SetUniqueName(name);
            return processSceneObject;
        }

        [UnityTest]
        public IEnumerator ObjectReferenceGetsUpdated()
        {
            // Given a Scale behavior with an obsolete reference,
            ProcessSceneObject processSceneObject = CreateObsoleteGameObject("ScaledObject");

            ScalingBehavior scalingBehavior = new ScalingBehavior();
#pragma warning disable CS0618 // Type or member is obsolete
            scalingBehavior.Data.Target = new SceneObjectReference(processSceneObject.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            // If I run EntityDataUpdater on it,
            ProcessUpdater.UpdateDataRecursively(scalingBehavior);

            // Then the reference is updated to the correct type.
            Assert.IsTrue(scalingBehavior.Data.Targets.Guids.Count == 1);
            Assert.IsTrue(scalingBehavior.Data.Targets.Values.Count() == 1);
            Assert.AreEqual(processSceneObject, scalingBehavior.Data.Targets.Values.First());

            // Cleanup
            GameObject.DestroyImmediate(processSceneObject.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LockablePropertiesAreUpdated()
        {
            // Given a step with some manually unlocked properties,
            ProcessSceneObject objectToUnlock = CreateObsoleteGameObject("ObjectToUnlock");
            objectToUnlock.AddProcessProperty<PropertyMock>();

            LockablePropertyReference lockablePropertyReference = new LockablePropertyReference();
#pragma warning disable CS0618 // Type or member is obsolete
            lockablePropertyReference.Target = new SceneObjectReference(objectToUnlock.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete
            IStep step = EntityFactory.CreateStep("TestStep");
            ILockableStepData lockableData = step.Data as ILockableStepData;
            lockableData.ToUnlock = new List<LockablePropertyReference>() { lockablePropertyReference };

            // When I update it,
            ProcessUpdater.UpdateDataRecursively(step);

            // Then the lockable properties are updated.
            Assert.IsTrue(lockablePropertyReference.TargetObject.HasValue());
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(objectToUnlock, lockablePropertyReference.TargetObject.Value);
#pragma warning restore CS0618 // Type or member is obsolete

            // Cleanup
            GameObject.DestroyImmediate(objectToUnlock.gameObject);
            yield return null;
        }
    }
}