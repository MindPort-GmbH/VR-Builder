using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

namespace VRBuilder.XRInteraction.XRI.StarterAssets
{
    [RequireComponent(typeof(Material))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CustomVignetteController : MonoBehaviour
    {
        const string defaultShader = "VR/TunnelingVignette";

        [SerializeField]
        [Tooltip("The size of the vignette")]
        private float targetApertureSize = 0.7f;
        
        [SerializeField]    
        [Tooltip("How long the transition should take")]
        private float transitionDuration = 0.3f;
        
        [SerializeField]
        private VignetteParameters currentParameters = new();
        
        private Material sharedMaterial;
        private MeshRenderer meshRender;
        private MeshFilter meshFilter;
        private MaterialPropertyBlock vignettePropertyBlock;
        private Coroutine currentVignetteCoroutine;
        
        /// <summary>
        /// Static class for constants of the shader
        /// </summary>
        private static class ShaderPropertyLookup
        {
            public static readonly int apertureSize = Shader.PropertyToID("_ApertureSize");
            public static readonly int featheringEffect = Shader.PropertyToID("_FeatheringEffect");
            public static readonly int vignetteColor = Shader.PropertyToID("_VignetteColor");
            public static readonly int vignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");
        }
        
        /// <summary>
        /// Starts the custom vignette effect
        /// </summary>
        public void StartCustomVignette()
        {
            if (currentVignetteCoroutine != null)
                StopCoroutine(currentVignetteCoroutine);
            
            currentVignetteCoroutine = StartCoroutine(TransitionApertureSize(targetApertureSize));
        }
        
        /// <summary>
        /// Resets the custom vignette effect to default value
        /// </summary>
        public void EndCustomVignette()
        {
            if (currentVignetteCoroutine != null)
            {
                StopCoroutine(currentVignetteCoroutine);
            }
            
            currentVignetteCoroutine = StartCoroutine(TransitionApertureSize(1.0f)); 
        }

        /// <summary>
        /// Applies the transition of the vignette size to the target size in a specific transition time
        /// </summary>
        /// <param name="targetSize">Target size of the vignette</param>
        /// <returns>Null IEnumerator because it is not used</returns>
        private IEnumerator TransitionApertureSize(float targetSize)
        {
            //Get the current aperture size
            float startSize = currentParameters.apertureSize;
            float timeElapsed = 0;
            
            while (timeElapsed < transitionDuration)
            {
                //Calculate a new size with smooth interpolation
                float newSize = Mathf.Lerp(startSize, targetSize, timeElapsed / transitionDuration);
                currentParameters.apertureSize = newSize;
                
                //Update our provider's aperture size
                UpdateTunnelingVignette(currentParameters);
                
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            
            currentParameters.apertureSize = targetSize;
            UpdateTunnelingVignette(currentParameters);
            
            currentVignetteCoroutine = null;
        }
        
        /// <summary>
        /// Updates the tunneling vignette with the vignette parameters.
        /// </summary>
        /// <param name="parameters">The <see cref="VignetteParameters"/> uses to update the material values.</param>
        /// <remarks>
        /// Use this method with caution when other <see cref="ITunnelingVignetteProvider"/> instances are updating the material simultaneously.
        /// Calling this method will automatically try to set up the material and its renderer for the <see cref="TunnelingVignetteController"/> if it is not set up already.
        /// </remarks>
        private void UpdateTunnelingVignette(VignetteParameters parameters)
        {
            parameters ??= new VignetteParameters()
            {
                apertureSize = 0.7f,
                apertureVerticalPosition = 0.0f,
                featheringEffect = 0.2f,
                vignetteColor = Color.black,
                vignetteColorBlend = Color.black,
            };

            if (TrySetUpMaterial())
            {
                meshRender.GetPropertyBlock(vignettePropertyBlock);
                vignettePropertyBlock.SetFloat(ShaderPropertyLookup.apertureSize, parameters.apertureSize);
                vignettePropertyBlock.SetFloat(ShaderPropertyLookup.featheringEffect, parameters.featheringEffect);
                vignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColor, parameters.vignetteColor);
                vignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColorBlend, parameters.vignetteColorBlend);
                meshRender.SetPropertyBlock(vignettePropertyBlock);
            }

            // Update the Transform y-position to match apertureVerticalPosition
            var thisTransform = transform;
            var localPosition = thisTransform.localPosition;
            if (!Mathf.Approximately(localPosition.y, parameters.apertureVerticalPosition))
            {
                localPosition.y = parameters.apertureVerticalPosition;
                thisTransform.localPosition = localPosition;
            }
        }

        private bool TrySetUpMaterial()
        {
            if (meshRender == null)
                meshRender = GetComponent<MeshRenderer>();
            if (meshRender == null)
                meshRender = gameObject.AddComponent<MeshRenderer>();

            vignettePropertyBlock ??= new MaterialPropertyBlock();

            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("The default mesh for the TunnelingVignetteController is not set. " +
                    "Make sure to import it from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                return false;
            }

            if (meshRender.sharedMaterial == null)
            {
                var defaultShader = Shader.Find(CustomVignetteController.defaultShader);
                if (defaultShader == null)
                {
                    Debug.LogWarning("The default material for the TunnelingVignetteController is not set, and the default Shader: " + CustomVignetteController.defaultShader
                        + " cannot be found. Make sure they are imported from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                    return false;
                }

                Debug.LogWarning("The default material for the TunnelingVignetteController is not set. " +
                    "Make sure it is imported from the Tunneling Vignette Sample of XR Interaction Toolkit. + " +
                    "Try creating a material using the default Shader: " + CustomVignetteController.defaultShader, this);

                sharedMaterial = new Material(defaultShader)
                {
                    name = "DefaultTunnelingVignette",
                };
                meshRender.sharedMaterial = sharedMaterial;
            }
            else
            {
                sharedMaterial = meshRender.sharedMaterial;
            }

            return true;
        }
    }
}