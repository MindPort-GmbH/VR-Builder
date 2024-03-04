using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Validation
{
    /// <summary>
    /// Validator that checks if the object has one of the required guids either as
    /// its unique id or as a user tag.
    /// </summary>
    public class HasGuidValidation : Validator, ITagContainer
    {
        [SerializeField]
        private List<string> tags = new List<string>();

        /// <inheritdoc/>
        public IEnumerable<Guid> Tags => tags.Select(tag => Guid.Parse(tag));

        public event EventHandler<TaggableObjectEventArgs> TagAdded;
        public event EventHandler<TaggableObjectEventArgs> TagRemoved;

        /// <inheritdoc/>
        public void AddTag(Guid tag)
        {
            if (HasTag(tag) == false)
            {
                tags.Add(tag.ToString());
                TagAdded?.Invoke(this, new TaggableObjectEventArgs(tag));
            }
        }

        /// <inheritdoc/>
        public bool HasTag(Guid tag)
        {
            return Tags.Contains(tag);
        }

        /// <inheritdoc/>
        public bool RemoveTag(Guid tag)
        {
            bool removed = false;

            if (HasTag(tag))
            {
                removed = tags.Remove(tag.ToString());
                TagRemoved?.Invoke(this, new TaggableObjectEventArgs(tag));
            }

            return removed;
        }

        /// <inheritdoc/>
        public override bool Validate(GameObject obj)
        {
            ProcessSceneObject processSceneObject = obj.GetComponent<ProcessSceneObject>();

            if (processSceneObject == null)
            {
                return false;
            }

            if (Tags.Count() == 0)
            {
                return true;
            }

            return Tags.Any(tag => processSceneObject.HasTag(tag));
        }
    }
}