using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGroupNode : StepGraphNode
    {
        private ExecuteChapterBehavior behavior;
        protected ExecuteChapterBehavior Behavior
        {
            get
            {
                if (behavior == null)
                {
                    behavior = (ExecuteChapterBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is ExecuteChapterBehavior);
                }

                return behavior;
            }
        }

        public StepGroupNode(IStep step) : base(step)
        {
            titleButtonContainer.Clear();
            extensionContainer.style.backgroundColor = new Color(.2f, .2f, .2f, .8f);

            DrawExpandButton();
        }

        private void DrawExpandButton()
        {
            Button expandButton = new Button(() => OnClickExpand());
            Label expandLabel = new Label("Expand");
            expandButton.contentContainer.Add(expandLabel);
            extensionContainer.Add(expandButton);

            Button explodeButton = new Button(() => OnClickExplode());
            Label explodeLabel = new Label("Explode");
            explodeButton.contentContainer.Add(explodeLabel);
            extensionContainer.Add(explodeButton);
        }

        private void OnClickExplode()
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter();
            IEnumerable<ITransition> leadingTransitions = currentChapter.Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == step);

            foreach(IStep addedStep in Behavior.Data.Chapter.Data.Steps)
            {
                currentChapter.Data.Steps.Add(addedStep);
                addedStep.StepMetadata.Position = step.StepMetadata.Position;
            }

            foreach(ITransition transition in leadingTransitions)
            {
                transition.Data.TargetStep = Behavior.Data.Chapter.Data.FirstStep;
            }

            if(currentChapter.Data.FirstStep == step)
            {
                currentChapter.Data.FirstStep = Behavior.Data.Chapter.Data.FirstStep;
            }

            currentChapter.Data.Steps.Remove(step);

            GlobalEditorHandler.RequestNewChapter(currentChapter);
        }

        private void OnClickExpand()
        {
            GlobalEditorHandler.RequestNewChapter(Behavior.Data.Chapter);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(null);
        }
    }
}
