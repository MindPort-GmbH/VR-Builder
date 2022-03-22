using UnityEditor;
using UnityEngine;
using VRBuilder.XRInteraction;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.Editor.XRInteraction
{
    /// <summary>
    /// Drawer class for <see cref="DirectInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(DirectInteractor)), CanEditMultipleObjects]
    internal class DirectInteractorEditor : UnityEditor.Editor
    {
        private SerializedProperty interactionManager;
        private SerializedProperty interactionLayerMask;
        private SerializedProperty attachTransform;
        private SerializedProperty startingSelectedInteractable;
        private SerializedProperty selectActionTrigger;
        private SerializedProperty hideControllerOnSelect;
        private SerializedProperty precisionGrab;

        private SerializedProperty playAudioClipOnSelectEntered;
        private SerializedProperty audioClipForOnSelectEntered;
        private SerializedProperty playAudioClipOnSelectExited;
        private SerializedProperty audioClipForOnSelectExited;
        private SerializedProperty playAudioClipOnHoverEntered;
        private SerializedProperty audioClipForOnHoverEntered;
        private SerializedProperty playAudioClipOnHoverExited;
        private SerializedProperty audioClipForOnHoverExited;

        private SerializedProperty playHapticsOnSelectEntered;
        private SerializedProperty hapticSelectEnterIntensity;
        private SerializedProperty hapticSelectEnterDuration;
        private SerializedProperty playHapticsOnHoverEntered;
        private SerializedProperty hapticHoverEnterIntensity;
        private SerializedProperty hapticHoverEnterDuration;
        private SerializedProperty playHapticsOnSelectExited;
        private SerializedProperty hapticSelectExitIntensity;
        private SerializedProperty hapticSelectExitDuration;
        private SerializedProperty playHapticsOnHoverExited;
        private SerializedProperty hapticHoverExitIntensity;
        private SerializedProperty hapticHoverExitDuration;
        
        private SerializedProperty onHoverEnter;
        private SerializedProperty onHoverExit;
        private SerializedProperty onSelectEnter;
        private SerializedProperty onSelectExit;

        private static class Tooltips
        {
            public static readonly GUIContent InteractionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent InteractionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent AttachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent StartingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            public static readonly GUIContent SelectActionTrigger = new GUIContent("Select Action Trigger", "Choose whether the select action is triggered by current state or state transitions.");
            public static readonly GUIContent HideControllerOnSelect = new GUIContent("Hide Controller On Select", "Hide controller on select.");
            public static readonly GUIContent PrecisionGrab = new GUIContent("Precision Grab", "Toggles precision grab on this interactor.");
            
            
            public static readonly GUIContent PlayAudioClipOnSelectEntered = new GUIContent("On Select Entered", "Play an audio clip when the Select state is entered.");
            public static readonly GUIContent AudioClipForOnSelectEntered = new GUIContent("AudioClip To Play", "The audio clip to play when the Select state is entered.");
            public static readonly GUIContent PlayAudioClipOnSelectExited = new GUIContent("On Select Exited", "Play an audio clip when the Select state is exited.");
            public static readonly GUIContent AudioClipForOnSelectExited = new GUIContent("AudioClip To Play", "The audio clip to play when the Select state is exited.");
            public static readonly GUIContent PlayAudioClipOnHoverEntered = new GUIContent("On Hover Entered", "Play an audio clip when the Hover state is entered.");
            public static readonly GUIContent AudioClipForOnHoverEntered = new GUIContent("AudioClip To Play", "The audio clip to play when the Hover state is entered.");
            public static readonly GUIContent PlayAudioClipOnHoverExited = new GUIContent("On Hover Exited", "Play an audio clip when the Hover state is exited.");
            public static readonly GUIContent AudioClipForOnHoverExited = new GUIContent("AudioClip To Play", "The audio clip to play when the Hover state is exited.");

            public static readonly GUIContent PlayHapticsOnSelectEntered = new GUIContent("On Select Entered", "Play haptics when the select state is entered.");
            public static readonly GUIContent HapticSelectEnterIntensity = new GUIContent("Haptic Intensity", "Haptics intensity to play when the Select state is entered.");
            public static readonly GUIContent HapticSelectEnterDuration = new GUIContent("Duration", "Haptics duration (in seconds) to play when the Select state is entered.");
            public static readonly GUIContent PlayHapticsOnHoverEntered = new GUIContent("On Hover Entered", "Play haptics when the hover state is entered.");
            public static readonly GUIContent HapticHoverEnterIntensity = new GUIContent("Haptic Intensity", "Haptics intensity to play when the Hover state is entered.");
            public static readonly GUIContent HapticHoverEnterDuration = new GUIContent("Duration", "Haptics duration (in seconds) to play when the Hover state is entered.");
            public static readonly GUIContent PlayHapticsOnSelectExited = new GUIContent("On Select Exited", "Play haptics when the select state is exited.");
            public static readonly GUIContent HapticSelectExitIntensity = new GUIContent("Haptic Intensity", "Haptics intensity to play when the Select state is exited.");
            public static readonly GUIContent HapticSelectExitDuration = new GUIContent("Duration", "Haptics duration (in seconds) to play when the Select state is exited.");
            public static readonly GUIContent PlayHapticsOnHoverExited = new GUIContent("On Hover Exited", "Play haptics when the Hover state is exited.");
            public static readonly GUIContent HapticHoverExitIntensity = new GUIContent("Haptic Intensity", "Haptics intensity to play when the Hover state is exited.");
            public static readonly GUIContent HapticHoverExitDuration = new GUIContent("Duration", "Haptics duration (in seconds) to play when the Hover state is exited.");
            
            public static readonly string StartingInteractableWarning = "A Starting Selected Interactable will be instantly deselected unless the Interactor's Toggle Select Mode is set to 'Toggle' or 'Sticky'.";
            public static readonly string MissingRequiredController = "XR Direct Interactor requires the GameObject to have an XR Controller component. Add one to ensure this component can respond to user input.";
        }

        private void OnEnable()
        {
            interactionManager = serializedObject.FindProperty("m_InteractionManager");
            interactionLayerMask = serializedObject.FindProperty("m_InteractionLayers");
            attachTransform = serializedObject.FindProperty("m_AttachTransform");
            startingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");
            selectActionTrigger = serializedObject.FindProperty("m_SelectActionTrigger");
            
            precisionGrab = serializedObject.FindProperty("precisionGrab");
            hideControllerOnSelect = serializedObject.FindProperty("m_HideControllerOnSelect");
            
            playAudioClipOnSelectEntered = serializedObject.FindProperty("m_PlayAudioClipOnSelectEntered");
            audioClipForOnSelectEntered = serializedObject.FindProperty("m_AudioClipForOnSelectEntered");
            playAudioClipOnSelectExited = serializedObject.FindProperty("m_PlayAudioClipOnSelectExited");
            audioClipForOnSelectExited = serializedObject.FindProperty("m_AudioClipForOnSelectExited");
            playAudioClipOnHoverEntered = serializedObject.FindProperty("m_PlayAudioClipOnHoverEntered");
            audioClipForOnHoverEntered = serializedObject.FindProperty("m_AudioClipForOnHoverEntered");
            playAudioClipOnHoverExited = serializedObject.FindProperty("m_PlayAudioClipOnHoverExited");
            audioClipForOnHoverExited = serializedObject.FindProperty("m_AudioClipForOnHoverExited");
            
            playHapticsOnSelectEntered = serializedObject.FindProperty("m_PlayHapticsOnSelectEntered");
            hapticSelectEnterIntensity = serializedObject.FindProperty("m_HapticSelectEnterIntensity");
            hapticSelectEnterDuration = serializedObject.FindProperty("m_HapticSelectEnterDuration");
            playHapticsOnHoverEntered = serializedObject.FindProperty("m_PlayHapticsOnHoverEntered");
            hapticHoverEnterIntensity = serializedObject.FindProperty("m_HapticHoverEnterIntensity");
            hapticHoverEnterDuration = serializedObject.FindProperty("m_HapticHoverEnterDuration");
            playHapticsOnSelectExited = serializedObject.FindProperty("m_PlayHapticsOnSelectExited");
            hapticSelectExitIntensity = serializedObject.FindProperty("m_HapticSelectExitIntensity");
            hapticSelectExitDuration = serializedObject.FindProperty("m_HapticSelectExitDuration");
            playHapticsOnHoverExited = serializedObject.FindProperty("m_PlayHapticsOnHoverExited");
            hapticHoverExitIntensity = serializedObject.FindProperty("m_HapticHoverExitIntensity");
            hapticHoverExitDuration = serializedObject.FindProperty("m_HapticHoverExitDuration");
            
            onSelectEnter = serializedObject.FindProperty("m_OnSelectEntered");
            onSelectExit = serializedObject.FindProperty("m_OnSelectExited");
            onHoverEnter = serializedObject.FindProperty("m_OnHoverEntered");
            onHoverExit = serializedObject.FindProperty("m_OnHoverExited");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour((DirectInteractor)target), typeof(DirectInteractor), false);
            EditorGUI.EndDisabledGroup();

            foreach (Object targetObject in serializedObject.targetObjects)
            {
                DirectInteractor interactor = (DirectInteractor)targetObject;
                
                if (interactor.GetComponent<XRController>() == null && interactor.GetComponent<ActionBasedController>() == null)
                {
                    EditorGUILayout.HelpBox(Tooltips.MissingRequiredController, MessageType.Warning, true);
                    break;
                }
            }

            EditorGUILayout.PropertyField(interactionManager, Tooltips.InteractionManager);
            EditorGUILayout.PropertyField(interactionLayerMask, Tooltips.InteractionLayerMask);
            EditorGUILayout.PropertyField(attachTransform, Tooltips.AttachTransform);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(selectActionTrigger, Tooltips.SelectActionTrigger);
            EditorGUILayout.PropertyField(startingSelectedInteractable, Tooltips.StartingSelectedInteractable);
            
            
            if (startingSelectedInteractable.objectReferenceValue != null && (selectActionTrigger.enumValueIndex == 2 || selectActionTrigger.enumValueIndex == 3))
            {
                EditorGUILayout.HelpBox(Tooltips.StartingInteractableWarning, MessageType.Warning, true);
            }
            
            EditorGUILayout.PropertyField(hideControllerOnSelect, Tooltips.HideControllerOnSelect);
            EditorGUILayout.PropertyField(precisionGrab, Tooltips.PrecisionGrab);

            EditorGUILayout.Space();

            playAudioClipOnSelectEntered.isExpanded = EditorGUILayout.Foldout(playAudioClipOnSelectEntered.isExpanded, EditorGUIUtility.TrTempContent("Audio Events"), true);
            
            if (playAudioClipOnSelectEntered.isExpanded)
            {
                EditorGUILayout.PropertyField(playAudioClipOnSelectEntered, Tooltips.PlayAudioClipOnSelectEntered);
                
                if (playAudioClipOnSelectEntered.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnSelectEntered, Tooltips.AudioClipForOnSelectEntered);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnSelectExited, Tooltips.PlayAudioClipOnSelectExited);
                
                if (playAudioClipOnSelectExited.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnSelectExited, Tooltips.AudioClipForOnSelectExited);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnHoverEntered, Tooltips.PlayAudioClipOnHoverEntered);
                
                if (playAudioClipOnHoverEntered.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnHoverEntered, Tooltips.AudioClipForOnHoverEntered);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playAudioClipOnHoverExited, Tooltips.PlayAudioClipOnHoverExited);
                
                if (playAudioClipOnHoverExited.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(audioClipForOnHoverExited, Tooltips.AudioClipForOnHoverExited);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            playHapticsOnSelectEntered.isExpanded = EditorGUILayout.Foldout(playHapticsOnSelectEntered.isExpanded, EditorGUIUtility.TrTempContent("Haptic Events"), true);
            
            if (playHapticsOnSelectEntered.isExpanded)
            {
                EditorGUILayout.PropertyField(playHapticsOnSelectEntered, Tooltips.PlayHapticsOnSelectEntered);
                
                if (playHapticsOnSelectEntered.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticSelectEnterIntensity, Tooltips.HapticSelectEnterIntensity);
                    EditorGUILayout.PropertyField(hapticSelectEnterDuration, Tooltips.HapticSelectEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnSelectExited, Tooltips.PlayHapticsOnSelectExited);
                
                if (playHapticsOnSelectExited.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticSelectExitIntensity, Tooltips.HapticSelectExitIntensity);
                    EditorGUILayout.PropertyField(hapticSelectExitDuration, Tooltips.HapticSelectExitDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnHoverEntered, Tooltips.PlayHapticsOnHoverEntered);
                
                if (playHapticsOnHoverEntered.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticHoverEnterIntensity, Tooltips.HapticHoverEnterIntensity);
                    EditorGUILayout.PropertyField(hapticHoverEnterDuration, Tooltips.HapticHoverEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(playHapticsOnHoverExited, Tooltips.PlayHapticsOnHoverExited);
                
                if (playHapticsOnHoverExited.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(hapticHoverExitIntensity, Tooltips.HapticHoverExitIntensity);
                    EditorGUILayout.PropertyField(hapticHoverExitDuration, Tooltips.HapticHoverExitDuration);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();
            
            onHoverEnter.isExpanded = EditorGUILayout.Foldout(onHoverEnter.isExpanded, EditorGUIUtility.TrTempContent("Interactor Events"), true);

            if (onHoverEnter.isExpanded)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(onHoverEnter);
                EditorGUILayout.PropertyField(onHoverExit);
                EditorGUILayout.PropertyField(onSelectEnter);
                EditorGUILayout.PropertyField(onSelectExit);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}