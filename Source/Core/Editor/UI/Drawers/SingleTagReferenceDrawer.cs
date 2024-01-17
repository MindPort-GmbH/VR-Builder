using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.UndoRedo;
using Object = UnityEngine.Object;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SingleTagReferenceDrawer : AbstractDrawer
    {
        protected bool isUndoOperation;
        protected const string undoGroupName = "brotcat";

        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return rect;
            }

            isUndoOperation = false;
            SceneObjectTagBase identifier = (SceneObjectTagBase)currentValue;
            Type valueType = identifier.GetReferenceType();

            Rect guiLineRect = rect;
            Guid oldUniqueName = identifier.Guid;
            GameObject selectedSceneObject = GetGameObjectFromID(oldUniqueName);

            CheckForMisconfigurationIssues(selectedSceneObject, valueType, ref rect, ref guiLineRect);

            selectedSceneObject = EditorGUI.ObjectField(guiLineRect, label, selectedSceneObject, typeof(GameObject), true) as GameObject;

            Guid newUniqueName = GetIDFromSelectedObject(selectedSceneObject, valueType, oldUniqueName);

            identifier.Guid = newUniqueName;
            changeValueCallback(identifier);

            return rect;
        }

        protected GameObject GetGameObjectFromID(Guid guid)
        {
            if (guid == null || guid == Guid.Empty)
            {
                return null;
            }

            // If the Runtime Configurator exists, we try to retrieve the process object
            try
            {
                if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guid) == false)
                {
                    Debug.LogError("Guid not found in registry");
                    // If the saved unique name is not registered in the scene, perhaps is actually a GameObject's InstanceID
                    //return GetGameObjectFromInstanceID(objectUniqueName);
                }

                ISceneObject sceneObject = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByGuid(guid);
                return sceneObject.GameObject;
            }
            catch
            {
                return null;
            }
        }

        protected Guid GetIDFromSelectedObject(GameObject selectedSceneObject, Type valueType, Guid oldUniqueName)
        {
            Guid newUniqueName = Guid.Empty;

            if (selectedSceneObject != null)
            {
                if (selectedSceneObject.GetComponent<ProcessSceneObject>() != null)
                {
                    newUniqueName = selectedSceneObject.GetComponent<ProcessSceneObject>().Guid;
                }
                else if (EditorUtility.DisplayDialog("No Process Scene Object component", "This object does not have a Process Scene Object component.\n" +
                    "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                    "Do you want to add one now?", "Yes", "No"))
                {
                    newUniqueName = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                    EditorUtility.SetDirty(selectedSceneObject);
                }
            }

            return newUniqueName;
        }

        protected void CheckForMisconfigurationIssues(GameObject selectedSceneObject, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (selectedSceneObject != null && selectedSceneObject.GetComponent(valueType) == null)
            {
                string warning = $"{selectedSceneObject.name} is not configured as {valueType.Name}";
                const string button = "Fix it";

                EditorGUI.HelpBox(guiLineRect, warning, MessageType.Error);
                guiLineRect = AddNewRectLine(ref originalRect);

                if (GUI.Button(guiLineRect, button))
                {
                    // Only relevant for Undoing a Process Property.
                    bool isAlreadySceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() != null && typeof(ISceneObjectProperty).IsAssignableFrom(valueType);
                    Component[] alreadyAttachedProperties = selectedSceneObject.GetComponents(typeof(Component));

                    RevertableChangesHandler.Do(
                        new ProcessCommand(
                            () => SceneObjectAutomaticSetup(selectedSceneObject, valueType),
                            () => UndoSceneObjectAutomaticSetup(selectedSceneObject, valueType, isAlreadySceneObject, alreadyAttachedProperties)),
                        undoGroupName);
                }

                guiLineRect = AddNewRectLine(ref originalRect);
            }
        }

        protected void SceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() ?? selectedSceneObject.AddComponent<ProcessSceneObject>();

            if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
            {
                sceneObject.AddProcessProperty(valueType);
            }

            isUndoOperation = true;
        }

        private void UndoSceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType, bool hadProcessComponent, Component[] alreadyAttachedProperties)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
            {
                sceneObject.RemoveProcessProperty(valueType, true, alreadyAttachedProperties);
            }

            if (hadProcessComponent == false)
            {
                Object.DestroyImmediate((ProcessSceneObject)sceneObject);
            }

            isUndoOperation = true;
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
