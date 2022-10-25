using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Editor.Settings
{
    public class SceneObjectTags : SettingsObject<SceneObjectTags>
    {
        [Serializable]
        private struct Tag
        {
            public string label;
            public Guid guid;

            public Tag(string label)
            {
                this.label = label;
                this.guid = Guid.NewGuid();
            }
        }

        [SerializeField]
        [HideInInspector]
        private List<Tag> tags = new List<Tag>();
        //[SerializeField]
        //private Dictionary<Guid, string> tags = new Dictionary<Guid, string>();

        public IEnumerable<Guid> Tags => tags.Select(t => t.guid);        

        public bool CreateTag(string label)
        {
            if (tags.Any(tag => tag.label == label))
            {
                return false;
            }

            tags.Add(new Tag(label));
            return true;
        }

        public string GetLabel(Guid guid)
        {
            return tags.First(tag => tag.guid == guid).label;
        }
    }
}
