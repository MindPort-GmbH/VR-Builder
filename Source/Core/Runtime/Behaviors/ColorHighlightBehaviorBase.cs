using System;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Shared base behavior for color-based highlighting.
    /// </summary>
    /// <typeparam name="TData">Behavior data type.</typeparam>
    /// <typeparam name="TProperty">Target property type.</typeparam>
    [DataContract(IsReference = true)]
    public abstract class ColorHighlightBehaviorBase<TData, TProperty> : Behavior<TData>, IOptional, IHighlightingBehavior
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

        protected ColorHighlightBehaviorBase()
        {
        }

        protected ColorHighlightBehaviorBase(Guid objectId, Color defaultColor)
        {
            Data.TargetObjects = new MultipleScenePropertyReference<TProperty>(objectId);
            Data.Color = defaultColor;
        }

        /// <summary>
        /// Applies the highlight state to a single property.
        /// </summary>
        protected abstract void ApplyHighlight(TProperty property, Color color);

        /// <summary>
        /// Removes the highlight state from a single property.
        /// </summary>
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
}
