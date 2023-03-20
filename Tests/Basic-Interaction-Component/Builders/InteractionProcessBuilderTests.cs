using System;
using System.Collections;
using System.Linq;
using VRBuilder.BasicInteraction;
using VRBuilder.Core.Properties;
using VRBuilder.BasicInteraction.Builders;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Builder;
using VRBuilder.Tests.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using UnityEngine.Events;

namespace VRBuilder.Tests.Interaction
{
    public class InteractionProcessBuilderTests : RuntimeTests
    {
        private class DummySnapZoneProperty : LockableProperty, ISnapZoneProperty
        {
            protected override void InternalSetLocked(bool lockState)
            {
                throw new NotImplementedException();
            }
#pragma warning disable CS0067 // Disable "event is never used" warning.
            public event EventHandler<EventArgs> ObjectSnapped;
            public event EventHandler<EventArgs> ObjectUnsnapped;
#pragma warning restore CS0067

            
            public bool IsObjectSnapped { get; }
            
            public ISnappableProperty SnappedObject { get; set; }
            
            public GameObject SnapZoneObject { get; }
            
            public void Configure(IMode mode)
            {
                throw new NotImplementedException();
            }
        }
        
        private class DummySnappableProperty : ProcessSceneObjectProperty, ISnappableProperty
        {
#pragma warning disable CS0067 // Disable "event is never used" warning.
            public event EventHandler<EventArgs> Snapped;
            public event EventHandler<EventArgs> Unsnapped;
#pragma warning restore CS0067
            
            public bool IsSnapped { get; }
            
            public bool LockObjectOnSnap { get; }
            
            public ISnapZoneProperty SnappedZone { get; set; }

            public UnityEvent<SnappablePropertyEventArgs> OnSnapped => throw new NotImplementedException();

            public UnityEvent<SnappablePropertyEventArgs> OnUnsnapped => throw new NotImplementedException();

            public void FastForwardSnapInto(ISnapZoneProperty snapZone)
            {
                throw new NotImplementedException();
            }
        }

        private class DummyUsableProperty : LockableProperty, IUsableProperty
        {
            protected override void InternalSetLocked(bool lockState)
            {
                throw new NotImplementedException();
            }
            
#pragma warning disable CS0067 // Disable "event is never used" warning.
            public event EventHandler<EventArgs> UsageStarted;
            public event EventHandler<EventArgs> UsageStopped;
#pragma warning restore CS0067
            
            public bool IsBeingUsed { get; }
             
            public void FastForwardUse()
            {
                throw new NotImplementedException();
            }
        }

        private class DummyTouchableProperty : LockableProperty, ITouchableProperty
        {
            protected override void InternalSetLocked(bool lockState)
            {
                throw new NotImplementedException();
            }
            
#pragma warning disable CS0067 // Disable "event is never used" warning.
            public event EventHandler<EventArgs> Touched;
            public event EventHandler<EventArgs> Untouched;
#pragma warning restore CS0067
            
            public bool IsBeingTouched { get; }

            public UnityEvent<TouchablePropertyEventArgs> OnTouched => throw new NotImplementedException();

            public UnityEvent<TouchablePropertyEventArgs> OnUntouched => throw new NotImplementedException();

            public void FastForwardTouch()
            {
                throw new NotImplementedException();
            }

            public void ForceSetTouched(bool isTouched)
            {
                throw new NotImplementedException();
            }
        }
        
        [UnityTest]
        public IEnumerator BuildingSnapZonePutTest()
        {
            // Given a snap zone and snappable property and a builder for a process with a PutIntoSnapZone default step
            GameObject snapZoneGo = new GameObject("SnapZone");
            ProcessSceneObject snapZone = snapZoneGo.AddComponent<ProcessSceneObject>();
            snapZoneGo.AddComponent<DummySnapZoneProperty>();
            snapZone.ChangeUniqueName("SnapZone");

            GameObject putGo = new GameObject("Puttable");
            ProcessSceneObject objectToPut = putGo.AddComponent<ProcessSceneObject>();
            putGo.AddComponent<DummySnappableProperty>();
            objectToPut.ChangeUniqueName("ToPut");

            LinearProcessBuilder builder = new LinearProcessBuilder("TestProcess")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.PutIntoSnapZone("TestSnapZonePutStep", "SnapZone", "ToPut")));

            // When you build a process with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with a SnappedCondition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestSnapZonePutStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is SnappedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as SnappedCondition).Data.Target.Value.SceneObject, objectToPut));

            // Cleanup
            Object.DestroyImmediate(snapZoneGo);
            Object.DestroyImmediate(putGo);

            return null;
        }
        
        [UnityTest]
        public IEnumerator BuildingUseTest()
        {
            // Given a usable property and a builder for a process with Use default step
            GameObject usableGo = new GameObject("Usable");
            ProcessSceneObject usable = usableGo.AddComponent<ProcessSceneObject>();
            usableGo.AddComponent<DummyUsableProperty>();
            usable.ChangeUniqueName("Usable");

            LinearProcessBuilder builder = new LinearProcessBuilder("TestProcess")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.Use("TestUseStep", "Usable")));

            // When you build a process with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with an UsedCondition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestUseStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is UsedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as UsedCondition).Data.UsableProperty.Value.SceneObject, usable));

            // Cleanup
            Object.DestroyImmediate(usableGo);

            return null;
        }
        
        [UnityTest]
        public IEnumerator BuildingTouchTest()
        {
            // Given you have a touchable property and a builder for a process with Touch default step
            GameObject touchableGo = new GameObject("Touchable");
            ProcessSceneObject touchable = touchableGo.AddComponent<ProcessSceneObject>();
            touchableGo.AddComponent<DummyTouchableProperty>();
            touchable.ChangeUniqueName("Touchable");

            LinearProcessBuilder builder = new LinearProcessBuilder("TestProcess")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.Touch("TestTouchStep", "Touchable")));

            // When you build a process with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with a TouchCOndition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestTouchStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is TouchedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as TouchedCondition).Data.TouchableProperty.Value.SceneObject, touchable));

            // Cleanup
            Object.DestroyImmediate(touchableGo);

            return null;
        }
    }
}