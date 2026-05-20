using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.BehaviorDrawers
{
    /// <summary>
    /// Auto-draws <see cref="PlayAudioBehavior.EntityData"/> like any other behavior data, then
    /// appends a Preview / Stop button that plays the configured clip in-editor via
    /// <c>RuntimeConfigurator.Configuration.InstructionPlayer</c>.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(PlayAudioBehavior.EntityData))]
    internal class PlayAudioBehaviorElementDrawer : ObjectElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = base.CreateElement(value, changeCallback, label);

            if (value is not PlayAudioBehavior.EntityData data)
            {
                return root;
            }

            root.Add(BuildPreviewControls(data));
            return root;
        }

        private static VisualElement BuildPreviewControls(PlayAudioBehavior.EntityData data)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-play-audio__preview");
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 6;

            Button previewButton = new Button { text = "Preview", tooltip = "Play the configured audio clip in the editor" };
            previewButton.AddToClassList("vrb-play-audio__preview-button");
            previewButton.style.flexGrow = 1f;
            row.Add(previewButton);

            previewButton.clicked += () =>
            {
                AudioSource player = TryGetInstructionPlayer();
                if (player == null)
                {
                    UnityEngine.Debug.LogWarning("Audio preview is unavailable — no InstructionPlayer is configured on the Process Controller.");
                    return;
                }

                if (player.isPlaying)
                {
                    player.Stop();
                    player.clip = null;
                    previewButton.text = "Preview";
                    return;
                }

                if (data.AudioData == null || data.AudioData.IsReady == false)
                {
                    data.AudioData?.InitializeAudioClip();
                    EditorApplication.delayCall += () => PlayWhenReady(data, player, previewButton);
                    previewButton.text = "Loading…";
                    return;
                }

                StartPlayback(data, player, previewButton);
            };

            return row;
        }

        private static void PlayWhenReady(PlayAudioBehavior.EntityData data, AudioSource player, Button button)
        {
            if (data?.AudioData == null || player == null)
            {
                return;
            }

            if (data.AudioData.IsLoading)
            {
                EditorApplication.delayCall += () => PlayWhenReady(data, player, button);
                return;
            }

            if (data.AudioData.IsReady)
            {
                StartPlayback(data, player, button);
            }
            else
            {
                button.text = "Preview";
            }
        }

        private static void StartPlayback(PlayAudioBehavior.EntityData data, AudioSource player, Button button)
        {
            player.clip = data.AudioData.AudioClip;
            player.Play();
            button.text = "Stop";

            // Reset the button text once the clip finishes naturally.
            float clipLength = data.AudioData.AudioClip != null ? data.AudioData.AudioClip.length : 0f;
            double resetAt = EditorApplication.timeSinceStartup + clipLength + 0.05f;
            void RestoreLabel()
            {
                if (player == null) return;

                if (EditorApplication.timeSinceStartup < resetAt && player.isPlaying)
                {
                    EditorApplication.delayCall += RestoreLabel;
                    return;
                }

                if (player.isPlaying == false)
                {
                    button.text = "Preview";
                }
            }
            EditorApplication.delayCall += RestoreLabel;
        }

        private static AudioSource TryGetInstructionPlayer()
        {
            if (RuntimeConfigurator.Exists == false) return null;

            try
            {
                return RuntimeConfigurator.Configuration.InstructionPlayer;
            }
            catch
            {
                return null;
            }
        }
    }
}
