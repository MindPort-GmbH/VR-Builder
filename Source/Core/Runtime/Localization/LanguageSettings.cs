// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using UnityEngine;
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
        /// Language which should be used if no localization settings are present.
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

                Locale locale = GetLocaleFromString(ApplicationLanguage);

                if(locale.Identifier.CultureInfo != null)
                {
                    return locale;
                }
                else
                {
                    return Locale.CreateLocale(System.Globalization.CultureInfo.CurrentCulture);
                }                
            }
        }

        public Locale GetLocaleFromString(string languageCode)
        {
            Locale locale = Locale.CreateLocale(languageCode);
            if(locale.Identifier.CultureInfo == null)
            {
                string convertedCode;
                if(LanguageUtils.TryConvertToTwoLetterIsoCode(languageCode, out convertedCode))
                {
                    locale = Locale.CreateLocale(convertedCode);
                }                
            }

            return locale;
        }
    }
}
