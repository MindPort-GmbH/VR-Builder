// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Serializable reference to a <see cref="LockableProperty"/>
    /// </summary>
    [DataContract(IsReference = true)]
    public class LockablePropertyReference
    {
        /// <summary>
        /// Reference to the scene object the LockableProperty is attached to.
        /// </summary>
        [DataMember]
        [Obsolete("Use TargetObject instead.")]
        [LegacyProperty(nameof(TargetObject))]
        public SceneObjectReference Target;

        /// <summary>
        /// Reference to the scene object the LockableProperty is attached to.
        /// </summary>
        [DataMember]
        public SingleSceneObjectReference TargetObject;

        /// <summary>
        /// Type name of the LockableProperty.
        /// </summary>
        [DataMember]
        public string Type;

        [IgnoreDataMember]
        private LockableProperty property;

        public LockablePropertyReference()
        {

        }

        public LockablePropertyReference(LockableProperty property)
        {
            TargetObject = new SingleSceneObjectReference(property.SceneObject.Guid);
            Type = property.GetType().AssemblyQualifiedName;

#pragma warning disable CS0618 // Type or member is obsolete - We want remove all calls to deprecated code but this needs to stay for compatibility reasons
            Target = new SceneObjectReference(property.SceneObject.Guid.ToString());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the referenced <see cref="LockableProperty"/>.
        /// </summary>
        public LockableProperty GetProperty()
        {
            if (property == null)
            {
                foreach (ISceneObjectProperty prop in TargetObject.Value.Properties)
                {
                    if (prop.GetType().AssemblyQualifiedName.Equals(Type))
                    {
                        property = (LockableProperty)prop;
                        break;
                    }
                }
            }

            return property;
        }
    }
}
