using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Editor.UI.Graphics
{
    public class EndChapterNode : StepGraphNode
    {
        public EndChapterNode(IStep step) : base(step)
        {
            GoToChapterBehavior behavior = (GoToChapterBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is GoToChapterBehavior);

            TextField textField = new TextField();
            textField.value = behavior.Data.ChapterGuid.ToString();
            textField.RegisterCallback<FocusOutEvent>(e => OnTextInput(textField));
            mainContainer.Add(textField);

            titleButtonContainer.Clear();

            RefreshPorts();
        }

        private void OnTextInput(TextField textField)
        {
            GoToChapterBehavior behavior = (GoToChapterBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is GoToChapterBehavior);

            if (behavior != null)
            {
                behavior.Data.ChapterGuid = Guid.Parse(textField.value);
            }
        }

        public override Port AddTransitionPort(bool isDeletablePort = true, int index = -1)
        {
            return null;
        }
    }
}
