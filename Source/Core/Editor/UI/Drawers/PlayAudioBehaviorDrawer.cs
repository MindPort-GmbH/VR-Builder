using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Default drawer for <see cref="PlayAudioBehavior"/>. It sets the displayed name to "Play Audio File".
    /// </summary>
    [DefaultProcessDrawer(typeof(PlayAudioBehavior.EntityData))]
    public class PlayAudioBehaviorDrawer : NameableDrawer
    {
        private bool previewAudio;
        private bool hasBeenPlayed;
        private float audioStartTime;
        
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

            if (label != null && label != GUIContent.none && (label.image || label.text != null))
            {
                height += DrawLabel(nextPosition, currentValue, changeValueCallback, label);
            }

            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            if (currentValue is PlayAudioBehavior.EntityData data)
            {
                MemberInfo text = data.AudioData.GetType().GetMember(nameof(data.AudioData.ClipData)).First();
                nextPosition = DrawerLocator.GetDrawerForMember(text, data.AudioData)
                    .Draw(nextPosition, data.AudioData.ClipData, (value) => ChangeValue(() => value, () => data.AudioData.ClipData, (newValue) => data.AudioData.ClipData = (string) newValue), "Text/Key");
                height += nextPosition.height;
                height += EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = rect.y + height;

                MemberInfo volume = data.GetType().GetMember(nameof(data.Volume)).First();
                nextPosition = DrawerLocator.GetDrawerForMember(volume, data)
                    .Draw(nextPosition, data.Volume, (value) => ChangeValue(() => value, () => data.Volume, (newValue) => data.Volume = (float) newValue), "Volume");
                height += nextPosition.height;
                height += EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = rect.y + height;
                
                if (TextToSpeechSettings.Instance.GetCurrentTextToSpeechProvider() is ITextToSpeechSpeaker && data.AudioData is TextToSpeechAudio textToSpeechAudio)
                {
                    MemberInfo speaker = textToSpeechAudio.GetType().GetMember(nameof(textToSpeechAudio.Speaker)).FirstOrDefault();
                    nextPosition = DrawerLocator.GetDrawerForMember(speaker, data)
                        .Draw(nextPosition, textToSpeechAudio.Speaker, (value) => ChangeValue(() => value, () => textToSpeechAudio.Speaker, (newValue) => textToSpeechAudio.Speaker = (string) newValue), "Selected Speaker");
                    height += nextPosition.height;
                    height += EditorDrawingHelper.VerticalSpacing;
                    nextPosition.y = rect.y + height;
                }
                
                AudioSource audioSource = null;

                try
                {
                    audioSource = RuntimeConfigurator.Configuration.InstructionPlayer;
                }
                catch
                {
                    // ignored
                }

                EditorGUI.BeginDisabledGroup(!audioSource);
                if (audioSource)
                {
                    if (previewAudio && !hasBeenPlayed && data.AudioData.IsReady)
                    {
                        audioStartTime = Time.time;
                        audioSource.clip = data.AudioData.AudioClip;
                        audioSource.Play();
                        hasBeenPlayed = true;
                    }
                    // Show different UI based on audio state
                    if (audioSource.isPlaying)
                    {
                        // Audio is currently playing - show stop button
                        if (GUI.Button(nextPosition, "Stop") || Time.time > audioStartTime + data.AudioData.AudioClip.length)
                        {
                            audioSource.Stop();
                            audioSource.clip = null;
                            previewAudio = false;
                            hasBeenPlayed = false;
                        }
                    }
                    else if (data.AudioData.IsLoading)
                    {
                        // Audio is still loading - show loading indicator
                        GUI.Label(nextPosition, "Loading audio...");
                    }
                    else
                    {
                        // Audio is ready to play or needs initialization
                        if (!audioSource.isPlaying || !previewAudio)
                        {
                            // Initial state or after stopping - show preview button
                            if (GUI.Button(nextPosition, "Preview"))
                            {
                                previewAudio = true;
                                hasBeenPlayed = false;
                                // Start async load
                                data.AudioData.InitializeAudioClip();
                            }
                        }
                    }

                }
                EditorGUI.EndDisabledGroup();

                if (!audioSource)
                {
                    EditorGUI.HelpBox(nextPosition, "Audio preview not available.", MessageType.Info);
                }

                height += nextPosition.height;
                height += EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = rect.y + height;

                nextPosition = DrawerLocator.GetDrawerForValue(data.ExecutionStages, typeof(BehaviorExecutionStages)).Draw(nextPosition, data.ExecutionStages,
                    (value) => ChangeValue(() => value, () => data.ExecutionStages, (newValue) => data.ExecutionStages = (BehaviorExecutionStages) newValue), "Execution stages");
            }
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            rect.height = height;
            return rect;
        }

        /// <inheritdoc />
        protected override GUIContent GetTypeNameLabel(object value, Type declaredType)
        {
            return value is not PlayAudioBehavior.EntityData behavior ? base.GetTypeNameLabel(value, declaredType) : base.GetTypeNameLabel(behavior.AudioData, behavior.AudioData.GetType());
        }
    }
}
