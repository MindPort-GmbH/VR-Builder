using System;
using UnityEditor.Experimental.GraphView;

namespace VRBuilder.Editor.UI.Graphics
{
    public class EdgeConnectorListenerEventArgs : EventArgs
    {
        public Edge Edge;

        public EdgeConnectorListenerEventArgs(Edge edge)
        {
            Edge = edge;
        }
    }
}
