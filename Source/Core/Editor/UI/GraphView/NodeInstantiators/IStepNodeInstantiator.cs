using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Instantiates a node matching the 
    /// </summary>
    public interface IStepNodeInstantiator
    {
        /// <summary>
        /// Display name of the instantiated node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If true, it will appear in the node menu.
        /// </summary>
        bool IsUserCreatable { get; }

        /// <summary>
        /// Nodes with a lower value will appear first in the menu.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Metadata string value.
        /// </summary>
        string Representation { get; }

        /// <summary>
        /// Creates a graphview node of the corresponding type. 
        /// </summary>
        ProcessGraphNode InstantiateNode(IStep step);

        /// <summary>
        /// Creates a step suitable for this type of node.
        /// </summary>        
        IStep CreateStep();
    }
}
