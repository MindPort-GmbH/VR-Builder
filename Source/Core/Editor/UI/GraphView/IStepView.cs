using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Windows
{
    public interface IStepView
    {
        void SetStep(IStep newStep);

        void ResetStepView();
    }
}
