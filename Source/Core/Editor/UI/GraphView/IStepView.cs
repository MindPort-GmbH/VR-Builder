namespace VRBuilder.Core.Editor.UI.GraphView
{
    /// <summary>
    /// Interface for GUI step view.
    /// </summary>
    public interface IStepView
    {
        /// <summary>
        /// Sets a new step.
        /// </summary>        
        void SetStep(IStep newStep);

        /// <summary>
        /// Marks the view as dirty and requests a refresh.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Resets the step view.
        /// </summary>
        void ResetStepView();
    }
}
