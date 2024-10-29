using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core.Editor.ProcessUpgradeTool.Converters;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters
{
    /// <summary>
    /// Iterates through elements of a <see cref="IList{T}"/> and replaces them with an up to date
    /// version if a suitable converter is found.
    /// </summary>    
    public abstract class ListUpdater<T> : IUpdater
    {
        /// <inheritdoc/>
        public Type UpdatedType => typeof(IList<T>);

        /// <inheritdoc/>
        public void Update(MemberInfo memberInfo, object owner)
        {
            IList<T> list = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IList<T>;

            for (int i = 0; i < list.Count; i++)
            {
                IConverter converter = ProcessUpgradeTool.Converters.FirstOrDefault(converter => converter.ConvertedType == list[i].GetType());

                if (converter != null)
                {
                    list[i] = (T)converter.Convert(list[i]);
                }
            }
        }
    }
}
