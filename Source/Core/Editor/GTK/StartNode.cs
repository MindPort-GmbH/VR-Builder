using System;
using Unity.GraphToolkit.Editor;

namespace MindPort.GTKProcessEditor.Editor
{
    // Chapter start node: a single output that points at the chapter's first step.
    [Serializable]
    [UseWithGraph(typeof(ProcessGraph))]
    public class StartNode : Node
    {
        public const string OutputPortName = "Out";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort(OutputPortName)
                .WithDisplayName("Start")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
