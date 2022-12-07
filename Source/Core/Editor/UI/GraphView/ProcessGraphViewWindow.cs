using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Editor.UI.Windows;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Editor windows that displays the process using a graphview.
    /// </summary>
    public class ProcessGraphViewWindow : ProcessEditorWindow
    {
        private EditorIcon titleIcon;

        internal EditorIcon TitleIcon
        {
            get
            {
                if (titleIcon == null)
                {
                    titleIcon = new EditorIcon("icon_process_editor");
                }

                return titleIcon;
            }
        }

        private ProcessGraphView graphView;
        private Box chapterHierarchy;

        [SerializeField]
        private ProcessMenuView chapterMenu;

        private IMGUIContainer chapterViewContainer;
        private IProcess currentProcess;
        private IChapter currentChapter;

        private void CreateGUI()
        {
            wantsMouseMove = true;
            if (chapterMenu == null)
            {
                chapterMenu = CreateInstance<ProcessMenuView>();
            }

            chapterMenu.MenuExtendedChanged += (sender, args) => { chapterViewContainer.style.width = args.IsExtended ? ProcessMenuView.ExtendedMenuWidth : ProcessMenuView.MinimizedMenuWidth; };

            chapterViewContainer = new IMGUIContainer();
            rootVisualElement.Add(chapterViewContainer);
            chapterViewContainer.StretchToParentSize();
            chapterViewContainer.style.width = ProcessMenuView.ExtendedMenuWidth;
            chapterViewContainer.style.backgroundColor = new StyleColor(new Color32(51, 51, 51, 192));

            graphView = ConstructGraphView();
            chapterHierarchy = ConstructChapterHierarchy();

            GlobalEditorHandler.ProcessWindowOpened(this);
        }

        private void OnGUI()
        {
            SetTabName();
        }

        private void OnDisable()
        {
            GlobalEditorHandler.ProcessWindowClosed(this);
        }

        private void SetTabName()
        {
            titleContent = new GUIContent("Process Editor", TitleIcon.Texture);
        }

        private ProcessGraphView ConstructGraphView()
        {
            ProcessGraphView graphView = new ProcessGraphView()
            {
                name = "Process Graph"
            };

            graphView.StretchToParentSize();            
            rootVisualElement.Add(graphView);
            graphView.SendToBack();

            return graphView;
        }

        /// <inheritdoc/>
        internal override void SetChapter(IChapter chapter)
        {
            SetupChapterHierarchy(chapter);

            currentChapter = chapter;

            if(graphView == null)
            {
                graphView = ConstructGraphView();
            }

            graphView.SetChapter(currentChapter);
        }

        /// <inheritdoc/>
        internal override void SetProcess(IProcess process)
        {
            RevertableChangesHandler.FlushStack();

            currentProcess = process;

            if (currentProcess == null)
            {
                return;
            }

            chapterMenu.Initialise(currentProcess, this);            
            chapterViewContainer.onGUIHandler = () => chapterMenu.Draw();            

            chapterMenu.ChapterChanged += (sender, args) =>
            {
                SetChapter(args.CurrentChapter);
            };

            SetChapter(currentProcess.Data.FirstChapter);
        }

        /// <inheritdoc/>
        internal override IChapter GetChapter()
        {
            return currentProcess == null ? null : chapterMenu.CurrentChapter;
        }

        /// <inheritdoc/>
        internal override void RefreshChapterRepresentation()
        {            
            if(currentProcess != null)
            {
                graphView.RefreshSelectedNode();
            }
        }

        private Box ConstructChapterHierarchy()
        {
            Box box = new Box();

            box.style.alignSelf = Align.FlexEnd;
            rootVisualElement.Add(box);

            return box;
        }

        private void SetupChapterHierarchy(IChapter chapter)
        {
            List<Button> buttons = chapterHierarchy.contentContainer.Children().Select(child => child as Button).ToList();

            if(GlobalEditorHandler.GetCurrentProcess().Data.Chapters.Contains(chapter))
            {
                chapterHierarchy.contentContainer.Clear();
            }

            int index = buttons.IndexOf(buttons.FirstOrDefault(button => button.userData == chapter));

            if (index < 0)
            {
                Button button = new Button(() => GlobalEditorHandler.RequestNewChapter(chapter));
                button.text = chapter.Data.Name;
                button.userData = chapter;
                chapterHierarchy.contentContainer.Add(button);
            }
            else
            {
                for (int i = index + 1; i < chapterHierarchy.contentContainer.childCount; i++)
                {
                    chapterHierarchy.contentContainer.RemoveAt(i);
                }
            }
        }
    }
}
