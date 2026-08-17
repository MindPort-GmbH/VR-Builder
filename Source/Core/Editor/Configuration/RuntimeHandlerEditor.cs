// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Source.Core.Runtime.Localization;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Windows;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Editor.Configuration
{
    /// <summary>
    /// Custom editor for choosing the process configuration in the Unity game object inspector.
    /// </summary>
    [CustomEditor(typeof(RuntimeHandler))]
    public class RuntimeHandlerEditor : UnityEditor.Editor
    {
        private const string dummyProcessName = "<none>";
        private const string missingProcessName = "<Missing Process>";

        private RuntimeHandler runtimeHandler;
        private RuntimeService runtimeService;
        private LanguageService languageService;
        private LanguageHandler languageHandler;

        private static List<string> processDisplayNames = new List<string> { dummyProcessName };
        private static bool isDirty = true;

        static RuntimeHandlerEditor()
        {
            ProcessAssetPostprocessor.ProcessFileStructureChanged += OnProcessFileStructureChanged;
        }

        /// <summary>
        /// True when the process list is empty or missing.
        /// </summary>
        public static bool IsProcessListEmpty()
        {
            if (isDirty)
            {
                PopulateProcessList();
            }

            return processDisplayNames.Count == 1 && processDisplayNames[0] == dummyProcessName;
        }

        protected void OnEnable()
        {
            runtimeHandler = target as RuntimeHandler;

            if (runtimeHandler == null)
            {
                return;
            }

            ServiceRegistryLoader.Instance.Register();

            if (ServiceRegistry.Has<RuntimeService>())
            {
                runtimeService = ServiceRegistry.Get<RuntimeService>();
                languageService = ServiceRegistry.Get<LanguageService>();

                // Subscribe before wiring the configurator so an existing selection triggers the editor handler.
                runtimeService.SelectedProcessChanged += OnSelectedProcessChanged;
                languageService.SelectedLocalizationTableChanged += OnSelectedProcessChanged;

                if (runtimeService.Handler == null)
                {
                    runtimeService.Handler = runtimeHandler;
                }
                else if (ReferenceEquals(runtimeService.Handler, runtimeHandler) == false)
                {
                    UnityEngine.Debug.LogWarning($"Multiple RuntimeConfigurators found. Using '{runtimeService.Handler}' as the active process configuration.");
                }
            }

            // Create process path if not present.
            string absolutePath = Path.Combine(Application.streamingAssetsPath, EditorConfigurator.Instance.ProcessStreamingAssetsSubdirectory);
            if (Directory.Exists(absolutePath) == false)
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        private void OnDisable()
        {
            if (runtimeService != null)
            {
                runtimeService.SelectedProcessChanged -= OnSelectedProcessChanged;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (runtimeService == null || ServiceRegistry.Has<RuntimeService>() == false)
            {
                EditorGUILayout.HelpBox("The RuntimeService is not registered. Run the VR Builder scene setup first.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // Processes can change without recompile so we have to check for them.
            UpdateAvailableProcesses();

            EditorGUI.BeginDisabledGroup(IsProcessListEmpty());
            {
                DrawProcessSelectionDropDown();

                if (LocalizationSettings.HasSettings)
                {
                    DrawLocalisationSettings();
                }

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Open Process Editor"))
                    {
                        OpenProcessEditor();
                    }

                    if (GUILayout.Button(new GUIContent("Show Process in Explorer...")))
                    {
                        ShowProcessInExplorer();
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private string GetSelectedProcessPath()
        {
            return runtimeService.SelectedProcess;
        }

        private void SetSelectedProcess(string processName)
        {
            runtimeService.SelectedProcess = ProcessAssetUtils.GetProcessStreamingAssetPath(processName);

            // Persist the selection on the scene's configurator.
            EditorUtility.SetDirty(runtimeHandler);
            if (runtimeHandler.RuntimeConfiguration is UnityEngine.Object runtimeConfigurationObject)
            {
                EditorUtility.SetDirty(runtimeConfigurationObject);
            }

            if (runtimeHandler.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(runtimeHandler.gameObject.scene);
            }
        }

        private void OpenProcessEditor()
        {
            GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessNameFromPath(GetSelectedProcessPath()));
            GlobalEditorHandler.StartEditingProcess();
        }

        private void ShowProcessInExplorer()
        {
            string processName = ProcessAssetUtils.GetProcessNameFromPath(GetSelectedProcessPath());
            string assetPath = ProcessAssetUtils.GetProcessAssetPath(processName);

            if (File.Exists(assetPath))
            {
                EditorUtility.RevealInFinder(assetPath);
            }
            else
            {
                UnityEngine.Debug.LogError($"The process asset '{assetPath}' was not found. Did you delete or manually rename it?");
            }
        }

        private void OnSelectedProcessChanged(string selectedProcessPath)
        {
            if (string.IsNullOrEmpty(selectedProcessPath))
            {
                return;
            }

            GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessNameFromPath(selectedProcessPath));
        }

        private static void PopulateProcessList()
        {
            List<string> processes = ProcessAssetUtils.GetAllProcesses().ToList();

            // Create dummy entry if no files are present.
            if (processes.Count == 0)
            {
                processDisplayNames.Clear();
                processDisplayNames.Add(dummyProcessName);
                return;
            }

            processDisplayNames = processes;
            processDisplayNames.Sort();
        }

        private void DrawLocalisationSettings()
        {
            bool isProcessEditorOpen = EditorWindow.HasOpenInstances<ProcessGraphViewWindow>() || WindowUtils.IsAnyStepWindowOpen();

            EditorGUI.BeginDisabledGroup(isProcessEditorOpen);
            DrawLocalizationTableDropDown();
            EditorGUI.EndDisabledGroup();

            if (isProcessEditorOpen)
            {
                EditorGUILayout.HelpBox("The Process Editor and Step Inspector windows need to be closed in order to change the localization table.", MessageType.Info);
                if (GUILayout.Button("Close Process Editor and Step Inspector"))
                {
                    WindowUtils.CloseProcessEditorWindow();
                    WindowUtils.CloseStepWindow();
                }
            }
        }

        private void DrawLocalizationTableDropDown()
        {
            EditorGUILayout.BeginHorizontal();

            List<StringTableCollection> stringTables = LocalizationEditorSettings.GetStringTableCollections().ToList();
            List<string> stringTableNames = new List<string> { dummyProcessName };
            stringTableNames.AddRange(LocalizationEditorSettings.GetStringTableCollections().Select(table => $"{table.Group}/{table.TableCollectionName}"));
            int index = stringTables.FindIndex(table => table.TableCollectionName == GetCurrentProcessLocalizationTable()) + 1;

            index = EditorGUILayout.Popup("Localization Table", index, stringTableNames.ToArray());

            index--;
            string newLocalizationTable = index < 0 ? string.Empty : stringTables[index].TableCollectionName;

            if (GetCurrentProcessLocalizationTable() != newLocalizationTable)
            {
                SaveLocalizationTableInProcess(newLocalizationTable);
            }

            EditorGUILayout.EndHorizontal();
        }

        private IProcess LoadCurrentProcess()
        {
            string processName = ProcessAssetUtils.GetProcessNameFromPath(GetSelectedProcessPath());

            if (string.IsNullOrEmpty(processName))
            {
                return null;
            }

            IProcess currentProcess = GlobalEditorHandler.GetCurrentProcess();
            if (currentProcess != null && currentProcess.Data.Name == processName)
            {
                return currentProcess;
            }

            return ProcessAssetManager.Load(processName);
        }

        private string GetCurrentProcessLocalizationTable()
        {
            IProcess process = LoadCurrentProcess();
            return process?.ProcessMetadata?.StringLocalizationTable ?? string.Empty;
        }

        private void SaveLocalizationTableInProcess(string localizationTable)
        {
            IProcess process = LoadCurrentProcess();

            if (process == null)
            {
                UnityEngine.Debug.LogWarning("No process loaded. The localization table was not saved.");
                return;
            }

            process.ProcessMetadata.StringLocalizationTable = localizationTable;
            ProcessAssetManager.Save(process);
        }

        private void DrawProcessSelectionDropDown()
        {
            if (processDisplayNames[0] == dummyProcessName)
            {
                UnityEngine.Debug.LogError("No processes found. Did you delete or manually rename them?");
                EditorGUILayout.Popup("Selected Process", 0, processDisplayNames.ToArray());
                return;
            }

            string processName = ProcessAssetUtils.GetProcessNameFromPath(GetSelectedProcessPath());
            int index = string.IsNullOrEmpty(processName) ? 0 : processDisplayNames.FindIndex(name => name == processName);

            bool hasMissingProcess = CheckForMissingProcess(ref index);

            index = EditorGUILayout.Popup("Selected Process", index, processDisplayNames.ToArray());

            // Only assign the stored process if it is not missing or the user selected a different process.
            if (hasMissingProcess == false || (index > 0 && index < processDisplayNames.Count - 1))
            {
                string newProcessName = processDisplayNames[index];

                if (newProcessName != missingProcessName && newProcessName != processName)
                {
                    SetSelectedProcess(newProcessName);
                }
            }
        }

        private bool CheckForMissingProcess(ref int index)
        {
            if (index >= 0)
            {
                return false;
            }

            string processPath = GetSelectedProcessPath();
            UnityEngine.Debug.LogError($"The stored process '{processPath}' was not found. Did you delete or manually rename it?");

            if (processDisplayNames.Contains(missingProcessName) == false)
            {
                processDisplayNames.Add(missingProcessName);
            }

            index = processDisplayNames.Count - 1;
            return true;
        }

        private void UpdateAvailableProcesses()
        {
            if (isDirty == false)
            {
                return;
            }

            isDirty = false;
            PopulateProcessList();

            // Auto-select the first available process if no process is stored on the configurator.
            if (string.IsNullOrEmpty(GetSelectedProcessPath()) && processDisplayNames[0] != dummyProcessName)
            {
                SetSelectedProcess(processDisplayNames[0]);
            }
        }

        private static void OnProcessFileStructureChanged(object sender, ProcessAssetPostprocessorEventArgs args)
        {
            isDirty = true;
        }
    }
}
