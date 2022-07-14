using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.XRInteraction
{
    /// <summary>
    /// Checks if the highlight objects of the snapzones in the scene have non-readable meshes that cannot be displayed in builds.
    /// </summary>
    [InitializeOnLoad]
    public static class SnapZoneHighlightChecker
    {
        static SnapZoneHighlightChecker()
        {
            foreach (SnapZone snapZone in Object.FindObjectsOfType<SnapZone>())
            {
                if (snapZone.ShownHighlightObject != null)
                {
                    foreach (Mesh mesh in snapZone.ShownHighlightObject.GetComponentsInChildren<MeshFilter>().Select(meshFilter => meshFilter.sharedMesh))
                    {
                        MakeMeshReadable(mesh, snapZone.ShownHighlightObject);
                    }

                    foreach (Mesh mesh in snapZone.ShownHighlightObject.GetComponentsInChildren<SkinnedMeshRenderer>().Select(renderer => renderer.sharedMesh))
                    {
                        MakeMeshReadable(mesh, snapZone.ShownHighlightObject);
                    }
                }
            }
        }

        private static void MakeMeshReadable(Mesh mesh, GameObject gameObject)
        {
            if (mesh.isReadable == false)
            {
                SerializedObject serializedMesh = new SerializedObject(mesh);
                serializedMesh.FindProperty("m_IsReadable").boolValue = true;
                serializedMesh.ApplyModifiedProperties();
                Debug.LogWarning($"The mesh <i>{mesh.name}</i> on <i>{gameObject.name}</i> was not set readable. It has been set readable so the snap zone highlight can be displayed in builds. To make this warning disappear, please enable <b>Read/Write</b> in the mesh import settings.");
            }
        }
    }
}