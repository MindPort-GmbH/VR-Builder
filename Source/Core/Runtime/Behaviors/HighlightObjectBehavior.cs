using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Marker interface for all behaviors that highlight scene objects.
    /// </summary>
    public interface IHighlightingBehavior : IBehavior
    {
    }

    /// <summary>
    /// Marker interface for object highlight behaviors.
    /// </summary>
    public interface IObjectHighlightBehavior : IHighlightingBehavior
    {
    }

    /// <summary>
    /// Common data contract for color-based highlight behaviors.
    /// </summary>
    /// <typeparam name="TProperty">Referenced property type.</typeparam>
    public interface IColorHighlightBehaviorData<TProperty> : IBehaviorData
        where TProperty : class, ISceneObjectProperty
    {
        /// <summary>
        /// Mode parameter of the configured highlight color.
        /// </summary>
        ModeParameter<Color> CustomColor { get; set; }

        /// <summary>
        /// Highlight color in the step inspector.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Target scene objects.
        /// </summary>
        MultipleScenePropertyReference<TProperty> TargetObjects { get; set; }
    }

    /// <summary>
    /// Common base behavior for color-based highlighting.
    /// </summary>
    /// <typeparam name="TData">Behavior data type.</typeparam>
    /// <typeparam name="TProperty">Target property type.</typeparam>
    [DataContract(IsReference = true)]
    public abstract class AbstractColorHighlightBehavior<TData, TProperty> : Behavior<TData>, IOptional, IHighlightingBehavior
        where TData : class, IBehaviorData, IColorHighlightBehaviorData<TProperty>, new()
        where TProperty : class, ISceneObjectProperty
    {
        private class ActivatingProcess : InstantProcess<TData>
        {
            private readonly Action<TProperty, Color> applyHighlight;

            public ActivatingProcess(TData data, Action<TProperty, Color> applyHighlight) : base(data)
            {
                this.applyHighlight = applyHighlight;
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (TProperty property in Data.TargetObjects.Values)
                {
                    applyHighlight(property, Data.Color);
                }
            }
        }

        private class DeactivatingProcess : InstantProcess<TData>
        {
            private readonly Action<TProperty> removeHighlight;

            public DeactivatingProcess(TData data, Action<TProperty> removeHighlight) : base(data)
            {
                this.removeHighlight = removeHighlight;
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (TProperty property in Data.TargetObjects.Values)
                {
                    removeHighlight(property);
                }
            }
        }

        private class EntityConfigurator : Configurator<TData>
        {
            public EntityConfigurator(TData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Configure(IMode mode, Stage stage)
            {
                Data.CustomColor.Configure(mode);
            }
        }

        protected AbstractColorHighlightBehavior()
        {
        }

        protected AbstractColorHighlightBehavior(Guid objectId, Color defaultColor)
        {
            Data.TargetObjects = new MultipleScenePropertyReference<TProperty>(objectId);
            Data.Color = defaultColor;
        }

        protected abstract void ApplyHighlight(TProperty property, Color color);

        protected abstract void RemoveHighlight(TProperty property);

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data, ApplyHighlight);
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data, RemoveHighlight);
        }

        /// <inheritdoc />
        protected override IConfigurator GetConfigurator()
        {
            return new EntityConfigurator(Data);
        }
    }

    /// <summary>
    /// Behavior that highlights the target <see cref="ISceneObject"/> with the specified color until the behavior is being deactivated.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/highlight-object")]
    public class HighlightObjectBehavior : AbstractColorHighlightBehavior<HighlightObjectBehavior.EntityData, IHighlightProperty>, IObjectHighlightBehavior
    {
        private static readonly Color32 defaultHighlightColor = new Color32(231, 64, 255, 126);

        /// <summary>
        /// "Highlight object" behavior's data.
        /// </summary>
        [DisplayName("Highlight Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData, IColorHighlightBehaviorData<IHighlightProperty>
        {
            private ModeParameter<Color> customColor;

            /// <summary>
            /// <see cref="ModeParameter{T}"/> of the highlight color.
            /// Process modes can change the highlight color.
            /// </summary>
            public ModeParameter<Color> CustomColor
            {
                get { return customColor ??= new ModeParameter<Color>("HighlightColor", defaultHighlightColor); }
                set { customColor = value; }
            }

            /// <summary>
            /// Highlight color set in the Step Inspector.
            /// </summary>
            [DataMember(Name = "HighlightColor")]
            [JsonProperty("HighlightColor")]
            [DisplayName("Color")]
            public Color Color
            {
                get { return CustomColor.Value; }

                set { CustomColor = new ModeParameter<Color>("HighlightColor", value); }
            }

            /// <summary>
            /// Target scene object to be highlighted.
            /// </summary>
            [DataMember]
            [DisplayName("Objects")]
            public MultipleScenePropertyReference<IHighlightProperty> TargetObjects { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Highlight {TargetObjects}";

        }

        [JsonConstructor, Preserve]
        public HighlightObjectBehavior() : this(Guid.Empty, defaultHighlightColor)
        {
        }

        public HighlightObjectBehavior(Guid objectId, Color highlightColor) : base(objectId, highlightColor)
        {
        }

        public HighlightObjectBehavior(IHighlightProperty target) : this(target, defaultHighlightColor)
        {
        }

        public HighlightObjectBehavior(IHighlightProperty target, Color highlightColor) : this(ProcessReferenceUtils.GetUniqueIdFrom(target), highlightColor)
        {
        }

        /// <inheritdoc />
        protected override void ApplyHighlight(IHighlightProperty property, Color color)
        {
            property?.Highlight(color);
        }

        /// <inheritdoc />
        protected override void RemoveHighlight(IHighlightProperty property)
        {
            property?.Unhighlight();
        }
    }
}
