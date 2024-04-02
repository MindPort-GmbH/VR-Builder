using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class ListUpdater<T> : IUpdater
    {
        public Type SupportedType => typeof(IList<T>);

        public void Update(MemberInfo memberInfo, object owner)
        {
            IList<T> list = ReflectionUtils.GetValueFromPropertyOrField(owner, memberInfo) as IList<T>;

            for (int i = 0; i < list.Count; i++)
            {
                IConverter converter = ProcessUpgradeTool.EntityConverters.FirstOrDefault(converter => converter.ConvertedType == list[i].GetType());

                if (converter != null)
                {
                    list[i] = (T)converter.Convert(list[i]);
                }
            }
        }
    }
}
