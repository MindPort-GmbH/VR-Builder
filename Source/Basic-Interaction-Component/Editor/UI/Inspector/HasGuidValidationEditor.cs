using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.BasicInteraction.Validation;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.BasicInteraction.Editor.UI.Inspector
{
    /// Notes: 
    /// The class has lots of code duplication with ProcessSceneReferenceDrawer but creating a common code base will not be useful.
    /// This is because this class should actually be implemented with UI Toolkit and not the old UGUI.
    /// Implementing ProcessSceneReferenceDrawer with UI Toolkit is currently not possible.
    /// MP-2512
    [CustomEditor(typeof(HasGuidValidation))]
    [CanEditMultipleObjects]
    public class HasGuidValidationEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset searchableList;

        [SerializeField]
        private VisualTreeAsset groupListItem;
        protected GUIStyle richTextLabelStyle;
        protected GUIStyle dropBoxStyle;

        private Rect lastButtonRect;

        public override void OnInspectorGUI()
        {
            InitializeRichTextLabelStyle();
            InitializeDropBoxStyle();

            List<IGuidContainer> guidContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();
            List<SceneObjectGroups.SceneObjectGroup> availableGroups = SceneObjectGroups.Instance.Groups.Where(guid => !guidContainers.All(c => c.HasGuid(guid.Guid))).ToList();
            Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup group) => AddGroup(group);

            EditorGUILayout.LabelField("<b>Allowed objects</b>", richTextLabelStyle);
            DrawDragAndDropArea(onItemSelected);
            DrawModifyGroupSelectionButton(onItemSelected, availableGroups);
            DrawSelectedGroupsAndGameObjects(guidContainers);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);
        }

        private void AddGroup(SceneObjectGroups.SceneObjectGroup group)
        {
            Guid guid = group.Guid;
            foreach (UnityEngine.Object target in targets)
            {
                IGuidContainer guidContainer = target as IGuidContainer;
                bool setDirty = false;

                if (guidContainer.Guids.Contains(guid) == false)
                {
                    guidContainer.AddGuid(guid);
                    setDirty = true;
                }

                if (setDirty)
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private void DrawDragAndDropArea(Action<SceneObjectGroups.SceneObjectGroup> selectedItemCallback)
        {
            Action<GameObject> droppedGameObject = (GameObject selectedSceneObject) => HandleDroopedGameObject(selectedItemCallback, selectedSceneObject);
            DropAreaGUI(droppedGameObject);
        }

        private void HandleDroopedGameObject(Action<SceneObjectGroups.SceneObjectGroup> selectedItemCallback, GameObject selectedSceneObject)
        {
            if (selectedSceneObject != null)
            {
                ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                if (processSceneObject == null)
                {
                    Guid newGuid = OpenMissingProcessSceneObjectDialog(selectedSceneObject);

                    if (newGuid != Guid.Empty)
                    {
                        selectedItemCallback?.Invoke(GetGroup(newGuid));
                    }
                }
                else
                {
                    var guids = GetAllGuids(processSceneObject);
                    if (guids.Count() == 1)
                    {
                        selectedItemCallback?.Invoke(GetGroup(guids.First()));
                    }
                    else
                    {
                        // If the PSO is in multiple groups we let the user decide which one he wants to take
                        OpenSearchableGroupListDropdown(selectedItemCallback, GetGroups(GetAllGuids(processSceneObject)));
                    }
                }
            }
        }

        private void DrawModifyGroupSelectionButton(Action<SceneObjectGroups.SceneObjectGroup> onItemSelected, List<SceneObjectGroups.SceneObjectGroup> availableGroups)
        {
            if (GUILayout.Button("Select groups"))
            {
                OpenSearchableGroupListDropdown(onItemSelected, availableGroups);
            }

            ///  Unity's GUILayout system doesn't allow for direct querying of element dimensions or positions before they are rendered. 
            ///  This is a workaround to get the position until we convert this component fully to UI Toolkit. 
            if (Event.current.type == EventType.Repaint)
            {
                lastButtonRect = GUILayoutUtility.GetLastRect();
            }
        }

        private void OpenSearchableGroupListDropdown(Action<SceneObjectGroups.SceneObjectGroup> selectedItemCallback, List<SceneObjectGroups.SceneObjectGroup> availableGroups = null)
        {
            SearchableGroupListPopup content = new SearchableGroupListPopup(selectedItemCallback, searchableList, groupListItem);
            content.SetAvailableGroups(availableGroups);
            content.SetWindowSize(windowWith: lastButtonRect.width);
            UnityEditor.PopupWindow.Show(lastButtonRect, content);
        }

        protected Guid OpenMissingProcessSceneObjectDialog(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;

            if (selectedSceneObject != null)
            {
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

        private List<SceneObjectGroups.SceneObjectGroup> GetGroups(IEnumerable<Guid> groupsOnObject)
        {
            List<SceneObjectGroups.SceneObjectGroup> groups = new List<SceneObjectGroups.SceneObjectGroup>();
            foreach (Guid guid in groupsOnObject)
            {
                groups.Add(GetGroup(guid));
            }
            return groups;
        }

        private SceneObjectGroups.SceneObjectGroup GetGroup(Guid guid)
        {
            SceneObjectGroups.SceneObjectGroup group;

            if (!SceneObjectGroups.Instance.TryGetGroup(guid, out group))
            {
                group = new SceneObjectGroups.SceneObjectGroup($"{SceneObjectGroups.UniqueGuidNameItalic}", guid);
            }

            return group;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Guids);
        }

        protected void DropAreaGUI(Action<GameObject> dropAction)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (GameObject dragged_object in DragAndDrop.objectReferences)
                        {
                            dropAction(dragged_object);
                        }
                    }
                    break;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Box($"Drop a game object on this component to assign it or any groups it belongs to.", dropBoxStyle);
            GUILayout.EndHorizontal();
        }

        private void DrawSelectedGroupsAndGameObjects(IEnumerable<IGuidContainer> guidContainers)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (guidContainers.Any(container => container.Guids.Count() > 0))
            {
                GUILayout.Label("Registered objects in scene:");
            }

            List<Guid> displayedGuids = new List<Guid>();

            foreach (IGuidContainer container in guidContainers)
            {
                foreach (Guid guidToDisplay in container.Guids)
                {
                    if (displayedGuids.Contains(guidToDisplay))
                    {
                        continue;
                    }

                    displayedGuids.Add(guidToDisplay);

                    IEnumerable<ISceneObject> processSceneObjectInGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    DrawLabel(guidToDisplay, guidContainers.All(container => container.HasGuid(guidToDisplay)) == false);

                    EditorGUI.BeginDisabledGroup(processSceneObjectInGroup.Count() == 0);
                    if (GUILayout.Button("Select"))
                    {
                        // Select all game objects in the group in the Hierarchy
                        Selection.objects = processSceneObjectInGroup.Select(processSceneObject => processSceneObject.GameObject).ToArray();
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Remove"))
                    {
                        container.RemoveGuid(guidToDisplay);
                        GUILayout.EndHorizontal();
                        return;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    foreach (ISceneObject sceneObject in processSceneObjectInGroup)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                        GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                        GUILayout.Label($"{sceneObject.GameObject.name}");
                        if (GUILayout.Button("Show"))
                        {
                            EditorGUIUtility.PingObject(sceneObject.GameObject);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawLabel(Guid guidToDisplay, bool italicize)
        {
            string label;

            SceneObjectGroups.SceneObjectGroup group;
            if (SceneObjectGroups.Instance.TryGetGroup(guidToDisplay, out group))
            {
                label = group.Label;
            }
            else if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guidToDisplay))
            {
                label = SceneObjectGroups.UniqueGuidNameItalic;
            }
            else
            {
                label = $"{SceneObjectGroups.GuidNotRegisteredText} - {guidToDisplay}.";
            }

            if (italicize)
            {
                label = $"<i>{label}</i>";
            }

            GUILayout.Label($"Group: {label}", richTextLabelStyle);
        }

        private void InitializeRichTextLabelStyle()
        {
            if (richTextLabelStyle == null)
            {
                richTextLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
            }
        }

        private void InitializeDropBoxStyle()
        {
            if (dropBoxStyle == null)
            {
                dropBoxStyle = new GUIStyle(GUI.skin.box);
                dropBoxStyle.normal.textColor = Color.white;
            }
        }
    }
}