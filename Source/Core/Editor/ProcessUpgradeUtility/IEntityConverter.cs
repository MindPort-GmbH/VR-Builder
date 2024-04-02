using System;
using VRBuilder.Core;

namespace VRBuilder.Editor.ProcessUpdater
{
    public interface IEntityConverter
    {
        Type ConvertedType { get; }

        IEntity Convert(IEntity oldEntity);
    }
}
