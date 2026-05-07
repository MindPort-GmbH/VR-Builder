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
    /// Behavior that highlights the target <see cref="ISceneObject"/> with the specified color until the behavior is being deactivated.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/highlight-object")]
    public class HighlightObjectBehavior : ColorHighlightBehaviorBase<HighlightObjectBehavior.EntityData, IHighlightProperty>, IObjectHighlightBehavior
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
