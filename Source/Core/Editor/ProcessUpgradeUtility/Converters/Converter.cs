using System;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class Converter<TIn, TOut> : IConverter where TIn : class where TOut : class
    {
        public Type ConvertedType => typeof(TIn);

        protected abstract TOut PerformConversion(TIn entity);

        public object Convert(object oldEntity)
        {
            return PerformConversion((TIn)oldEntity);
        }
    }
}
