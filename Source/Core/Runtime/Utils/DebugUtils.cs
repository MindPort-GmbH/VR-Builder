using UnityEngine;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Utility class for debug tools.
    /// </summary>
    public static class DebugUtils
    {
        /// <summary>
        /// Draws a custom wire cylinder gizmo.
        /// </summary>
        public static void DrawWireCylinderGizmo(Vector3 startPoint, Vector3 endPoint, float width, Color color)
        {
            Gizmos.color = color;

            // Calculate cylinder direction and length
            Vector3 direction = endPoint - startPoint;
            float length = direction.magnitude;
            Vector3 directionNormalized = direction.normalized;

            // Calculate rotation
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, directionNormalized);
            Vector3 midPoint = (startPoint + endPoint) / 2;

            // Scale the cylinder to match the specified length and radius
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(midPoint, rotation, new Vector3(width, length / 2, width));

            // Draw the cylinder
            Gizmos.DrawWireMesh(GetCylinderMesh());

            // Reset the Gizmos matrix
            Gizmos.matrix = oldMatrix;
        }

        private static Mesh GetCylinderMesh()
        {
            // Generate or get a cylinder mesh
            GameObject tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh cylinderMesh = tempCylinder.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(tempCylinder);
            return cylinderMesh;
        }
    }
}
