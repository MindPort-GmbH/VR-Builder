using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.ProcessUtils
{
    /// <summary>
    /// Struct for a process variable. Accommodates the value coming from a <see cref="IDataProperty{T}"/>, or being a constant value set e.g. in the Step Inspector.
    /// </summary>  
    [DataContract(IsReference = true)]
    public struct ProcessVariable<T>
    {
        /// <summary>
        /// Constant value.
        /// </summary>
        [DataMember]
        public T ConstValue { get; set; }

        /// <summary>
        /// Property reference for the variable.
        /// </summary>
        [DataMember]
        public SingleScenePropertyReference<IDataProperty<T>> Property { get; set; }

        [DataMember]
        [Obsolete("Use Property instead.")]
        [LegacyProperty(nameof(Property))]
        public ScenePropertyReference<IDataProperty<T>> PropertyReference { get; set; }

        /// <summary>
        /// If true, <see cref="ConstValue"/> is used. Else the value will be fetched from the <see cref="PropertyReference"/>.
        /// </summary>
        [DataMember]
        public bool IsConst { get; set; }

        public ProcessVariable(T constValue, Guid referenceId, bool isConst)
        {
            ConstValue = constValue;
            Property = new SingleScenePropertyReference<IDataProperty<T>>(referenceId);
            IsConst = isConst;

#pragma warning disable CS0618 // Type or member is obsolete - We want remove all calls to deprecated code but this needs to stay for compatibility reasons
            PropertyReference = new ScenePropertyReference<IDataProperty<T>>(referenceId.ToString());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public ProcessVariable(T constValue, IEnumerable<Guid> referenceIds, bool isConst)
        {
            ConstValue = constValue;
            Property = new SingleScenePropertyReference<IDataProperty<T>>(referenceIds);
            IsConst = isConst;

#pragma warning disable CS0618 // Type or member is obsolete - We want remove all calls to deprecated code but this needs to stay for compatibility reasons
            PropertyReference = new ScenePropertyReference<IDataProperty<T>>();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the current value of this variable.
        /// </summary>
        public T Value => IsConst ? ConstValue : Property.Value.GetValue();

        public override string ToString()
        {
            if (IsConst && ConstValue != null)
            {
                return ConstValue.ToString();
            }
            else if (IsConst == false && Property.HasValue())
            {
                return Property.Value.ToString();
            }

            return "[NULL]";
        }
    }
}