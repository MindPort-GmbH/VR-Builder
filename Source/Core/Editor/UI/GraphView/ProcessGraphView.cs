using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class ProcessGraphView : GraphView
    {
        private Vector2 defaultNodeSize = new Vector2(200, 300);

        public ProcessNode EntryNode { get; private set; }

        public ProcessGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ProcessGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            EntryNode = CreateEntryPointNode();
            AddElement(EntryNode);
        }

        private ProcessNode CreateEntryPointNode()
        {
            ProcessNode node = new ProcessNode
            {
                title = "Start",
                GUID = Guid.NewGuid().ToString(),
                IsEntryPoint = true,
            };

            Port transitionPort = CreatePort(node, Direction.Output);
            transitionPort.portName = "Next";
            node.outputContainer.Add(transitionPort);

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public Port AddTransitionPort(ProcessNode node)
        {
            Port port = CreatePort(node, Direction.Output);

            int outputPortCount = node.outputContainer.Query("connector").ToList().Count;
            port.portName = $"Transition {outputPortCount}";

            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();

            return port;
        }

        internal ProcessNode CreateStepNode(IStep step)
        {
            ProcessNode node = new ProcessNode
            {
                title = step.Data.Name,
                GUID = Guid.NewGuid().ToString(),
            };

            Port inputPort = CreatePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            Button addTransitionButton = new Button(() => { AddTransitionPort(node); });
            addTransitionButton.text = "New Transition";
            node.titleContainer.Add(addTransitionButton);  

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

            return node;
        }

        private Port CreatePort(ProcessNode node, Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
        }
    }
}
