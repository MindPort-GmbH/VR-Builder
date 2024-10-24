using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Conditions
{
    /// <summary>
    /// A condition that compares two <see cref="IDataProperty{T}"/>s and completes when the comparison returns true.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder-tutorials/states-data-add-on")]
    public class CompareValuesCondition<T> : Condition<CompareValuesCondition<T>.EntityData> where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// The data for a <see cref="CompareValuesCondition{T}"/>
        /// </summary>
        [DisplayName("Compare Values")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [HideInProcessInspector]
            public T LeftValue { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public SingleScenePropertyReference<IDataProperty<T>> LeftProperty { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use LeftProperty instead.")]
            [LegacyProperty(nameof(LeftProperty))]
            public ScenePropertyReference<IDataProperty<T>> LeftValueProperty { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool IsLeftConst { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public IOperationCommand<T, bool> Operation { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public T RightValue { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public SingleScenePropertyReference<IDataProperty<T>> RightProperty { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use RightProperty instead.")]
            [LegacyProperty(nameof(RightProperty))]
            public ScenePropertyReference<IDataProperty<T>> RightValueProperty { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool IsRightConst { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    string leftProperty = IsLeftConst ? LeftValue == null ? "[NULL]" : LeftValue.ToString() : LeftProperty.HasValue() ? LeftProperty.ToString() : "[NULL]";
                    string rightProperty = IsRightConst ? RightValue == null ? "[NULL]" : RightValue.ToString() : RightProperty.HasValue() ? RightProperty.ToString() : "[NULL]";

                    return $"Compare ({leftProperty} {Operation} {rightProperty})";
                }
            }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            protected override bool CheckIfCompleted()
            {
                T left = Data.IsLeftConst ? Data.LeftValue : Data.LeftProperty.Value.GetValue();
                T right = Data.IsRightConst ? Data.RightValue : Data.RightProperty.Value.GetValue();

                return Data.Operation.Execute(left, right);
            }
        }

        [JsonConstructor, Preserve]
        public CompareValuesCondition() : this(Guid.Empty, Guid.Empty, default, default, false, false, new EqualToOperation<T>())
        {
        }

        public CompareValuesCondition(IDataProperty<T> leftProperty, IDataProperty<T> rightProperty, T leftValue, T rightValue, bool isLeftConst, bool isRightConst, IOperationCommand<T, bool> operation) :
            this(ProcessReferenceUtils.GetUniqueIdFrom(leftProperty), ProcessReferenceUtils.GetUniqueIdFrom(rightProperty), leftValue, rightValue, isLeftConst, isRightConst, operation)
        {
        }

        public CompareValuesCondition(Guid leftPropertyId, Guid rightPropertyId, T leftValue, T rightValue, bool isLeftConst, bool isRightConst, IOperationCommand<T, bool> operation)
        {
            Data.LeftProperty = new SingleScenePropertyReference<IDataProperty<T>>(leftPropertyId);
            Data.RightProperty = new SingleScenePropertyReference<IDataProperty<T>>(rightPropertyId);
            Data.LeftValue = leftValue;
            Data.RightValue = rightValue;
            Data.IsLeftConst = isLeftConst;
            Data.IsRightConst = isRightConst;
            Data.Operation = operation;
        }

        /// <inheritdoc />
        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }

        /// <summary>
        /// Constructs concrete types in order for them to be seen by IL2CPP's ahead of time compilation.
        /// </summary>
        private class AOTHelper
        {
            CompareValuesCondition<float> flt = new CompareValuesCondition<float>();
            CompareValuesCondition<string> str = new CompareValuesCondition<string>();
            CompareValuesCondition<bool> bln = new CompareValuesCondition<bool>();
        }
    }
}
