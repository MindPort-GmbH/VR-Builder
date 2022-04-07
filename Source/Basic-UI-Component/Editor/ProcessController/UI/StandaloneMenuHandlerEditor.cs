using System;
using System.Collections.Generic;
using VRBuilder.UX;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UX
{
    /// <summary>
    /// Editor drawer for <see cref="StandaloneMenuHandler"/>.
    /// </summary>
    [CustomEditor(typeof(StandaloneMenuHandler))]
    internal class StandaloneMenuHandlerEditor : UnityEditor.Editor
    {
        private enum Button
        {
            None = 0,
            MenuButton,
            Trigger,
            Grip,
            TriggerPressed,
            GripPressed,
            PrimaryButton,
            PrimaryTouch,
            SecondaryButton,
            SecondaryTouch,
            Primary2DAxisTouch,
            Primary2DAxisClick,
            Secondary2DAxisTouch,
            Secondary2DAxisClick,
            PrimaryAxis2DUp,
            PrimaryAxis2DDown,
            PrimaryAxis2DLeft,
            PrimaryAxis2DRight,
            SecondaryAxis2DUp,
            SecondaryAxis2DDown,
            SecondaryAxis2DLeft,
            SecondaryAxis2DRight
        }

        private static readonly Dictionary<Button, Type> ButtonMapping = new Dictionary<Button, Type>
        {
            {Button.None, null},
            {Button.MenuButton, typeof(bool)},
            {Button.Trigger, typeof(float)},
            {Button.Grip, typeof(float)},
            {Button.TriggerPressed, typeof(bool)},
            {Button.GripPressed, typeof(bool)},
            {Button.PrimaryButton, typeof(bool)},
            {Button.PrimaryTouch, typeof(bool)},
            {Button.SecondaryButton, typeof(bool)},
            {Button.SecondaryTouch, typeof(bool)},
            {Button.Primary2DAxisTouch, typeof(bool)},
            {Button.Primary2DAxisClick, typeof(bool)},
            {Button.Secondary2DAxisTouch, typeof(bool)},
            {Button.Secondary2DAxisClick, typeof(bool)},
            {Button.PrimaryAxis2DUp, typeof(Vector2)},
            {Button.PrimaryAxis2DDown, typeof(Vector2)},
            {Button.PrimaryAxis2DLeft, typeof(Vector2)},
            {Button.PrimaryAxis2DRight, typeof(Vector2)},
            {Button.SecondaryAxis2DUp, typeof(Vector2)},
            {Button.SecondaryAxis2DDown, typeof(Vector2)},
            {Button.SecondaryAxis2DLeft, typeof(Vector2)},
            {Button.SecondaryAxis2DRight, typeof(Vector2)}
        };

        private readonly GUIContent actionButtonInformation = new GUIContent("Action Button", "The button that triggers the Standalone Process Controller");

        private SerializedProperty buttonTypeNameProperty;
        private SerializedProperty buttonNameProperty;
        private Button button = Button.MenuButton;
        private int buttonIndex;

        private void OnEnable()
        {
            buttonTypeNameProperty = serializedObject.FindProperty("buttonTypeName");
            buttonNameProperty = serializedObject.FindProperty("buttonName");
            
            if (Enum.TryParse(buttonNameProperty.stringValue, out Button value))
            {
                button = value;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            button = (Button) EditorGUILayout.EnumPopup(actionButtonInformation, button);

            if (EditorGUI.EndChangeCheck())
            {
                if (ButtonMapping.TryGetValue(button, out Type type))
                {
                    buttonTypeNameProperty.stringValue = type != null ? type.ToString() : string.Empty;
                    buttonNameProperty.stringValue = button.ToString();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
