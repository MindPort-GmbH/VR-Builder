using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Tests.RuntimeUtils;
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

        [UnityTest]
        public IEnumerator ObjectReferenceGetsUpdated()
        {
            // Given a Scale behavior with an obsolete reference,
            string uniqueName = "ScaledObject";
            GameObject scaledObject = new GameObject("ScaledObject");
            ObsoleteProcessSceneObject processSceneObject = scaledObject.AddComponent<ObsoleteProcessSceneObject>();
            processSceneObject.SetUniqueName(uniqueName);

            ScalingBehavior scalingBehavior = new ScalingBehavior();
#pragma warning disable CS0618 // Type or member is obsolete
            scalingBehavior.Data.Target = new SceneObjectReference(uniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            // If I run EntityDataUpdater on it,
            EntityDataUpdater updater = new EntityDataUpdater();
            updater.UpdateData(scalingBehavior);

            // Then the reference is updated to the correct type.
            Assert.IsTrue(scalingBehavior.Data.Targets.Guids.Count == 1);
            Assert.IsTrue(scalingBehavior.Data.Targets.Values.Count() == 1);
            Assert.AreEqual(processSceneObject, scalingBehavior.Data.Targets.Values.First());

            // Cleanup
            GameObject.DestroyImmediate(scaledObject);
            yield return null;
        }
    }
}