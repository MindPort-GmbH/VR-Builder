// Copyright (c) 2016 Unity Technologies. MIT license - license_unity.txt
// #NVJOB Water Shaders. MIT license - license_nvjob.txt
// #NVJOB Water Shaders v2.0 - https://nvjob.github.io/unity/nvjob-water-shaders-v2
// #NVJOB Nicholas Veselov - https://nvjob.github.io
// Support this asset - https://nvjob.github.io/donate


Shader "#NVJOB/Water Shaders V2/Water Specular" {


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



Properties{
//----------------------------------------------

[HideInInspector]_AlbedoTex1("Albedo Texture 1", 2D) = "white" {}
[HideInInspector][HDR]_AlbedoColor("Albedo Color", Color) = (0.15,0.161,0.16,1)
[HideInInspector][NoScaleOffset]_AlbedoTex2("Albedo Texture 2", 2D) = "gray" {}
[HideInInspector]_Albedo2Tiling("Albedo 2 Tiling", float) = 1
[HideInInspector]_Albedo2Flow("Albedo 2 Flow", float) = 1
[HideInInspector]_AlbedoIntensity("Brightness", Range(0.1, 5)) = 1
[HideInInspector]_AlbedoContrast("Contrast", Range(-0.5, 3)) = 1
[HideInInspector]_Shininess("Shininess", Range(0.01, 1)) = 0.15
[HideInInspector][HDR]_SpecColor("Specular Color", Color) = (0.086,0.086,0.086,1)
[HideInInspector]_SoftFactor("Soft Factor", Range(0.0001, 1)) = 0.5
[HideInInspector]_NormalMap1("Normal Map 1", 2D) = "bump" {}
[HideInInspector]_NormalMap1Strength("Normal Map 1 Strength", Range(0.001, 10)) = 1
[HideInInspector][NoScaleOffset]_NormalMap2("Normal Map 2", 2D) = "bump" {}
[HideInInspector]_NormalMap2Tiling("Normal Map 2 Tiling", float) = 0.7
[HideInInspector]_NormalMap2Strength("Normal Map 2 Strength", Range(0.001, 10)) = 1
[HideInInspector]_NormalMap2Flow("Normal Map 2 Flow", float) = 0.5
[HideInInspector]_MicrowaveScale("Micro Waves Scale", Range(0.5, 10)) = 1
[HideInInspector]_MicrowaveStrength("Micro Waves Strength", Range(0.001, 1.5)) = 0.5
[HideInInspector]_ParallaxAmount("Parallax Amount", float) = 0.1
[HideInInspector]_ParallaxFlow("Parallax Flow", float) = 40
[HideInInspector]_ParallaxNormal2Offset("Parallax Normal Map 2 Offset", float) = 1
[HideInInspector]_ParallaxNoiseGain("Parallax Noise Gain", Range(0.0 , 1.0)) = 0.3
[HideInInspector]_ParallaxNoiseAmplitude("Parallax Noise Amplitude", Range(0.0 , 5.0)) = 3
[HideInInspector]_ParallaxNoiseFrequency("Parallax Noise Frequency", Range(0.0 , 6.0)) = 1
[HideInInspector]_ParallaxNoiseScale("Parallax Noise Scale", Float) = 1
[HideInInspector]_ParallaxNoiseLacunarity("Parallax Noise Lacunarity", Range(1 , 6)) = 4
[HideInInspector]_ReflectionCube("Reflection Cubemap", Cube) = "" {}
[HideInInspector][HDR]_ReflectionColor("Reflection Color", Color) = (0.28,0.29,0.25,0.5)
[HideInInspector]_ReflectionStrength("Reflection Strength", Range(0, 10)) = 0.15
[HideInInspector]_ReflectionSaturation("Reflection Saturation", Range(0, 5)) = 1
[HideInInspector]_ReflectionContrast("Reflection Contrast", Range(0, 5)) = 1
[HideInInspector][HDR]_MirrorColor("Mirror Reflection Color", Color) = (1,1,1,0.5)
[HideInInspector]_MirrorDepthColor("Mirror Reflection Depth Color", Color) = (0,0,0,0.5)
[HideInInspector]_MirrorStrength("Reflection Strength", Range(0, 5)) = 1
[HideInInspector]_MirrorSaturation("Reflection Saturation", Range(0, 5)) = 1
[HideInInspector]_MirrorContrast("Reflection Contrast", Range(0, 5)) = 1
[HideInInspector]_MirrorFPOW("Mirror FPOW", Float) = 5.0
[HideInInspector]_MirrorR0("Mirror R0", Float) = 0.01
[HideInInspector]_MirrorWavePow("Reflections Wave Strength", Float) = 1
[HideInInspector]_MirrorWaveScale("Reflections Wave Scale", Float) = 1
[HideInInspector]_MirrorWaveFlow("Reflections Wave Flow", Float) = 5
[HideInInspector]_MirrorReflectionTex("_MirrorReflectionTex", 2D) = "gray" {}
[HideInInspector][HDR]_FoamColor("Foam Color", Color) = (1, 1, 1, 1)
[HideInInspector]_FoamFlow("Foam Flow", Float) = 10
[HideInInspector]_FoamGain("Foam Gain", Float) = 0.6
[HideInInspector]_FoamAmplitude("Foam Amplitude", Float) = 15
[HideInInspector]_FoamFrequency("Foam Frequency", Float) = 4
[HideInInspector]_FoamScale("Foam Scale", Float) = 0.1
[HideInInspector]_FoamLacunarity("Foam Lacunarity", Float) = 5
[HideInInspector]_FoamSoft("Foam Soft", Vector) = (0.25, 0.6, 1, 0)

//----------------------------------------------
}



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



SubShader{
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

Tags{ "Queue" = "Geometry+800" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
LOD 200
Cull Off
ZWrite On

CGPROGRAM
#pragma shader_feature_local EFFECT_ALBEDO2
#pragma shader_feature_local EFFECT_NORMALMAP2
#pragma shader_feature_local EFFECT_MICROWAVE
#pragma shader_feature_local EFFECT_PARALLAX
#pragma shader_feature_local EFFECT_REFLECTION
#pragma shader_feature_local EFFECT_MIRROR
#pragma shader_feature_local EFFECT_FOAM
#pragma surface surf BlinnPhong alpha:fade vertex:vert exclude_path:prepass noshadowmask noshadow
#pragma target 3.0

//----------------------------------------------

#include "NvWaters.cginc"

//----------------------------------------------

void surf(Input IN, inout SurfaceOutput o) {

#ifdef EFFECT_PARALLAX
float2 offset = OffsetParallax(IN);
IN.uv_AlbedoTex1 -= offset;
IN.uv_NormalMap1 += offset;
float2 uvn = IN.uv_NormalMap1;
uvn.xy += float2(_NvWatersMovement.z, _NvWatersMovement.w);
#ifdef EFFECT_NORMALMAP2
float2 uvnd = IN.uv_NormalMap1 + (offset * _ParallaxNormal2Offset);
uvnd.xy += float2(_NvWatersMovement.z, _NvWatersMovement.w) * _NormalMap2Flow;
#endif
#else
float2 uvn = IN.uv_NormalMap1;
uvn.xy += float2(_NvWatersMovement.z, _NvWatersMovement.w);
#ifdef EFFECT_NORMALMAP2
float2 uvnd = IN.uv_NormalMap1;
uvnd.xy += float2(_NvWatersMovement.z, _NvWatersMovement.w) * _NormalMap2Flow;
#endif
#endif

float2 uv = IN.uv_AlbedoTex1;
uv.xy += float2(_NvWatersMovement.x, _NvWatersMovement.y);
#ifdef EFFECT_ALBEDO2
float2 uvd = IN.uv_AlbedoTex1;
uvd.xy += float2(_NvWatersMovement.x, _NvWatersMovement.y) * _Albedo2Flow;
#endif

float4 tex = tex2D(_AlbedoTex1, uv) * _AlbedoColor;
#ifdef EFFECT_ALBEDO2
tex *= tex2D(_AlbedoTex2, uvd * _Albedo2Tiling);
#endif
tex *= _AlbedoIntensity;
float3 albedo = ((tex - 0.5) * _AlbedoContrast + 0.5).rgb;

float3 normal = UnpackNormal(tex2D(_NormalMap1, uvn)) * _NormalMap1Strength;
#ifdef EFFECT_NORMALMAP2
normal += UnpackNormal(tex2D(_NormalMap2, uvnd * _NormalMap2Tiling)) * _NormalMap2Strength;
#ifdef EFFECT_MICROWAVE
normal += UnpackNormal(tex2D(_NormalMap2, (uv + uvnd) * 2 * _MicrowaveScale)) * _MicrowaveStrength;
#endif
#endif

#ifdef EFFECT_REFLECTION
o.Emission = SpecularReflection(IN, tex, normal);
#endif

#ifdef EFFECT_MIRROR
o.Emission = (o.Emission + MirrorReflection(IN, normal)) * 0.5;
#endif

#ifdef EFFECT_FOAM
albedo = FoamFactor(IN, albedo, uvn);
#endif

o.Normal = normal;
o.Gloss = tex.a;
o.Specular = _Shininess;
o.Albedo.rgb = albedo;
o.Alpha = SoftFactor(IN);

}

//----------------------------------------------

ENDCG

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
}


FallBack "Legacy Shaders/Reflective/Bumped Diffuse"
CustomEditor "NVWaterMaterials"


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
