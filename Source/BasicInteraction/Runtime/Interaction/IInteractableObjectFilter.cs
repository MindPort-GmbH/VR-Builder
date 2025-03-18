namespace VRBuilder.BasicInteraction.Interaction
{
    /// <summary>
    /// Determines if a given interactor is allowed to interact with an associated interactable.
    /// </summary>
    public interface IInteractableObjectFilter
    {
        bool IsHoverableBy(IRestrictedInteractor interactor);

        bool IsSelectableBy(IRestrictedInteractor interactor);

        bool IsActivableBy(IRestrictedInteractor interactor);
    }
}