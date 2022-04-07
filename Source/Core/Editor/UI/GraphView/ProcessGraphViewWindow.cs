using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

            if (titleIcon == null)
            {
                titleIcon = new EditorIcon("icon_process_editor");
            }

            CreateToolbar();


            //Foldout chapterList = new Foldout();
            //chapterList.StretchToParentSize();
            //chapterList.style.width = ChapterMenuView.ExtendedMenuWidth;
            //chapterList.style.backgroundColor = Color.red;
            //rootVisualElement.Add(chapterList);

            chapterViewContainer = new IMGUIContainer();
            rootVisualElement.Add(chapterViewContainer);
            chapterViewContainer.StretchToParentSize();
            chapterViewContainer.style.width = ChapterMenuView.ExtendedMenuWidth;
            chapterViewContainer.onGUIHandler = () => chapterMenu.Draw();

            //DEBUG
            SetProcess(GlobalEditorHandler.GetCurrentProcess());
        }

        private void OnEnable()
        {
            GlobalEditorHandler.ProcessWindowOpened(this);
        }

        private void OnGUI()
        {
            if (currentProcess == null)
            {
                return;
            }

            SetTabName();

            float width = chapterMenu.IsExtended ? ProcessMenuView.ExtendedMenuWidth : ProcessMenuView.MinimizedMenuWidth;
            Rect scrollRect = new Rect(width, 0f, position.size.x - width, position.size.y);

            //Vector2 centerViewpointOnCanvas = currentScrollPosition + scrollRect.size / 2f;

            //HandleEditorCommands(centerViewpointOnCanvas);
            //chapterMenu.Draw();
        }

        private void OnDisable()
        {
            GlobalEditorHandler.ProcessWindowClosed(this);
            //rootVisualElement.Remove(graphView);
        }

        [MenuItem("Window/Process Graph")]
        public static void OpenProcessGraphView()
        {
            EditorWindow window = GetWindow<ProcessGraphViewWindow>();
            window.titleContent = new GUIContent("Process Graph");
        }

        private void SetTabName()
        {
            titleContent = new GUIContent("Workflow", titleIcon.Texture);
        }

        private ProcessGraphView ConstructGraphView(IChapter chapter)
        {
            ProcessGraphView graphView = new ProcessGraphView()
            {
                name = chapter.Data.Name,
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

            if(graphView != null)
            {
                rootVisualElement.Remove(graphView);
            }

            graphView = ConstructGraphView(chapter);

            IDictionary<IStep, StepGraphNode> stepNodes = SetupSteps(chapter);

            foreach (IStep step in stepNodes.Keys)
            {
                StepGraphNode node = stepNodes[step];
                graphView.AddElement(node);
            }

            SetupTransitions(chapter, graphView.EntryNode, stepNodes);
        }

        private void LinkNodes(Port output, Port input)
        {
            Edge edge = new Edge
            {
                output = output,
                input = input,
            };

            edge.input.Connect(edge);
            edge.output.Connect(edge);
            graphView.Add(edge);

            output.portName = $"To {input.node.title}";
        }

        private void SetupTransitions(IChapter chapter, ProcessGraphNode entryNode, IDictionary<IStep, StepGraphNode> stepNodes)
        {
            if (chapter.Data.FirstStep != null)
            {
                LinkNodes(graphView.EntryNode.outputContainer[0].Query<Port>(), stepNodes[chapter.Data.FirstStep].inputContainer[0].Query<Port>());
            }

            foreach (IStep step in stepNodes.Keys)
            {
                foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
                {
                    Port outputPort = graphView.AddTransitionPort(stepNodes[step]);
                    
                    if (transition.Data.TargetStep != null)
                    {
                        ProcessGraphNode target = stepNodes[transition.Data.TargetStep];
                        LinkNodes(outputPort, target.inputContainer[0].Query<Port>()); 
                    }

                    //IStep closuredStep = step;
                    //ITransition closuredTransition = transition;
                    //int transitionIndex = step.Data.Transitions.Data.Transitions.IndexOf(closuredTransition);
                }
            }
        }

        private IDictionary<IStep, StepGraphNode> SetupSteps(IChapter chapter)
        {
            return chapter.Data.Steps.OrderBy(step => step == chapter.ChapterMetadata.LastSelectedStep).ToDictionary(step => step, graphView.CreateStepNode);
        }

        internal override void SetProcess(IProcess currentProcess)
        {
            RevertableChangesHandler.FlushStack();

            this.currentProcess = currentProcess;

            if (currentProcess == null)
            {
                return;
            }

            chapterMenu.Initialise(currentProcess, chapterViewContainer);
            chapterMenu.ChapterChanged += (sender, args) =>
            {
                SetChapter(args.CurrentChapter);
            };

            graphView = ConstructGraphView(currentProcess.Data.FirstChapter);
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
                ConstructGraphView(currentProcess.Data.FirstChapter);
            }
        }
    }
}
