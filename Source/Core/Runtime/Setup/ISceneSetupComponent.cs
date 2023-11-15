namespace VRBuilder.Core.Setup
{
    /// <summary>
    /// Can be interacted with by scene setup scripts.
    /// </summary>
    public interface ISceneSetupComponent
    {
        /// <summary>
        /// Execute setup for this component.
        /// </summary>
        void ExecuteSetup();
    }
}
