using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRBuilder.Core.SceneObjects
{
    [ExecuteInEditMode]
    public class ProcessTagContainer : MonoBehaviour, ITagContainer
    {
        [SerializeField]
        protected List<string> tags = new List<string>();

        public IEnumerable<Guid> Tags => tags.Select(tag => Guid.Parse(tag));

        public bool IsLocked { get; private set; }

        public event EventHandler<TaggableObjectEventArgs> TagAdded;
        public event EventHandler<TaggableObjectEventArgs> TagRemoved;
        public event EventHandler<LockStateChangedEventArgs> Locked;
        public event EventHandler<LockStateChangedEventArgs> Unlocked;

        public void AddTag(Guid tag)
        {
            if (Tags.Contains(tag) == false)
            {
                tags.Add(tag.ToString());
                TagAdded?.Invoke(this, new TaggableObjectEventArgs(tag.ToString()));
            }
        }

        public bool HasTag(Guid tag)
        {
            return Tags.Contains(tag);
        }

        public bool RemoveTag(Guid tag)
        {
            if(tags.Remove(tag.ToString()))
            {
                TagRemoved?.Invoke(this, new TaggableObjectEventArgs(tag.ToString()));
                return true;
            }

            return false;
        }

        public void SetLocked(bool lockState)
        {
            if (IsLocked == lockState)
            {
                return;
            }

            IsLocked = lockState;

            if (IsLocked)
            {
                if (Locked != null)
                {
                    Locked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
            else
            {
                if (Unlocked != null)
                {
                    Unlocked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
        }
    }
}
