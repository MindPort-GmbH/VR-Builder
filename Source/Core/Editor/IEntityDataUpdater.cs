using System;
using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public interface IEntityDataUpdater
    {
        Type SupportedType { get; }

        void UpdateData(IDataOwner dataOwner);
    }
}
