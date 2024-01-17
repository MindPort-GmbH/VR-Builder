using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class MultipleObjectReference<T> : SceneObjectTag<T> where T : class
    {
        public abstract IEnumerable<T> Values { get; }
        //{
        //    get
        //    {
        //        if (Guid == null || Guid == Guid.Empty)
        //        {
        //            return new List<T>();
        //        }

        //        IEnumerable<ISceneObject> sceneObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid);




        //        // Allows non-unique referencing system to have guid but no property
        //        try
        //        {
        //            value = sceneObject.GetProperty<T>();
        //        }
        //        catch (PropertyNotFoundException)
        //        {
        //        }

        //        return value;
        //    }
        //}

        internal override bool AllowMultipleValues => true;


        public static implicit operator List<T>(MultipleObjectReference<T> reference)
        {
            return reference.Values.ToList();
        }

        public MultipleObjectReference()
        {
        }

        public MultipleObjectReference(Guid guid) : base(guid)
        {
        }
    }
}
