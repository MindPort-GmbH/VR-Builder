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
        public class Tag
        {
            [SerializeField]
            private string label;
            public string Label => label;

            [SerializeField]
            private string guidString;

            private Guid guid;
            public Guid Guid
            {
                get
                {
                    if (guid == null || guid == Guid.Empty) 
                    {
                        guid = Guid.Parse(guidString);
                    }

                    return guid;
                }
            }

            public Tag(string label)
            {
                this.label = label;
                this.guidString = Guid.NewGuid().ToString();
            }

            public void Rename(string label)
            {
                this.label = label;
            }
        }

        [SerializeField, HideInInspector]
        private List<Tag> tags = new List<Tag>();

        public IEnumerable<Tag> Tags => tags;   

        public bool CreateTag(string label)
        {
            if (tags.Any(tag => tag.Label == label))
            {
                return false;
            }

            tags.Add(new Tag(label));
            return true;
        }

        public bool RemoveTag(Guid guid)
        {
            return tags.RemoveAll(tag => tag.Guid == guid) > 0;
        }

        public bool TagExists(Guid guid)
        {
            return tags.Any(tag => tag.Guid == guid);
        }

        public string GetLabel(Guid guid)
        {
            return tags.First(tag => tag.Guid == guid).Label;
        }

        public bool RenameTag(Tag tag, string label)
        {
            if (string.IsNullOrEmpty(label) || tags.Any(tag => tag.Label == label)) 
            {
                return false;
            }

            tag.Rename(label);
            return true;
        }
    }
}
