#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.UI;
using VRBuilder.XRInteraction.Interactors;

namespace VRBuilder.XRInteraction.Editor.UI.Inspector
{
    [CustomEditor(typeof(SnapZone))]
    public class SnapZoneEditor : UnityEditor.Editor
    {
        private SerializedProperty shownHighlightProp = null;
        private GameObject lastShownHighlightProp = null;
        private const string ShownHighlightFieldName = "shownHighlightObject";
        private static readonly EditorIcon infoIcon = new EditorIcon("icon_info");
        private const float iconButtonSize = 24f;

        private void OnEnable()
        {
            shownHighlightProp = serializedObject.FindProperty(ShownHighlightFieldName);
            if (shownHighlightProp != null)
            {
                lastShownHighlightProp = shownHighlightProp.objectReferenceValue as GameObject;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            SnapZone snapZone = (SnapZone)target;

            DrawReadWriteWarningUI(snapZone);
            DetectHighlightChangeAndHandle(snapZone);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawReadWriteWarningUI(SnapZone snapZone)
        {
            GameObject highlight = shownHighlightProp != null ? shownHighlightProp.objectReferenceValue as GameObject : null;
            if (highlight == null)
            {
                return;
            }

            List<Mesh> nonReadableMeshes;
            Dictionary<string, ModelImporter> importers;
            List<Mesh> nonFixableMeshes;

            GetNonReadableMeshesAndImporters(highlight, out nonReadableMeshes, out importers, out nonFixableMeshes);

            if (nonReadableMeshes.Count == 0)
            {
                return;
            }

            string message = "The assigned Shown Highlight Object contains one or more meshes that are not Read/Write enabled. " +
                "Without Read/Write, the snap zone highlight won't be visible in builds.";
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            EditorGUILayout.Space();

            if (importers.Count > 0)
            {
                EditorGUILayout.LabelField("Affected model assets:", EditorStyles.boldLabel);

                List<string> paths = importers.Keys.OrderBy((string p) => { return p; }).ToList();
                foreach (string path in paths)
                {
                    DrawEditModelGUI(importers, path);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Enable Read/Write for all models"))
                    {
                        EnableReadWriteOnImporters(importers);
                        RebuildPreviewMesh(snapZone);
                        EditorUtility.SetDirty(snapZone);
                    }
                }
            }

            // Surface meshes we can't auto-fix (no AssetDatabase path / no importer)
            if (nonFixableMeshes.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Meshes that cannot be auto-fixed:", EditorStyles.boldLabel);
                foreach (Mesh mesh in nonFixableMeshes.Distinct())
                {
                    EditorGUILayout.LabelField(mesh.name);
                }

                EditorGUILayout.HelpBox(
                    "These meshes are not backed by a model asset (no importer found). " +
                    "Make sure they are imported with Read/Write enabled, or replace them with meshes that have Read/Write enabled.",
                    MessageType.Info);
            }
        }

        private static void DrawEditModelGUI(Dictionary<string, ModelImporter> importers, string path)
        {
            ModelImporter importer = importers[path];
            string fileName = System.IO.Path.GetFileName(path);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(fileName);

                GUILayout.FlexibleSpace();

                GUIContent selectModel = new GUIContent(infoIcon.Texture, "Select the model asset in the Project window.");

                if (GUILayout.Button(selectModel, GUILayout.Width(iconButtonSize), GUILayout.Height(iconButtonSize)))
                {
                    UnityEngine.Object modelAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (modelAsset != null)
                    {
                        EditorGUIUtility.PingObject(modelAsset);
                        Selection.activeObject = modelAsset;
                    }
                }
            }
        }

        private void DetectHighlightChangeAndHandle(SnapZone snapZone)
        {
            GameObject current = shownHighlightProp != null ? shownHighlightProp.objectReferenceValue as GameObject : null;

            if (current == lastShownHighlightProp)
            {
                return;
            }

            lastShownHighlightProp = current;

            RebuildPreviewMesh(snapZone);
            EditorUtility.SetDirty(snapZone);
        }

        private static void GetNonReadableMeshesAndImporters(GameObject go, out List<Mesh> nonReadableMeshes, out Dictionary<string, UnityEditor.ModelImporter> importers, out List<Mesh> nonFixableMeshes)
        {
            nonReadableMeshes = new List<Mesh>();
            importers = new Dictionary<string, UnityEditor.ModelImporter>();
            nonFixableMeshes = new List<Mesh>();

            if (go == null)
            {
                return;
            }

            IEnumerable<Mesh> allMeshes = GetAllMeshes(go);
            foreach (Mesh mesh in allMeshes)
            {
                if (mesh == null)
                {
                    continue;
                }

                if (mesh.isReadable == false)
                {
                    nonReadableMeshes.Add(mesh);

                    string path = AssetDatabase.GetAssetPath(mesh);
                    if (string.IsNullOrEmpty(path))
                    {
                        nonFixableMeshes.Add(mesh);
                        continue;
                    }

                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer != null && importers.ContainsKey(path) == false)
                    {
                        importers.Add(path, importer);
                    }
                }
            }
        }

        private static void EnableReadWriteOnImporters(Dictionary<string, ModelImporter> importers)
        {
            foreach (KeyValuePair<string, ModelImporter> kv in importers)
            {
                ModelImporter modelImporter = kv.Value;
                if (modelImporter == null)
                {
                    continue;
                }

                if (modelImporter.isReadable == false)
                {
                    modelImporter.isReadable = true;
                    try
                    {
                        modelImporter.SaveAndReimport();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to reimport '{kv.Key}': {e.Message}");
                    }
                }
            }
        }

        private void RebuildPreviewMesh(SnapZone snapZone)
        {
            if (snapZone == null)
            {
                return;
            }

            snapZone.UpdateHighlightMeshFilterCache();
        }

        private static IEnumerable<Mesh> GetAllMeshes(GameObject go)
        {
            if (go == null)
            {
                yield break;
            }

            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf != null && mf.sharedMesh != null)
                {
                    yield return mf.sharedMesh;
                }
            }

            SkinnedMeshRenderer[] skinned = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in skinned)
            {
                if (smr != null && smr.sharedMesh != null)
                {
                    yield return smr.sharedMesh;
                }
            }
        }
    }
}
#endif
