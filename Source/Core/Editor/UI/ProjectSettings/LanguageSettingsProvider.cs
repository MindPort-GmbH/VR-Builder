// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    public class LanguageSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Language";

        public LanguageSettingsProvider() : base(Path, SettingsScope.Project) { }

        protected override void InternalDraw(string searchContext)
        {
            LanguageSettings config = LanguageSettings.Instance;

            if (LocalizationSettings.HasSettings)
            {
                ShowLocalePopup();
            }
            else
            {
                ShowLanguageSelector();
            }
        }

        private void ShowLanguageSelector()
        {
            string oldLanguage = LanguageSettings.Instance.ApplicationLanguage;
            LanguageSettings.Instance.ApplicationLanguage = EditorGUILayout.TextField("Language Code", LanguageSettings.Instance.ApplicationLanguage);

            Locale locale = LanguageSettings.Instance.GetLocaleFromString(LanguageSettings.Instance.ApplicationLanguage);
            if (locale.Identifier.CultureInfo != null)
            {
                EditorGUILayout.LabelField($"Application language: {locale}");
            }
            else
            {
                EditorGUILayout.HelpBox($"Code \"{locale.Identifier.Code}\" not recognized.\nSystem default ({System.Globalization.CultureInfo.CurrentCulture}) will be used.", MessageType.Error);
            }

            if (LanguageSettings.Instance.ApplicationLanguage != oldLanguage)
            {
                EditorUtility.SetDirty(LanguageSettings.Instance);
            }
        }

        private void ShowLocalePopup()
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("Project Locale ", LocalizationSettings.ProjectLocale ? LocalizationSettings.ProjectLocale.ToString() : "None");
            GUI.enabled = true;

            if (LocalizationSettings.AvailableLocales != null && LocalizationSettings.AvailableLocales.Locales.Count > 1)
            {
                int selectedIndex = 0;
                List<string> supportedLanguages = new List<string>();
                supportedLanguages.Add("None (Use Project Locale)");
                var locales = LocalizationSettings.AvailableLocales.Locales;
                for (int i = 0; i < locales.Count; ++i)
                {
                    if (locales[i] == LocalizationSettings.SelectedLocale)
                        selectedIndex = i + 1;

                    supportedLanguages.Add(locales[i].ToString());
                }

                int newIndex = EditorGUILayout.Popup("Active Locale", selectedIndex, supportedLanguages.ToArray());
                if (newIndex <= 0)
                    LocalizationSettings.SelectedLocale = null;
                else
                    LocalizationSettings.SelectedLocale = locales[newIndex - 1];
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.TextField("Active Locale", LocalizationSettings.SelectedLocale ? LocalizationSettings.SelectedLocale.ToString() : "None");
                GUI.enabled = true;
            }
        }


        public override void OnDeactivate()
        {
            if (EditorUtility.IsDirty(LanguageSettings.Instance))
            {
                LanguageSettings.Instance.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new LanguageSettingsProvider();
            return provider;
        }
    }
}
