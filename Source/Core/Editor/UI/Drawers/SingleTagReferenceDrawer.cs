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

            //if (oldUniqueName != newUniqueName)
            //{
            //    RevertableChangesHandler.Do(
            //        new ProcessCommand(
            //            () =>
            //            {
            //                uniqueNameReference.UniqueName = newUniqueName;
            //                changeValueCallback(uniqueNameReference);
            //            },
            //            () =>
            //            {
            //                uniqueNameReference.UniqueName = oldUniqueName;
            //                changeValueCallback(uniqueNameReference);
            //            }),
            //        isUndoOperation ? undoGroupName : string.Empty);

            //    if (isUndoOperation)
            //    {
            //        RevertableChangesHandler.CollapseUndoOperations(undoGroupName);


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
                else if (EditorUtility.DisplayDialog("No PSO", "No PSO. Create one?", "Yes", "No"))
                {
                    newUniqueName = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                    // TODO set dirty
                }
            }

            return newUniqueName;
        }

        private GameObject GetGameObjectFromInstanceID(string objectUniqueName)
        {
            GameObject gameObject = null;

            if (int.TryParse(objectUniqueName, out int instanceId))
            {
                gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            }

            return gameObject;
        }

        private string GetUniqueNameFromSceneObject(GameObject selectedSceneObject)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (sceneObject != null)
            {
                return sceneObject.UniqueName;
            }

            Debug.LogWarning($"Game Object \"{selectedSceneObject.name}\" does not have a Process Object component.");
            return string.Empty;
        }

        private string GetUniqueNameFromProcessProperty(GameObject selectedProcessPropertyObject, Type valueType, string oldUniqueName)
        {
            if (selectedProcessPropertyObject.GetComponent(valueType) is ISceneObjectProperty processProperty)
            {
                return processProperty.SceneObject.UniqueName;
            }

            Debug.LogWarning($"Scene Object \"{selectedProcessPropertyObject.name}\" with Unique Name \"{oldUniqueName}\" does not have a {valueType.Name} component.");
            return string.Empty;
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

            //if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(sceneObject.Guid) == false)
            //{
            //    // Sets a UniqueName and then registers it.
            //    sceneObject.SetSuitableName();
            //}

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
