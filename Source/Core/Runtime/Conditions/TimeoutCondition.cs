using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace VRBuilder.Core.Conditions
{
    /// <summary>
    /// A condition that completes when a certain amount of time has passed.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/timeout-condition")]
    public class TimeoutCondition : Condition<TimeoutCondition.EntityData>
    {
        /// <summary>
        /// The data for timeout condition.
        /// </summary>
        [DisplayName("Intent Recognition")]
        public class EntityData : IConditionData
        {
            /// <summary>
            /// The delay before the condition completes.
            /// </summary>
            [DataMember]
            [UsesSpecificProcessDrawer("MultiLineStringDrawer")]
            [DisplayName("Intent")]
            public string Timeout { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    return $"Intent Recognition";
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

            private float timeStarted;

            /// <inheritdoc />
            protected override bool CheckIfCompleted()
            {
                return false;//Time.time - timeStarted >= Data.Timeout;
            }

            /// <inheritdoc />
            public override void Start()
            {
                timeStarted = Time.time;
                base.Start();
            }
        }

        [JsonConstructor, Preserve]
        public TimeoutCondition() : this("")
        {
        }

        public TimeoutCondition(string timeout)
        {
            Data.Timeout = timeout;
        }

        /// <inheritdoc />
        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }
    }
}
