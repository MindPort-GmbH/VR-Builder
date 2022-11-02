using System;
using System.Collections.Generic;

namespace VRBuilder.Core.SceneObjects
{
    public class TaggableObjectEventArgs : EventArgs
    {
        public readonly string Tag;            

        public TaggableObjectEventArgs(string tag)
        {
            Tag = tag;            
        }
    }
     
    public interface ITagContainer : ILockable
    {
        event EventHandler<TaggableObjectEventArgs> TagAdded;

        event EventHandler<TaggableObjectEventArgs> TagRemoved;

        IEnumerable<Guid> Tags { get; }

        bool HasTag(Guid tag);

        void AddTag(Guid tag);

        bool RemoveTag(Guid tag);
    }
}
