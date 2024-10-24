using VRBuilder.Core.Behaviors;
using VRBuilder.TextToSpeech.Audio;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.TextToSpeech.Editor.UI.Behaviors
{
    /// <inheritdoc />
    public class TextToSpeechMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Guidance/Play TextToSpeech Audio";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new PlayAudioBehavior(new TextToSpeechAudio(""), BehaviorExecutionStages.Activation, true);
        }
    }
}
