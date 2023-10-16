// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.UI.Graphics;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.Configuration
{
    /// <summary>
    /// Custom editor for choosing the process configuration in the Unity game object inspector.
    /// </summary>
    [CustomEditor(typeof(RuntimeConfigurator))]
    public class RuntimeConfiguratorEditor : UnityEditor.Editor
    {
        private const string configuratorSelectedProcessPropertyName = "selectedProcessStreamingAssetsPath";
        private const string processLocalizationTablePropertyName = "processStringLocalizationTable";

        private RuntimeConfigurator configurator;
        private SerializedProperty configuratorSelectedProcessProperty;
        private SerializedProperty processLocalizationTableProperty;

        private static readonly List<Type> configurationTypes;
        private static readonly string[] configurationTypeNames;

        private static List<string> processDisplayNames = new List<string> { "<none>" };

        private string defaultProcessPath;
        private static bool isDirty = true;

        static RuntimeConfiguratorEditor()
        {
#pragma warning disable 0618
            configurationTypes = ReflectionUtils.GetConcreteImplementationsOf<IRuntimeConfiguration>().Except(new[] { typeof(RuntimeConfigWrapper) }).ToList();
#pragma warning restore 0618
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

            return processDisplayNames.Count == 1 && processDisplayNames[0] == "<none>";
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
            if (processes.Any() == false)
            {
                processDisplayNames.Clear();
                processDisplayNames.Add("<none>");
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
            List<string> stringTableNames = new List<string> { "<None>" };
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
            int index = 0;

            string processName = ProcessAssetUtils.GetProcessNameFromPath(configurator.GetSelectedProcess());

            if (string.IsNullOrEmpty(processName) == false)
            {
                index = processDisplayNames.FindIndex(processName.Equals);
            }

            index = EditorGUILayout.Popup("Selected Process", index, processDisplayNames.ToArray());

            if (index < 0)
            {
                index = 0;
            }

            string newProcessStreamingAssetsPath = ProcessAssetUtils.GetProcessStreamingAssetPath(processDisplayNames[index]);

            if (IsProcessListEmpty() == false && configurator.GetSelectedProcess() != newProcessStreamingAssetsPath)
            {
                SetConfiguratorSelectedProcess(newProcessStreamingAssetsPath);
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
