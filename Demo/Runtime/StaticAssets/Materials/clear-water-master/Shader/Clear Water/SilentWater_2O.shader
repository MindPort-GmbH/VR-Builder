Shader "Silent/Clear Water 2 Opaque" {
	Properties {
		_Tint ("Surface Colour", Color) = (1,1,1,1)
		_MainTex ("Surface Texture", 2D) = "white" {}
		[Space]
		_Glossiness ("Smoothness", Range(0,1)) = 1.0
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[ToggleUI]_IgnoreUVs("Ignore Mesh UVs", Float) = 1
		[ToggleUI]_IgnoreVertexColour("Ignore Vertex Colour", Float) = 1

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

		[Header(Forward Rendering Options)]
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 2
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull [_CullMode]

		CGPROGRAM
		#pragma surface surf Standard vertex:vert addshadow
		#pragma target 3.5

		#include "SilentWater.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			IN.color = _IgnoreVertexColour? 1.0 : IN.color;
			fixed4 surface = tex2D (_MainTex, IN.foamUVs.zw) * _Tint * IN.color;
			fixed3 interior = 0;

			IN.worldNormal = WorldNormalVector( IN, float3( 0, 0, 1 ) );

			// == Ambient environment sample ==
		    float3 viewDir = UnityWorldSpaceViewDir(IN.worldPos);
		    float3 reflectionDir = -viewDir;
		    float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(
		            unity_SpecCube0, reflectionDir, UNITY_SPECCUBE_LOD_STEPS
		        );

			// == Wave normals ==
			float3 wave1 = UnpackScaleNormal(tex2D(_Wave, IN.waveUVs.xy), _WaveStrength);
			float3 wave2 = UnpackScaleNormal(tex2D(_Wave2, IN.waveUVs.zw), _Wave2Strength);

			o.Normal = BlendNormalsPD(wave1, wave2);

			// Apply specular AA filter to smooth shimmering.
			o.Smoothness = GetGeometricNormalVariance(_Glossiness, IN.worldNormal, 0.5, 0.5);

			// == Surface reduction ==
			float NdotV = max(0, dot(IN.VFace * IN.worldNormal, normalize(viewDir)));
			float surfaceReduction = (FresnelLerpFast(1-_Metallic, 1-_Glossiness, NdotV));

			// Apply some lighting to the foam for dynamic lighting conditions.
			interior.rgb *= envSample;

			// Fix later: instances where vertex colour alpha is 0, 
			// but output alpha is > 0 so the edge is dark. 
			o.Albedo = surface.rgb * surfaceReduction;
		}
		ENDCG
	}
}
