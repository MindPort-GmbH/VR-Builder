//using NUnit.Framework;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.TestTools;
//using VRBuilder.BasicInteraction;
//using VRBuilder.BasicInteraction.Behaviors;
//using VRBuilder.Core;
//using VRBuilder.Core.Behaviors;
//using VRBuilder.Tests.Utils;
//using VRBuilder.XRInteraction.Properties;

//namespace VRBuilder.Tests.Interaction
//{
//    public class UnsnapBehaviorTests : BehaviorTests
//    {
//        protected override IBehavior CreateDefaultBehavior()
//        {
//            return new UnsnapBehavior();
//        }

//        private SnapZoneProperty CreateSnapZone(string name = "Snap Zone")
//        {
//            GameObject snapZoneObject = new GameObject(name);
//            Collider collider = snapZoneObject.AddComponent<SphereCollider>();
//            collider.isTrigger = true;
//            SnapZoneProperty snapZoneProperty = snapZoneObject.AddComponent<SnapZoneProperty>();

//            return snapZoneProperty;
//        }

//        private SnappableProperty CreateSnappableObject(string name = "Snappable Object")
//        {
//            GameObject snappableObject = new GameObject(name);
//            return snappableObject.AddComponent<SnappableProperty>();
//        }

//        [UnityTest]
//        public IEnumerator CreateByName()
//        {
//            // Given two process scene objects and the required parameters,
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();
//            string snappablePropertyName = snappableProperty.SceneObject.UniqueName;
//            string snapZoneName = snapZoneProperty.SceneObject.UniqueName;

//            // When we create Unsnap Behavior and pass process scene objects by their unique name,
//            UnsnapBehavior behavior = new UnsnapBehavior(snappablePropertyName, snapZoneName);

//            // Then all properties are properly assigned
//            Assert.AreEqual(snapZoneProperty, behavior.Data.SnapZone.Value);
//            Assert.AreEqual(snappableProperty, behavior.Data.SnappedObject.Value);

//            yield return null;
//        }

//        [UnityTest]
//        public IEnumerator CreateByReference()
//        {
//            // Given two process scene objects and the required parameters,
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            // When we create Unsnap Behavior and pass process scene objects by reference,
//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, snapZoneProperty);

//            // Then all properties are properly assigned
//            Assert.AreEqual(snapZoneProperty, behavior.Data.SnapZone.Value);
//            Assert.AreEqual(snappableProperty, behavior.Data.SnappedObject.Value);

//            yield return null;
//        }

//        [UnityTest]
//        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, null);

//            // When we mark it to fast-forward and activate it,
//            behavior.LifeCycle.MarkToFastForward();
//            behavior.LifeCycle.Activate();

//            // Then it autocompletes immediately.
//            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);

//            yield break;
//        }

//        [UnityTest]
//        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, null);

//            // When we mark it to fast-forward, activate and immediately deactivate it,
//            behavior.LifeCycle.MarkToFastForward();
//            behavior.LifeCycle.Activate();

//            while (behavior.LifeCycle.Stage != Stage.Active)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            behavior.LifeCycle.Deactivate();

//            // Then it autocompletes immediately.
//            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator FastForwardActivatingBehavior()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, null);

//            behavior.LifeCycle.Activate();

//            while (behavior.LifeCycle.Stage != Stage.Activating)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // When we mark it to fast-forward,
//            behavior.LifeCycle.MarkToFastForward();

//            // Then it autocompletes immediately.
//            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator FastForwardDeactivatingBehavior()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, null);

//            behavior.LifeCycle.Activate();

//            while (behavior.LifeCycle.Stage != Stage.Active)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            behavior.LifeCycle.Deactivate();

//            while (behavior.LifeCycle.Stage != Stage.Deactivating)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // When we mark it to fast-forward,
//            behavior.LifeCycle.MarkToFastForward();

//            // Then it autocompletes immediately.
//            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator UnsnapByObject()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, null);

//            // When an unsnap behavior is activated,

//            behavior.LifeCycle.Activate();

//            while (Stage.Active != behavior.LifeCycle.Stage)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // Then the object is unsnapped.
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator UnsnapBySnapZone()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(null, snapZoneProperty);

//            // When an unsnap behavior is activated,

//            behavior.LifeCycle.Activate();

//            while (Stage.Active != behavior.LifeCycle.Stage)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // Then the object is unsnapped.
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator UnsnapByObjectAndSnapZone()
//        {
//            // Given a snapped object and an unsnap behavior,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty snappableProperty = CreateSnappableObject();

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappableProperty);

//            yield return null;

//            bool wasObjectSnapped = snappableProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappableProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(snappableProperty, snapZoneProperty);

//            // When an unsnap behavior is activated,

//            behavior.LifeCycle.Activate();

//            while (Stage.Active != behavior.LifeCycle.Stage)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // Then the object is unsnapped.
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.IsNull(snapZone.SnappedObject);
//            Assert.IsNull(snappableProperty.SnappedZone);
//        }

//        [UnityTest]
//        public IEnumerator NoUnsnapNonMatchingObject()
//        {
//            // Given a snapped object and an unsnap behavior specifying a different object,           
//            SnapZoneProperty snapZoneProperty = CreateSnapZone();
//            SnappableProperty notSnappedProperty = CreateSnappableObject("Not Snapped Object");
//            SnappableProperty snappedProperty = CreateSnappableObject("Snapped Object");

//            ISnapZone snapZone = snapZoneProperty.GetComponent<ISnapZone>();

//            snapZone.ForceSnap(snappedProperty);

//            yield return null;

//            bool wasObjectSnapped = snappedProperty.Equals(snapZone.SnappedObject) && snapZoneProperty.Equals(snappedProperty.SnappedZone);

//            UnsnapBehavior behavior = new UnsnapBehavior(notSnappedProperty, snapZoneProperty);

//            // When an unsnap behavior is activated,

//            behavior.LifeCycle.Activate();

//            while (Stage.Active != behavior.LifeCycle.Stage)
//            {
//                yield return null;
//                behavior.Update();
//            }

//            // Then the object is not unsnapped.
//            Assert.IsTrue(wasObjectSnapped);
//            Assert.AreEqual(snappedProperty, snapZone.SnappedObject);
//            Assert.AreEqual(snapZoneProperty, snappedProperty.SnappedZone);
//        }
//    }
//}