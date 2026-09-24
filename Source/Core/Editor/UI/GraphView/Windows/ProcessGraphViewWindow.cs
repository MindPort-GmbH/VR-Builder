using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.Windows;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.GraphView.Windows
{
    /// <summary>
    /// Editor windows that displays the process using a graphview.
    /// </summary>
    public class ProcessGraphViewWindow : ProcessEditorWindow
    {
        private EditorIcon titleIcon;

        internal EditorIcon TitleIcon
        {
            get
            {
                if (titleIcon == null)
                {
                    titleIcon = new EditorIcon("icon_process_editor");
                }

                return titleIcon;
            }
        }

        private ProcessGraphView graphView;
        private Box chapterHierarchy;

        [SerializeField]
        private ProcessMenuView chapterMenu;
        [SerializeField]
        private VisualTreeAsset noProcessWarningAsset;

        private IMGUIContainer chapterViewContainer;
        private IProcess currentProcess;
        private IChapter currentChapter;

        private VisualElement noProcessWarning;
        private bool isFileChanged;
        private DateTime lastFileChange;
        private object lockObject = new object();

        private void CreateGUI()
        {
            wantsMouseMove = true;
            if (chapterMenu == null)
            {
                chapterMenu = CreateInstance<ProcessMenuView>();
            }

            chapterMenu.MenuExtendedChanged += (sender, args) => { chapterViewContainer.style.width = args.IsExtended ? ProcessMenuView.ExtendedMenuWidth : ProcessMenuView.MinimizedMenuWidth; };
            chapterMenu.RefreshRequested += (sender, args) => { chapterViewContainer.MarkDirtyLayout(); };

            chapterViewContainer = new IMGUIContainer();
            rootVisualElement.Add(chapterViewContainer);
            chapterViewContainer.StretchToParentSize();
            chapterViewContainer.style.width = ProcessMenuView.ExtendedMenuWidth;
            chapterViewContainer.style.backgroundColor = new StyleColor(new Color32(51, 51, 51, 192));

            graphView = ConstructGraphView();
            chapterHierarchy = ConstructChapterHierarchy();
            noProcessWarning = CreateMissingProcessWarning();

            ProcessAssetManager.ExternalFileChange += OnExternalFileChange;
            EditorApplication.update += PromptForExternalFileChange;

            GlobalEditorHandler.ProcessWindowOpened(this);
        }

        private void OnGUI()
        {
            SetTabName();
        }

        private void PromptForExternalFileChange()
        {
            bool shouldPrompt;
            lock (lockObject)
            {
                shouldPrompt = isFileChanged && DateTime.UtcNow - lastFileChange >= TimeSpan.FromMilliseconds(200);
                if (shouldPrompt)
                {
                    isFileChanged = false;
                }
            }

            if (!shouldPrompt)
            {
                return;
            }

            bool hasExternalChange;
            try
            {
                hasExternalChange = ProcessAssetManager.HasExternalFileChange();
            }
            catch (System.IO.IOException)
            {
                lock (lockObject)
                {
                    isFileChanged = true;
                    lastFileChange = DateTime.UtcNow;
                }
                return;
            }

            if (!hasExternalChange)
            {
                return;
            }

            string dialogTitle = "External process file change";
            string dialogMessage = "A process file was changed outside The VR Builder Process Editor. Reload the process from disk?\n Reloading discards unsaved edits. Saving the current process will overwrite the external change.";
            string reloadButton = "Reload from disk";
            string saveButton = "Save open process";

            if (EditorUtility.DisplayDialog(dialogTitle, dialogMessage, reloadButton, saveButton))
            {
                GlobalEditorHandler.SetCurrentProcess(EditorPrefs.GetString(GlobalEditorHandler.LastEditedProcessNameKey));
            }
            else if (currentProcess != null)
            {
                ProcessAssetManager.Save(currentProcess);
            }
        }

        private void OnDisable()
        {
            ProcessAssetManager.ExternalFileChange -= OnExternalFileChange;
            EditorApplication.update -= PromptForExternalFileChange;
            GlobalEditorHandler.ProcessWindowClosed(this);
        }

        private void OnExternalFileChange(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                isFileChanged = true;
                lastFileChange = DateTime.UtcNow;
            }
        }

        private void SetTabName()
        {
            titleContent = new GUIContent("Process Editor", TitleIcon.Texture);
        }

        private ProcessGraphView ConstructGraphView()
        {
            ProcessGraphView graphView = new ProcessGraphView()
            {
                name = "Process Graph"
            };

            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            graphView.SendToBack();

            return graphView;
        }

        /// <inheritdoc/>
        internal override void SetChapter(IChapter chapter)
        {
            SetupChapterHierarchy(chapter);

            currentChapter = chapter;

            if (graphView == null)
            {
                graphView = ConstructGraphView();
            }

            graphView.SetChapter(currentChapter);
        }

        /// <inheritdoc/>
        internal override void SetProcess(IProcess process)
        {
            RevertableChangesHandler.FlushStack();

            currentProcess = process;

            // If the process is null, remove the graph view and chapter view and display a warning label.
            if (currentProcess == null)
            {
                chapterViewContainer.RemoveFromHierarchy();
                ResetGraphView();
                noProcessWarning.style.display = DisplayStyle.Flex;
                return;
            }
            else if (noProcessWarning.style.display != DisplayStyle.None)
            {
                noProcessWarning.style.display = DisplayStyle.None;
                rootVisualElement.Add(chapterViewContainer);
                graphView = ConstructGraphView();
            }

            chapterMenu.Initialise(currentProcess, this);
            chapterViewContainer.onGUIHandler = () => chapterMenu.Draw();

            chapterMenu.ChapterChanged += (sender, args) =>
            {
                SetChapter(args.CurrentChapter);
            };

            SetChapter(currentProcess.Data.FirstChapter);
        }

        private VisualElement CreateMissingProcessWarning()
        {
            EditorUtils.CheckVisualTreeAssets(nameof(ProcessGraphViewWindow), new List<VisualTreeAsset>() { noProcessWarningAsset });
            noProcessWarning = noProcessWarningAsset.CloneTree();
            noProcessWarning.style.display = DisplayStyle.None;
            // Ensure it takes full space making it centered
            noProcessWarning.style.flexGrow = 1;
            rootVisualElement.Add(noProcessWarning);
            return noProcessWarning;
        }

        /// <summary>
        /// Resets the process graph window to an empty state.
        /// </summary>
        internal void ResetGraphView()
        {
            graphView?.Clear();
            graphView?.RemoveFromHierarchy();
            graphView = null;
        }

        /// <inheritdoc/>
        internal override IChapter GetChapter()
        {
            return currentChapter;
        }

        /// <inheritdoc/>
        internal override void RefreshChapterRepresentation()
        {
            if (currentProcess != null)
            {
                graphView.RefreshSelectedNode();
            }
        }

        private Box ConstructChapterHierarchy()
        {
            Box box = new Box();

            box.style.alignSelf = Align.FlexStart;
            box.style.left = ProcessMenuView.ExtendedMenuWidth;
            box.contentContainer.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(box);

            chapterMenu.MenuExtendedChanged += (sender, args) => { box.style.left = args.IsExtended ? ProcessMenuView.ExtendedMenuWidth : ProcessMenuView.MinimizedMenuWidth; };

            return box;
        }

        private void SetupChapterHierarchy(IChapter chapter)
        {
            bool isRoot = GlobalEditorHandler.GetCurrentProcess().Data.Chapters.Contains(chapter);
            if (GlobalEditorHandler.GetCurrentProcess().Data.Chapters.Contains(chapter))
            {
                chapterHierarchy.contentContainer.Clear();
            }

            chapterHierarchy.visible = !isRoot;

            List<ChapterHierarchyElement> elements = chapterHierarchy.contentContainer.Children().Select(child => child as ChapterHierarchyElement).ToList();

            int index = elements.IndexOf(elements.FirstOrDefault(container => container.Chapter == chapter));

            if (index < 0)
            {
                elements.ForEach(element => element.SetInteractable(true));

                ChapterHierarchyElement element = new ChapterHierarchyElement(chapter, elements.Count() == 0);

                chapterHierarchy.Add(element);
            }
            else
            {
                while (chapterHierarchy.contentContainer.childCount > index + 1)
                {
                    chapterHierarchy.contentContainer.RemoveAt(index + 1);
                }

                elements[index].SetInteractable(false);
            }
        }

        private class ChapterHierarchyElement : VisualElement
        {
            private IChapter chapter;
            public IChapter Chapter => chapter;

            private Label chapterLabel;
            private Button chapterButton;

            public ChapterHierarchyElement(IChapter chapter, bool isFirstElement, bool isInteractable = false)
            {
                this.chapter = chapter;

                contentContainer.style.flexDirection = FlexDirection.Row;

                if (isFirstElement == false)
                {
                    Label separator = new Label(">");
                    separator.style.alignSelf = Align.Center;
                    Add(separator);
                }

                chapterButton = new Button(() => GlobalEditorHandler.RequestNewChapter(Chapter));
                chapterButton.text = Chapter.Data.Name;

                chapterLabel = new Label(Chapter.Data.Name);
                chapterLabel.style.alignSelf = Align.Center;

                SetInteractable(isInteractable);
            }

            public void SetInteractable(bool isInteractable)
            {
                if (isInteractable)
                {
                    if (contentContainer.Children().Contains(chapterLabel))
                    {
                        Remove(chapterLabel);
                    }
                    Add(chapterButton);
                }
                else
                {
                    if (contentContainer.Children().Contains(chapterButton))
                    {
                        Remove(chapterButton);
                    }
                    Add(chapterLabel);
                }
            }
        }
    }
}
