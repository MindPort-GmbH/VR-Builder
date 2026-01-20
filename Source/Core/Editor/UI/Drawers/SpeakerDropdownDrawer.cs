using System.Collections.Generic;
using System.IO;
using Source.Core.Runtime.TextToSpeech;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech;
using VRBuilder.Core.TextToSpeech.Providers;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for a dropdown listing all availed speakers of <see cref="ITextToSpeechProvider"/> if multiple speaker supported and allowing to select one by index.
    /// </summary>
    public class SpeakerDropdownDrawer : DropdownDrawer<string>
    {
        /// <inheritdoc/>
        protected override IList<DropDownElement<string>> PossibleOptions => options;

        private List<DropDownElement<string>> options = new();

        public SpeakerDropdownDrawer()
        {
            BuildSceneList();

            TextToSpeechSettings.Instance.ProviderChanged += BuildSceneList;
        }

        private void BuildSceneList()
        {
            options.Clear();

            if (RuntimeConfigurator.Exists)
            {
                var textToSpeechProvider = TextToSpeechSettings.Instance.GetCurrentTextToSpeechProvider();

                if (textToSpeechProvider is ITextToSpeechSpeaker speakers)
                {
                    foreach (var speaker in speakers.GetSpeaker())
                    {
                        options.Add(new DropDownElement<string>(speaker, speaker));
                    }
                }
            }
        }
    }
}
