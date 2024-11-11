using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Validation
{
    public class IsObjectWithTagValidation : Validator, IGuidContainer
    {
        [SerializeField]
        private List<string> tags = new List<string>();
        public IEnumerable<Guid> Guids => tags.Select(tag => Guid.Parse(tag));

        public event EventHandler<GuidContainerEventArgs> GuidAdded;
        public event EventHandler<GuidContainerEventArgs> GuidRemoved;

        public void AddGuid(Guid tag)
        {
            if (HasGuid(tag) == false)
            {
                tags.Add(tag.ToString());
                GuidAdded?.Invoke(this, new GuidContainerEventArgs(tag));
            }
        }

        public bool HasGuid(Guid tag)
        {
            return Guids.Contains(tag);
        }

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

            return Guids.Any(tag => processSceneObject.HasGuid(tag));
        }
    }
}