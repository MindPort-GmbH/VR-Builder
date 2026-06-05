using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MindPort.GTKProcessEditor.Editor
{
    // Reaches the Graph Toolkit window/selection (still internal in the 6.4+ module) by class/member name via
    // reflection; recovered nodes are then used through the public INode API.
    internal static class GtkGraphAccess
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static VisualElement FindGraphView(VisualElement root)
        {
            if (root == null)
            {
                return null;
            }

            Type type = root.GetType();
            if (type.Name.Contains("GraphView") && type.GetMethod("GetSelection", Type.EmptyTypes) != null)
            {
                return root;
            }

            foreach (VisualElement child in root.Children())
            {
                VisualElement found = FindGraphView(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static List<INode> GetSelectedNodes()
        {
            List<INode> nodes = new List<INode>();

            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null)
            {
                return nodes;
            }

            VisualElement graphView = FindGraphView(window.rootVisualElement);
            MethodInfo getSelection = graphView?.GetType().GetMethod("GetSelection", Type.EmptyTypes);
            if (getSelection == null)
            {
                return nodes;
            }

            if (getSelection.Invoke(graphView, null) is IEnumerable selection)
            {
                foreach (object model in selection)
                {
                    if (TryGetUserNode(model, out INode node))
                    {
                        nodes.Add(node);
                    }
                }
            }

            return nodes;
        }

        public static ProcessGraph GetFocusedProcessGraph()
        {
            // Prefer recovering it from a selected node via the public INode.Graph.
            foreach (INode node in GetSelectedNodes())
            {
                if (node.Graph is ProcessGraph fromNode)
                {
                    return fromNode;
                }
            }

            // Fallback (nothing selected): reflect the graph view's model graph.
            EditorWindow window = EditorWindow.focusedWindow;
            VisualElement graphView = window != null ? FindGraphView(window.rootVisualElement) : null;
            if (graphView == null)
            {
                return null;
            }

            object graphModel = graphView.GetType().GetProperty("GraphModel", Flags)?.GetValue(graphView);
            object graph = graphModel?.GetType().GetProperty("Graph", Flags)?.GetValue(graphModel);
            return graph as ProcessGraph;
        }

        public static bool ContainsGraphView(VisualElement root)
        {
            if (root == null)
            {
                return false;
            }

            if (root.GetType().Name.Contains("GraphView"))
            {
                return true;
            }

            foreach (VisualElement child in root.Children())
            {
                if (ContainsGraphView(child))
                {
                    return true;
                }
            }

            return false;
        }

        public static EditorWindow FindProcessGraphWindow(string processName)
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window == null || window.rootVisualElement == null)
                {
                    continue;
                }

                VisualElement graphView = FindGraphView(window.rootVisualElement);
                if (graphView == null)
                {
                    continue;
                }

                object graphModel = graphView.GetType().GetProperty("GraphModel", Flags)?.GetValue(graphView);
                object graph = graphModel?.GetType().GetProperty("Graph", Flags)?.GetValue(graphModel);
                if (graph is ProcessGraph processGraph && processGraph.ProcessName == processName)
                {
                    return window;
                }
            }

            return null;
        }

        // Recovers the user INode from a selected (internal) graph-element model via its "Node" member,
        // falling back to any INode-typed member so a module rename doesn't silently break selection.
        private static bool TryGetUserNode(object model, out INode node)
        {
            node = null;
            if (model == null)
            {
                return false;
            }

            Type type = model.GetType();

            if (type.GetProperty("Node", Flags)?.GetValue(model) is INode named)
            {
                node = named;
                return true;
            }

            foreach (PropertyInfo property in type.GetProperties(Flags))
            {
                if (property.GetIndexParameters().Length == 0 && typeof(INode).IsAssignableFrom(property.PropertyType)
                    && property.GetValue(model) is INode found)
                {
                    node = found;
                    return true;
                }
            }

            return false;
        }
    }
}
