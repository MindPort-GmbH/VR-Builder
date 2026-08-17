using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for a dropdown listing all configured voice profiles in <see cref="TextToSpeechServiceSettings"/>.
    /// </summary>
    public class SpeakerDropdownDrawer : DropdownDrawer<string>
    {
        private List<DropDownElement<string>> options = new();
        private GUIContent[] labels;

        /// <inheritdoc/>
        protected override IList<DropDownElement<string>> PossibleOptions => options;

        public SpeakerDropdownDrawer()
        {
            BuildProfileList();

            // Remove existing subscription
            TextToSpeechServiceSettings.Instance.ProviderChanged -= BuildProfileList;
            TextToSpeechServiceSettings.Instance.VoiceProfilesChanged -= BuildProfileList;
            
            TextToSpeechServiceSettings.Instance.ProviderChanged += BuildProfileList;
            TextToSpeechServiceSettings.Instance.VoiceProfilesChanged += BuildProfileList;
        }
        
        ~SpeakerDropdownDrawer()
        {
            TextToSpeechServiceSettings.Instance.ProviderChanged -= BuildProfileList;
            TextToSpeechServiceSettings.Instance.VoiceProfilesChanged -= BuildProfileList;
        }

        private void BuildProfileList()
        {
            options.Clear();

            foreach (var profile in TextToSpeechServiceSettings.Instance.VoiceProfiles)
            {
                options.Add(new DropDownElement<string>(profile.DisplayName, profile.DisplayName));
            }

            if (options.Count == 0)
            {
                options.Add(new DropDownElement<string>("Default", "Default"));
            }

            labels = options.Select(item => item.Label).ToArray();
        }
    }
}
