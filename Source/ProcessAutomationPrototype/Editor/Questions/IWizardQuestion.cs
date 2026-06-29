using System;
using UnityEngine.UIElements;

namespace VRBuilder.ProcessAutomationPrototype.Editor.Questions
{
    /// <summary>
    /// A wizard question rendered in the conversation UI.
    /// </summary>
    public interface IWizardQuestion
    {
        /// <summary>The assistant prompt shown as a chat message.</summary>
        string Title { get; }

        /// <summary>Builds the input controls for this question.</summary>
        /// <param name="prefill">Previous answer when navigating back; otherwise null.</param>
        void BuildComposer(VisualElement composer, object prefill, Action<object, string> submit, Action<string> error);

        /// <summary>Default answer used when estimating how many questions remain in the guided flow.</summary>
        object SimulationDefault { get; }
    }
}
