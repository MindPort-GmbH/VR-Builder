using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Runtime.Utils
{
    public abstract class ComponentSettings<TObject, TSettings> : SettingsObject<TSettings> where TSettings : ScriptableObject, new()
    {
        protected static TSettings settings;

        /// <summary>
        /// Loads the first existing settings found in the project.
        /// If non are found, it creates and saves a new instance with default values.
        /// </summary>
        public static TSettings Settings => RetrieveSnapZoneSettings();

        public abstract void ApplySettings(TObject target);

        protected static TSettings RetrieveSnapZoneSettings()
        {
            if (settings == null)
            {
                string filter = $"t:ScriptableObject {nameof(TSettings)}";

                foreach (string guid in AssetDatabase.FindAssets(filter))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    return settings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TSettings)) as TSettings;
                }

                settings = CreateNewConfiguration();
            }

            return settings;
        }

        protected static TSettings CreateNewConfiguration()
        {
            TSettings snapZoneSettings = CreateInstance<TSettings>();

            string filePath = "Assets/MindPort/VR Builder/Resources";

            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            AssetDatabase.CreateAsset(snapZoneSettings, $"{filePath}/{nameof(TSettings)}.asset");
            AssetDatabase.Refresh();

            return snapZoneSettings;
        }
    }
}
