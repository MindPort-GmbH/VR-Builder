using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>Persists wizard session history under Library/VRBuilder/ProcessWizardSessions/.</summary>
    public class WizardSessionStore
    {
        private const int MaxSessions = 50;
        private const string RelativeFolder = "VRBuilder/ProcessWizardSessions";
        private const string IndexFileName = "sessions.json";

        private static string FolderPath =>
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", RelativeFolder);

        private static string IndexPath => Path.Combine(FolderPath, IndexFileName);

        public IReadOnlyList<WizardSessionRecord> LoadAll()
        {
            EnsureFolder();
            if (!File.Exists(IndexPath))
            {
                return Array.Empty<WizardSessionRecord>();
            }

            try
            {
#if NEWTONSOFT_JSON
                string json = File.ReadAllText(IndexPath);
                WizardSessionRecordList list = JsonConvert.DeserializeObject<WizardSessionRecordList>(json);
                if (list?.Sessions == null)
                {
                    return Array.Empty<WizardSessionRecord>();
                }

                return list.Sessions
                    .OrderByDescending(session => session.UpdatedAtUtc)
                    .ToList();
#else
                return Array.Empty<WizardSessionRecord>();
#endif
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Process Wizard: could not load session history. {exception.Message}");
                return Array.Empty<WizardSessionRecord>();
            }
        }

        public void Save(WizardSessionRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.Id))
            {
                return;
            }

#if !NEWTONSOFT_JSON
            return;
#else
            EnsureFolder();
            List<WizardSessionRecord> sessions = LoadMutable();
            int existingIndex = sessions.FindIndex(session => session.Id == record.Id);
            if (existingIndex >= 0)
            {
                sessions[existingIndex] = record;
            }
            else
            {
                sessions.Add(record);
            }

            sessions = sessions
                .OrderByDescending(session => session.UpdatedAtUtc)
                .Take(MaxSessions)
                .ToList();

            string json = JsonConvert.SerializeObject(new WizardSessionRecordList { Sessions = sessions }, Formatting.Indented);
            File.WriteAllText(IndexPath, json);
#endif
        }

        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

#if NEWTONSOFT_JSON
            EnsureFolder();
            List<WizardSessionRecord> sessions = LoadMutable();
            int removed = sessions.RemoveAll(session => session.Id == id);
            if (removed == 0)
            {
                return;
            }

            string json = JsonConvert.SerializeObject(new WizardSessionRecordList { Sessions = sessions }, Formatting.Indented);
            File.WriteAllText(IndexPath, json);
#endif
        }

        public static string NewId() => Guid.NewGuid().ToString("N");

        public static string UtcNowIso() =>
            DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        private static void EnsureFolder()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        private List<WizardSessionRecord> LoadMutable()
        {
            IReadOnlyList<WizardSessionRecord> loaded = LoadAll();
            return loaded.Count == 0 ? new List<WizardSessionRecord>() : loaded.ToList();
        }
    }
}
