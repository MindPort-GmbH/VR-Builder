using System;
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
            ProcessVariableSelectableValue<T> selectableValue = new ProcessVariableSelectableValue<T>(processVariable.ConstValue, processVariable.Property, processVariable.IsConst);

            rect.height += DrawerLocator.GetDrawerForValue(selectableValue, selectableValue.GetType()).Draw(rect, selectableValue, (value) => ChangeValue(

                () =>
                {
                    ProcessVariableSelectableValue<T> processVariableSelectableValue = value as ProcessVariableSelectableValue<T>;
                    ProcessVariable<T> variable = new ProcessVariable<T>();

                    variable.ConstValue = processVariableSelectableValue.FirstValue;
                    variable.Property = processVariableSelectableValue.SecondValue;
                    variable.IsConst = processVariableSelectableValue.IsFirstValueSelected;
                    return variable;
                },
                () => processVariable,
                (newValue) =>
                {
                    processVariable = (ProcessVariable<T>)newValue;
                    changeValueCallback(newValue);
                }
                ), label).height;

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

            Debug.LogWarning($"Scene Object \"{selectedProcessPropertyObject.name}\" with Object ID \"{oldGuid}\" does not have a {valueType.Name} component.");
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