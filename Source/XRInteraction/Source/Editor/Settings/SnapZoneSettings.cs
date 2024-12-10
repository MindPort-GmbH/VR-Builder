using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Settings;
using VRBuilder.XRInteraction.Interactors;

namespace VRBuilder.XRInteraction.Editor.Settings
{
    /// <summary>
    /// Settings for <see cref="SnapZone"/>s for e.g. automatic creation of such snap zones.
    /// </summary>
    [CreateAssetMenu(fileName = "SnapZoneSettings", menuName = "VR Builder/SnapZoneSettings", order = 1)]
    public class SnapZoneSettings : ComponentSettings<SnapZone, SnapZoneSettings>
    {
        private const string MaterialsPath = "Assets/MindPort/VR Builder/Resources/SnapZones";

        /// <summary>
        /// Only Interactables with this LayerMask will interact with this <see cref="VRBuilder.XRInteraction.SnapZone"/>.
        /// </summary>
        [Tooltip("Only Interactables with this LayerMask will interact with this SnapZone.")]
        public InteractionLayerMask InteractionLayerMask = 1;

        /// <summary>
        /// This color is used as the snap zone highlight color when no object is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used as the snap zone highlight color when no object is hovering but `Snap Zone Active` is true.")]
        public Color HighlightColor = new Color32(102, 150, 255, 50);

        [SerializeField]
        [Tooltip("The material used for the highlight object. Should be transparent.\n\n[This field overrides 'HighlightColor']")]
        private Material highlightMaterial;

        /// <summary>
        /// This color is used when a valid <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used when a valid object is hovering the snap zone.")]
        public Color ValidationColor = new Color32(120, 241, 200, 126);

        /// <summary>
        /// This color is used when an invalid <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used when an invalid object is hovering the snap zone.")]
        public Color InvalidColor = new Color32(243, 77, 20, 126);

        [SerializeField]
        [Tooltip("The material shown when a valid object is hovering the snap zone. Should be transparent.\n\n[This field overrides 'ValidHighlightColor']")]
        private Material validationMaterial;

        [SerializeField]
        [Tooltip("The material shown when an invalid object is hovering the snap zone. Should be transparent.\n\n[This field overrides 'InvalidHighlightColor']")]
        private Material invalidMaterial;

        /// <summary>
        /// The material used for drawing when an <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>. Should be transparent.
        /// </summary>
        public Material HighlightMaterial => SetupHighlightMaterial();

        /// <summary>
        /// The material used for the highlight object, when a valid object is hovering. Should be transparent.
        /// </summary>
        public Material ValidationMaterial => SetupValidationMaterial();

        /// <summary>
        /// The material used for the highlight object, when an invalid object is hovering. Should be transparent.
        /// </summary>
        public Material InvalidMaterial => SetupInvalidMaterial();

        /// <summary>
        /// Applies current settings to provided <see cref="SnapZone"/>.
        /// </summary>
        public override void ApplySettings(SnapZone snapZone)
        {
            snapZone.interactionLayers = InteractionLayerMask;
            snapZone.HighlightMeshMaterial = HighlightMaterial;
            snapZone.ValidationMaterial = ValidationMaterial;
            snapZone.InvalidMaterial = InvalidMaterial;
        }

        private Material SetupHighlightMaterial()
        {
            if (highlightMaterial == null)
            {
                highlightMaterial = UseDefaultMaterial("SnapZoneHighlightMaterial");
            }

            highlightMaterial.color = HighlightColor;
            return highlightMaterial;
        }

        private Material SetupInvalidMaterial()
        {
            if (invalidMaterial == null)
            {
                invalidMaterial = UseDefaultMaterial("SnapZoneInvalidMaterial");
            }

            invalidMaterial.color = InvalidColor;
            return invalidMaterial;
        }

        private Material SetupValidationMaterial()
        {
            if (validationMaterial == null)
            {
                validationMaterial = UseDefaultMaterial("SnapZoneValidationMaterial");
            }

            validationMaterial.color = ValidationColor;
            return validationMaterial;
        }

        private Material UseDefaultMaterial(string materialName)
        {
            if (Directory.Exists(MaterialsPath) == false)
            {
                Directory.CreateDirectory(MaterialsPath);
            }

            string filePath = $"{MaterialsPath}/{materialName}.mat";

            if (File.Exists(filePath))
            {
                try
                {
                    return (Material)AssetDatabase.LoadAssetAtPath(filePath, typeof(Material));
                }
                catch (Exception)
                {
                    Debug.LogError($"Material at '{filePath}' is corrupted or not a material. A new one is created in the same file path.");
                }
            }

            Material material = CreateMaterial();
            material.name = materialName;
            AssetDatabase.CreateAsset(material, filePath);
            AssetDatabase.Refresh();
            return material;
        }

        private Material CreateMaterial()
        {
            string shaderName = GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard";
            Shader defaultShader = Shader.Find(shaderName);

            if (defaultShader == null)
            {
                throw new NullReferenceException($"{nameof(GetType)} failed to create a default material," +
                    $" shader \"{shaderName}\" was not found. Make sure the shader is included into the game build.");
            }

            Material material = new Material(defaultShader);

            if (GraphicsSettings.currentRenderPipeline)
            {
                material.SetFloat("_Surface", 1);
            }
            else
            {
                material.SetFloat("_Mode", 3);
            }

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            return material;
        }
    }
}
