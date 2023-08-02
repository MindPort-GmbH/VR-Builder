using VRBuilder.Core.Behaviors;
using VRBuilder.TextToSpeech.Audio;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.TextToSpeech.UI.Behaviors
{
    /// <inheritdoc />
    public class LocalizedTextToSpeechMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Guidance/Play Localized TextToSpeech Audio";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new PlayAudioBehavior(new LocalizedTextToSpeechAudio("", ""), BehaviorExecutionStages.Activation, true, null);
        }
    }
}
