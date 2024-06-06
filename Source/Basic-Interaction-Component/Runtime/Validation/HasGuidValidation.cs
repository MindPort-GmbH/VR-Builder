using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Validation
{
    /// <summary>
    /// Validator that checks if the object has one of the required guids either as
    /// its object ID or as a group.
    /// </summary>
    public class HasGuidValidation : Validator, IGuidContainer
    {
        [SerializeField]
        private List<string> guids = new List<string>();

        /// <inheritdoc/>
        public IEnumerable<Guid> Guids => guids.Select(tag => Guid.Parse(tag));

        public event EventHandler<GuidContainerEventArgs> GuidAdded;
        public event EventHandler<GuidContainerEventArgs> GuidRemoved;

        /// <inheritdoc/>
        public void AddGuid(Guid guid)
        {
            if (HasGuid(guid) == false)
            {
                guids.Add(guid.ToString());
                GuidAdded?.Invoke(this, new GuidContainerEventArgs(guid));
            }
        }

        /// <inheritdoc/>
        public bool HasGuid(Guid guid)
        {
            return Guids.Contains(guid);
        }

        /// <inheritdoc/>
        public bool RemoveGuid(Guid guid)
        {
            bool removed = false;

            if (HasGuid(guid))
            {
                removed = guids.Remove(guid.ToString());
                GuidRemoved?.Invoke(this, new GuidContainerEventArgs(guid));
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

            return Guids.Any(guid => processSceneObject.Guid == guid || processSceneObject.HasGuid(guid));
        }
    }
}