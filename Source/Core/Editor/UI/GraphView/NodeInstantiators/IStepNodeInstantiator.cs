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
        /// Metadata string value.
        /// </summary>
        string Representation { get; }

        /// <summary>
        /// Creates a graphview node of the corresponding type. 
        /// </summary>
        ProcessGraphNode InstantiateNode(IStep step);
    }
}
