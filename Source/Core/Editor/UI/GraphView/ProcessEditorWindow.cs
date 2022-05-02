using UnityEditor;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Windows
{
    /// <summary>
    /// Base class for a process GUI editor window.
    /// </summary>
    public abstract class ProcessEditorWindow : EditorWindow
    {
        /// <summary>
        /// Sets the process to be displayed.
        /// </summary>        
        internal abstract void SetProcess(IProcess currentProcess);

        /// <summary>
        /// Gets the current chapter.
        /// </summary>        
        internal abstract IChapter GetChapter();

        /// <summary>
        /// Updates the chapter view.
        /// </summary>
        internal abstract void RefreshChapterRepresentation();
    }
}
