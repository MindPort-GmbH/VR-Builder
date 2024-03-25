using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public interface IEntityDataUpdater<T> where T : class, IDataOwner
    {
        IDataOwner GetUpdatedData(T data);
    }
}
