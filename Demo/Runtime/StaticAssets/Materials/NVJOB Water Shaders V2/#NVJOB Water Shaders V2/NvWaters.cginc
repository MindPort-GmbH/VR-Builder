// Copyright (c) 2016 Unity Technologies. MIT license - license_unity.txt
// #NVJOB Water Shaders. MIT license - license_nvjob.txt
// #NVJOB Water Shaders v2.0 - https://nvjob.github.io/unity/nvjob-water-shaders-v2
// #NVJOB Nicholas Veselov - https://nvjob.github.io
// Support this asset - https://nvjob.github.io/donate


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


sampler2D_float _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;

//----------------------------------------------

float4 _NvWatersMovement;

//----------------------------------------------

sampler2D _AlbedoTex1;
float4 _AlbedoColor;
float _AlbedoIntensity;
float _AlbedoContrast;
float _SoftFactor;

//----------------------------------------------

float _Shininess;
float _Glossiness;
float _Metallic;

//----------------------------------------------

#ifdef EFFECT_ALBEDO2
sampler2D _AlbedoTex2;
float _Albedo2Tiling;
float _Albedo2Flow;
#endif

//----------------------------------------------

sampler2D _NormalMap1;
float _NormalMap1Strength;

#ifdef EFFECT_NORMALMAP2
sampler2D _NormalMap2;
float _NormalMap2Tiling;
float _NormalMap2Strength;
float _NormalMap2Flow;
#endif

//----------------------------------------------

#ifdef EFFECT_MICROWAVE
float _MicrowaveScale;
float _MicrowaveStrength;
#endif

//----------------------------------------------

#ifdef EFFECT_PARALLAX
float _ParallaxAmount;
float _ParallaxFlow;
float _ParallaxNormal2Offset;
float _ParallaxNoiseGain;
float _ParallaxNoiseAmplitude;
float _ParallaxNoiseFrequency;
float _ParallaxNoiseScale;
float _ParallaxNoiseLacunarity;
#endif

//----------------------------------------------

#ifdef EFFECT_REFLECTION
samplerCUBE _ReflectionCube;
float4 _ReflectionColor;
float _ReflectionStrength;
float _ReflectionSaturation;
float _ReflectionContrast;
#endif

//----------------------------------------------

#ifdef EFFECT_MIRROR
sampler2D _GrabTexture : register(s0);
sampler2D _MirrorReflectionTex : register(s3);
float4 _MirrorColor;
float4 _MirrorDepthColor;
float _WeirdScale;
float _MirrorFPOW;
float _MirrorR0;
float _MirrorSaturation;
float _MirrorStrength;
float _MirrorContrast;
float _MirrorWavePow;
float _MirrorWaveScale;
float _MirrorWaveFlow;
float4 _GrabTexture_TexelSize;
#endif

//----------------------------------------------

#ifdef EFFECT_FOAM
float4 _FoamColor;
float _FoamFlow;
float _FoamGain;
float _FoamAmplitude;
float _FoamFrequency;
float _FoamScale;
float _FoamLacunarity;
float4 _FoamSoft;
#endif


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


struct Input {
float2 uv_AlbedoTex1;
float2 uv_NormalMap1;
float3 worldRefl;
float3 worldPos;
float4 screenPos;
float eyeDepth;
float3 viewDir;
INTERNAL_DATA
};


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


