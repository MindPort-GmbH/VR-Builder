using UnityEditor;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Windows
{
    public abstract class ProcessEditorWindow : EditorWindow
    {
        internal abstract void SetProcess(IProcess currentProcess);
        internal abstract IChapter GetChapter();
        internal abstract void RefreshChapterRepresentation();
    }
}
