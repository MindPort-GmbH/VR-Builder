using System.Runtime.Serialization;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Data structure to add the possibility to run at specific execution stages
    /// </summary>
    public interface IBehaviorExecutionStages
    {
        /// <summary>
        /// A property that determines if the Behavior should be run at activation or deactivation (or both).
        /// </summary>
        [DataMember]
        public BehaviorExecutionStages ExecutionStages { get; set; }
    }
}
