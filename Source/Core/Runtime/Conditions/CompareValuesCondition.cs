using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Properties.Operations;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.UI.SelectableValues;
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
            public ProcessVariableSelectableValue<T> Left;

            [DataMember]
            [HideInProcessInspector]
            public IOperationCommand<T, bool> Operation { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public ProcessVariableSelectableValue<T> Right;

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    string leftProperty = Left.Value == null ? "[NULL]" : Left.Value.ToString();
                    string rightProperty = Right.Value == null ? "[NULL]" : Right.Value.ToString();

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
                return Data.Operation.Execute(Data.Left.Value, Data.Right.Value);
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
            Data.Left = new ProcessVariableSelectableValue<T>(leftValue, new SingleScenePropertyReference<IDataProperty<T>>(leftPropertyId), isLeftConst);
            Data.Right = new ProcessVariableSelectableValue<T>(rightValue, new SingleScenePropertyReference<IDataProperty<T>>(rightPropertyId), isRightConst);
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
