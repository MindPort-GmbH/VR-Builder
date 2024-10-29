using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
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
