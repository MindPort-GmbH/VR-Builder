using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class ProcessGraphViewWindow : EditorWindow
    {
        private ProcessGraphView graphView;

        private IProcess currentProcess;
        private IChapter currentChapter;

        [MenuItem("Window/Process Graph")]
        public static void OpenProcessGraphView()
        {
            EditorWindow window = GetWindow<ProcessGraphViewWindow>();
            window.titleContent = new GUIContent("Process Graph");
        }

        private void ConstructGraphView(IChapter chapter)
        {
            graphView = new ProcessGraphView()
            {
                name = chapter.Data.Name,
            };

            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            //Button newStepButton = new Button(() => { graphView.CreateNode("New Step"); });
            //newStepButton.text = "New Step";
            //toolbar.Add(newStepButton);

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

            ConstructGraphView(chapter);

            IDictionary<IStep, ProcessNode> stepNodes = SetupSteps(chapter);

            foreach (IStep step in stepNodes.Keys)
            {
                ProcessNode node = stepNodes[step];
                node.SetPosition(new Rect(step.StepMetadata.Position, new Vector2(200, 300))); //TODO don't hardcode size
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
        }

        private void SetupTransitions(IChapter chapter, ProcessNode entryNode, IDictionary<IStep, ProcessNode> stepNodes)
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
                        ProcessNode target = stepNodes[transition.Data.TargetStep];
                        LinkNodes(outputPort, target.inputContainer[0].Query<Port>()); 
                    }

                    //IStep closuredStep = step;
                    //ITransition closuredTransition = transition;
                    //int transitionIndex = step.Data.Transitions.Data.Transitions.IndexOf(closuredTransition);

                }
            }
        }


        private IDictionary<IStep, ProcessNode> SetupSteps(IChapter chapter)
        {
            return chapter.Data.Steps.OrderBy(step => step == chapter.ChapterMetadata.LastSelectedStep).ToDictionary(step => step, graphView.CreateStepNode);
        }


     

        private void OnEnable()
        {
            //DEBUG
            SetChapter(GlobalEditorHandler.GetCurrentProcess().Data.FirstChapter);
            CreateToolbar();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }
}
