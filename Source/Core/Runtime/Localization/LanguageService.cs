using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TinkerFlowDebug.addons.ProcessEngine.Source.Localization;
using UnityEngine.Localization;
using VRBuilder.Core;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Runtime.Utils;

namespace Source.Core.Runtime.Localization
{
    public class LanguageService: ILanguageService
    {
        public ILanguageConfiguration Configuration
        {
            get;
            set;
        }

        public string ProcessStringLocalizationTable
        {
            get;
        }

        public CultureInfo ActiveOrDefaultLocale { get; set; }

        public string ActiveOrDefaultRegionCode
        {
            get;
            set;
        }
        public string ApplicationLanguage
        {
            get;
            set;
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Get Locale object from a language or language code string.
        /// </summary>
        /// <param name="languageOrCode">The language or language code string.</param>
        /// <returns>The Locale object corresponding to the language code string or NULL.</returns>
        public CultureInfo GetCultureInfoFromString(string languageOrCode)
        {
            return GetLocaleFromString(languageOrCode).ToCultureInfo();
        }

        /// <summary>
        /// Get Locale object from a language or language code string.
        /// </summary>
        /// <param name="languageOrCode">The language or language code string.</param>
        /// <returns>The Locale object corresponding to the language code string or NULL.</returns>
        public Locale GetLocaleFromString(string languageOrCode)
        {
            Locale locale = Locale.CreateLocale(languageOrCode);
            if (locale.Identifier.CultureInfo == null)
            {
                if (TryConvertToTwoLetterIsoCode(languageOrCode, out var convertedCode))
                {
                    locale = Locale.CreateLocale(convertedCode);
                }
            }

            return locale;
        }

        public bool TryConvertToTwoLetterIsoCode(string language, out string result)
		{
			if (IsTwoLettersIsoCode(language))
			{
				result = language.ToLower();
				return true;
			}

			try
			{
				result = ConvertNaturalLanguageNameToTwoLetterIsoCode(language);
				return true;
			}
			catch (ArgumentException)
			{
				result = null;
				return false;
			}
		}

        public bool IsTwoLettersIsoCode(string language)
        {
            if (language == null)
            {
                return false;
            }

            // Some two-letter ISO codes are three letters long.
            if (language.Length is < 2 or > 3)
            {
                return false;
            }

            try
            {
                // If CultureInfo constructor is able to parse the string, it's two-letter ISO code.
                // ReSharper disable once ObjectCreationAsStatement
                new CultureInfo(language);
                return true;
            }
            catch (ArgumentException)
            {
                // Otherwise, it isn't.
                return false;
            }
        }

        public string ConvertNaturalLanguageNameToTwoLetterIsoCode(string languageName)
        {
            if (languageName == null)
            {
                throw new ArgumentNullException(nameof(languageName), "languageName is null");
            }
            IEnumerable<CultureInfo> allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            CultureInfo languageCulture = allCultures.FirstOrDefault(culture =>
            {
                char[] symbolsToRemove = { '(', ')', ' ' };

                string preparedCultureName = symbolsToRemove.Aggregate(culture.EnglishName, (current, symbol) => current.Replace(symbol.ToString(), ""));

                return string.Compare(preparedCultureName, languageName, StringComparison.OrdinalIgnoreCase) == 0;
            });
            return languageCulture is not null ? languageCulture.TwoLetterISOLanguageName : throw new ArgumentException("languageName is not a supported language name", nameof(languageName));
        }

        public void SetConfiguration(object configuration)
        {
           Configuration = (ILanguageConfiguration)configuration;
        }

        public string GetLocalizedString(string localizationKey, string localizationTable)
        {
            if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(localizationTable))
            {
                LocalizedString localizedString = new LocalizedString(localizationTable, localizationKey);
                return localizedString.GetLocalizedString();
            }
            return localizationKey;
        }

        public string GetLocalizedString(string localizationKey)
        {
            if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(LanguageSettingsLocator.Current.ProcessStringLocalizationTable))
            {
                LocalizedString localizedString = new LocalizedString(LanguageSettingsLocator.Current.ProcessStringLocalizationTable, localizationKey);
                localizedString.LocaleOverride = LanguageSettingsLocator.Current.ActiveOrDefaultLocale.ToUnity();
                return localizedString.GetLocalizedString();
            }
            return localizationKey;
        }

        public string GetLocalizedString(string localizationKey, string localizationTable, CultureInfo locale)
        {
            if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(localizationTable))
            {
                LocalizedString localizedString = new LocalizedString(localizationTable, localizationKey);
                localizedString.LocaleOverride = locale.ToUnity();
                return localizedString.GetLocalizedString();
            }
            return localizationKey;
        }

        public string GetLocalizedChapterName(IChapter chapter, string localizationTable, CultureInfo locale)
        {
            return chapter is not null ? GetLocalizedString(chapter.Data.Name, localizationTable, locale) : "";
        }

        public string GetLocalizedStepName(IStep step, string localizationTable, CultureInfo locale)
        {
            return step is not null ? GetLocalizedString(step.Data.Name, localizationTable, locale) : "";
        }
    }
}
