using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to multiple <see cref="ISceneObjectProperty"/>s.
    /// </summary>    
    [DataContract(IsReference = true)]
    public class MultipleScenePropertyReference<T> : MultipleSceneReference<T> where T : class, ISceneObjectProperty
    {
        protected override IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue)
        {
            if (IsEmpty())
            {
                return null;
            }

            IEnumerable<T> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            IEnumerable<ISceneObject> sceneObjects = new List<ISceneObject>();

            foreach (Guid guid in Guids)
            {
                sceneObjects = sceneObjects.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid));
            }

            // Can't find corresponding process objects, value is null.
            if (sceneObjects.Count() == 0)
            {
                return value;
            }

            value = null;

            // Allows non-unique referencing system to have guid but no property
            try
            {
                value = sceneObjects.Select(so => so.GetProperty<T>());
            }
            catch (PropertyNotFoundException)
            {
            }

            return value;
        }
    }
}
