// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using Source.Core.Runtime.Localization;
using VRBuilder.Core.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Language settings for VR Builder.
    /// </summary>
    public class LanguageServiceSettings : SettingsObject<LanguageServiceSettings>, ILanguageConfiguration
    {
        /// <summary>
        /// Language that should be used if no localization settings are present.
        /// </summary>
        public string ApplicationLanguage = "En";

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
        ///
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

                Locale locale = ServiceRegistry.Get<LanguageService>().GetLocaleFromString(ApplicationLanguage);

                if (locale.Identifier.CultureInfo != null)
                {
                    return locale;
                }
                else
                {
                    return Locale.CreateLocale(System.Globalization.CultureInfo.CurrentCulture);
                }
            }
        }

    }
}
