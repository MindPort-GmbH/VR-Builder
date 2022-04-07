using System;
using NUnit.Framework;
using System.Collections;
using VRBuilder.Core.Utils;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Tests.Utils;
using UnityEngine.Rendering;

namespace VRBuilder.BasicInteraction.Tests
{
    public class HighlighterTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator StartHighlight()
        {
            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                // Given a GameObject with at least one Renderer
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                IHighlighter highlighter = cube.AddComponent(highlighterImplementation) as IHighlighter;;

                Assert.That(highlighter != null);
                Assert.IsFalse(highlighter.IsHighlighting);

                // When StartHighlighting
                highlighter.StartHighlighting(CreateHighlightMaterial());

                yield return null;

                // Then the object is highlighted
                Assert.IsTrue(highlighter.IsHighlighting);
            }
        }
        
        [UnityTest]
        public IEnumerator StopHighlight()
        {
            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                // Given a GameObject with at least one Renderer
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                IHighlighter highlighter = cube.AddComponent(highlighterImplementation) as IHighlighter;;

                Assert.That(highlighter != null);
                Assert.IsFalse(highlighter.IsHighlighting);

                // When StartHighlighting
                highlighter.StartHighlighting(CreateHighlightMaterial());

                yield return null;

                // Then the object is highlighted and then stopped.
                Assert.IsTrue(highlighter.IsHighlighting);
                highlighter.StopHighlighting();
                
                yield return null;
                Assert.IsFalse(highlighter.IsHighlighting);
            }
        }
        
        [UnityTest]
        public IEnumerator HighlightWithColor()
        {
            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                // Given a GameObject with at least one Renderer
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                IHighlighter highlighter = cube.AddComponent(highlighterImplementation) as IHighlighter;
                Material testMaterial = CreateHighlightMaterial();
                Color testColor = Color.green;

                testMaterial.color = testColor;

                Assert.That(highlighter != null);
                Assert.IsFalse(highlighter.IsHighlighting);

                // When StartHighlighting
                highlighter.StartHighlighting(testMaterial);

                yield return null;

                // Then the object is highlighted with provided color
                Material highlightMaterial = highlighter.GetHighlightMaterial();

                Assert.IsTrue(highlighter.IsHighlighting);
                Assert.That(highlightMaterial.color == testColor);
            }
        }

        [UnityTest]
        public IEnumerator HighlightWithMaterial()
        {
            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                // Given a GameObject with at least one Renderer
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                IHighlighter highlighter = cube.AddComponent(highlighterImplementation) as IHighlighter;
                Material testMaterial = CreateHighlightMaterial();

                Assert.That(highlighter != null);
                Assert.IsFalse(highlighter.IsHighlighting);

                // When StartHighlighting
                highlighter.StartHighlighting(testMaterial);

                yield return null;

                // Then the object is highlighted with provided material
                Material highlightMaterial = highlighter.GetHighlightMaterial();

                Assert.IsTrue(highlighter.IsHighlighting);
                Assert.That(testMaterial == highlightMaterial);
            }
        }
        
        [UnityTest]
        public IEnumerator HighlightWithTexture()
        {
            foreach (Type highlighterImplementation in ReflectionUtils.GetConcreteImplementationsOf<IHighlighter>())
            {
                // Given a GameObject with at least one Renderer
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                IHighlighter highlighter = cube.AddComponent(highlighterImplementation) as IHighlighter;
                Material testMaterial = CreateHighlightMaterial();
                Texture testTexture = Texture2D.redTexture;

                testMaterial.mainTexture = testTexture;

                Assert.That(highlighter != null);
                Assert.IsFalse(highlighter.IsHighlighting);

                // When StartHighlighting
                highlighter.StartHighlighting(testMaterial);

                yield return null;

                // Then the object is highlighted with provided texture
                Material highlightMaterial = highlighter.GetHighlightMaterial();

                Assert.IsTrue(highlighter.IsHighlighting);
                Assert.That(testTexture == highlightMaterial.mainTexture);
            }
        }

        private Material CreateHighlightMaterial()
        {
            string shaderName = GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard";
            Shader defaultShader = Shader.Find(shaderName);
            
            return new Material(defaultShader);
        }
    }
}
