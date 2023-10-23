using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;
using Object = UnityEngine.Object;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class ConfettiBehaviorTests : RuntimeTests
    {
        private const string pathToPrefab = "Confetti/Prefabs/RandomConfettiMachine";
        private const string pathToMockPrefab = "Confetti/Prefabs/MockConfettiMachine";
        private const string positionProviderName = "Target Position";
        private const float duration = 0.2f;
        private const float areaRadius = 11f;
        private readonly IMode defaultMode = new Mode("Default", new WhitelistTypeRule<IOptional>());

        [UnityTest]
        public IEnumerator CreateByReference()
        {
            // Given the path to the confetti machine prefab, the position provider name, the duration, the bool isAboveUser, the area radius, and the activation mode,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            BehaviorExecutionStages executionStages = BehaviorExecutionStages.ActivationAndDeactivation;

            // When we create ConfettiBehavior and pass process objects by reference,
            ConfettiBehavior confettiBehavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, executionStages);
            confettiBehavior.Configure(defaultMode);

            // Then all properties of the ConfettiBehavior are properly assigned
            Assert.AreEqual(false, confettiBehavior.Data.IsAboveUser);
            Assert.AreEqual(positionProvider, confettiBehavior.Data.PositionProvider.Value);
            Assert.AreEqual(pathToPrefab, confettiBehavior.Data.ConfettiMachinePrefabPath);
            Assert.AreEqual(areaRadius, confettiBehavior.Data.AreaRadius);
            Assert.AreEqual(duration, confettiBehavior.Data.Duration);
            Assert.AreEqual(executionStages, confettiBehavior.Data.ExecutionStages);

            yield break;
        }

        [UnityTest]
        public IEnumerator CreateByName()
        {
            // Given the path to the confetti machine prefab, the position provider name, the duration, the bool isAboveUser, the area radius, and the activation mode,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            BehaviorExecutionStages executionStages = BehaviorExecutionStages.ActivationAndDeactivation;

            // When we create ConfettiBehavior and pass process objects by their unique name,
            ConfettiBehavior confettiBehavior = new ConfettiBehavior(false, positionProviderName, pathToMockPrefab, areaRadius, duration, executionStages);
            confettiBehavior.Configure(defaultMode);

            // Then all properties of the MoveObjectBehavior are properly assigned.
            Assert.AreEqual(false, confettiBehavior.Data.IsAboveUser);
            Assert.AreEqual(positionProvider, confettiBehavior.Data.PositionProvider.Value);
            Assert.AreEqual(pathToMockPrefab, confettiBehavior.Data.ConfettiMachinePrefabPath);
            Assert.AreEqual(areaRadius, confettiBehavior.Data.AreaRadius);
            Assert.AreEqual(duration, confettiBehavior.Data.Duration);
            Assert.AreEqual(executionStages, confettiBehavior.Data.ExecutionStages);

            yield break;
        }

        [UnityTest]
        public IEnumerator ActivationWithSpawnedMachine()
        {
            // Given a positive duration, a position provider, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior and wait until it's activating,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            ConfettiMachine machine = GameObject.FindObjectOfType<ConfettiMachine>();

            // Then the activation state of the behavior is "activating" and the ConfettiMachine exists in the scene.
            Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
            Assert.IsTrue(machine != null);
        }

        [UnityTest]
        public IEnumerator RemovedMachineAfterPositiveDuration()
        {
            // Given a positive duration, a position provider, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior and wait for one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            // And wait duration seconds,
            float startTime = Time.time;
            while (Time.time < startTime + duration + 0.1f)
            {
                yield return null;
                behavior.Update();
            }

            // Then behavior activation is completed, and the confetti machine should be deleted.
            string prefabName = "Behavior" + pathToPrefab.Substring(pathToPrefab.LastIndexOf("/", StringComparison.Ordinal) + 1);
            Assert.IsTrue(GameObject.Find(prefabName) == null);
        }

        [UnityTest]
        public IEnumerator NegativeDuration()
        {
            // Given a negative duration, a position provider, some valid default settings, and the activation mode = Activation,
            float newDuration = -0.25f;

            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, newDuration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // Wait one update cycle,
            yield return null;
            behavior.Update();

            // And wait one end cycle,
            yield return null;
            behavior.Update();

            // Then behavior activation is immediately completed, and the confetti machine should be deleted.
            string prefabName = "Behavior" + pathToPrefab.Substring(pathToPrefab.LastIndexOf("/", StringComparison.Ordinal) + 1);
            GameObject machine = GameObject.Find(prefabName);

            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(machine == null);
        }

        [UnityTest]
        public IEnumerator ZeroDuration()
        {
            // Given a duration equals zero, a position provider, some valid default settings, and the activation mode = Activation,
            float newDuration = 0f;

            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, newDuration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // Wait one update cycle,
            yield return null;
            behavior.Update();

            // And wait one end cycle,
            yield return null;
            behavior.Update();

            // Then behavior activation is immediately completed, and the confetti machine should be in the scene.
            string prefabName = "Behavior" + pathToPrefab.Substring(pathToPrefab.LastIndexOf("/", StringComparison.Ordinal) + 1);
            GameObject machine = GameObject.Find(prefabName);

            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(machine == null);

            // Cleanup created game objects.
            Object.DestroyImmediate(target);
        }

        [UnityTest]
        public IEnumerator SpawnAtPositionProvider()
        {
            // Given the position provider process object, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            target.transform.position = new Vector3(5, 10, 20);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // And wait one update cycle,
            yield return null;
            behavior.Update();

            ConfettiMachine machine = GameObject.FindObjectOfType<ConfettiMachine>();

            Assert.IsFalse(machine == null);
            Assert.IsTrue(machine.transform.position == target.transform.position);
        }

        [UnityTest]
        public IEnumerator StillActivatingWhenPositiveDurationNotFinished()
        {
            // Given the position provider process object, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, 2f, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // And wait two update cycles,
            yield return null;
            behavior.Update();

            yield return null;
            behavior.Update();

            // Then the activation state of the behavior is "activating".
            Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator IsActiveAfterPositiveDuration()
        {
            // Given the position provider process object, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior and wait for one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // And wait for it to be active,
            float startTime = Time.time;
            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            float behaviorDuration = Time.time - startTime;

            // Then the activation state of the behavior is "active" after the expected duration.
            Assert.AreEqual(duration, behaviorDuration, Time.deltaTime);
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator NotExistingPrefab()
        {
            // Given the position provider process object, an invalid path to a not existing prefab, some valid default settings, and the activation mode = Activation,
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToMockPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When I activate that behavior and wait for one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            string prefabName = "Behavior" + pathToMockPrefab.Substring(pathToMockPrefab.LastIndexOf("/", StringComparison.Ordinal) + 1);
            GameObject machine = GameObject.Find(prefabName);

            // Then the activation state of the behavior is "active" and there is no confetti machine in the scene.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(null, machine);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a ConfettiBehavior with activation mode "Activation",
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a ConfettiBehavior with activation mode "Activation",
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a ConfettiBehavior with activation mode "Deactivation",
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Deactivation);
            behavior.Configure(defaultMode);

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
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active ConfettiBehavior with activation mode "Activation",
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Activation);
            behavior.Configure(defaultMode);

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
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given an active ConfettiBehavior with activation mode "Deactivation",
            GameObject target = new GameObject(positionProviderName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            ConfettiBehavior behavior = new ConfettiBehavior(false, positionProvider, pathToPrefab, areaRadius, duration, BehaviorExecutionStages.Deactivation);
            behavior.Configure(defaultMode);

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
        }
    }
}
