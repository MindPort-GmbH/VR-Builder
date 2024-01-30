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
using VRBuilder.Editor.UI.Windows;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for <see cref="SceneObjectTagBase"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(SceneObjectTagBase))]
    public class SceneObjectTagDrawer : AbstractDrawer
    {
        private const string noComponentSelected = "<none>";
        protected bool isUndoOperation;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            SceneObjectTagBase sceneObjectTags = (SceneObjectTagBase)currentValue;
            //TODO Fix valueType & valueProperty which currently null
            PropertyInfo valueProperty = currentValue.GetType().GetProperty("Value");
            Type valueType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(valueProperty);

            Guid oldGuid = sceneObjectTags.Guid;

            //Tags
            if (sceneObjectTags.Guids == null)
            {
                sceneObjectTags.Guids = new List<Guid>();
            }

            Rect guiLineRect = rect;

            //Drawer Limitations 
            int sceneObjectsLimit = int.MaxValue;
            CheckForLimitations(sceneObjectTags.Guids, sceneObjectsLimit, ref rect, ref guiLineRect);
            //CheckForObjectUniqueness(oldGuid, ref rect, ref guiLineRect);

            // Fixit Button
            // TODO switch to list of tags
            SceneObjectTags.Tag currentTag = SceneObjectTags.Instance.Tags.Where(tag => tag.Guid == sceneObjectTags.Guid).FirstOrDefault();
            if (currentTag != null)
            {
                foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(currentTag.Guid))
                {
                    //TODO Fix valueType which currently null
                    //CheckForMisconfigurationIssues(sceneObject.GameObject, valueType, ref rect, ref guiLineRect);
                }
            }

            //Button
            if (GUI.Button(guiLineRect, "Modify Tag Selection"))
            {
                Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                {
                    Debug.Log("Modify Tags button - Selected Tags: " + selectedTags.Aggregate("", (acc, tag) => acc + tag.Label + ", " + tag.Guid + ", "));

                    sceneObjectTags.Guids = selectedTags.Select(tag => tag.Guid).ToList();
                    SetNewTag(sceneObjectTags, oldGuid, selectedTags.First().Guid, ref rect, ref guiLineRect, changeValueCallback);

                    //TODO: Everything after SetNewTag is not called as it an exception is happening I suspect it has to do with being inside an action.
                    Debug.Log("onItemsSelected End");
                };

                var content = (SearchableTagListWindow)EditorWindow.GetWindow(typeof(SearchableTagListWindow), true, "Assign Tags");
                content.Initialize(onItemsSelected);
                content.SelectTags(GetTags(sceneObjectTags.Guids));

                //TODO: Finish size and position implementation
                //content.SetWindowSize(windowWith: rect.width);
            }
            guiLineRect = AddNewRectLine(ref rect);

            //Object Field
            GameObject selectedSceneObject = null;
            selectedSceneObject = EditorGUI.ObjectField(guiLineRect, label, selectedSceneObject, typeof(GameObject), true) as GameObject;

            if (selectedSceneObject != null)
            {
                ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                if (processSceneObject == null)
                {
                    Debug.LogWarning("The selected object has no ProcessSceneObject component.");
                    Guid newGuid = OpenMissingProcessSceneObjectDialog(selectedSceneObject);
                    //processSceneObject = selectedSceneObject.AddComponent<ProcessSceneObject>();

                    if (newGuid != Guid.Empty)
                        SetNewTag(sceneObjectTags, oldGuid, newGuid, ref rect, ref guiLineRect, changeValueCallback);
                }
                else if (processSceneObject.AllTags.Count() == 1)
                {
                    sceneObjectTags.Guids = new List<Guid>() { processSceneObject.Guid };
                    SetNewTag(sceneObjectTags, oldGuid, processSceneObject.Guid, ref rect, ref guiLineRect, changeValueCallback);
                }
                else // if the PSO has multiple tags we let the user decide which ones he wants to take
                {
                    Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                    {
                        Debug.Log("Drag and drop - Selected Tags: " + selectedTags.Aggregate("", (acc, tag) => acc + tag.Label + ", " + tag.Guid + ", "));

                        sceneObjectTags.Guids = selectedTags.Select(tag => tag.Guid).ToList();
                        SetNewTag(sceneObjectTags, oldGuid, selectedTags.First().Guid, ref rect, ref guiLineRect, changeValueCallback);

                        //TODO: Everything after SetNewTag is not called as it an exception is happening I suspect it has to do with being inside an action.
                        Debug.Log("onItemsSelected End");
                    };

                    var content = (SearchableTagListWindow)EditorWindow.GetWindow(typeof(SearchableTagListWindow), true, "Assign Tags");
                    content.Initialize(onItemsSelected);

                    List<SceneObjectTags.Tag> tags = GetTags(processSceneObject.Tags);
                    content.UpdateAvailableTags(tags);
                    content.SelectTags(tags);
                    //TODO: Finish size and position implementation
                    //content.SetWindowSize(windowWith: rect.width);
                }
            }

            //Tags List
            DisplaySelectedObjects(sceneObjectTags, oldGuid, ref rect, ref guiLineRect);

            return rect;
        }

        protected Guid OpenMissingProcessSceneObjectDialog(GameObject selectedSceneObject)
        {

            Guid guid = Guid.Empty;

            if (selectedSceneObject != null)
            {
                //TODO: Implement don't ask me again
                if (EditorUtility.DisplayDialog("No Process Scene Object component", "This object does not have a Process Scene Object component.\n" +
                    "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                    "Do you want to add one now?", "Yes", "No"))
                {
                    guid = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                    EditorUtility.SetDirty(selectedSceneObject);
                }
            }

            return guid;
        }

        private List<SceneObjectTags.Tag> GetTags(IEnumerable<Guid> tagsOnSceneObject)
        {
            List<SceneObjectTags.Tag> availableTags = SceneObjectTags.Instance.Tags.Where(tag => tagsOnSceneObject.Contains(tag.Guid)).ToList();
            return availableTags;
        }

        private void SetNewTag(SceneObjectTagBase nameReference, Guid oldGuid, Guid newGuid, ref Rect rect, ref Rect guiLineRect, Action<object> changeValueCallback)
        {
            if (oldGuid != newGuid)
            {
                ChangeValue(
                () =>
                {
                    //TODO: implement this for nameReference.TagGuids
                    //nameReference.UniqueName = newGuid.ToString();
                    return nameReference;
                },
                () =>
                {
                    //TODO: implement this for nameReference.TagGuids
                    //nameReference.UniqueName = oldGuid.ToString();
                    return nameReference;
                },
                changeValueCallback);
            }

            DisplaySelectedObjects(nameReference, newGuid, ref rect, ref guiLineRect);
        }

        private void DisplaySelectedObjects(SceneObjectTagBase nameReference, Guid guid, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (nameReference.Guids == null)
            {
                return;
            }

            if (nameReference.Guids.Count() > 0)
            {
                guiLineRect = AddNewRectLine(ref originalRect);
                GUI.Label(guiLineRect, "Registered objects in scene:");
            }

            //TODO: create foldout like in NonUniqueSceneObjectRegistryEditorWindow
            foreach (Guid guidToDisplay in nameReference.Guids)
            {
                IEnumerable<ISceneObject> sceneObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guidToDisplay);

                //TODO: tag will be null here if the tag was deleted we need to add a check and show a error message further up
                string label = SceneObjectTags.Instance.GetLabel(guidToDisplay);

                if (string.IsNullOrEmpty(label))
                {
                    label = $"Unique Tag: {guidToDisplay}";
                }

                guiLineRect = AddNewRectLine(ref originalRect);

                GUILayout.BeginArea(guiLineRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                GUILayout.Label($"Tag: {label}");
                if (GUILayout.Button("Select"))
                {
                    //TODO: Select the objects in the scene
                }
                if (GUILayout.Button("Remove"))
                {
                    nameReference.Guids.Remove(guidToDisplay);
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                foreach (ISceneObject sceneObject in sceneObjects)
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
            else
            if (taggedObjects == 0)
            {
                string error = $"No objects found in scene. This will result in a null reference.";
                EditorGUI.HelpBox(guiLineRect, error, MessageType.Error);
                guiLineRect = AddNewRectLine(ref originalRect);
            }

            return;
        }

        private void CheckForLimitations(List<Guid> currentGuidTags, int sceneObjectsLimit, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            int taggedObjects = 0;
            foreach (Guid guid in currentGuidTags)
            {
                taggedObjects += RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid).Count();
            }

            if (taggedObjects > sceneObjectsLimit)
            {
                string warning = $"This only supports {sceneObjectsLimit} scene objects at a time.";
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
