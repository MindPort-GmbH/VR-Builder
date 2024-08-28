using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that highlights the target <see cref="ISceneObject"/> with an outline in the specified color until the behavior is deactivated.
    /// </summary>
    [DataContract(IsReference = true)]
    public class OutlineObjectsBehavior : Behavior<OutlineObjectsBehavior.EntityData>, IOptional
    {
        /// <summary>
        /// "Outline object" behavior's data.
        /// </summary>        
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// <see cref="ModeParameter{T}"/> of the highlight color.
            /// Process modes can change the highlight color.
            /// </summary>
            public ModeParameter<Color> CustomOutlineColor { get; set; }

            /// <summary>
            /// Highlight color set in the Step Inspector.
            /// </summary>
            [DataMember]
            [DisplayName("Color")]
            public Color OutlineColor
            {
                get { return CustomOutlineColor.Value; }

                set { CustomOutlineColor = new ModeParameter<Color>("OutlineColor", value); }
            }

            /// <summary>
            /// Target scene objects to be outlined.
            /// </summary>
            [DataMember]
            [DisplayName("Objects")]
            public MultipleScenePropertyReference<IOutlineProperty> TargetObjects { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Outline {TargetObjects}";
        }

        private class ActivatingProcess : InstantProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IOutlineProperty property in Data.TargetObjects.Values)
                {
                    property?.ShowOutline(Data.OutlineColor);
                }
            }
        }

        private class DeactivatingProcess : InstantProcess<EntityData>
        {
            public DeactivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IOutlineProperty property in Data.TargetObjects.Values)
                {
                    property?.HideOutline();
                }
            }
        }

        private class EntityConfigurator : Configurator<EntityData>
        {
            /// <inheritdoc />
            public override void Configure(IMode mode, Stage stage)
            {
                Data.CustomOutlineColor.Configure(mode);
            }

            public EntityConfigurator(EntityData data) : base(data)
            {
            }
        }

        [JsonConstructor, Preserve]
        public OutlineObjectsBehavior() : this(Guid.Empty, new Color32(119, 245, 199, 255))
        {
        }

        public OutlineObjectsBehavior(Guid objectId, Color highlightColor)
        {
            Data.TargetObjects = new MultipleScenePropertyReference<IOutlineProperty>(objectId);
            Data.OutlineColor = highlightColor;
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data);
        }

        /// <inheritdoc />
        protected override IConfigurator GetConfigurator()
        {
            return new EntityConfigurator(Data);
        }
    }
}
