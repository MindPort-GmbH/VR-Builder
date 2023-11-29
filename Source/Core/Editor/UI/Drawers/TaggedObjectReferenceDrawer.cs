using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Process drawer for <see cref="UniqueNameReference"/> members.
    /// </summary>
    //[DefaultProcessDrawer(typeof(UniqueNameReference))]
    public class TaggedObjectReferenceDrawer : AbstractDrawer
    {
        protected bool isUndoOperation;
        protected const string undoGroupName = "brotcat";

        protected readonly HashSet<string> missingUniqueNames = new HashSet<string>();

        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return rect;
            }

            isUndoOperation = false;
            UniqueNameReference uniqueNameReference = (UniqueNameReference)currentValue;
            PropertyInfo valueProperty = currentValue.GetType().GetProperty("Value");
            Type valueType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(valueProperty);

            if (valueProperty == null)
            {
                throw new ArgumentException("Only ObjectReference<> implementations should inherit from the UniqueNameReference type.");
            }

            Rect guiLineRect = rect;
            string oldUniqueName = uniqueNameReference.UniqueName;
            GameObject selectedSceneObject = GetGameObjectFromID(oldUniqueName);

            if (selectedSceneObject == null && string.IsNullOrEmpty(oldUniqueName) == false && missingUniqueNames.Contains(oldUniqueName) == false)
            {
                missingUniqueNames.Add(oldUniqueName);
                Debug.LogError($"The process object with the unique name '{oldUniqueName}' cannot be found!");
            }

            CheckForMisconfigurationIssues(selectedSceneObject, valueType, ref rect, ref guiLineRect);
            selectedSceneObject = EditorGUI.ObjectField(guiLineRect, label, selectedSceneObject, typeof(GameObject), true) as GameObject;

            string newUniqueName = GetIDFromSelectedObject(selectedSceneObject, valueType, oldUniqueName);

            if (oldUniqueName != newUniqueName)
            {
                RevertableChangesHandler.Do(
                    new ProcessCommand(
                        () =>
                        {
                            uniqueNameReference.UniqueName = newUniqueName;
                            changeValueCallback(uniqueNameReference);
                        },
                        () =>
                        {
                            uniqueNameReference.UniqueName = oldUniqueName;
                            changeValueCallback(uniqueNameReference);
                        }),
                    isUndoOperation ? undoGroupName : string.Empty);

                if (isUndoOperation)
                {
                    RevertableChangesHandler.CollapseUndoOperations(undoGroupName);
                }
            }

            return rect;
        }

        protected GameObject GetGameObjectFromID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            // If the Runtime Configurator exists, we try to retrieve the process object
            try
            {
                if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsName(guid) == false)
                {
                    //TODO: Referencing - When before using fixit we create a GlobalObjectId as placeholder
                    return GetGameObjectFromGlobalObjectId(guid);

                    // OLD Implementation 
                    // If the saved unique name is not registered in the scene, perhaps is actually a GameObject's InstanceID
                    //return GetGameObjectFromInstanceID(objectUniqueName);


                }

                ISceneObject sceneObject = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByName(guid);
                return sceneObject.GameObject;
            }
            catch
            {
                return null;
            }
        }

        protected string GetIDFromSelectedObject(GameObject selectedSceneObject, Type valueType, string oldUniqueName)
        {
            string newUniqueName = string.Empty;

            if (selectedSceneObject != null)
            {
                if (selectedSceneObject.GetComponent(valueType) != null)
                {
                    if (typeof(ISceneObject).IsAssignableFrom(valueType))
                    {
                        newUniqueName = GetUniqueNameFromSceneObject(selectedSceneObject);
                    }
                    else if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
                    {
                        newUniqueName = GetUniqueNameFromProcessProperty(selectedSceneObject, valueType, oldUniqueName);
                    }
                }
                else
                {
                    newUniqueName = GlobalObjectId.GetGlobalObjectIdSlow(selectedSceneObject).ToString();
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

        private GameObject GetGameObjectFromGlobalObjectId(string globalObjectId)
        {
            GameObject gameObject = null;
            GlobalObjectId convertedUnityGUnityUid;

            bool isGuuid = GlobalObjectId.TryParse(globalObjectId, out convertedUnityGUnityUid);
            if (isGuuid)
            {
                try
                {
                    gameObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(convertedUnityGUnityUid) as GameObject;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Object with {globalObjectId} is not a game object. Handling this is not yet implemented.");
                }
            }

            return gameObject;
        }

        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4. Guid string is returned as name.")]
        private string GetUniqueNameFromSceneObject(GameObject selectedSceneObject)
        {
            ProcessSceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (sceneObject != null)
            {
                if (sceneObject.Tags.Count() > 0)
                {
                    return sceneObject.Tags.First().ToString();
                }
            }

            Debug.LogWarning($"Game Object \"{selectedSceneObject.name}\" does not have a Process Object component.");
            return string.Empty;
        }

        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4. Guid string is returned as name.")]
        private string GetUniqueNameFromProcessProperty(GameObject selectedProcessPropertyObject, Type valueType, string oldUniqueName)
        {
            if (selectedProcessPropertyObject.GetComponent(valueType) is ISceneObjectProperty processProperty)
            {
                ITagContainer tagContainer = processProperty.SceneObject as ITagContainer;

                if (tagContainer != null)
                {
                    if (tagContainer.Tags.Count() > 0)
                    {
                        return tagContainer.Tags.First().ToString();
                    }
                }
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

            //TODO: Referencing - Left this here during refactoring. Probably not needed after removing the UniqueName property
            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(sceneObject.Guid) == false)
            {
                // Sets a UniqueName and then registers it.
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(sceneObject);
            }

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
                UnityEngine.Object.DestroyImmediate((ProcessSceneObject)sceneObject);
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
