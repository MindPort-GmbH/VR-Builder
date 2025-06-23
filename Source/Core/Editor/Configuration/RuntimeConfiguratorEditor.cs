// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Windows;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.Configuration
{
    /// <summary>
    /// Custom editor for choosing the process configuration in the Unity game object inspector.
    /// </summary>
    [CustomEditor(typeof(RuntimeConfigurator))]
    public class RuntimeConfiguratorEditor : UnityEditor.Editor
    {
        private const string configuratorSelectedProcessPropertyName = "selectedProcessStreamingAssetsPath";
        private const string processLocalizationTablePropertyName = "processStringLocalizationTable";
        private const string dummyProcessName = "<none>";
        private const string missingProcessName = "<Missing Process>";

        private RuntimeConfigurator configurator;
        private SerializedProperty configuratorSelectedProcessProperty;
        private SerializedProperty processLocalizationTableProperty;

        private static readonly List<Type> configurationTypes;
        private static readonly string[] configurationTypeNames;

        private static List<string> processDisplayNames = new List<string> { dummyProcessName };

        private string defaultProcessPath;
        private static bool isDirty = true;

        static RuntimeConfiguratorEditor()
        {
            configurationTypes = ReflectionUtils.GetConcreteImplementationsOf<BaseRuntimeConfiguration>().ToList();
            configurationTypes.Sort(((type1, type2) => string.Compare(type1.Name, type2.Name, StringComparison.Ordinal)));
            configurationTypeNames = configurationTypes.Select(t => t.Name).ToArray();

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
            configurator = target as RuntimeConfigurator;

            configuratorSelectedProcessProperty = serializedObject.FindProperty(configuratorSelectedProcessPropertyName);
            processLocalizationTableProperty = serializedObject.FindProperty(processLocalizationTablePropertyName);

            defaultProcessPath = EditorConfigurator.Instance.ProcessStreamingAssetsSubdirectory;

            // Create process path if not present.
            string absolutePath = Path.Combine(Application.streamingAssetsPath, defaultProcessPath);
            if (Directory.Exists(absolutePath) == false)
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Processes can change without recompile so we have to check for them.
            UpdateAvailableProcesses();

            DrawRuntimeConfigurationDropDown();

            EditorGUI.BeginDisabledGroup(IsProcessListEmpty());
            {
                DrawProcessSelectionDropDown();

                if (LocalizationSettings.HasSettings == true)
                {
                    DrawLocalisationSettings();
                }

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Open Process Editor"))
                    {
                        GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessNameFromPath(configurator.GetSelectedProcess()));
                        GlobalEditorHandler.StartEditingProcess();
                    }

                    if (GUILayout.Button(new GUIContent("Show Process in Explorer...")))
                    {
                        string absolutePath = $"{new FileInfo(ProcessAssetUtils.GetProcessAssetPath(ProcessAssetUtils.GetProcessNameFromPath(configurator.GetSelectedProcess())))}";
                        EditorUtility.RevealInFinder(absolutePath);
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private static void PopulateProcessList()
        {
            List<string> processes = ProcessAssetUtils.GetAllProcesses().ToList();

            // Create dummy entry if no files are present.
            if (processes.Count != 0 == false)
            {
                processDisplayNames.Clear();
                processDisplayNames.Add(dummyProcessName);
                return;
            }

            processDisplayNames = processes;
            processDisplayNames.Sort();
        }

        private void DrawRuntimeConfigurationDropDown()
        {
            int index = configurationTypes.FindIndex(t =>
                t.AssemblyQualifiedName == configurator.GetRuntimeConfigurationName());
            index = EditorGUILayout.Popup("Configuration", index, configurationTypeNames);
            configurator.SetRuntimeConfigurationName(configurationTypes[index].AssemblyQualifiedName);
        }

        private void DrawLocalisationSettings()
        {
            bool isProcessEditorOpen = EditorWindow.HasOpenInstances<ProcessGraphViewWindow>() || EditorWindow.HasOpenInstances<StepWindow>();

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
            int index = stringTables.FindIndex(table => table.TableCollectionName == processLocalizationTableProperty.stringValue) + 1;

            index = EditorGUILayout.Popup("Localization Table", index, stringTableNames.ToArray());

            index--;
            string newLocalizationTable = index < 0 ? string.Empty : stringTables[index].TableCollectionName;

            if (processLocalizationTableProperty.stringValue != newLocalizationTable)
            {
                processLocalizationTableProperty.stringValue = newLocalizationTable;
                SaveLocalizationTableInProcess(newLocalizationTable);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SaveLocalizationTableInProcess(string localizationTable)
        {
            IProcess process = ProcessAssetManager.Load(GetProcessNameFromPath(configurator.GetSelectedProcess()));
            process.ProcessMetadata.StringLocalizationTable = localizationTable;
            ProcessAssetManager.Save(process);
        }

        private static string GetProcessNameFromPath(string path)
        {
            int slashIndex = path.LastIndexOf('/');
            string fileName = path.Substring(slashIndex + 1);
            int pointIndex = fileName.LastIndexOf('.');
            fileName = fileName.Substring(0, pointIndex);

            return fileName;
        }

        private void DrawProcessSelectionDropDown()
        {
            if (processDisplayNames[0] == dummyProcessName)
            {
                UnityEngine.Debug.LogError("No processes found. Did you delete or manually rename them?");
                EditorGUILayout.Popup("Selected Process", 0, processDisplayNames.ToArray());
                return;
            }

            string processName = ProcessAssetUtils.GetProcessNameFromPath(configurator.GetSelectedProcess());
            int index = string.IsNullOrEmpty(processName) ? 0 : processDisplayNames.FindIndex(processName.Equals);

            bool hasMissingProcess = CheckForMissingProcess(ref index);

            index = EditorGUILayout.Popup("Selected Process", index, processDisplayNames.ToArray());

            // only assign the stored process if it is not missing or the user selected a different process
            if (!hasMissingProcess || (index > 0 && index < processDisplayNames.Count - 1))
            {
                UpdateSelectedProcess(index);
            }
        }

        private bool CheckForMissingProcess(ref int index)
        {
            if (index >= 0)
            {
                return false;
            }

            string processName = ProcessAssetUtils.GetProcessNameFromPath(configurator.GetSelectedProcess());
            UnityEngine.Debug.LogError($"The stored process '{ProcessAssetUtils.GetProcessAssetPath(processName)}' was not found. Did you delete or manually rename it?");
            processDisplayNames.Add(missingProcessName);
            index = processDisplayNames.Count - 1;
            return true;
        }

        private void UpdateSelectedProcess(int index)
        {
            string newProcessPath = ProcessAssetUtils.GetProcessStreamingAssetPath(processDisplayNames[index]);
            if (IsProcessListEmpty() == false && configurator.GetSelectedProcess() != newProcessPath)
            {
                SetConfiguratorSelectedProcess(newProcessPath);
                GlobalEditorHandler.SetCurrentProcess(processDisplayNames[index]);
            }
        }

        private void SetConfiguratorSelectedProcess(string newPath)
        {
            configuratorSelectedProcessProperty.stringValue = newPath;
        }

        private static void OnProcessFileStructureChanged(object sender, ProcessAssetPostprocessorEventArgs args)
        {
            isDirty = true;
        }

        private void UpdateAvailableProcesses()
        {
            if (isDirty == false)
            {
                return;
            }

            PopulateProcessList();

            if (string.IsNullOrEmpty(configurator.GetSelectedProcess()))
            {
                SetConfiguratorSelectedProcess(ProcessAssetUtils.GetProcessStreamingAssetPath(processDisplayNames[0]));
                GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessAssetPath(configurator.GetSelectedProcess()));
            }
        }
    }
}
