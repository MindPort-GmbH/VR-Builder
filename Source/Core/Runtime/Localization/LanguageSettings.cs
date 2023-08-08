// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Core.Localization
{
    /// <summary>
    /// Language settings for VR Builder.
    /// </summary>
    public class LanguageSettings : SettingsObject<LanguageSettings>
    {
        /// <summary>
        /// Returns the active or default language.
        /// </summary>
        public string ActiveOrDefaultLanguage
        {
            get
            {
                return ActiveOrDefaultLocale.Identifier.Code;
            }
        }

        /// <summary>
        /// Returns the active or default locale.
        /// </summary>
        public Locale ActiveOrDefaultLocale
        {
            get
            {
                if (LocalizationSettings.HasSettings)
                {
                    if (LocalizationSettings.SelectedLocale != null)
                    {
                        return LocalizationSettings.SelectedLocale;
                    }

                    if (LocalizationSettings.ProjectLocale != null)
                    {
                        return LocalizationSettings.ProjectLocale;
                    }
                }

                return Locale.CreateLocale(UnityEngine.SystemLanguage.English);
            }
        }
    }
}
