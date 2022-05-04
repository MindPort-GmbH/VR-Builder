using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Entry point node in a graph view editor.
    /// </summary>
    public class EntryPointNode : ProcessGraphNode
    {
        public EntryPointNode(): base()
        {
            title = "Start";
            IsEntryPoint = true;

            capabilities = Capabilities.Ascendable;
            titleButtonContainer.Clear();

            AddTransitionPort(false);

            SetPosition(new Rect(GlobalEditorHandler.GetCurrentChapter().ChapterMetadata.EntryNodePosition, defaultNodeSize));
        }        

        public override IStep[] Outputs => new[] { GlobalEditorHandler.GetCurrentChapter().Data.FirstStep };

        public override IStep EntryPoint => null;

        public override Vector2 Position { get => GlobalEditorHandler.GetCurrentChapter().ChapterMetadata.EntryNodePosition; set => GlobalEditorHandler.GetCurrentChapter().ChapterMetadata.EntryNodePosition = value; }

        public override string Name { get => title; set => title = value; }

        public override void AddToChapter(IChapter chapter)
        {
        }

        public override void Refresh()
        {
        }

        public override void RemoveFromChapter(IChapter chapter)
        {
        }

        public override void SetOutput(int index, IStep output)
        {
            GlobalEditorHandler.GetCurrentChapter().Data.FirstStep = output;
        }

        protected override void RemovePortWithUndo(Port port)
        {
        }
    }
}
