using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer a tag selector for a <see cref="UniqueNameReference"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(UniqueNameReference))]
    public class SingleObjectTagDrawer : AbstractDrawer
    {
        private const string noComponentSelected = "<none>";
        protected bool isUndoOperation;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            UniqueNameReference nameReference = (UniqueNameReference)currentValue;
            PropertyInfo valueProperty = currentValue.GetType().GetProperty("Value");
            Type valueType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(valueProperty);

            Guid oldGuid = Guid.Empty;
            Guid.TryParse(nameReference.UniqueName, out oldGuid);

            SceneObjectTags.Tag[] tags = SceneObjectTags.Instance.Tags.ToArray();
            List<string> labels = tags.Select(tag => tag.Label).ToList();
            SceneObjectTags.Tag currentTag = tags.FirstOrDefault(tag => tag.Guid == oldGuid);
            Rect guiLineRect = rect;

            CheckForObjectUniqueness(oldGuid, ref rect, ref guiLineRect);

            if (currentTag != null)
            {
                foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(currentTag.Guid))
                {
                    CheckForMisconfigurationIssues(sceneObject.GameObject, valueType, ref rect, ref guiLineRect);
                }
            }

            EditorGUI.BeginDisabledGroup(tags.Length == 0);

            int selectedTagIndex = Array.IndexOf(tags, currentTag);
            bool isTagInvalid = false;

            if (selectedTagIndex == -1)
            {
                selectedTagIndex = 0;
                labels.Insert(0, noComponentSelected);
                isTagInvalid = true;
            }

            selectedTagIndex = EditorGUI.Popup(guiLineRect, label.text, selectedTagIndex, labels.ToArray());
            guiLineRect = AddNewRectLine(ref guiLineRect);
            EditorGUI.EndDisabledGroup();

            if (isTagInvalid && selectedTagIndex == 0)
            {
                return rect;
            }
            else if (isTagInvalid)
            {
                selectedTagIndex--;
            }

            Guid newGuid = tags[selectedTagIndex].Guid;

            if (oldGuid != newGuid)
            {
                ChangeValue(
                () =>
                {
                    nameReference.UniqueName = newGuid.ToString();
                    return nameReference;
                },
                () =>
                {
                    nameReference.UniqueName = oldGuid.ToString();
                    return nameReference;
                },
                changeValueCallback);
            }

            DisplaySelectedObjects(oldGuid, ref rect, ref guiLineRect);

            return rect;
        }

        private void DisplaySelectedObjects(Guid guid, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            IEnumerable<ISceneObject> selectedObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid);

            if (selectedObjects.Count() > 0)
            {
                guiLineRect = AddNewRectLine(ref originalRect);
                GUI.Label(guiLineRect, "Registered objects in scene:");
            }

            foreach (ISceneObject sceneObject in selectedObjects)
            {
                guiLineRect = AddNewRectLine(ref originalRect);

                GUILayout.BeginArea(guiLineRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                if (GUILayout.Button("Show"))
                {
                    EditorGUIUtility.PingObject(sceneObject.GameObject);
                }

                GUILayout.Label($"{sceneObject.GameObject.name}");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

            }
        }

        private void CheckForObjectUniqueness(Guid oldGuid, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            int taggedObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(oldGuid).Count();

            if (taggedObjects > 1)
            {
                string warning = $"Please ensure only one object with this tag is present in the scene.";
                EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
                guiLineRect = AddNewRectLine(ref originalRect);
            }
            else if (taggedObjects == 0)
            {
                string error = $"No objects found in scene. This will result in a null reference.";
                EditorGUI.HelpBox(guiLineRect, error, MessageType.Error);
                guiLineRect = AddNewRectLine(ref originalRect);
            }

            return;
        }

        protected void CheckForMisconfigurationIssues(GameObject selectedSceneObject, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (selectedSceneObject != null && selectedSceneObject.GetComponent(valueType) == null)
            {
                string warning = $"{selectedSceneObject.name} is not configured as {valueType.Name}";
                const string button = "Fix it";
                EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
                guiLineRect = AddNewRectLine(ref originalRect);

                if (GUI.Button(guiLineRect, button))
                {
                    // Only relevant for Undoing a Process Property.
                    bool isAlreadySceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() != null && typeof(ISceneObjectProperty).IsAssignableFrom(valueType);
                    Component[] alreadyAttachedProperties = selectedSceneObject.GetComponents(typeof(Component));

                    RevertableChangesHandler.Do(
                        new ProcessCommand(
                            () => SceneObjectAutomaticSetup(selectedSceneObject, valueType),
                            () => UndoSceneObjectAutomaticSetup(selectedSceneObject, valueType, isAlreadySceneObject, alreadyAttachedProperties)));
                }

                guiLineRect = AddNewRectLine(ref originalRect);
            }
        }

        protected Rect AddNewRectLine(ref Rect currentRect)
        {
            Rect newRectLine = currentRect;
            newRectLine.height = EditorDrawingHelper.SingleLineHeight;
            newRectLine.y += currentRect.height + EditorDrawingHelper.VerticalSpacing;

            currentRect.height += EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;
            return newRectLine;
        }

        protected void SceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() ?? selectedSceneObject.AddComponent<ProcessSceneObject>();

            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(sceneObject.Guid) == false)
            {
                // Sets a UniqueName and then registers it.
                sceneObject.SetSuitableName();
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
    }
}
