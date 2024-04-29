using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Assigns a value to a <see cref="ProcessSceneReferenceBase"/> property from an obsolete
    /// <see cref="UniqueNameReference"/> or <see cref="SceneObjectTagBase"/>.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class ProcessSceneReferencePropertyUpdater : PropertyUpdater<ProcessSceneReferenceBase, object>
    {
        /// <inheritdoc/>
        protected override bool PerformUpgrade(ref ProcessSceneReferenceBase newProperty, ref object oldProperty)
        {
            if (oldProperty is UniqueNameReference uniqueNameReference)
            {
                return UpgradeUniqueReference(ref newProperty, ref uniqueNameReference);
            }
            else if (oldProperty is SceneObjectTagBase sceneObjectTag)
            {
                return UpgradeSceneObjectTag(ref newProperty, ref sceneObjectTag);
            }

            return false;
        }

        private bool UpgradeSceneObjectTag(ref ProcessSceneReferenceBase newProperty, ref SceneObjectTagBase sceneObjectTag)
        {
            newProperty.ResetGuids(new List<Guid> { sceneObjectTag.Guid });
            return true;
        }

        private bool UpgradeUniqueReference(ref ProcessSceneReferenceBase newProperty, ref UniqueNameReference oldProperty)
        {
            if (string.IsNullOrEmpty(oldProperty.UniqueName))
            {
                return true;
            }

            // Attempt to find an object with the given unique name in the scene.
            string uniqueName = oldProperty.UniqueName;
            ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == uniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            if (referencedObject == null)
            {
                return false;
            }

            newProperty.ResetGuids(new List<Guid>() { referencedObject.Guid });
            return true;
        }

        /// <inheritdoc/>
        protected override bool ShouldBeUpdated(ProcessSceneReferenceBase property)
        {
            return property == null || property.IsEmpty();
        }
    }
}
