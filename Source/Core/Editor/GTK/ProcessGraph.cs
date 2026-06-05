using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace MindPort.GTKProcessEditor.Editor
{
    // Graph Toolkit asset that draws a VR Builder process (chapter 1). Stores only which process it shows.
    [Serializable]
    [GraphAttribute(FileExtension)]
    public class ProcessGraph : Graph
    {
        public const string FileExtension = "vrbgtk";

        [SerializeField]
        private string processName;

        public string ProcessName
        {
            get => processName;
            set => processName = value;
        }

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);
            GraphSync.Schedule(this);
        }
    }
}
