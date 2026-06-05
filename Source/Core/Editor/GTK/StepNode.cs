using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace MindPort.GTKProcessEditor.Editor
{
    // A VR Builder step: one input and one output per transition. StepGuid links the node to its step.
    [Serializable]
    [UseWithGraph(typeof(ProcessGraph))]
    public class StepNode : Node
    {
        public const string InputPortName = "In";
        public const string OutputPortPrefix = "Out";
        public const string NameOption = "Name";

        [SerializeField]
        private string stepGuid;

        [SerializeField]
        private string stepName;

        [SerializeField]
        private string[] transitionLabels = Array.Empty<string>();

        public string StepGuid
        {
            get => stepGuid;
            set => stepGuid = value;
        }

        public string StepName
        {
            get => stepName;
            set => stepName = value;
        }

        public string[] TransitionLabels
        {
            get => transitionLabels;
            set => transitionLabels = value ?? Array.Empty<string>();
        }

        public static string GetOutputPortName(int index)
        {
            return $"{OutputPortPrefix}{index}";
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(NameOption)
                .WithDisplayName("Name")
                .WithTooltip("Step name (from the VR Builder process).")
                .WithDefaultValue(stepName ?? string.Empty)
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(InputPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            int count = transitionLabels != null ? transitionLabels.Length : 0;
            for (int i = 0; i < count; i++)
            {
                string label = string.IsNullOrEmpty(transitionLabels[i]) ? $"Transition {i + 1}" : transitionLabels[i];
                context.AddOutputPort(GetOutputPortName(i))
                    .WithDisplayName(label)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }
    }
}
