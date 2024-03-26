using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.Utils
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ProcessSceneReferencePropertyUpdater : PropertyUpdater<ProcessSceneReferenceBase, UniqueNameReference>
    {
        protected override bool PerformUpgrade(ProcessSceneReferenceBase newProperty, UniqueNameReference oldProperty)
        {
            // Attempt to find an object with the given unique name in the scene.
            ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == oldProperty.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            if (referencedObject == null)
            {
                return false;
            }

            newProperty.ResetGuids(new List<Guid>() { referencedObject.Guid });
            return true;
        }
    }
}
