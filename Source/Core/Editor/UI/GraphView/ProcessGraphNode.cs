using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Step node in a graph view editor.
    /// </summary>
    public class ProcessGraphNode : Node
    {
        Label label;        

        public ProcessGraphNode() : base()
        {
            label = titleContainer.Q<Label>();
            label.RegisterCallback<MouseDownEvent>(e => OnMouseDownEvent(e));
        }

        void OnMouseDownEvent(MouseDownEvent e)
        {
            if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
            {
                OpenTextEditor();
                e.PreventDefault();
                e.StopImmediatePropagation();
            }
        }

        private void OpenTextEditor()
        {
            label.text = "";
            TextField textField = new TextField();
            textField.value = Step.Data.Name;

            textField.RegisterCallback<FocusOutEvent>(e => OnEditTextFinished(textField));
            label.Add(textField);

            textField.Focus();
            textField.SelectAll();
        }

        private void OnEditTextFinished(TextField textField)
        {
            Step.Data.Name = textField.value;
            label.text = textField.value;
            label.Remove(textField);            
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(Step);
            GlobalEditorHandler.StartEditingStep();
        }

        /// <summary>
        /// True if this is the "Start" node.
        /// </summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>
        /// Step stored in this node.
        /// </summary>
        public IStep Step { get; set; }        
    }
}
