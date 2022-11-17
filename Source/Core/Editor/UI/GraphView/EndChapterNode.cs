using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    public class EndChapterNode : StepGraphNode
    {
        private GoToChapterBehavior behavior;
        protected GoToChapterBehavior Behavior
        {
            get
            {
                if(behavior == null)
                {
                    behavior = (GoToChapterBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is GoToChapterBehavior);
                }

                return behavior;
            }
        }

        public EndChapterNode(IStep step) : base(step)
        {
            DrawChapterSelectionField();

            titleButtonContainer.Clear();
        }

        //public override Port AddTransitionPort(bool isDeletablePort = true, int index = -1)
        //{
        //    return null;
        //}

        private void DrawChapterSelectionField()
        {
            List<IChapter> chapters = GlobalEditorHandler.GetCurrentProcess().Data.Chapters.ToList();
            IChapter selectedChapter = chapters.FirstOrDefault(chapter => chapter.ChapterMetadata.Guid == Behavior.Data.ChapterGuid);
            int selectedIndex = 0;
            if(selectedChapter != null && chapters.Contains(selectedChapter))
            {
                selectedIndex = chapters.IndexOf(selectedChapter);
            }

            PopupField<Guid> chapterSelector = new PopupField<Guid>("Next chapter:", chapters.Select(chapter => chapter.ChapterMetadata.Guid).ToList(), selectedIndex, (guid) => FormatSelectedValue(guid, chapters), (guid) => FormatSelectedValue(guid, chapters));
            chapterSelector.RegisterValueChangedCallback((value) => OnChapterSelected(value));            
            mainContainer.Add(chapterSelector);
        }

        private string FormatSelectedValue(Guid guid, IEnumerable<IChapter> chapters)
        {
            return chapters.First(chapter => chapter.ChapterMetadata.Guid == guid).Data.Name;
        }

        private void OnChapterSelected(ChangeEvent<Guid> value)
        {   PopupField<Guid> chapterSelector = value.target as PopupField<Guid>;
            RevertableChangesHandler.Do(new ProcessCommand(
                () => Behavior.Data.ChapterGuid = value.newValue,
                () => { Behavior.Data.ChapterGuid = value.previousValue; chapterSelector.SetValueWithoutNotify(value.previousValue); }
                ));            
        }
    }
}
