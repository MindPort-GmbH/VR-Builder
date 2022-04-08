using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace VRBuilder.Editor.UI.Graphics
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        public event EventHandler<EdgeConnectorListenerEventArgs> ConnectorDroppedOnPort;

        public void OnDrop(GraphView graphView, Edge edge)
        {            
            ConnectorDroppedOnPort?.Invoke(this, new EdgeConnectorListenerEventArgs(edge));
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    }
}
