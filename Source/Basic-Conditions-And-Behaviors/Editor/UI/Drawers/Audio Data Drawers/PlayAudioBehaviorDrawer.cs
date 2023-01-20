using System;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.Drawers;
using UnityEngine;
using VRBuilder.Editor.UI;
using VRBuilder.Core.Configuration;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Default drawer for <see cref="PlayAudioBehavior"/>. It sets displayed name to "Play Audio File".
    /// </summary>
    [DefaultProcessDrawer(typeof(PlayAudioBehavior.EntityData))]
    public class PlayAudioBehaviorDrawer : NameableDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            float height = rect.height;
            height += EditorDrawingHelper.VerticalSpacing;
            height += EditorDrawingHelper.SingleLineHeight;

            Rect nextPosition = new Rect(rect.x, rect.y + height, rect.width, rect.height);

            PlayAudioBehavior.EntityData data = currentValue as PlayAudioBehavior.EntityData;

            nextPosition = DrawerLocator.GetDrawerForValue(data.AudioData, typeof(IAudioData)).Draw(nextPosition, data.AudioData, (value) => ChangeValue(() => value, () => data.AudioData, changeValueCallback), "Audio data");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            MemberInfo volume = data.GetType().GetMember(nameof(data.Volume)).First();
            nextPosition = DrawerLocator.GetDrawerForMember(volume, data).Draw(nextPosition, data.Volume, (value) => ChangeValue(() => value, () => data.Volume, (newValue) => data.Volume = (float)newValue), "Volume");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            AudioSource audioSource = RuntimeConfigurator.Configuration.InstructionPlayer;

            if (audioSource.isPlaying)
            {
                if (GUI.Button(nextPosition, "Stop"))
                {
                    audioSource.Stop();
                }
            }
            else
            {
                if (GUI.Button(nextPosition, "Preview"))
                {
                    data.AudioData.InitializeAudioClip();

                    RuntimeConfigurator.Configuration.InstructionPlayer.PlayOneShot(data.AudioData.AudioClip, data.Volume);
                }
            }

            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawerLocator.GetDrawerForValue(data.ExecutionStages, typeof(BehaviorExecutionStages)).Draw(nextPosition, data.ExecutionStages, (value) => ChangeValue(() => value, () => data.ExecutionStages, (newValue) => data.ExecutionStages = (BehaviorExecutionStages)newValue), "Execution stages");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            rect.height = height;
            return rect;
        }

        /// <inheritdoc />
        protected override GUIContent GetTypeNameLabel(object value, Type declaredType)
        {
            PlayAudioBehavior.EntityData behavior = value as PlayAudioBehavior.EntityData;

            if (behavior == null)
            {
                return base.GetTypeNameLabel(value, declaredType);
            }

            return base.GetTypeNameLabel(behavior.AudioData, behavior.AudioData.GetType());
        }
    }
}
