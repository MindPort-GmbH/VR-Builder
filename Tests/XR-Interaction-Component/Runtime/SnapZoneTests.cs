using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Editor.XRInteraction;
using VRBuilder.Tests.Utils;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.XRInteraction.Tests
{
    public class SnapZoneTests : RuntimeTests
    {
        private class SnappablePropertyMock : SnappableProperty
        {
            public void SetSnapZone(SnapZoneProperty snapZone)
            {
                SnappedZone = snapZone;
            }

            public void SetSnapped(SnapZoneProperty snapZone)
            {
                snapZone.SnapZone.ForceSelect(gameObject.GetComponent<XRBaseInteractable>());
            }
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            XRTestUtilities.CreateInteractionManager();
        }

        [UnityTest]
        public IEnumerator HoverMeshShown()
        {
            // Given a snappable property with an AlwaysShowSnapzoneHighlight parameter set to false.
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"ShowSnapzoneHoverMeshes", true}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone zone = snapZoneProperty.SnapZone;

            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When activated
            condition.LifeCycle.Activate();
            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlightAlwaysActive variable should be false
            Assert.IsTrue(snapZoneProperty.SnapZone.showInteractableHoverMeshes);
        }

        [UnityTest]
        public IEnumerator HoverMeshNotShown()
        {
            // Given a snappable property with an AlwaysShowSnapzoneHighlight parameter set to false.
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"ShowSnapzoneHoverMeshes", false}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone zone = snapZoneProperty.SnapZone;

            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When activated
            condition.LifeCycle.Activate();
            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlightAlwaysActive variable should be false
            Assert.IsFalse(snapZoneProperty.SnapZone.showInteractableHoverMeshes);
        }

        [UnityTest]
        public IEnumerator HighlightObjectShown()
        {
            // Given a snappable property with an AlwaysShowSnapzoneHighlight parameter set to true
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test",  new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"ShowSnapzoneHighlightObject", true}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone zone = snapZoneProperty.SnapZone;

            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            yield return null;

            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlightAlwaysActive variable should be true
            Assert.IsTrue(snapZoneProperty.SnapZone.ShowHighlightObject);
        }

        [UnityTest]
        public IEnumerator HighlightObjectNotShown()
        {
            // Given a snappable property with a snapzone highlight deactivated parameter active.
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"ShowSnapzoneHighlightObject", false}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone zone = snapZoneProperty.SnapZone;

            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            yield return null;

            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlight is still inactive.
            Assert.IsFalse(snapZoneProperty.SnapZone.ShowHighlightObject);
        }

        [UnityTest]
        public IEnumerator HighlightColorCanBeChanged()
        {
            // Given a snappable property with a highlight color changed
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();
            Material testMaterial = new Material(Shader.Find("Standard"));
            testMaterial.color = Color.yellow;

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test",  new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"HighlightMaterial", testMaterial}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone zone = snapZoneProperty.SnapZone;

            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            yield return null;
            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlight color changed properly
            Assert.AreEqual(Color.yellow, snapZoneProperty.SnapZone.HighlightMeshMaterial.color);
        }

        [UnityTest]
        public IEnumerator CompleteOnTargetedSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Emit grabbed event
            mockedProperty.SetSnapZone(snapZoneProperty);
            mockedProperty.SetSnapped(snapZoneProperty);

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator DontCompleteOnWrongSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZoneProperty wrongSnapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Emit grabbed event
            mockedProperty.SetSnapZone(wrongSnapZoneProperty);
            mockedProperty.SetSnapped(wrongSnapZoneProperty);

            int frameCountEnd = Time.frameCount + 5;
            while (Time.frameCount <= frameCountEnd)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "Condition should not be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenSnappedOnStart()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenSnappedOnActivationWithTargetSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator DontCompleteWhenSnappedWrongOnActivation()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZoneProperty wrongSnapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(wrongSnapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            int frameCountEnd = Time.frameCount + 5;
            while (Time.frameCount <= frameCountEnd)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "SnappedCondition should not be complete!");
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a snapped condition
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then the condition is completed and the object is snapped.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);

            int beforeSnapped = Time.frameCount;

            yield return new WaitUntil(() => snapZoneProperty.IsObjectSnapped);

            int afterSnapped = Time.frameCount;

            Assert.IsTrue(snapZoneProperty.IsObjectSnapped);
            Assert.IsTrue(afterSnapped - beforeSnapped <= 3);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a snapped condition
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);

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
            Assert.IsFalse(snapZoneProperty.IsObjectSnapped);
        }

        [Test]
        public void SetSnapZoneWithSnapZoneSettings()
        {
            // Given a snap zone
            SnapZoneSettings settings = SnapZoneSettings.Settings;
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZone snapZone = snapZoneProperty.SnapZone;

            // When the snap zone settings are modified and the changes applied to the snap zone
            InteractionLayerMask testLayerMask = 0;
            Color testHighlightColor = Color.green;
            Color testValidationColor = Color.blue;
            Color testInvalidColor = Color.red;

            Assert.NotNull(settings);
#if XRIT_0_10_OR_NEWER
            Assert.That(snapZone.interactionLayers != testLayerMask);
#else
            Assert.That(snapZone.InteractionLayerMask != testLayerMask);
#endif
            Assert.That(snapZone.HighlightMeshMaterial.color != testHighlightColor);
            Assert.That(snapZone.ValidationMaterial.color != testValidationColor);

            settings.InteractionLayerMask = testLayerMask;
            settings.HighlightColor = testHighlightColor;
            settings.ValidationColor = testValidationColor;
            settings.InvalidColor = testInvalidColor;

            settings.ApplySettingsToSnapZone(snapZone);

            // Then the snap zone is updated.
#if XRIT_0_10_OR_NEWER
            Assert.That(snapZone.interactionLayers == testLayerMask);
#else
            Assert.That(snapZone.InteractionLayerMask == testLayerMask);
#endif
            Assert.That(snapZone.HighlightMeshMaterial.color == testHighlightColor);
            Assert.That(snapZone.ValidationMaterial.color == testValidationColor);
            Assert.That(snapZone.InvalidMaterial.color == testInvalidColor);
        }

        private SnappablePropertyMock CreateSnappablePropertyMock()
        {
            GameObject snappable = new GameObject("Target");
            snappable.transform.position = new Vector3(10, 10, 10);
            snappable.AddComponent<SphereCollider>().isTrigger = false;
            SnappablePropertyMock property = snappable.AddComponent<SnappablePropertyMock>();
            return property;
        }

        private SnapZoneProperty CreateSnapZoneProperty()
        {
            GameObject snapZoneObject = new GameObject("SnapZone");
            snapZoneObject.AddComponent<SphereCollider>().isTrigger = true;
            SnapZoneProperty property = snapZoneObject.AddComponent<SnapZoneProperty>();

            return property;
        }
    }
}
