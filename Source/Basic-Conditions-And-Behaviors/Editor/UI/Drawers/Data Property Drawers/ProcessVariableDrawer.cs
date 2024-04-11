using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Drawer for <see cref="ProcessVariable{T}"/>
    /// </summary>
    internal abstract class ProcessVariableDrawer<T> : AbstractDrawer where T : IEquatable<T>
    {
        /// <summary>
        /// Draws the field for the constant value depending on its type.
        /// </summary>
        protected abstract T DrawConstField(T value);

        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return rect;
            }

            ProcessVariable<T> processVariable = (ProcessVariable<T>)currentValue;
            ProcessSceneReferenceBase propertyReference = processVariable.Property;
            Type valueType = propertyReference.GetReferenceType();

            GameObject selectedSceneObject = processVariable.Property.Value?.SceneObject.GameObject;
            Guid oldGuid = propertyReference.Guids.FirstOrDefault();
            Rect guiLineRect = rect;

            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUI.BeginDisabledGroup(processVariable.IsConst);
            selectedSceneObject = EditorGUILayout.ObjectField("", selectedSceneObject, typeof(GameObject), true) as GameObject;
            EditorGUI.EndDisabledGroup();


            if (GUILayout.Toggle(!processVariable.IsConst, "Property reference", BuilderEditorStyles.RadioButton, GUILayout.Width(192)))
            {
                processVariable.IsConst = false;
                changeValueCallback(processVariable);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            guiLineRect = AddNewRectLine(ref rect);
            T oldConstValue = processVariable.ConstValue;
            T newConstValue = processVariable.ConstValue;

            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUI.BeginDisabledGroup(processVariable.IsConst == false);
            newConstValue = DrawConstField(newConstValue);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Toggle(processVariable.IsConst, "Constant value", BuilderEditorStyles.RadioButton, GUILayout.Width(192)))
            {
                processVariable.IsConst = true;
                changeValueCallback(processVariable);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Guid newGuid = GetIDFromSelectedObject(selectedSceneObject, valueType, oldGuid);

            if (oldGuid != newGuid)
            {
                processVariable.Property.ResetGuids(new List<Guid>() { newGuid });
                changeValueCallback(processVariable);
            }

            if (newConstValue != null && newConstValue.Equals(processVariable.ConstValue) == false)
            {
                processVariable.ConstValue = newConstValue;
                changeValueCallback(processVariable);
            }

            return rect;
        }

        private Guid GetIDFromSelectedObject(GameObject selectedSceneObject, Type valueType, Guid oldUniqueName)
        {
            Guid newGuid = Guid.Empty;

            if (selectedSceneObject != null)
            {
                if (selectedSceneObject.GetComponent(valueType) != null)
                {
                    if (typeof(ISceneObject).IsAssignableFrom(valueType))
                    {
                        newGuid = GetUniqueIdFromSceneObject(selectedSceneObject);
                    }
                    else if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
                    {
                        newGuid = GetUniqueIdFromProcessProperty(selectedSceneObject, valueType, oldUniqueName);
                    }
                }
                else
                {
                    // TODO handle non-PSO
                }
            }

            return newGuid;
        }

        private Guid GetUniqueIdFromSceneObject(GameObject selectedSceneObject)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (sceneObject != null)
            {
                return sceneObject.Guid;
            }

            Debug.LogWarning($"Game Object \"{selectedSceneObject.name}\" does not have a Process Object component.");
            return Guid.Empty;
        }

        private Guid GetUniqueIdFromProcessProperty(GameObject selectedProcessPropertyObject, Type valueType, Guid oldGuid)
        {
            if (selectedProcessPropertyObject.GetComponent(valueType) is ISceneObjectProperty processProperty)
            {
                return processProperty.SceneObject.Guid;
            }

            Debug.LogWarning($"Scene Object \"{selectedProcessPropertyObject.name}\" with Object Id \"{oldGuid}\" does not have a {valueType.Name} component.");
            return Guid.Empty;
        }

        protected Rect AddNewRectLine(ref Rect currentRect)
        {
            Rect newRectLine = currentRect;
            newRectLine.height = EditorDrawingHelper.SingleLineHeight;
            newRectLine.y += currentRect.height + EditorDrawingHelper.VerticalSpacing;

            currentRect.height += EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;
            return newRectLine;
        }
    }
}