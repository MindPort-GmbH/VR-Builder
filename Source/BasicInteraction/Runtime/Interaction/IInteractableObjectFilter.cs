namespace VRBuilder.BasicInteraction.Interaction
{
    /// <summary>
    /// Determines if a given interaction is allowed to interact with an associated interactable.
    /// </summary>
    public interface IInteractableObjectFilter
    {
        bool IsHoverableBy(IInteractorRestriction interactor);

        bool IsSelectableBy(IInteractorRestriction interactor);

        bool IsActivableBy(IInteractorRestriction interactor);
    }
}