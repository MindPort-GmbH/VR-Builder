using System;
using UnityEngine;
using VRBuilder.Core.Localization;

namespace Source.Core.Runtime.Localization
{
    public class LanguageHandler: MonoBehaviour, ILanguageHandler
    {
        /// <summary>
        /// Raised when the selected process changes. The string argument is the new streaming-assets path of the selected process.
        /// </summary>
        public event Action<string> SelectedLocalisatzionChanged;
    }
}
