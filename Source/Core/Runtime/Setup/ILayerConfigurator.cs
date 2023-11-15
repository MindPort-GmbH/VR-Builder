namespace VRBuilder.Core.Setup
{
    /// <summary>
    /// Can configure layer masks on an interactor or interactable.
    /// </summary>
    public interface ILayerConfigurator
    {
        LayerSet LayerSet { get; }

        /// <summary>
        /// Set up the layer masks to the specified layer names.
        /// </summary>
        void ConfigureLayers(string interactionLayerName, string raycastLayerName);
    }
}
