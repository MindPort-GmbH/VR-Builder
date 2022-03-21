using System.Collections;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class MoveObjectBehaviorTests : RuntimeTests
    {
        private const string movedName = "Moved Object";
        private const string positionProviderName = "Target Position";

        [UnityTest]
        public IEnumerator CreateByReference()
        {
            // Given two process objects and a duration,
            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject targetGo = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = targetGo.AddComponent<ProcessSceneObject>();
            positionProvider.ChangeUniqueName(positionProviderName);

            float duration = 0.25f;

            // When we create MoveObjectBehavior and pass process objects by reference,
            MoveObjectBehavior moveObjectBehavior = new MoveObjectBehavior(moved, positionProvider, duration);

            // Then all properties of the MoveObjectBehavior are properly assigned
            Assert.AreEqual(moved, moveObjectBehavior.Data.Target.Value);
            Assert.AreEqual(positionProvider, moveObjectBehavior.Data.PositionProvider.Value);
            Assert.AreEqual(moveObjectBehavior.Data.Duration,duration);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(targetGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateByName()
        {
            // Given two process objects and a duration,
            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject targetGo = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = targetGo.AddComponent<ProcessSceneObject>();
            positionProvider.ChangeUniqueName(positionProviderName);

            float duration = 0.25f;

            // When we create MoveObjectBehavior and pass process objects by their unique name,
            MoveObjectBehavior moveObjectBehavior = new MoveObjectBehavior(movedName, positionProviderName, duration);

            // Then all properties of the MoveObjectBehavior are properly assigned
            Assert.AreEqual(moved, moveObjectBehavior.Data.Target.Value);
            Assert.AreEqual(positionProvider, moveObjectBehavior.Data.PositionProvider.Value);
            Assert.AreEqual(moveObjectBehavior.Data.Duration,duration);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(targetGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PositiveDuration()
        {
            // Given MoveObjectBehavior that takes two process objects with different positions and rotations, and positive transition duration,
            float duration = 0.05f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject positionProviderGo = new GameObject(positionProviderName);
            positionProviderGo.transform.position = new Vector3(1, 2, 50);
            positionProviderGo.transform.rotation = Quaternion.Euler(57, 195, 188);
            ProcessSceneObject target = positionProviderGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When I activate that behavior and wait for transition duration,
            behavior.LifeCycle.Activate();

            float startTime = Time.time;
            while (Stage.Active != behavior.LifeCycle.Stage)
            {
                yield return null;
                behavior.Update();
            }

            // Then behavior activation is completed, and moved object position and rotation matches positionProvider's.
            Assert.IsTrue(Time.time - startTime > duration);
            Assert.IsTrue((movedGo.transform.position - positionProviderGo.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(movedGo.transform.rotation, positionProviderGo.transform.rotation) > 0.999f);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(positionProviderGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator NegativeDuration()
        {
            // Given MoveObjectBehavior that takes two process objects with different positions and rotations, and negative transition duration,
            float duration = -2.5f;
            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject positionProviderGameObject = new GameObject(positionProviderName);
            positionProviderGameObject.transform.position = new Vector3(1, 2, 50);
            positionProviderGameObject.transform.rotation = Quaternion.Euler(123, 15, 8);
            ProcessSceneObject positionProvider = positionProviderGameObject.AddComponent<ProcessSceneObject>();
            positionProvider.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, positionProvider, duration);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate that behavior,
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            // Then it immediately completes its activation, and moved object position and rotation matches the ones of positionProvider.
            Assert.IsTrue(behavior.LifeCycle.Stage == Stage.Active);
            Assert.IsTrue((movedGo.transform.position - positionProviderGameObject.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(movedGo.transform.rotation, positionProviderGameObject.transform.rotation) > 0.999f);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(positionProviderGameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ZeroDuration()
        {
            // Given MoveObjectBehavior that takes two process objects with different positions and rotations, and transition duration that equals zero,
            float duration = 0f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject targetGo = new GameObject(positionProviderName);
            targetGo.transform.position = new Vector3(1, 2, 50);
            targetGo.transform.rotation = Quaternion.Euler(123, 15, 8);
            ProcessSceneObject target = targetGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate that behavior,
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            // Then it immediately completes its activation, and moved object position and rotation matches the ones of positionProvider.
            Assert.IsTrue(behavior.LifeCycle.Stage == Stage.Active);
            Assert.IsTrue((movedGo.transform.position - targetGo.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(movedGo.transform.rotation, targetGo.transform.rotation) > 0.999f);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(targetGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SamePosition()
        {
            // Given MoveObjectBehavior that takes two process objects with the same position and rotation, and positive transition duration,
            float duration = 0.05f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject targetGo = new GameObject(positionProviderName);
            ProcessSceneObject target = targetGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the behavior,
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            // Then it does not finish its activation immediately.
            Assert.IsTrue(behavior.LifeCycle.Stage == Stage.Activating);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(targetGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given MoveObjectBehavior that takes two process objects with different positions and rotations, and positive transition duration,
            float duration = 0.05f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject positionProviderGo = new GameObject(positionProviderName);
            positionProviderGo.transform.position = new Vector3(1, 2, 50);
            positionProviderGo.transform.rotation = Quaternion.Euler(57, 195, 188);
            ProcessSceneObject target = positionProviderGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it wasn't activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(positionProviderGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given MoveObjectBehavior that takes two process objects with different positions and rotations, and positive transition duration,
            float duration = 0.05f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject positionProviderGo = new GameObject(positionProviderName);
            positionProviderGo.transform.position = new Vector3(1, 2, 50);
            positionProviderGo.transform.rotation = Quaternion.Euler(57, 195, 188);
            ProcessSceneObject target = positionProviderGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately, and moved object position and rotation matches the ones of positionProvider.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue((movedGo.transform.position - positionProviderGo.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(movedGo.transform.rotation, positionProviderGo.transform.rotation) > 0.999f);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(positionProviderGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an activated MoveObjectBehavior that takes two process objects with different positions and rotations, and positive transition duration,
            float duration = 0.05f;

            GameObject movedGo = new GameObject(movedName);
            ProcessSceneObject moved = movedGo.AddComponent<ProcessSceneObject>();
            moved.ChangeUniqueName(movedName);

            GameObject positionProviderGo = new GameObject(positionProviderName);
            positionProviderGo.transform.position = new Vector3(1, 2, 50);
            positionProviderGo.transform.rotation = Quaternion.Euler(57, 195, 188);
            ProcessSceneObject target = positionProviderGo.AddComponent<ProcessSceneObject>();
            target.ChangeUniqueName(positionProviderName);

            MoveObjectBehavior behavior = new MoveObjectBehavior(moved, target, duration);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately, and moved object position and rotation matches the ones of positionProvider.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue((movedGo.transform.position - positionProviderGo.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(movedGo.transform.rotation, positionProviderGo.transform.rotation) > 0.999f);

            // Cleanup created game objects.
            Object.DestroyImmediate(movedGo);
            Object.DestroyImmediate(positionProviderGo);

            yield return null;
        }
    }
}
