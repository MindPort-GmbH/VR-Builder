using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Builders;
using VRBuilder.Core.Tests.Utils.Mocks;
using VRBuilder.Editor.ProcessUpgradeTool;

namespace VRBuilder.Core.Tests
{
    public class ProcessUpgradeToolTests : RuntimeTests
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

        public class ProcessVariableDataOwner : IDataOwner<ProcessVariableDataOwner.EntityData>
        {
            public ProcessVariableDataOwner()
            {
                Data = new EntityData();
            }

            [DataMember]
            public EntityData Data { get; set; }

            IData IDataOwner.Data => (IData)Data;

            [DataContract(IsReference = true)]
            public class EntityData : IData
            {
                [DataMember]
                public ProcessVariable<string> ProcessVariable { get; set; }

                public Metadata Metadata { get; set; }
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
        public IEnumerator UniqueObjectReferenceGetsUpdated()
        {
            // Given a Scale behavior with an obsolete reference,
            ProcessSceneObject processSceneObject = CreateObsoleteGameObject("ScaledObject");

            ScalingBehavior scalingBehavior = new ScalingBehavior();
#pragma warning disable CS0618 // Type or member is obsolete
            scalingBehavior.Data.Target = new SceneObjectReference(processSceneObject.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            // If I run EntityDataUpdater on it,
            ProcessUpgradeTool.UpdateDataRecursively(scalingBehavior);

            // Then the reference is updated to the correct type.
            Assert.IsTrue(scalingBehavior.Data.Targets.Guids.Count == 1);
            Assert.IsTrue(scalingBehavior.Data.Targets.Values.Count() == 1);
            Assert.AreEqual(processSceneObject, scalingBehavior.Data.Targets.Values.First());

            // Cleanup
            GameObject.DestroyImmediate(processSceneObject.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneObjectTagGetsUpdated()
        {
            // Given a enable component with tag behavior with an obsolete tag,
            Guid tag = Guid.NewGuid();
            ProcessSceneObject processSceneObject = CreateObsoleteGameObject("TestObject");
#pragma warning disable CS0618 // Type or member is obsolete
            SetComponentEnabledByTagBehavior behavior = new SetComponentEnabledByTagBehavior();
            behavior.Data.TargetTag = new SceneObjectTag<ISceneObject>(tag);
#pragma warning restore CS0618 // Type or member is obsolete

            // When I update it,
            ProcessUpgradeTool.UpdateDataRecursively(behavior);

            // Then the referece is updated.
            Assert.IsTrue(behavior.Data.TargetObjects.Guids.Count == 1);
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(behavior.Data.TargetTag.Guid, behavior.Data.TargetObjects.Guids.First());
#pragma warning restore CS0618 // Type or member is obsolete

            // Cleanup
            yield return null;
            GameObject.DestroyImmediate(processSceneObject.gameObject);
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
            ProcessUpgradeTool.UpdateDataRecursively(step);

            // Then the lockable properties are updated.
            Assert.IsTrue(lockablePropertyReference.TargetObject.HasValue());
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(objectToUnlock, lockablePropertyReference.TargetObject.Value);
#pragma warning restore CS0618 // Type or member is obsolete

            // Cleanup
            GameObject.DestroyImmediate(objectToUnlock.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ObsoleteEnableObjectWithTagBehaviorsAreUpdated()
        {
            // Given a step with an obsolete behavior,
            Guid tag = Guid.NewGuid();
#pragma warning disable CS0618 // Type or member is obsolete
            SetObjectsWithTagEnabledBehavior setObjectsEnabledBehavior = new SetObjectsWithTagEnabledBehavior(tag, false);
#pragma warning restore CS0618 // Type or member is obsolete
            BasicStepBuilder stepBuilder = new BasicStepBuilder("TestStep");
            stepBuilder.AddBehavior(setObjectsEnabledBehavior);
            Step step = stepBuilder.Build();
            Step referenceStep = step.Clone() as Step;

            // When it is updated,
            ProcessUpgradeTool.UpdateDataRecursively(step);

            // Then the behavior is replaced.
            Assert.AreEqual(referenceStep.Data.Behaviors.Data.Behaviors.Count(), step.Data.Behaviors.Data.Behaviors.Count());
            Assert.AreEqual(1, step.Data.Behaviors.Data.Behaviors.Count());

            IBehavior oldBehavior = referenceStep.Data.Behaviors.Data.Behaviors.First();
            IBehavior newBehavior = step.Data.Behaviors.Data.Behaviors.First();

            Assert.IsTrue(newBehavior is SetObjectsEnabledBehavior);
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsTrue(oldBehavior is SetObjectsWithTagEnabledBehavior);
            Assert.AreEqual(((SetObjectsWithTagEnabledBehavior)oldBehavior).Data.TargetObjects, ((SetObjectsEnabledBehavior)newBehavior).Data.TargetObjects);
#pragma warning restore CS0618 // Type or member is obsolete

            yield return null;
        }

        [UnityTest]
        public IEnumerator ProcessVariablesAreUpdated()
        {
            // Given a data owner with a process variable,
            string constValue = "ConstValue";
            ProcessSceneObject referencedObject = CreateObsoleteGameObject("StringDataProperty");
            TextDataProperty dataProperty = referencedObject.AddProcessProperty<TextDataProperty>() as TextDataProperty;
            ProcessVariable<string> processVariable = new ProcessVariable<string>();
#pragma warning disable CS0618 // Type or member is obsolete
            processVariable.PropertyReference = new ScenePropertyReference<IDataProperty<string>>(referencedObject.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete
            processVariable.ConstValue = constValue;
            processVariable.IsConst = true;

            ProcessVariableDataOwner dataOwner = new ProcessVariableDataOwner();
            dataOwner.Data.ProcessVariable = processVariable;

            // When it is updated,
            ProcessUpgradeTool.UpdateDataRecursively(dataOwner);

            // Then the values have changed.
            Assert.AreEqual(constValue, dataOwner.Data.ProcessVariable.ConstValue);
            Assert.AreEqual(true, dataOwner.Data.ProcessVariable.IsConst);
            Assert.AreEqual(referencedObject.Guid, dataOwner.Data.ProcessVariable.Property.Guids.First());

            yield return null;

        }
    }
}