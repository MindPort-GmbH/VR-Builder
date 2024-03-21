using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public interface IEntityDataUpdater<T> where T : class, IData
    {
        T GetUpdatedData(T data);
    }
}
