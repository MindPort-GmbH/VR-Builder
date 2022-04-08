using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGraphNode : ProcessGraphNode
    {
        public StepGraphNode()
        {
            //RegisterCallback<event>(evt => Debug.Log("Selected"));
        }

        public IStep Step { get; set; }        
    }
}
