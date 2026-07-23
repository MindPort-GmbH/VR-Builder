// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRBuilder.Core.Highlighting
{
    /// <inheritdoc cref="IHighlighter" />
    /// <remarks>
    /// Highlights are always queued following a LIFO (Last In First Out) scheme. 
    /// </remarks>
    [DisallowMultipleComponent]
    public class DefaultHighlighter : AbstractHighlighter
    {
        private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        /// <inheritdoc/>
        public override bool IsHighlighting => activeHighlightIds.Count > 0;

        private readonly List<string> activeHighlightIds = new List<string>();

        protected virtual void Reset()
        {
            RefreshCachedRenderers();
        }

        protected virtual void OnDisable()
        {
            if (IsHighlighting)
            {
                ReenableRenderers();
                activeHighlightIds.Clear();
            }
        }

        protected virtual void OnDestroy()
        {
            DestroyAllCachedMaterials();
        }


        public override void StartHighlighting(string highlightMaterialId)
        {
            if (CanObjectBeHighlighted() == false)
            {
                return;
            }

            EnsureRenderersReady();

            if (materialCache.ContainsKey(highlightMaterialId) == false)
            {
                Debug.LogWarning(
                    $"[{nameof(DefaultHighlighter)}] No material registered for ID '{highlightMaterialId}' on '{name}'." +
                    $" Call one of the convenience overloads first, or directly register via materialCache.",
                    gameObject);
                return;
            }

            if (activeHighlightIds.Contains(highlightMaterialId) == false)
            {
                activeHighlightIds.Add(highlightMaterialId);
            }

            ApplyCurrentHighlight();
        }

        /// <summary>
        /// Highlights this object with given <paramref name="highlightColor"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        /// <returns>An ID corresponding to the highlight, should be used in <see cref="StopHighlighting"/>.</returns>
        public string StartHighlighting(Color highlightColor, string highlightID)
        {
            Material material = CreateHighlightMaterial(highlightColor);
            return StartHighlighting(material, highlightID);
        }

        /// <summary>
        /// Highlights this object with given <paramref name="highlightMaterial"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        /// <returns>An ID corresponding to the highlight, should be used in <see cref="StopHighlighting"/>.</returns>
        public string StartHighlighting(Material highlightMaterial, string highlightID)
        {
            RegisterMaterial(highlightID, highlightMaterial);
            StartHighlighting(highlightID);
            return highlightID;
        }

        /// <summary>
        /// Highlights this object with given <paramref name="highlightTexture"/>.
        /// </summary>
        /// <remarks>Every highlight requires an ID to avoid duplications.</remarks>
        /// <returns>An ID corresponding to the highlight, should be used in <see cref="StopHighlighting"/>.</returns>
        public string StartHighlighting(Texture highlightTexture, string highlightID)
        {
            Material material = CreateHighlightMaterial(highlightTexture);
            return StartHighlighting(material, highlightID);
        }

        /// <inheritdoc/>
        public override void StopHighlighting()
        {
            activeHighlightIds.Clear();
            ReenableRenderers();
        }

        /// <summary>
        /// Stops a highlight of given <paramref name="highlightID"/>.
        /// </summary>
        public void StopHighlighting(string highlightID)
        {
            if (activeHighlightIds.Remove(highlightID) == false)
            {
                return;
            }

            if (activeHighlightIds.Count > 0)
            {
                ApplyCurrentHighlight();
            }
            else
            {
                ReenableRenderers();
            }
        }

        /// <inheritdoc/>
        public override Material GetHighlightMaterial()
        {
            string topId = GetTopHighlightId();
            return topId != null && materialCache.TryGetValue(topId, out Material material) ? material : null;
        }

        public override string GetHighlightMaterialId()
        {
            return GetTopHighlightId() ?? string.Empty;
        }

        private void RegisterMaterial(string materialId, Material material)
        {
            if (materialCache.TryGetValue(materialId, out Material existing) && existing != null && existing != material)
            {
                DestroyImmediate(existing);
            }

            materialCache[materialId] = material;
        }

        private void EnsureRenderersReady()
        {
            if (highlightMeshRenderer == null || renderers == null || renderers.Length == 0)
            {
                RefreshCachedRenderers();
            }
        }

        private void ApplyCurrentHighlight()
        {
            string topId = GetTopHighlightId();
            if (topId == null)
            {
                return;
            }

            if (materialCache.TryGetValue(topId, out Material material) == false || material == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DefaultHighlighter)}] Cache miss for ID '{topId}' on '{name}'. Removing from stack.",
                    gameObject);
                activeHighlightIds.Remove(topId);
                return;
            }

            DisableRenders();
            highlightMeshRenderer.sharedMaterial = material;
        }

        private string GetTopHighlightId()
        {
            return activeHighlightIds.Count > 0 ? activeHighlightIds[^1] : null;
        }

        private void DestroyAllCachedMaterials()
        {
            foreach (Material material in materialCache.Values)
            {
                if (material != null)
                {
                    DestroyImmediate(material);
                }
            }

            materialCache.Clear();
        }

        /// <summary>
        /// Regenerates the cached renderers. Only works when no highlight is active.
        /// </summary>
        public void ForceRefreshCachedRenderers()
        {
            if (IsHighlighting)
            {
                return;
            }

            ReenableRenderers();

            if (Application.isPlaying && gameObject.isStatic)
            {
                return;
            }

            ClearCacheRenderers();
            RefreshCachedRenderers();
        }

        /// <summary>
        /// Disables all original renderers and enables the highlight mesh renderer.
        /// </summary>
        protected void DisableRenders()
        {
            if (highlightMeshRenderer != null)
            {
                highlightMeshRenderer.enabled = true;
                highlightMeshRenderer.gameObject.SetActive(true);
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Re-enables all original renderers and hides the highlight mesh renderer.
        /// </summary>
        protected void ReenableRenderers()
        {
            if (highlightMeshRenderer != null)
            {
                highlightMeshRenderer.enabled = false;
                highlightMeshRenderer.gameObject.SetActive(false);
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
    }
}