void vert(inout appdata_full v, out Input o) {
UNITY_INITIALIZE_OUTPUT(Input, o);
COMPUTE_EYEDEPTH(o.eyeDepth);
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float Noise(float2 uv, float gain, float amplitude, float frequency, float scale, float lacunarity, float octaves) {
float result;
float frequencyL = frequency;
float amplitudeL = amplitude;
uv = uv * scale;
for (int i = 0; i < octaves; i++) {
float2 i = floor(uv * frequencyL);
float2 f = frac(uv * frequencyL);
float2 t = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
float2 a = i + float2(0.0, 0.0);
float2 b = i + float2(1.0, 0.0);
float2 c = i + float2(0.0, 1.0);
float2 d = i + float2(1.0, 1.0);
a = -1.0 + 2.0 * frac(sin(float2(dot(a, float2(127.1, 311.7)), dot(a, float2(269.5, 183.3)))) * 43758.5453123);
b = -1.0 + 2.0 * frac(sin(float2(dot(b, float2(127.1, 311.7)), dot(b, float2(269.5, 183.3)))) * 43758.5453123);
c = -1.0 + 2.0 * frac(sin(float2(dot(c, float2(127.1, 311.7)), dot(c, float2(269.5, 183.3)))) * 43758.5453123);
d = -1.0 + 2.0 * frac(sin(float2(dot(d, float2(127.1, 311.7)), dot(d, float2(269.5, 183.3)))) * 43758.5453123);
float A = dot(a, f - float2(0.0, 0.0));
float B = dot(b, f - float2(1.0, 0.0));
float C = dot(c, f - float2(0.0, 1.0));
float D = dot(d, f - float2(1.0, 1.0));
float noise = (lerp(lerp(A, B, t.x), lerp(C, D, t.x), t.y));
result = amplitudeL * noise;
frequencyL *= lacunarity;
amplitudeL *= gain;
}
return result * 0.5 + 0.5;
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifdef EFFECT_PARALLAX
float2 OffsetParallax(Input IN) {
float2 uvnh = IN.worldPos.xz;
uvnh += float2(_NvWatersMovement.z, _NvWatersMovement.w) * _ParallaxFlow;
float nh = Noise(uvnh, _ParallaxNoiseGain, _ParallaxNoiseAmplitude, _ParallaxNoiseFrequency * 0.1, _ParallaxNoiseScale * 0.1, _ParallaxNoiseLacunarity, 3);
return ParallaxOffset(nh, _ParallaxAmount, IN.viewDir);
}
#endif


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifdef EFFECT_REFLECTION
float3 SpecularReflection(Input IN, float4 albedo, float3 normal) {
float4 reflcol = texCUBE(_ReflectionCube, WorldReflectionVector(IN, normal));
reflcol *= albedo.a;
reflcol *= _ReflectionStrength;
float LumRef = dot(reflcol, float3(0.2126, 0.7152, 0.0722));
float3 reflcolL = lerp(LumRef.xxx, reflcol, _ReflectionSaturation);
reflcolL = ((reflcolL - 0.5) * _ReflectionContrast + 0.5);
return reflcolL * _ReflectionColor.rgb;
}
#endif


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifdef EFFECT_MIRROR
float4 MirrorReflection(Input IN, float3 normal) {
IN.screenPos.xy = normal * _GrabTexture_TexelSize.xy * IN.screenPos.z + IN.screenPos.xy;
float nvwxz = _NvWatersMovement.z * _MirrorWaveFlow * 10;
IN.screenPos.x += sin((nvwxz + IN.screenPos.y) * _MirrorWaveScale) * _MirrorWavePow * 0.1;
half4 reflcol = tex2Dproj(_MirrorReflectionTex, IN.screenPos);
reflcol *= _MirrorStrength;
float LumRef = dot(reflcol, float3(0.2126, 0.7152, 0.0722));
reflcol.rgb = lerp(LumRef.xxx, reflcol, _MirrorSaturation);
reflcol.rgb = ((reflcol.rgb - 0.5) * _MirrorContrast + 0.5);
reflcol *= _MirrorColor;
float3 refrColor = tex2Dproj(_GrabTexture, IN.screenPos);
refrColor = _MirrorDepthColor * refrColor;
half fresnel = saturate(1.0 - dot(normal, normalize(IN.viewDir)));
fresnel = pow(fresnel, _MirrorFPOW);
fresnel = _MirrorR0 + (1.0 - _MirrorR0) * fresnel;
return reflcol * fresnel + half4(refrColor.xyz, 1.0) * (1.0 - fresnel);
}
#endif


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



float SoftFade(Input IN, float value, float softf) {
float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos + value));
return saturate(softf * (LinearEyeDepth(rawZ) - IN.eyeDepth));
}



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



float SoftFactor(Input IN) {
return _AlbedoColor.a * SoftFade(IN, 0.0001, _SoftFactor);
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#ifdef EFFECT_FOAM
float3 FoamFactor(Input IN, float3 albedo, float2 uv) {
float2 foamuv = IN.worldPos.xz;
foamuv += float2(_NvWatersMovement.z, _NvWatersMovement.w) * -_FoamFlow;
float foamuvnoi = Noise(foamuv, _FoamGain, _FoamAmplitude, _FoamFrequency, _FoamScale, _FoamLacunarity, 3);
float fade = pow(SoftFade(IN, foamuvnoi, _FoamSoft.x), _FoamSoft.z);
float3 foam = tex2D(_AlbedoTex1, uv) * _FoamColor;
if (fade > _FoamSoft.y) albedo = lerp(foam, albedo, fade);
else albedo = lerp(albedo, foam, fade);
return albedo;
}
#endif



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////