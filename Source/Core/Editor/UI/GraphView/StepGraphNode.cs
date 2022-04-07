using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGraphNode : ProcessGraphNode
    {
        public StepGraphNode()
        {
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                Step.StepMetadata.Position = GetPosition().position;
            });
        }

        public IStep Step { get; set; }        
    }
}
