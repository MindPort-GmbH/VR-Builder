using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.BasicInteraction;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Tests.Utils;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.XRInteraction.Tests.Behaviors
{
    public class HighlightObjectTests : RuntimeTests
    {
        internal class DummyHighlightProperty : HighlightProperty
        {
            /// <inheritdoc/>
            public override void Highlight(Color highlightColor)
            {
                base.Highlight(highlightColor);
                IsHighlighted = true;
            }

            /// <inheritdoc/>
            public override void Unhighlight()
            {
                base.Unhighlight();
                IsHighlighted = false;
            }
        }

        private const string targetName = "XR Interactable";

        [UnityTest]
        public IEnumerator CreateHighlightPropertyWhitoutInteractableHighlighter()
        {
            // Given an empty GameObject.
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;

            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                Assert.That(interactable.GetComponent(highlighterImplementation) == null);
            }

            // When we add a DummyHighlightProperty to it.
            interactable.AddComponent<DummyHighlightProperty>();

            bool highlighterExists = ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>().Any(highlighterImplementation => interactable.GetComponent(highlighterImplementation) != null);

            Assert.IsTrue(highlighterExists);

            yield break;
        }

        [UnityTest]
        public IEnumerator CreateDummyHighlightPropertyWhitInteractableHighlighter()
        {
            // Given an empty GameObject.
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;

            Assert.That(interactable.GetComponent<InteractableObject>() == null);
            Assert.That(interactable.GetComponent<InteractableHighlighter>() == null);
            Assert.That(interactable.GetComponent<HighlightProperty>() == null);
            Assert.That(interactable.GetComponent<ProcessSceneObject>() == null);
            Assert.That(interactable.GetComponent<Rigidbody>() == null);

            // When we add a DummyHighlightProperty to it.
            interactable.AddComponent<InteractableHighlighter>();
            interactable.AddComponent<DummyHighlightProperty>();

            // Then it also get all required dependencies.
            Assert.That(interactable.GetComponent<InteractableObject>() != null);
            Assert.That(interactable.GetComponent<InteractableHighlighter>() != null);
            Assert.That(interactable.GetComponent<HighlightProperty>() != null);
            Assert.That(interactable.GetComponent<ProcessSceneObject>() != null);
            Assert.That(interactable.GetComponent<Rigidbody>() != null);

            Assert.AreEqual(1, interactable.GetComponents<InteractableObject>().Length);
            Assert.AreEqual(1, interactable.GetComponents<InteractableHighlighter>().Length);
            Assert.AreEqual(1, interactable.GetComponents<HighlightProperty>().Length);
            Assert.AreEqual(1, interactable.GetComponents<ProcessSceneObject>().Length);
            Assert.AreEqual(1, interactable.GetComponents<Rigidbody>().Length);

            yield break;
        }

        [UnityTest]
        public IEnumerator StepWithHighlightBehavior()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty in a linear chapter.
            Color highlightColor = Color.yellow;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);

            TestLinearChapterBuilder chapterBuilder = TestLinearChapterBuilder.SetupChapterBuilder(1);
            chapterBuilder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlightBehavior);

            Chapter chapter = chapterBuilder.Build();
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter.
            chapter.LifeCycle.Activate();

            while (highlightBehavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageInStep = highlightBehavior.LifeCycle.Stage;
            bool objectHighlightedActiveInStep = highlightProperty.IsHighlighted;
            Color? colorInStep = highlightProperty.CurrentHighlightColor;

            chapter.Data.FirstStep.LifeCycle.Deactivate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageAfterStep = highlightBehavior.LifeCycle.Stage;
            bool objectHighlightedActiveAfterStep = highlightProperty.IsHighlighted;
            Color? colorAfterStep = highlightProperty.CurrentHighlightColor;

            // Then the highlight behavior is active during the step and inactive after it.
            Assert.AreEqual(Stage.Active, highlightStageInStep, "The HighlightObjectBehavior should be active during step");
            Assert.IsTrue(objectHighlightedActiveInStep, "The HighlightProperty should be active during step");
            Assert.AreEqual(highlightColor, colorInStep, $"The highlight color should be {highlightColor}");

            Assert.AreEqual(Stage.Inactive, highlightStageAfterStep, "The HighlightObjectBehavior should be deactivated after step");
            Assert.IsFalse(objectHighlightedActiveAfterStep, "The HighlightProperty should be inactive after step");
            Assert.IsNull(colorAfterStep, "The highlight color should be null after deactivation of step.");
        }

        [UnityTest]
        public IEnumerator HighlightColorIsSetByParameter()
        {
            // Given a HighlightProperty with a HighlightColor parameter set,
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();
            Color highlightColor = Color.green;

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"HighlightColor", highlightColor}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);
            highlightBehavior.Configure(testRuntimeConfiguration.Modes.CurrentMode);

            // When we activate it.
            highlightBehavior.LifeCycle.Activate();

            // Then the highlight color is changed.
            Assert.AreEqual(highlightColor, highlightBehavior.Data.HighlightColor);
            Assert.AreEqual(highlightColor, highlightProperty.CurrentHighlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlightBehavior.Data.TargetObjects.Values.First()).CurrentHighlightColor);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty.
            Color highlightColor = Color.cyan;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);

            // When we mark it to fast-forward.
            highlightBehavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, highlightBehavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty.
            Color highlightColor = Color.red;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);

            // When we mark it to fast-forward and activate it.
            highlightBehavior.LifeCycle.MarkToFastForward();
            highlightBehavior.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, highlightBehavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty.
            Color highlightColor = Color.white;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);

            // When we mark it to fast-forward, activate, and deactivate it.
            highlightBehavior.LifeCycle.MarkToFastForward();
            highlightBehavior.LifeCycle.Activate();
            highlightBehavior.LifeCycle.Deactivate();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, highlightBehavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty.
            Color highlightColor = Color.blue;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);

            // When we mark it active and fast-forward.
            highlightBehavior.LifeCycle.Activate();
            highlightBehavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, highlightBehavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a HighlightObjectBehavior with a HighlightProperty.
            Color highlightColor = Color.black;
            GameObject interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.name = targetName;
            DummyHighlightProperty highlightProperty = interactable.AddComponent<DummyHighlightProperty>();
            HighlightObjectBehavior highlightBehavior = new HighlightObjectBehavior(highlightProperty, highlightColor);
            highlightBehavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            highlightBehavior.LifeCycle.Activate();

            while (highlightBehavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                highlightBehavior.Update();
            }

            highlightBehavior.LifeCycle.Deactivate();

            // When we mark it to fast-forward.
            highlightBehavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, highlightBehavior.LifeCycle.Stage);
        }
    }
}
