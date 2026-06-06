namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    /// <summary>
    /// Cross-cutting state every panel and child drawer needs while building UI.
    /// </summary>
    public interface IElementDrawerContext
    {
        void NotifyStepModified();
    }
}
