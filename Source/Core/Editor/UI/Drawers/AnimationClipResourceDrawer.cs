using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Implementation of asset path drawer for <see cref="AnimationClip"/> assets.
    /// </summary>
    public class AnimationClipResourceDrawer : ResourcePathDrawer<AnimationClip>
    {
        public override void ValidateResource(AnimationClip resource)
        {
            if (resource.legacy == false)
            {
                resource.legacy = true;
                EditorUtility.SetDirty(resource);
                UnityEngine.Debug.Log($"Animation clip '{resource.name}' has been marked as Legacy since it is being used outside of an animator.");
            }
        }
    }
}
