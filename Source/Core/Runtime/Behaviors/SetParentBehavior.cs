using Newtonsoft.Json;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// This behavior changes the parent of a game object in the scene hierarchy. It can accept a null parent, in which case the object will be unparented.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/set-parent")]
    public class SetParentBehavior : Behavior<SetParentBehavior.EntityData>
    {
        [DisplayName("Set Parent")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Process object to reparent.
            /// </summary>
            [DataMember]
            public SingleSceneObjectReference TargetObject { get; set; }

            /// <summary>
            /// New parent game object.
            /// </summary>
            [DataMember]
            public SingleSceneObjectReference ParentObject { get; set; }

            /// <summary>
            /// If true, the object will be moved to the parent's transform.
            /// </summary>
            [DataMember]
            [DisplayName("Snap to parent transform")]
            public bool SnapToParentTransform { get; set; }

            public Metadata Metadata { get; set; }

            [IgnoreDataMember]
            public string Name => ParentObject.HasValue() ? $"Make {TargetObject} child of {ParentObject}" : $"Unparent {TargetObject}";
        }

        [JsonConstructor, Preserve]
        public SetParentBehavior() : this(Guid.Empty, Guid.Empty)
        {
        }

        public SetParentBehavior(ISceneObject target, ISceneObject parent, bool snapToParentTransform = false) : this(ProcessReferenceUtils.GetUniqueIdFrom(target), ProcessReferenceUtils.GetUniqueIdFrom(parent), snapToParentTransform)
        {
        }

        public SetParentBehavior(Guid target, Guid parent, bool snapToParentTransform = false)
        {
            Data.TargetObject = new SingleSceneObjectReference(target);
            Data.ParentObject = new SingleSceneObjectReference(parent);
            Data.SnapToParentTransform = snapToParentTransform;
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                if (Data.ParentObject.Value == null)
                {
                    Data.TargetObject.Value.GameObject.transform.SetParent(null);
                }
                else
                {
                    if (HasScaleIssues())
                    {
                        Debug.LogWarning($"'{Data.TargetObject.Value.GameObject.name}' is being parented to a hierarchy that has changes in rotation and scale. This may result in a distorted object after parenting.");
                    }

                    if (Data.SnapToParentTransform)
                    {
                        Data.TargetObject.Value.GameObject.transform.SetPositionAndRotation(Data.ParentObject.Value.GameObject.transform.position, Data.ParentObject.Value.GameObject.transform.rotation);
                    }

                    Data.TargetObject.Value.GameObject.transform.SetParent(Data.ParentObject.Value.GameObject.transform, true);
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }

            private bool HasScaleIssues()
            {
                Transform currentTransform = Data.TargetObject.Value.GameObject.transform;
                Transform parentTransform = Data.ParentObject.Value.GameObject.transform;

                bool changesScale = currentTransform.localScale != Vector3.one;
                bool changesRotation = currentTransform.rotation != parentTransform.rotation && Data.SnapToParentTransform == false;

                while (parentTransform != null)
                {
                    changesScale |= parentTransform.localScale != Vector3.one;

                    if (parentTransform.parent != null)
                    {
                        changesRotation |= parentTransform.rotation != parentTransform.parent.rotation;
                    }

                    parentTransform = parentTransform.parent;
                }

                return changesScale && changesRotation;
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
