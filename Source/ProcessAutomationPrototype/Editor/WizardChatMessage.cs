namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>A single message in a wizard conversation transcript.</summary>
    public class WizardChatMessage
    {
        public bool IsUser { get; }
        public string Text { get; }

        public WizardChatMessage(bool isUser, string text)
        {
            IsUser = isUser;
            Text = text;
        }
    }
}
