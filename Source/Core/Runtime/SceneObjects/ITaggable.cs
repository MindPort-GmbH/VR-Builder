using System.Collections.Generic;

namespace VRBuilder.Core.SceneObjects
{
    public interface ITaggable : ILockable
    {
        IEnumerable<string> Tags { get; }

        void AddTag(string tag);

        bool RemoveTag(string tag);
    }
}
