using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace MindPort.GTKProcessEditor.Editor
{
    // Creates nodes and connections via Graph Toolkit's public API.
    public sealed class GraphNodeFactory
    {
        private readonly ProcessGraph graph;

        public bool IsAvailable => graph != null;

        public GraphNodeFactory(ProcessGraph graph)
        {
            this.graph = graph;
        }

        public bool CreateNode(Node node, Vector2 position)
        {
            if (graph == null || node == null)
            {
                return false;
            }

            graph.AddNode(node);
            node.Position = position;
            return true;
        }

        public void Connect(IPort outputPort, IPort inputPort)
        {
            if (graph != null && outputPort != null && inputPort != null)
            {
                graph.Connect(outputPort, inputPort);
            }
        }

        public void Save()
        {
            if (graph != null)
            {
                GraphDatabase.SaveGraph(graph);
            }
        }
    }
}
