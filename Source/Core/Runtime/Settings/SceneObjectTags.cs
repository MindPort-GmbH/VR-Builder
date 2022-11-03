using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Core.Settings
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

            public Tag(string label, Guid guid)
            {
                this.label = label;
                this.guidString = guid.ToString();
            }

            public void Rename(string label)
            {
                this.label = label;
            }
        }

        [SerializeField, HideInInspector]
        private List<Tag> tags = new List<Tag>();

        public IEnumerable<Tag> Tags => tags;   

        public Tag CreateTag(string label, Guid guid)
        {
            if (tags.Any(tag => tag.Label == label) || tags.Any(tag => tag.Guid == guid))
            {
                return null;
            }

            Tag tag = new Tag(label, guid);
            tags.Add(tag);
            return tag;
        }

        public bool CanCreateTag(string label)
        {
            return string.IsNullOrEmpty(label) == false &&
                Tags.Any(tag => tag.Label == label) == false;
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
            if (TagExists(guid))
            {
                return tags.First(tag => tag.Guid == guid).Label;
            }
            else
            {
                return "";
            }
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
