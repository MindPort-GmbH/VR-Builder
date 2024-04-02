using System;
using VRBuilder.Core;

namespace VRBuilder.Editor.ProcessUpdater
{
    public abstract class EntityConverter<TIn, TOut> : IEntityConverter where TIn : class, IEntity where TOut : class, IEntity
    {
        public Type ConvertedType => typeof(TIn);

        protected abstract TOut PerformConversion(TIn entity);

        public IEntity Convert(IEntity oldEntity)
        {
            return PerformConversion((TIn)oldEntity);
        }
    }
}
