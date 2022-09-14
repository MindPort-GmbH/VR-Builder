Shader "Silent/Clear Water 2 Refractive" {
	Properties {
		_Tint ("Surface Colour", Color) = (1,1,1,1)
		_MainTex ("Surface Texture", 2D) = "white" {}
		_TintMultiply("Surface Colour Power", Range( 0 , 1)) = 1.0
		[Space]
		_Glossiness ("Smoothness", Range(0,1)) = 1.0
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[ToggleUI]_IgnoreUVs("Ignore Mesh UVs", Float) = 1
		[ToggleUI]_IgnoreVertexColour("Ignore Vertex Colour", Float) = 1
		[ToggleUI]_IgnoreDepthBuffer("Ignore Depth Buffer", Float) = 0

		[Header(Wave Settings)]
		[Normal]_Wave("Wave", 2D) = "bump" {}
		_WaveStrength("Wave Strength", Range( -1 , 1)) = 0.1
		_WaveScrollX("Wave Scroll X", Range( -1 , 1)) = 0
		_WaveScrollY("Wave Scroll Y", Range( -1 , 1)) = 0
		_WaveScrollSpeed("Wave Scroll Multiplier", Range( 0 , 4)) = 0
		[Space]
		[Normal]_Wave2("Wave 2", 2D) = "bump" {}
		_Wave2Strength("Wave 2 Strength", Range( -1 , 1)) = 0.1
		_Wave2ScrollX("Wave 2 Scroll X", Range( -1 , 1)) = 0
		_Wave2ScrollY("Wave 2 Scroll Y", Range( -1 , 1)) = 0
		_Wave2ScrollSpeed("Wave 2 Scroll Multiplier", Range( -0 , 4)) = 0
		_WaveReflectionDistortion("Wave Refraction Distortion", Range( 0 , 1)) = 1.0

		[Header(Foam Settings)]
		_Foam("Foam Texture", 2D) = "white" {}
		_FoamColour("Foam Colour", Color) = (1,1,1,1)
		_FoamDensity("Foam Density", Range(0, 4)) = 1
		[Gamma]_FoamMax("Foam Start", Range( 0 , 4)) = 0
		[Gamma]_FoamMin("Foam End", Range( 0 , 4)) = 1
		[Gamma]_FoamDistortion("Foam Distortion", Range( 0 , 1)) = 0.3
		[Gamma]_FoamScrollSpeed("Foam Scroll Multiplier", Range( 0 , 4)) = 1

		[Header(Water Fog Settings)]
		_DepthNear("Water Fog Start", Float) = -80
		_DepthFar("Water Fog End", Float) = 100
		//_DepthDensity("Water Fog Density", Range(0, 1)) = 0.15
		//_DepthDensityVertical("Water Fog Vertical Density", Range(0, 1)) = 0.15
		_DepthColour("Water Fog Colour", Color) = (0,0,0,1)
		_DepthMultiply("Water Fog Colour Power", Range( 0 , 1)) = 0.0

		[Header(Forward Rendering Options)]
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 2
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
		[HideInInspector] __Texcoord("Texcoord", 2D) = "black"
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" "IgnoreProjector" = "True" "IsEmissive" = "true" }
		Cull [_CullMode]
		LOD 200
		
		GrabPass{ "_GrabTextureWater" }

		CGPROGRAM
		#pragma surface surf Standard alpha vertex:vert addshadow fullforwardshadows
		#pragma target 3.5

		#define SCWS_GRABPASS

		#include "SilentWater.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			SilentWaterFunction(IN, o);
		}
		ENDCG
	}
}
