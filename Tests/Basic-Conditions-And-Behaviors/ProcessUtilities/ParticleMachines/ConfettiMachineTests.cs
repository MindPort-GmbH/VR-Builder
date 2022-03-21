using System;
using System.Collections;
using VRBuilder.Tests.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using VRBuilder.Core.ProcessUtils;

namespace VRBuilder.BaseTemplate.Tests.ParticleMachine
{
    public class ConfettiMachineTests : RuntimeTests
    {
        private const string pathToDefaultPrefab = "Confetti/Prefabs/RandomConfettiMachine";

        [UnityTest]
        public IEnumerator InstantiateDefaultPrefab()
        {
            // Given a valid path to the prefab,
            // When I instantiate the prefab,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));

            // Then it is not null.
            Assert.IsFalse(machineObject == null);

            yield break;
        }

        [UnityTest]
        public IEnumerator DefaultPrefabHasConfettiMachineComponent()
        {
            // Given a valid path to the prefab,
            // When I instantiate the prefab,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));

            // Then it has the "ConfettiMachine" component
            Assert.IsFalse(machineObject.GetComponent(typeof(ConfettiMachine)) == null);

            yield break;
        }

        [UnityTest]
        public IEnumerator DefaultPrefabHasChildrenWithParticleSystems()
        {
            // Given a valid path to the prefab,
            // When I instantiate the prefab,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));

            // Then it has "ParticleSystems" as children.
            Assert.IsFalse(machineObject.GetComponentInChildren(typeof(ParticleSystem), true) == null);

            yield break;
        }

        [UnityTest]
        public IEnumerator ActivateWithoutParametersTest()
        {
            // Given a valid confetti machine,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            // When I call Activate without parameters on it,
            confettiMachine.Activate();

            // Then the confetti machine is active and all of its particle systems are active and playing.
            Assert.IsTrue(confettiMachine.IsActive);

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>();
            Assert.IsTrue(particleSystems.Length > 0);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(particleSystem.isPlaying);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ActivateWithParametersTest()
        {
            // Given a valid confetti machine with particle systems and some valid positive parameters,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            float newDuration = 22.5f;
            float newRadius = 9.75f;

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float oldRate = particleSystems[0].emission.rateOverTimeMultiplier;

            // When I call Activate with parameters on it,
            confettiMachine.Activate(newRadius, newDuration);

            // Then the confetti machine is active and all of its particle systems are active and playing with the new given parameters.
            Assert.IsTrue(confettiMachine.IsActive);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(particleSystem.isPlaying);
                Assert.IsTrue(Math.Abs(particleSystem.shape.radius - newRadius) < 0.001f);
                Assert.IsTrue(Math.Abs(particleSystem.main.duration - newDuration) < 0.001f);
            }

            Assert.IsTrue(Math.Abs(particleSystems[0].emission.rateOverTimeMultiplier - (oldRate * newRadius * newRadius)) < 0.001f);

            yield break;
        }

