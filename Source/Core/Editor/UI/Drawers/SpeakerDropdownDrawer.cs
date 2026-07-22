using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for a dropdown listing all configured voice profiles in <see cref="TextToSpeechProviderSettings"/>.
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
            TextToSpeechProviderSettings.Instance.ProviderChanged -= BuildProfileList;
            TextToSpeechProviderSettings.Instance.VoiceProfilesChanged -= BuildProfileList;
            
            TextToSpeechProviderSettings.Instance.ProviderChanged += BuildProfileList;
            TextToSpeechProviderSettings.Instance.VoiceProfilesChanged += BuildProfileList;
        }
        
        ~SpeakerDropdownDrawer()
        {
            TextToSpeechProviderSettings.Instance.ProviderChanged -= BuildProfileList;
            TextToSpeechProviderSettings.Instance.VoiceProfilesChanged -= BuildProfileList;
        }

        private void BuildProfileList()
        {
            options.Clear();

            foreach (var profile in TextToSpeechProviderSettings.Instance.VoiceProfiles)
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
