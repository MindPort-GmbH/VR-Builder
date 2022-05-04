using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Process node in a graph view editor.
    /// </summary>
    public abstract class ProcessGraphNode : Node
    {
        private Label label;
        protected Vector2 defaultNodeSize = new Vector2(200, 300);

        /// <summary>
        /// True if this is the "Start" node.
        /// </summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>
        /// Name of the node.
        /// </summary>
        public abstract string Name { get; set; }

        public abstract IStep[] Outputs { get; }

        public abstract void SetOutput(int index, IStep output);

        public abstract void AddToChapter(IChapter chapter);

        public abstract void RemoveFromChapter(IChapter chapter);

        public abstract IStep EntryPoint { get; }

        public abstract Vector2 Position { get; set; }

        public ProcessGraphNode() : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ProcessGraphNode"));

            label = titleContainer.Q<Label>();
            label.RegisterCallback<MouseDownEvent>(e => OnMouseDownEvent(e));

            titleContainer.style.backgroundColor = new StyleColor(new Color32(38, 144, 119, 192));
        }

        protected Port CreatePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(ProcessExec));
        }

        public void UpdateOutputPortName(Port outputPort, Node input)
        {
            if (input == null)
            {
                outputPort.portName = "End Chapter";
            }
            else
            {
                outputPort.portName = $"To {input.title}";
            }
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
            textField.value = Name;

            textField.RegisterCallback<FocusOutEvent>(e => OnEditTextFinished(textField));
            label.Add(textField);

            textField.Focus();
            textField.SelectAll();
        }

        private void OnEditTextFinished(TextField textField)
        {
            Name = textField.value;
            label.text = textField.value;
            label.Remove(textField);            
        } 
    }
}
