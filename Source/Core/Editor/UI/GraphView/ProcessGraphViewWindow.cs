using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Editor.UI.Windows;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
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

        [SerializeField]
        private ChapterMenuView chapterMenu;

        private IMGUIContainer chapterViewContainer;
        private IProcess currentProcess;
        private IChapter currentChapter;

        private void CreateGUI()
        {
            wantsMouseMove = true;
            if (chapterMenu == null)
            {
                chapterMenu = CreateInstance<ChapterMenuView>();
            }

            //CreateToolbar();


            chapterViewContainer = new IMGUIContainer();
            rootVisualElement.Add(chapterViewContainer);
            chapterViewContainer.StretchToParentSize();
            chapterViewContainer.style.width = ChapterMenuView.ExtendedMenuWidth;
            chapterViewContainer.onGUIHandler = () => chapterMenu.Draw();

            graphView = ConstructGraphView();

            GlobalEditorHandler.ProcessWindowOpened(this);
        }

        private void OnEnable()
        {
        }

        private void OnGUI()
        {
            SetTabName();
        }

        private void OnDisable()
        {
            GlobalEditorHandler.ProcessWindowClosed(this);
        }

        [MenuItem("Window/Process Graph")]
        public static void OpenProcessGraphView()
        {
            EditorWindow window = GetWindow<ProcessGraphViewWindow>();
            window.titleContent = new GUIContent("Process Graph");
        }

        private void SetTabName()
        {
            titleContent = new GUIContent("Workflow", TitleIcon.Texture);
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

        private void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            Button newStepButton = new Button(() => { graphView.AddElement(graphView.CreateStepNode(EntityFactory.CreateStep("New Step"))); });
            newStepButton.text = "New Step";
            toolbar.Add(newStepButton);

            rootVisualElement.Add(toolbar);
        }

        private void SetChapter(IChapter chapter)
        {
            if (chapter != GlobalEditorHandler.GetCurrentChapter())
            {
                GlobalEditorHandler.SetCurrentChapter(chapter);
            }

            currentChapter = chapter;

            if(graphView == null)
            {
                graphView = ConstructGraphView();
            }

            graphView.SetChapter(currentChapter);
        }

        internal override void SetProcess(IProcess process)
        {
            RevertableChangesHandler.FlushStack();

            currentProcess = process;

            if (currentProcess == null)
            {
                return;
            }

            chapterMenu.Initialise(currentProcess, chapterViewContainer);
            chapterMenu.ChapterChanged += (sender, args) =>
            {
                SetChapter(args.CurrentChapter);
            };

            SetChapter(currentProcess.Data.FirstChapter);
        }

        internal override IChapter GetChapter()
        {
            return currentProcess == null ? null : currentProcess.Data.FirstChapter;
            //return currentProcess == null ? null : chapterMenu.CurrentChapter;
        }

        internal override void RefreshChapterRepresentation()
        {
            if(currentProcess != null)
            {
                SetChapter(currentChapter);
            }
        }
    }
}
