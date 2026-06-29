using System;
using System.Collections.Generic;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>Persisted wizard session snapshot for the history sidebar.</summary>
    [Serializable]
    public class WizardSessionRecord
    {
        public string Id;
        public WizardMode Mode;
        public string Title;
        public string CreatedAtUtc;
        public string UpdatedAtUtc;
        public List<WizardChatMessageRecord> Messages = new List<WizardChatMessageRecord>();
        public ProcessBlueprint Blueprint;
        public bool PromptAwaitingGenerate;
        public bool AwaitingAnother;
    }

    [Serializable]
    public class WizardChatMessageRecord
    {
        public bool IsUser;
        public string Text;
    }

    [Serializable]
    internal class WizardSessionRecordList
    {
        public List<WizardSessionRecord> Sessions = new List<WizardSessionRecord>();
    }
}
