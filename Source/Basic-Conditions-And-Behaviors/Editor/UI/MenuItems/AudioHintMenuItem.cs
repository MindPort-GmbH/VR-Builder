using System.Collections.Generic;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    public class AudioHintMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Guidance/Audio Hint";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            DelayBehavior delayBehavior = new DelayBehavior(5f);
            delayBehavior.Data.Name = "Wait for";

            PlayAudioBehavior audioBehavior = new PlayAudioBehavior(new ResourceAudio(""), BehaviorExecutionStages.Activation);
            audioBehavior.Data.Name = "Play Audio";

            BehaviorSequence behaviorSequence = new BehaviorSequence(true, new List<IBehavior>
            {
                delayBehavior,
                audioBehavior
            });
            
            behaviorSequence.Data.Name = "Audio Hint";
            behaviorSequence.Data.IsBlocking = false;

            return behaviorSequence;
        }
    }
}
