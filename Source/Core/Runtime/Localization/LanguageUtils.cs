// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.Localization;

namespace VRBuilder.Core.Localization
{
    /// <summary>
    /// Collection of language utilities.
    /// </summary>
    public static class LanguageUtils
    {
        #region Language Code Utils

        /// <summary>
        /// Convert natural language name to two-letters ISO code.
        /// </summary>
        /// <param name="language">
        /// String with natural language name or two-letters ISO code.
        /// </param>
        /// <param name="result">
        /// If <paramref name="language"/> is already in two-letters ISO code, simply returns it.
        /// If <paramref name="language"/> is a natural language name, returns two-symbol code.
        /// Otherwise, returns null.
        /// </param>
        /// <returns>
        /// Was operation successful or not.
        /// </returns>
        public static bool TryConvertToTwoLetterIsoCode(this string language, out string result)
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

        /// <summary>
        /// Helps to convert strings with full language names like "English" to a two-letter ISO language code.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="languageName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="languageName"/> is not natural language name.</exception>
        /// <returns>The two-letter ISO code from the given language name. If it can not parse the string, it returns null.</returns>
        private static string ConvertNaturalLanguageNameToTwoLetterIsoCode(this string languageName)
        {
            if (languageName == null)
            {
                throw new ArgumentNullException("languageName", "languageName is null");
            }

            IEnumerable<CultureInfo> allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            CultureInfo languageCulture = allCultures.FirstOrDefault(culture =>
            {
                string preparedCultureName = culture.EnglishName.RemoveSymbols('(', ')', ' ');
                return string.Compare(preparedCultureName, languageName, StringComparison.OrdinalIgnoreCase) == 0;
            });

            if (languageCulture != null)
            {
                return languageCulture.TwoLetterISOLanguageName;
            }

            throw new ArgumentException("languageName is not a supported language name", "languageName");
        }

        /// <summary>
        /// Check if <paramref name="language"/> is two-letter ISO code.
        /// </summary>
        private static bool IsTwoLettersIsoCode(string language)
        {
            if (language == null)
            {
                return false;
            }

            // Some two-letter ISO codes are three letters long.
            if (language.Length < 2 || language.Length > 3)
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

        /// <summary>
        /// Remove <paramref name="symbolsToRemove"/> from <paramref name="input"/> string.
        /// </summary>
        private static string RemoveSymbols(this string input, params char[] symbolsToRemove)
        {
            string result = input;
            foreach (char symbol in symbolsToRemove)
            {
                result = result.Replace(symbol.ToString(), "");
            }

            return result;
        }

        #endregion Language Code Utils

        #region Unity Localization Utils

        /// <summary>
        /// Try to localize a step name if used as a key in a localization table
        /// </summary>
        public static string GetLocalizedStepName(IStep step, string localizationTable, Locale locale)
        {
            if (step != null)
            {
                return GetLocalizedString(step.Data.Name, localizationTable, locale);
            }
            return "";
        }

        /// <summary>
        /// Try to localize a chapter name if used as a key in a localization table
        /// </summary>
        public static string GetLocalizedChapterName(IChapter chapter, string localizationTable, Locale locale)
        {
            if (chapter != null)
            {
                return GetLocalizedString(chapter.Data.Name, localizationTable, locale);
            }
            return "";
        }

        /// <summary>
        /// Try to get the localized string for a key and in a table with the currently selcted locale
        /// </summary>
        public static string GetLocalizedString(string localizationKey, string localizationTable)
        {
            if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(localizationTable))
            {
                LocalizedString localizedString = new LocalizedString(localizationTable, localizationKey);
                return localizedString.GetLocalizedString();
            }
            return localizationKey;
        }

        /// <summary>
        /// Try to get the localized string for a key and in a table with a custom locale
        /// </summary>
        public static string GetLocalizedString(string localizationKey, string localizationTable, Locale locale)
        {
            if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(localizationTable))
            {
                LocalizedString localizedString = new LocalizedString(localizationTable, localizationKey);
                localizedString.LocaleOverride = locale;
                return localizedString.GetLocalizedString();
            }
            return localizationKey;
        }

        #endregion Unity Localization Utils
    }
}
