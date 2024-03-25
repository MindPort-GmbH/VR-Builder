using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public interface IEntityDataUpdater<T> where T : class, IDataOwner
    {
        void UpdateData(T data);
    }
}
