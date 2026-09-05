// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Runtime.Source.Core.Runtime.Properties
{
    public class ModifyParentProperty : ProcessSceneObjectProperty, IModifyParentProperty
    {
        public void UnsetParent()
        {
            gameObject.transform.SetParent(null);
        }

        public void SetParent(ISceneObject parentObject, bool snapToParentTransform)
        {
            if (HasScaleIssues(parentObject, snapToParentTransform))
            {
                ForwardingLogger.LogWarning($"'{gameObject.name}' is being parented to a hierarchy that has changes in rotation and scale. This may result in a distorted object after parenting.");
            }

            if (snapToParentTransform)
            {
                gameObject.transform.SetPositionAndRotation(parentObject.GameObject.transform.position, parentObject.GameObject.transform.rotation);
            }

            gameObject.transform.SetParent(parentObject.GameObject.transform, true);
        }

        private bool HasScaleIssues(ISceneObject parentObject, bool snapToParentTransform)
        {
            var currentTransform = gameObject.transform;
            var parentTransform = parentObject.GameObject.transform;

            var changesScale = currentTransform.localScale != Vector3.one;
            var changesRotation = currentTransform.rotation != parentTransform.rotation && !snapToParentTransform;

            while (parentTransform)
            {
                changesScale |= parentTransform.localScale != Vector3.one;

                if (parentTransform.parent)
                {
                    changesRotation |= parentTransform.rotation != parentTransform.parent.rotation;
                }

                parentTransform = parentTransform.parent;
            }

            return changesScale && changesRotation;
        }
    }
}