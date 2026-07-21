// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using UnityEngine.Localization;
namespace VRBuilder.Core.Runtime.Utils
{
    public static class LocaleExtensions
    {
        public static CultureInfo ToCultureInfo(this Locale locale)
        {
            return new CultureInfo(locale.ToString());
        }

        public static Locale ToUnity(this CultureInfo locale)
        {
            return Locale.CreateLocale(locale);
        }
    }
}