        [UnityTest]
        public IEnumerator DeactivateWhenActiveTest()
        {
            // Given an active valid confetti machine,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            confettiMachine.Activate();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>();
            Assert.IsTrue(particleSystems.Length > 0);

            // When I deactivate it,
            confettiMachine.Deactivate();

            // Then the confetti machine is not active and all of its particle systems are deactivated and not playing.
            Assert.IsFalse(confettiMachine.IsActive);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsFalse(particleSystem.isPlaying);
                Assert.IsFalse(particleSystem.gameObject.activeSelf);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator DeactivateWhenNotActiveTest()
        {
            // Given an inactive valid confetti machine,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            // When I deactivate it,
            confettiMachine.Deactivate();

            // Then the confetti machine is not active and all of its particle systems are deactivated and not playing.
            Assert.IsFalse(confettiMachine.IsActive);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsFalse(particleSystem.isPlaying);
                Assert.IsFalse(particleSystem.gameObject.activeSelf);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeToPositiveEmissionDurationTest()
        {
            // Given a valid confetti machine with particle systems and a new positive duration,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newDuration = particleSystems[0].main.duration + confettiMachine.EmissionDuration + 1.47f;

            // When I change the emission duration,
            confettiMachine.ChangeEmissionDuration(newDuration);

            // Then it is accordingly changed in the confetti machine component itself and in all of its particle systems.
            Assert.IsTrue(Math.Abs(confettiMachine.EmissionDuration - newDuration) < 0.001f);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(Math.Abs(particleSystem.main.duration - newDuration) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeToNegativeEmissionDurationTest()
        {
            // Given a valid confetti machine with particle systems and a new negative duration,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newDuration = -5f;

            // When I change the emission duration,
            confettiMachine.ChangeEmissionDuration(newDuration);

            // Then it is accordingly changed in the confetti machine component itself and in all of its particle systems.
            Assert.IsTrue(Math.Abs(confettiMachine.EmissionDuration) < 0.1f);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                // Note it is not possible to set a particle system's duration to zero, Unity will silently set it to 0.05f.
                Assert.IsTrue(Math.Abs(particleSystem.main.duration) < 0.1f);
            }

            yield break;
        }


        [UnityTest]
        public IEnumerator ChangeToZeroEmissionDurationTest()
        {
            // Given a valid confetti machine with particle systems and the new duration equals zero,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newDuration = 0f;

            // When I change the emission duration,
            confettiMachine.ChangeEmissionDuration(newDuration);

            // Then it is accordingly changed in the confetti machine component itself and in all of its particle systems.
            Assert.IsTrue(Math.Abs(confettiMachine.EmissionDuration) < 0.1f);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                // Note it is not possible to set a particle system's duration to zero, Unity will silently set it to 0.05f.
                Assert.IsTrue(Math.Abs(particleSystem.main.duration) < 0.1f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeEmissionRateWithPositiveNumberTest()
        {
            // Given a valid confetti machine with particle systems and a new area radius,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float[] oldRates = new float[particleSystems.Length];

            for (int i = 0; i < oldRates.Length; i++)
            {
                oldRates[i] = particleSystems[i].emission.rateOverTimeMultiplier;
            }

            float multiplier = 7.5f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(multiplier);

            // Then the emission rate is accordingly changed. It equals squared radius multiplied by the default emission rate over time for radius equals 1.
            for (int i = 0; i < oldRates.Length; i++)
            {
                Assert.IsTrue(Math.Abs(particleSystems[i].emission.rateOverTimeMultiplier - (oldRates[i] * multiplier * multiplier)) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeEmissionRateWithNegativeNumberTest()
        {
            // Given a valid confetti machine with particle systems and a new negative area radius,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float[] oldRates = new float[particleSystems.Length];

            for (int i = 0; i < oldRates.Length; i++)
            {
                oldRates[i] = particleSystems[i].emission.rateOverTimeMultiplier;
            }

            float multiplier = -7.5f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(multiplier);

            // Then the emission rate is accordingly changed. It equals squared radius (radius has 0.01f as lowest value) multiplied by the default emission rate over time for radius equals 1.
            for (int i = 0; i < oldRates.Length; i++)
            {
                Assert.IsTrue(Math.Abs(particleSystems[i].emission.rateOverTimeMultiplier - (oldRates[i] * 0.01f * 0.01f)) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeEmissionRateWithZeroTest()
        {
            // Given a valid confetti machine with particle systems and the new radius equals zero,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float[] oldRates = new float[particleSystems.Length];

            for (int i = 0; i < oldRates.Length; i++)
            {
                oldRates[i] = particleSystems[i].emission.rateOverTimeMultiplier;
            }

            float multiplier = 0f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(multiplier);

            // Then the emission rate is accordingly changed. It equals squared radius (radius has 0.01f as lowest value) multiplied by the default emission rate over time for radius equals 1.
            for (int i = 0; i < oldRates.Length; i++)
            {
                Assert.IsTrue(Math.Abs(particleSystems[i].emission.rateOverTimeMultiplier - (oldRates[i] * 0.01f * 0.01f)) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeToPositiveAreaRadiusTest()
        {
            // Given a valid confetti machine with particle systems and a new area radius,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newRadius = particleSystems[0].shape.radius + 17.53f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(newRadius);

            // Then the radius is changed in all provided particle systems.
            foreach(ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(Math.Abs(particleSystem.shape.radius - newRadius) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeToNegativeAreaRadiusTest()
        {
            // Given a valid confetti machine with particle systems and a new negative area radius,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newRadius = -17.53f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(newRadius);

            // Then the radius is changed to 0.01f (lowest possible value) in all provided particle systems.
            foreach(ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(Math.Abs(particleSystem.shape.radius - 0.01f) < 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ChangeToZeroAreaRadiusTest()
        {
            // Given a valid confetti machine with particle systems and a area radius equals zero,
            GameObject machineObject = Object.Instantiate(Resources.Load<GameObject>(pathToDefaultPrefab));
            ConfettiMachine confettiMachine = machineObject.GetComponent<ConfettiMachine>();

            ParticleSystem[] particleSystems = machineObject.GetComponentsInChildren<ParticleSystem>(true);
            Assert.IsTrue(particleSystems.Length > 0);

            float newRadius = 0f;

            // When I change the area radius,
            confettiMachine.ChangeAreaRadius(newRadius);

            // Then the radius is changed to 0.01f (lowest possible value) in all provided particle systems.
            foreach(ParticleSystem particleSystem in particleSystems)
            {
                Assert.IsTrue(Math.Abs(particleSystem.shape.radius - 0.01f) < 0.001f);
            }

            yield break;
        }
    }
}
