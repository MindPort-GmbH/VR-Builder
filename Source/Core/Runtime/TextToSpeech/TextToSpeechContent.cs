using UnityEngine.Localization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech
{
    /// <summary>
    /// Utility implementation of the <see cref="ITextToSpeechContent"/> interface that provides a default <see cref="IsCached"/> method.
    /// </summary>
    public abstract class TextToSpeechContent : ITextToSpeechContent, ILocalizedContent
    {
        /// <inheritdoc/>
        public abstract string Text { get; set; }

        /// <inheritdoc/>
        public abstract string GetLocalizedContent();

        /// <inheritdoc/>
        public virtual bool IsCached(Locale locale)
        {
            return RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration().IsCached(locale, GetLocalizedContent());
        }
    }
}
