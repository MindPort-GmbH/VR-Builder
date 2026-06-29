using System;
using System.Linq;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>Converts live wizard sessions to persisted history records.</summary>
    public static class WizardSessionArchive
    {
        private const int TitleMaxLength = 48;

        public static WizardSessionRecord ToRecord(WizardSession session)
        {
            string now = WizardSessionStore.UtcNowIso();
            WizardSessionRecord record = new WizardSessionRecord
            {
                Id = WizardSessionStore.NewId(),
                Mode = session.Mode,
                Title = BuildTitle(session),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                Blueprint = session.PendingBlueprint,
                PromptAwaitingGenerate = session.PromptAwaitingGenerate,
                AwaitingAnother = session.AwaitingAnother,
            };

            foreach (WizardChatMessage message in session.Messages)
            {
                record.Messages.Add(new WizardChatMessageRecord
                {
                    IsUser = message.IsUser,
                    Text = message.Text,
                });
            }

            return record;
        }

        public static void TryArchive(WizardSession session, WizardSessionStore store)
        {
            if (session == null || store == null || session.Messages.Count == 0)
            {
                return;
            }

            if (session.Messages.Count <= session.LastArchivedMessageCount)
            {
                return;
            }

            store.Save(ToRecord(session));
            session.LastArchivedMessageCount = session.Messages.Count;
        }

        public static string BuildTitle(WizardSession session)
        {
            if (session.PendingBlueprint != null && !string.IsNullOrWhiteSpace(session.PendingBlueprint.ProcessName))
            {
                return Truncate(session.PendingBlueprint.ProcessName);
            }

            WizardChatMessage firstUser = session.Messages.FirstOrDefault(message => message.IsUser);
            if (firstUser != null && !string.IsNullOrWhiteSpace(firstUser.Text))
            {
                return Truncate(firstUser.Text.Replace('\n', ' ').Trim());
            }

            string prefix = session.Mode == WizardMode.Guided ? "Guided session" : "Prompt session";
            return $"{prefix} {DateTime.Now:MMM d, HH:mm}";
        }

        public static string BuildTitle(WizardSessionRecord record)
        {
            if (!string.IsNullOrWhiteSpace(record.Title))
            {
                return record.Title;
            }

            if (record.Blueprint != null && !string.IsNullOrWhiteSpace(record.Blueprint.ProcessName))
            {
                return Truncate(record.Blueprint.ProcessName);
            }

            WizardChatMessageRecord firstUser = record.Messages.FirstOrDefault(message => message.IsUser);
            if (firstUser != null && !string.IsNullOrWhiteSpace(firstUser.Text))
            {
                return Truncate(firstUser.Text.Replace('\n', ' ').Trim());
            }

            return record.Mode == WizardMode.Guided ? "Guided session" : "Prompt session";
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= TitleMaxLength)
            {
                return value;
            }

            return value.Substring(0, TitleMaxLength - 1) + "…";
        }
    }
}
