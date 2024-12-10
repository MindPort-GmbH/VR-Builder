using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Default drawer for <see cref="PlayAudioBehavior"/>. It sets displayed name to "Play Audio File".
    /// </summary>
    [DefaultProcessDrawer(typeof(PlayAudioBehavior.EntityData))]
    public class PlayAudioBehaviorDrawer : NameableDrawer
    {
        private bool hasBeenPlayed = false;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            Rect nextPosition = new Rect(rect.x, rect.y, rect.width, EditorDrawingHelper.HeaderLineHeight);
            float height = 0;

            if (currentValue == null)
            {
                EditorGUI.LabelField(rect, label);
                height += nextPosition.height;
                rect.height += height;
                return rect;
            }

            if (label != null && label != GUIContent.none && (label.image != null || label.text != null))
            {
                height += DrawLabel(nextPosition, currentValue, changeValueCallback, label);
            }

            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            PlayAudioBehavior.EntityData data = currentValue as PlayAudioBehavior.EntityData;

            nextPosition = DrawerLocator.GetDrawerForValue(data.AudioData, typeof(IAudioData)).Draw(nextPosition, data.AudioData, (value) => ChangeValue(() => value, () => data.AudioData, changeValueCallback), GUIContent.none);
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            MemberInfo volume = data.GetType().GetMember(nameof(data.Volume)).First();
            nextPosition = DrawerLocator.GetDrawerForMember(volume, data).Draw(nextPosition, data.Volume, (value) => ChangeValue(() => value, () => data.Volume, (newValue) => data.Volume = (float)newValue), "Volume");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            AudioSource audioSource = null;

            try
            {
                audioSource = RuntimeConfigurator.Configuration.InstructionPlayer;
            }
            catch
            {
            }

            EditorGUI.BeginDisabledGroup(audioSource == null);
            if (audioSource != null)
            {
                if (data.AudioData.AudioClip != null && !hasBeenPlayed)
                {
                    RuntimeConfigurator.Configuration.InstructionPlayer.PlayOneShot(data.AudioData.AudioClip, data.Volume);
                    hasBeenPlayed = true;

                }
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
                        hasBeenPlayed = false;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (audioSource == null)
            {
                EditorGUI.HelpBox(nextPosition, "Audio preview not available.", MessageType.Info);
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
