using UnityEngine.Video;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Implementation of asset path drawer for <see cref="VideoClip"/> assets.
    /// </summary>
    public class VideoClipResourceDrawer : ResourcePathDrawer<VideoClip>
    {
        /// <inheritdoc/>
        public override void ValidateResource(VideoClip resource)
        {
        }
    }
}
