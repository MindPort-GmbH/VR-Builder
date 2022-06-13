using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Instantiator for a default <see cref="IStep"/> node.
    /// </summary>
    public class DefaultStepNodeInstantiator : IStepNodeInstantiator
    {
        /// <inheritdoc/>
        public string Name => "Step";

        /// <inheritdoc/>
        public bool IsUserCreatable => true;

        /// <inheritdoc/>
        public string Representation => "default";

        /// <inheritdoc/>
        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new StepGraphNode(step);
        }

        //public DefaultStepNodeInstantiator()
        //{
        //}
    }
}
