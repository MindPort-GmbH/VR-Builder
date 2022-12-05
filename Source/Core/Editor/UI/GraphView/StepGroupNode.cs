using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGroupNode : ProcessGraphNode
    {
        private StepGroup stepGroup;

        public override string Name { get => stepGroup.Data.Name; set => stepGroup.Data.Name = value; }

        public override IStep[] Outputs => throw new System.NotImplementedException();

        public override IStep EntryPoint => stepGroup;

        public override Vector2 Position { get => stepGroup.StepMetadata.Position; set => stepGroup.StepMetadata.Position = value; }

        public override void AddToChapter(IChapter chapter)
        {
            chapter.Data.Steps.Add(stepGroup);
        }

        public override void RemoveFromChapter(IChapter chapter)
        {
            chapter.Data.Steps.Remove(stepGroup);
        }

        public override void SetOutput(int index, IStep output)
        {
            throw new System.NotImplementedException();
        }

        protected override void RemovePortWithUndo(Port port)
        {
            throw new System.NotImplementedException();
        }
    }
}
