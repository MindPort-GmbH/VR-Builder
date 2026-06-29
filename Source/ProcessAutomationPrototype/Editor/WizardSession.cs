using System.Collections.Generic;
using System.Linq;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>In-memory state for one guided or prompt wizard session.</summary>
    public class WizardSession
    {
        public WizardMode Mode { get; }

        public List<WizardChatMessage> Messages { get; } = new List<WizardChatMessage>();

        public ProcessBlueprint PendingBlueprint { get; set; }

        public WizardController Controller { get; set; }

        public bool GuidedStarted { get; set; }

        public string DraftPromptText { get; set; } = string.Empty;

        public string LastFailedPromptText { get; set; } = string.Empty;

        public bool PromptAwaitingGenerate { get; set; }

        public bool PromptIsBusy { get; set; }

        public bool AwaitingAnother { get; set; }

        public int LastArchivedMessageCount { get; set; }

        public WizardSession(WizardMode mode)
        {
            Mode = mode;
        }

        public bool HasProgress
        {
            get
            {
                if (PendingBlueprint != null)
                {
                    return true;
                }

                if (Mode == WizardMode.Guided)
                {
                    return GuidedStarted && (Controller?.CanGoBack == true || Controller?.IsFinished == true);
                }

                return Messages.Any(message => message.IsUser);
            }
        }

        public void Reset()
        {
            Messages.Clear();
            PendingBlueprint = null;
            GuidedStarted = false;
            DraftPromptText = string.Empty;
            LastFailedPromptText = string.Empty;
            PromptAwaitingGenerate = false;
            PromptIsBusy = false;
            AwaitingAnother = false;
            LastArchivedMessageCount = 0;
            Controller = null;
        }
    }
}
