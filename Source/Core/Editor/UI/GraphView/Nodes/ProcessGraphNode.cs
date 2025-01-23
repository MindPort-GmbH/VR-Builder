using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// Process node in a graph view editor.
    /// </summary>
    public abstract class ProcessGraphNode : Node
    {
        private static Dictionary<string, EditorIcon> iconCache = new Dictionary<string, EditorIcon>();
        protected const string deleteIconFileName = "icon_delete";
        protected const string editIconFileName = "icon_edit";
        private const string emptyOutputPortText = "Go to next Chapter";
        private const int maxStepNameLength = 24;

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

        /// <summary>
        /// Steps this node leads to.
        /// </summary>
        public abstract IStep[] Outputs { get; }

        /// <summary>
        /// Step other nodes connect to.
        /// </summary>
        public abstract IStep EntryPoint { get; }

        /// <summary>
        /// Position in the graph.
        /// </summary>
        public abstract Vector2 Position { get; set; }

        /// <summary>
        /// Sets an output to the specified step.
        /// </summary>        
        public abstract void SetOutput(int index, IStep output);

        /// <summary>
        /// Adds node to specified chapter.
        /// </summary>        
        public abstract void AddToChapter(IChapter chapter);

        /// <summary>
        /// Removes node from specified chapter.
        /// </summary>        
        public abstract void RemoveFromChapter(IChapter chapter);

        /// <summary>
        /// Remove port with undo.
        /// </summary>        
        protected abstract void RemovePortWithUndo(Port port);

        public ProcessGraphNode() : base()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("ProcessGraphNode");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            extensionContainer.style.backgroundColor = new Color(.2f, .2f, .2f, .8f);

            label = titleContainer.Q<Label>();
            label.RegisterCallback<MouseDownEvent>(e => OnMouseDownEvent(e));
        }

        /// <summary>
        /// Refreshes the node's graphics.
        /// </summary>
        public virtual void Refresh()
        {
            foreach (VisualElement element in outputContainer.Children())
            {
                Port port = element as Port;

                if (port == null)
                {
                    continue;
                }

                port.connections.ToList().ForEach(e => e.RemoveFromHierarchy());
            }

            outputContainer.Clear();

            Outputs.ToList().ForEach(o => AddTransitionPort());
        }

        /// <summary>
        /// Updates the name of the output port depending on the node it is connected with.
        /// </summary>
        public virtual void UpdateOutputPortName(Port outputPort, Node input)
        {
            if (input == null)
            {
                outputPort.portName = emptyOutputPortText;
            }
            else
            {
                string outputPortName = $"Go to {input.title}";

                if (outputPortName.Length > maxStepNameLength)
                {
                    outputPortName = $"{outputPortName.Remove(maxStepNameLength)}...";
                }

                outputPort.portName = outputPortName;
            }
        }

        /// <summary>
        /// Adds a potentially deletable transition port to this node.
        /// </summary>
        /// <param name="isDeletablePort">If true, a delete button is created which allows to delete the transition.</param>
        /// <param name="index">Index where to insert the port, if blank it will be added at the end of the list.</param>
        /// <returns>The created port.</returns>
        public virtual Port AddTransitionPort(bool isDeletablePort = true, int index = -1)
        {
            Port port = CreatePort(Direction.Output);

            if (isDeletablePort)
            {
                Button deleteButton = new Button(() => RemovePortWithUndo(port));

                Image icon = GetIcon(deleteIconFileName);
                deleteButton.Add(icon);
                icon.StretchToParentSize();

                deleteButton.style.alignSelf = Align.Stretch;

                port.contentContainer.Insert(1, deleteButton);
            }

            UpdateOutputPortName(port, null);

            if (index < 0)
            {
                outputContainer.Add(port);
            }
            else
            {
                outputContainer.Insert(index, port);
            }

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        protected Port CreatePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(ProcessExec));
        }

        private void OnMouseDownEvent(MouseDownEvent e)
        {
            if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
            {
                OpenTextEditor();
#if UNITY_6000
                focusController.IgnoreEvent(e);
#else
                e.PreventDefault();
#endif
                e.StopImmediatePropagation();
            }
        }

        protected Image GetIcon(string fileName)
        {
            if (iconCache.ContainsKey(fileName) == false)
            {
                iconCache.Add(fileName, new EditorIcon(fileName));
            }

            Image icon = new Image();
            icon.image = iconCache[fileName].Texture;
            icon.style.paddingBottom = 2;
            icon.style.paddingLeft = 2;
            icon.style.paddingRight = 2;
            icon.style.paddingTop = 2;

            return icon;
        }

        protected virtual void OpenTextEditor()
        {
            label.text = "";
            TextField textField = new TextField();
            textField.value = Name;

            textField.RegisterCallback<FocusOutEvent>(e => OnEditTextFinished(textField));
            label.Add(textField);

            textField.Focus();
            textField.SelectAll();
        }

        protected virtual void OnEditTextFinished(TextField textField)
        {
            Name = textField.value;
            label.text = textField.value;
            label.Remove(textField);
        }
    }
}
