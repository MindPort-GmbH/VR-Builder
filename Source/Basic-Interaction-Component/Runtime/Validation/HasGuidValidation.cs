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
    public class HasGuidValidation : Validator, IGuidContainer
    {
        [SerializeField]
        private List<string> tags = new List<string>();

        /// <inheritdoc/>
        public IEnumerable<Guid> Guids => tags.Select(tag => Guid.Parse(tag));

        public event EventHandler<GuidContainerEventArgs> GuidAdded;
        public event EventHandler<GuidContainerEventArgs> GuidRemoved;

        /// <inheritdoc/>
        public void AddGuid(Guid tag)
        {
            if (HasGuid(tag) == false)
            {
                tags.Add(tag.ToString());
                GuidAdded?.Invoke(this, new GuidContainerEventArgs(tag));
            }
        }

        /// <inheritdoc/>
        public bool HasGuid(Guid tag)
        {
            return Guids.Contains(tag);
        }

        /// <inheritdoc/>
        public bool RemoveGuid(Guid tag)
        {
            bool removed = false;

            if (HasGuid(tag))
            {
                removed = tags.Remove(tag.ToString());
                GuidRemoved?.Invoke(this, new GuidContainerEventArgs(tag));
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

            if (Guids.Count() == 0)
            {
                return true;
            }

            return Guids.Any(tag => processSceneObject.Guid == tag || processSceneObject.HasGuid(tag));
        }
    }
}