// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Silent/Clear Water"
{
	Properties
	{
		_Tint("Tint", Color) = (1,1,1,1)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.95
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[Normal]_Wave("Wave", 2D) = "bump" {}
		[Toggle]_IgnoreUVs("Ignore UVs", Float) = 0
		_WaveStrength("Wave Strength", Range( -2 , 2)) = 1
		_WaveScrollX("Wave Scroll X", Range( -1 , 1)) = 0
		_WaveScrollY("Wave Scroll Y", Range( -1 , 1)) = 0
		_WaveScrollSpeed("Wave Scroll Speed", Range( -1 , 1)) = 0
		[Normal]_Wave2("Wave 2", 2D) = "bump" {}
		_Wave2Strength("Wave 2 Strength", Range( -2 , 2)) = 1
		_Wave2ScrollX("Wave 2 Scroll X", Range( -1 , 1)) = 0
		_Wave2ScrollY("Wave 2 Scroll Y", Range( -1 , 1)) = 0
		_Wave2ScrollSpeed("Wave 2 Scroll Speed", Range( -1 , 1)) = 0
		_WaveReflectionDistortion("Wave Reflection Distortion", Range( 0 , 1)) = 0
		_Foam("Foam", 2D) = "black" {}
		[HDR]_FoamColour("Foam Colour", Color) = (1,1,1,1)
		_foamMax("Foam Start", Range( 0 , 4)) = 0
		_foamMin("Foam End", Range( 0 , 4)) = 0
		[Gamma]_FoamDistortion("Foam Distortion", Range( 0 , 1)) = 0.3
		_FoamSpeed("Foam Speed", Range( 0 , 1)) = 1
		_DepthColour("Depth Colour", Color) = (0,0,0,1)
		_DepthDistance("Depth Distance", Range( 0 , 100)) = 100
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 0
		[Toggle]_IgnoreVertexColour("Ignore Vertex Colour", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent-10" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull [_CullMode]
		Blend SrcAlpha OneMinusSrcAlpha , Zero Zero
		
		GrabPass{ "_GrabTextureWater" }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 4.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float4 screenPos;
			float vertexToFrag651;
			float4 vertexColor : COLOR;
			float4 vertexToFrag466;
			float2 vertexToFrag628;
			float3 worldPos;
			float2 vertexToFrag627;
			float3 worldNormal;
			INTERNAL_DATA
			float2 vertexToFrag629;
			float4 vertexToFrag469;
			half ASEVFace : VFACE;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform int _CullMode;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float4 _Tint;
		uniform float _DepthDistance;
		uniform float _IgnoreVertexColour;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTextureWater )
		uniform sampler2D _Wave;
		uniform float _WaveScrollX;
		uniform float _WaveScrollSpeed;
		uniform float _WaveScrollY;
		uniform float _Wave2ScrollX;
		uniform float _Wave2ScrollSpeed;
		uniform float _Wave2ScrollY;
		uniform float _IgnoreUVs;
		uniform float4 _Wave_ST;
		uniform float _WaveStrength;
		uniform sampler2D _Wave2;
		uniform float4 _Wave2_ST;
		uniform float _Wave2Strength;
		uniform float _WaveReflectionDistortion;
		uniform float _foamMin;
		uniform float _foamMax;
		uniform sampler2D _Foam;
		uniform float4 _Foam_ST;
		uniform float _FoamDistortion;
		uniform float _FoamSpeed;
		uniform float4 _FoamColour;
		uniform float4 _DepthColour;
		uniform float _Smoothness;
		uniform float _Metallic;


		float4 CalculateObliqueFrustumCorrection(  )
		{
			float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34);
			float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34);
			return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23);
		}


		float CorrectedLinearEyeDepth( float z , float correctionFactor )
		{
			return 1.f / (z / UNITY_MATRIX_P._34 + correctionFactor);
		}


		float FROM( float3 screenPos )
		{
			return UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
		}


		float GetGeometricNormalVariance( float perceptualSmoothness , float3 geometricNormalWS , float screenSpaceVariance , float threshold )
		{
			#define PerceptualSmoothnessToRoughness(perceptualSmoothness) (1.0 - perceptualSmoothness) * (1.0 - perceptualSmoothness)
			#define RoughnessToPerceptualSmoothness(roughness) 1.0 - sqrt(roughness)
			float3 deltaU = ddx(geometricNormalWS);
			float3 deltaV = ddy(geometricNormalWS);
			float variance = screenSpaceVariance * (dot(deltaU, deltaU) + dot(deltaV, deltaV));
			float roughness = PerceptualSmoothnessToRoughness(perceptualSmoothness);
			// Ref: Geometry into Shading - http://graphics.pixar.com/library/BumpRoughness/paper.pdf - equation (3)
			float squaredRoughness = saturate(roughness * roughness + min(2.0 * variance, threshold * threshold)); // threshold can be really low, square the value for easier
			return RoughnessToPerceptualSmoothness(sqrt(squaredRoughness));
		}


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float CallFresnelLerpFast( float3 specColor , float3 grazingTerm , float nv )
		{
			return FresnelLerpFast (specColor, grazingTerm, nv);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float4 localCalculateObliqueFrustumCorrection650 = CalculateObliqueFrustumCorrection();
			float dotResult653 = dot( float4( ase_vertex3Pos , 0.0 ) , localCalculateObliqueFrustumCorrection650 );
			o.vertexToFrag651 = dotResult653;
			float2 appendResult501 = (float2(( _WaveScrollX * _Time.y * _WaveScrollSpeed ) , ( _WaveScrollY * _Time.y * _WaveScrollSpeed )));
			float2 appendResult502 = (float2(( _Wave2ScrollX * _Time.y * _Wave2ScrollSpeed ) , ( _Wave2ScrollY * _Time.y * _Wave2ScrollSpeed )));
			float4 appendResult487 = (float4(appendResult501 , appendResult502));
			o.vertexToFrag466 = frac( appendResult487 );
			float2 uv0_Wave = v.texcoord.xy * _Wave_ST.xy + _Wave_ST.zw;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult563 = (float2(ase_worldPos.x , ase_worldPos.z));
			o.vertexToFrag628 = (( _IgnoreUVs )?( (appendResult563*_Wave_ST.xy + _Wave_ST.zw) ):( uv0_Wave ));
			float2 uv0_Wave2 = v.texcoord.xy * _Wave2_ST.xy + _Wave2_ST.zw;
			o.vertexToFrag627 = (( _IgnoreUVs )?( (appendResult563*_Wave2_ST.xy + _Wave2_ST.zw) ):( uv0_Wave2 ));
			float2 uv0_Foam = v.texcoord.xy * _Foam_ST.xy + _Foam_ST.zw;
			o.vertexToFrag629 = (( _IgnoreUVs )?( (appendResult563*_Foam_ST.xy + _Foam_ST.zw) ):( uv0_Foam ));
			float4 mainUVScrollXY141 = appendResult487;
			o.vertexToFrag469 = frac( ( _FoamSpeed * mainUVScrollXY141 ) );
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_SHADOWCASTER
				float staticSwitch338 = 0.125;
			#else
				float staticSwitch338 = 1.0;
			#endif
			SurfaceOutputStandardSpecular s213 = (SurfaceOutputStandardSpecular ) 0;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 unityObjectToViewPos416 = UnityObjectToViewPos( ase_vertex3Pos );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float clampDepth476 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy );
			float z655 = clampDepth476;
			float ObliqueFrustumCorrectionFactor652 = i.vertexToFrag651;
			float correctionFactor655 = ObliqueFrustumCorrectionFactor652;
			float localCorrectedLinearEyeDepth655 = CorrectedLinearEyeDepth( z655 , correctionFactor655 );
			float screenDepth639 = localCorrectedLinearEyeDepth655;
			float temp_output_402_0 = ( unityObjectToViewPos416.z + screenDepth639 );
			float foamFade418 = temp_output_402_0;
			float3 ase_worldPos = i.worldPos;
			float closeness295 = saturate( ( 1.0 / distance( _WorldSpaceCameraPos , ase_worldPos ) ) );
			float2 FoamUVs588 = i.vertexToFrag629;
			float2 WaveUVs573 = i.vertexToFrag628;
			float2 Wave2UVs574 = i.vertexToFrag627;
			float3 finalNormals170 = BlendNormals( UnpackScaleNormal( tex2D( _Wave, ( (i.vertexToFrag466).xy + WaveUVs573 ) ), _WaveStrength ) , UnpackScaleNormal( tex2D( _Wave2, ( (i.vertexToFrag466).zw + Wave2UVs574 ) ), _Wave2Strength ) );
			float4 tex2DNode42 = tex2D( _Foam, (FoamUVs588*1.0 + ( ( (finalNormals170).xy * _FoamDistortion ) + (i.vertexToFrag469).xy + (i.vertexToFrag469).zw )) );
			float3 screenPos479 = ase_screenPos.xyz;
			float localFROM479 = FROM( screenPos479 );
			float temp_output_478_0 = ( localCorrectedLinearEyeDepth655 - localFROM479 );
			float behindDepth518 = saturate( temp_output_478_0 );
			float4 finalFoam191 = ( saturate( ( ( ( foamFade418 - _foamMin ) * closeness295 ) / ( ( _foamMax - _foamMin ) * closeness295 ) ) ) * tex2DNode42 * _FoamColour * tex2DNode42.a * _FoamColour.a * behindDepth518 );
			s213.Albedo = finalFoam191.rgb;
			s213.Normal = WorldNormalVector( i , finalNormals170 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float z657 = ase_grabScreenPosNorm.b;
			float correctionFactor657 = ObliqueFrustumCorrectionFactor652;
			float localCorrectedLinearEyeDepth657 = CorrectedLinearEyeDepth( z657 , correctionFactor657 );
			float depthFadeToSurface314 = saturate( abs( ( ( screenDepth639 - localCorrectedLinearEyeDepth657 ) / 0.1 ) ) );
			float baseDepthFade636 = temp_output_478_0;
			float depthDistance225 = _DepthDistance;
			float temp_output_642_0 = ( baseDepthFade636 / depthDistance225 );
			float dotResult371 = dot( temp_output_642_0 , temp_output_642_0 );
			float depthFade1621 = ( 1.0 / dotResult371 );
			float finalOpacity221 = ( depthFadeToSurface314 * saturate( ( _Tint.a + ( 1.0 - saturate( depthFade1621 ) ) ) ) * (( _IgnoreVertexColour )?( 1.0 ):( i.vertexColor.a )) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV423 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode423 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV423, 5.0 ) );
			float3 lerpResult218 = lerp( float3( 0,0,0 ) , finalNormals170 , ( finalOpacity221 * _WaveReflectionDistortion * closeness295 * ( 1.0 - fresnelNode423 ) * behindDepth518 ));
			float4 temp_output_217_0 = ( float4( lerpResult218 , 0.0 ) + ase_grabScreenPosNorm );
			float4 screenColor29 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTextureWater,temp_output_217_0.xy/temp_output_217_0.w);
			float4 temp_cast_7 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch332 = temp_cast_7;
			#else
				float4 staticSwitch332 = screenColor29;
			#endif
			float4 grabPass149 = staticSwitch332;
			float eyeDepth251 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, temp_output_217_0.xy ));
			float screenDepthWarped253 = eyeDepth251;
			float distanceDepth1614 = localFROM479;
			float temp_output_616_0 = ( ( screenDepthWarped253 - distanceDepth1614 ) / depthDistance225 );
			float dotResult617 = dot( temp_output_616_0 , temp_output_616_0 );
			float temp_output_618_0 = ( 1.0 / dotResult617 );
			float depthFadeWarped74 = temp_output_618_0;
			float4 lerpResult56 = lerp( ( grabPass149 * _Tint ) , _DepthColour , ( _DepthColour.a * ( 1.0 - saturate( depthFadeWarped74 ) ) ));
			float4 lerpResult128 = lerp( grabPass149 , lerpResult56 , depthFadeToSurface314);
			float4 temp_cast_9 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch366 = temp_cast_9;
			#else
				float4 staticSwitch366 = lerpResult128;
			#endif
			float3 specColor539 = float3( 0,0,0 );
			float perceptualSmoothness598 = _Smoothness;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 switchResult601 = (((i.ASEVFace>0)?(ase_normWorldNormal):(( ase_normWorldNormal * float3( -1,-1,-1 ) ))));
			float3 geometricNormalWS598 = switchResult601;
			float2 _Vector0 = float2(0.5,0.5);
			float screenSpaceVariance598 = _Vector0.x;
			float threshold598 = _Vector0.y;
			float localGetGeometricNormalVariance598 = GetGeometricNormalVariance( perceptualSmoothness598 , geometricNormalWS598 , screenSpaceVariance598 , threshold598 );
			float geomRoughnessFactor209 = localGetGeometricNormalVariance598;
			float3 temp_cast_10 = (geomRoughnessFactor209).xxx;
			float3 grazingTerm539 = temp_cast_10;
			float dotResult543 = dot( ase_worldNormal , ase_worldViewDir );
			float nv539 = dotResult543;
			float localCallFresnelLerpFast539 = CallFresnelLerpFast( specColor539 , grazingTerm539 , nv539 );
			s213.Emission = ( staticSwitch366 * max( 0.0 , ( 1.0 - ( localCallFresnelLerpFast539 * finalOpacity221 ) ) ) ).rgb;
			s213.Specular = ( _Metallic * _Tint ).rgb;
			s213.Smoothness = geomRoughnessFactor209;
			s213.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi213 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g213 = UnityGlossyEnvironmentSetup( s213.Smoothness, data.worldViewDir, s213.Normal, float3(0,0,0));
			gi213 = UnityGlobalIllumination( data, s213.Occlusion, s213.Normal, g213 );
			#endif

			float3 surfResult213 = LightingStandardSpecular ( s213, viewDir, gi213 ).rgb;
			surfResult213 += s213.Emission;

			#ifdef UNITY_PASS_FORWARDADD//213
			surfResult213 -= s213.Emission;
			#endif//213
			c.rgb = ( surfResult213 * finalOpacity221 );
			c.a = staticSwitch338;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float clampDepth476 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy );
			float z655 = clampDepth476;
			float ObliqueFrustumCorrectionFactor652 = i.vertexToFrag651;
			float correctionFactor655 = ObliqueFrustumCorrectionFactor652;
			float localCorrectedLinearEyeDepth655 = CorrectedLinearEyeDepth( z655 , correctionFactor655 );
			float screenDepth639 = localCorrectedLinearEyeDepth655;
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float z657 = ase_grabScreenPosNorm.b;
			float correctionFactor657 = ObliqueFrustumCorrectionFactor652;
			float localCorrectedLinearEyeDepth657 = CorrectedLinearEyeDepth( z657 , correctionFactor657 );
			float depthFadeToSurface314 = saturate( abs( ( ( screenDepth639 - localCorrectedLinearEyeDepth657 ) / 0.1 ) ) );
			float3 screenPos479 = ase_screenPos.xyz;
			float localFROM479 = FROM( screenPos479 );
			float temp_output_478_0 = ( localCorrectedLinearEyeDepth655 - localFROM479 );
			float baseDepthFade636 = temp_output_478_0;
			float depthDistance225 = _DepthDistance;
			float temp_output_642_0 = ( baseDepthFade636 / depthDistance225 );
			float dotResult371 = dot( temp_output_642_0 , temp_output_642_0 );
			float depthFade1621 = ( 1.0 / dotResult371 );
			float finalOpacity221 = ( depthFadeToSurface314 * saturate( ( _Tint.a + ( 1.0 - saturate( depthFade1621 ) ) ) ) * (( _IgnoreVertexColour )?( 1.0 ):( i.vertexColor.a )) );
			float2 WaveUVs573 = i.vertexToFrag628;
			float2 Wave2UVs574 = i.vertexToFrag627;
			float3 finalNormals170 = BlendNormals( UnpackScaleNormal( tex2D( _Wave, ( (i.vertexToFrag466).xy + WaveUVs573 ) ), _WaveStrength ) , UnpackScaleNormal( tex2D( _Wave2, ( (i.vertexToFrag466).zw + Wave2UVs574 ) ), _Wave2Strength ) );
			float3 ase_worldPos = i.worldPos;
			float closeness295 = saturate( ( 1.0 / distance( _WorldSpaceCameraPos , ase_worldPos ) ) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV423 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode423 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV423, 5.0 ) );
			float behindDepth518 = saturate( temp_output_478_0 );
			float3 lerpResult218 = lerp( float3( 0,0,0 ) , finalNormals170 , ( finalOpacity221 * _WaveReflectionDistortion * closeness295 * ( 1.0 - fresnelNode423 ) * behindDepth518 ));
			float4 temp_output_217_0 = ( float4( lerpResult218 , 0.0 ) + ase_grabScreenPosNorm );
			float4 screenColor29 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTextureWater,temp_output_217_0.xy/temp_output_217_0.w);
			float4 temp_cast_2 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDADD
				float4 staticSwitch332 = temp_cast_2;
			#else
				float4 staticSwitch332 = screenColor29;
			#endif
			float4 grabPass149 = staticSwitch332;
			o.Emission = ( ( 1.0 - finalOpacity221 ) * grabPass149 ).rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float4 customPack3 : TEXCOORD3;
				float4 customPack4 : TEXCOORD4;
				float4 screenPos : TEXCOORD5;
				float4 tSpace0 : TEXCOORD6;
				float4 tSpace1 : TEXCOORD7;
				float4 tSpace2 : TEXCOORD8;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.x = customInputData.vertexToFrag651;
				o.customPack2.xyzw = customInputData.vertexToFrag466;
				o.customPack1.yz = customInputData.vertexToFrag628;
				o.customPack3.xy = customInputData.vertexToFrag627;
				o.customPack3.zw = customInputData.vertexToFrag629;
				o.customPack4.xyzw = customInputData.vertexToFrag469;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.vertexToFrag651 = IN.customPack1.x;
				surfIN.vertexToFrag466 = IN.customPack2.xyzw;
				surfIN.vertexToFrag628 = IN.customPack1.yz;
				surfIN.vertexToFrag627 = IN.customPack3.xy;
				surfIN.vertexToFrag629 = IN.customPack3.zw;
				surfIN.vertexToFrag469 = IN.customPack4.xyzw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18301
2565;510;1238;1382;-932.103;894.7345;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;654;-4946.924,-1361.402;Inherit;False;906;324.8269;;5;651;653;650;649;652;Oblique Frustum Correction Factor;1,1,1,1;0;0
Node;AmplifyShaderEditor.CustomExpressionNode;650;-4896.924,-1302.403;Float;False;float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34)@$float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34)@$return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23)@;4;False;0;CalculateObliqueFrustumCorrection;False;True;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;649;-4826.419,-1215.576;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;653;-4643.924,-1307.403;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;651;-4529.923,-1303.403;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;652;-4335.923,-1311.403;Float;False;ObliqueFrustumCorrectionFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;480;-5459.4,-595.0184;Inherit;False;1440;426;;12;476;639;614;518;608;636;478;479;477;655;656;671;Clip BG from Grab;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenDepthNode;476;-5427.717,-536.5738;Inherit;False;1;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;656;-5440.156,-455.0292;Inherit;False;652;ObliqueFrustumCorrectionFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;477;-5423.212,-346.5718;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;655;-5134.912,-515.8894;Float;False;return 1.f / (z / UNITY_MATRIX_P._34 + correctionFactor)@;1;False;2;True;z;FLOAT;0;In;;Float;False;True;correctionFactor;FLOAT;0;In;;Float;False;CorrectedLinearEyeDepth;False;True;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;479;-5241.884,-343.3955;Float;False;return UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z)@;1;False;1;True;screenPos;FLOAT3;0,0,0;In;;Float;False;FROM;False;True;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;172;-5602.201,1098.679;Inherit;False;1285;342.8;;11;225;247;244;58;243;371;372;291;621;641;642;Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;478;-4810.659,-501.5384;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;636;-4627.906,-375.0414;Float;False;baseDepthFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-5560.511,1334.076;Float;False;Property;_DepthDistance;Depth Distance;23;0;Create;True;0;0;False;0;False;100;1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;641;-5122.379,1172.094;Inherit;False;636;baseDepthFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;225;-5216.41,1350.382;Float;False;depthDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;579;-6067.507,-163.2844;Inherit;False;1565.848;890.8235;;20;627;575;588;586;584;587;603;573;574;572;570;568;571;569;563;605;604;560;628;629;UV Selection;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;169;-4010.134,16.29915;Inherit;False;1988.149;784.2443;Generate normals;29;113;117;118;112;111;109;170;19;22;8;123;122;141;466;486;487;488;489;496;497;498;499;500;501;502;503;505;576;577;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;642;-4905.379,1182.094;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;335;-5585.1,754.9299;Inherit;False;1359;295;;9;663;658;657;643;645;644;648;552;314;Depth Fade to Surface;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-3989.909,98.6537;Float;False;Property;_WaveScrollX;Wave Scroll X;6;0;Create;True;0;0;False;0;False;0;0.124;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;658;-5555.69,804.6633;Inherit;False;652;ObliqueFrustumCorrectionFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;371;-4746.953,1176.835;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;663;-5549.534,882.1628;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;118;-4000.733,709.7255;Float;False;Property;_Wave2ScrollSpeed;Wave 2 Scroll Speed;13;0;Create;True;0;0;False;0;False;0;0.205;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;496;-3930.062,395.595;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-3990.317,232.6535;Float;False;Property;_WaveScrollSpeed;Wave Scroll Speed;8;0;Create;True;0;0;False;0;False;0;0.138;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;560;-6043.5,136.3209;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;113;-3998.317,630.6537;Float;False;Property;_Wave2ScrollY;Wave 2 Scroll Y;12;0;Create;True;0;0;False;0;False;0;0.16;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-3999.909,560.6537;Float;False;Property;_Wave2ScrollX;Wave 2 Scroll X;11;0;Create;True;0;0;False;0;False;0;0.124;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3987.909,169.6536;Float;False;Property;_WaveScrollY;Wave Scroll Y;7;0;Create;True;0;0;False;0;False;0;0.16;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;639;-4854.115,-386.0827;Float;False;screenDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;372;-4594.953,1147.835;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureTransformNode;605;-5990.757,315.6655;Inherit;False;8;False;1;0;SAMPLER2D;_Sampler0605;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;497;-3651.062,112.595;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;657;-5246.446,884.8034;Float;False;return 1.f / (z / UNITY_MATRIX_P._34 + correctionFactor)@;1;False;2;True;z;FLOAT;0;In;;Float;False;True;correctionFactor;FLOAT;0;In;;Float;False;CorrectedLinearEyeDepth;False;True;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;500;-3651.062,640.595;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;498;-3644.062,295.595;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;643;-5217.178,801.0168;Inherit;False;639;screenDepth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureTransformNode;604;-5987.757,436.6655;Inherit;False;22;False;1;0;SAMPLER2D;_Sampler0604;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.DynamicAppendNode;563;-5857.5,160.3209;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;499;-3650.062,491.595;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;501;-3479.062,293.595;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;568;-5411.235,8.371889;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;621;-4515.941,1345.778;Float;False;depthFade1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;570;-5426.266,-111.787;Inherit;False;0;8;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;633;-1446.066,715.5806;Inherit;False;1544.042;500.6882;;10;75;51;374;348;10;41;349;553;214;221;Register Base Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;648;-5010.423,794.3093;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;502;-3488.062,487.595;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;571;-5428.267,148.613;Inherit;False;0;22;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;569;-5415.364,266.24;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;75;-1396.066,944.6634;Inherit;False;621;depthFade1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;572;-5209.531,-54.33877;Float;False;Property;_IgnoreUVs;Ignore UVs;5;0;Create;True;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;575;-5189.531,175.8845;Float;False;Property;_IgnoreUVs;Ignore UVs;30;0;Create;True;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;487;-3326.874,390.1274;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;644;-4865.179,807.0168;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;627;-4992.966,179.4027;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;645;-4730.679,840.0167;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;51;-1199.006,948.993;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;489;-3307.227,273.739;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexToFragmentNode;628;-5005.966,-49.59729;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;374;-1051.171,951.0428;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;516;-4996.383,-1018.772;Inherit;False;982.6411;406.6547;;6;269;295;272;279;267;270;Closeness;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexToFragmentNode;466;-3172.345,234.2337;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;574;-4775.531,170.8845;Float;False;Wave2UVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;552;-4605.908,826.6588;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1128.67,-270.0518;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;573;-4786.531,-54.1155;Float;False;WaveUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;348;-848.7709,1014.269;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;270;-4879.671,-822.6683;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;488;-2967.874,625.1274;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;269;-4905.199,-968.6854;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;576;-3161.906,367.5619;Inherit;False;573;WaveUVs;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;486;-2961.874,217.1274;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;314;-4457.518,826.2018;Float;False;depthFadeToSurface;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-851.187,910.5806;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;577;-3175.906,508.5619;Inherit;False;574;Wave2UVs;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-2878.115,116.1092;Float;False;Property;_WaveStrength;Wave Strength;5;0;Create;True;0;0;False;0;False;1;0.19;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;349;-606.215,1027.736;Float;False;Property;_IgnoreVertexColour;Ignore Vertex Colour;29;0;Create;True;0;0;False;0;False;0;2;0;FLOAT;1;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;505;-2786.062,497.595;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;553;-842.4477,813.7065;Inherit;False;314;depthFadeToSurface;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-2879.184,715.6748;Float;False;Property;_Wave2Strength;Wave 2 Strength;10;0;Create;True;0;0;False;0;False;1;0.16;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;267;-4663.671,-835.6683;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;41;-703.1002,905.8044;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;503;-2736.062,355.595;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;242;-3992.019,-1264.411;Inherit;False;1964;729.8;For underwater;17;218;30;29;251;253;49;217;223;215;219;222;332;333;424;423;425;519;Grab Pass;1,1,1,1;0;0
Node;AmplifyShaderEditor.FresnelNode;423;-3962.695,-1184.216;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;22;-2573.469,453.0291;Inherit;True;Property;_Wave2;Wave 2;9;1;[Normal];Create;True;0;0;False;0;False;-1;None;1b0c364c827145d4c9898dd381a762d2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;279;-4490.833,-850.5825;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;608;-4402.726,-496.7031;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;-271.4215,765.5806;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-2566.089,224.2726;Inherit;True;Property;_Wave;Wave;4;1;[Normal];Create;True;0;0;False;0;False;-1;None;1b0c364c827145d4c9898dd381a762d2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;19;-2260.896,327.1832;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;424;-3679.703,-1182.879;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;272;-4367.671,-838.6683;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;221;-121.0236,767.816;Float;False;finalOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;518;-4238.647,-505.2713;Float;False;behindDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;425;-3614.6,-1074.783;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;222;-3926.346,-942.2128;Inherit;False;221;finalOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;219;-3971.228,-787.7805;Float;False;Property;_WaveReflectionDistortion;Wave Reflection Distortion;14;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;170;-2195.952,639.9818;Float;False;finalNormals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;295;-4222.487,-847.0268;Float;False;closeness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;519;-3884.768,-681.1007;Inherit;False;518;behindDepth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;215;-3941.326,-1017.004;Inherit;False;170;finalNormals;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;-3652.829,-934.8582;Inherit;False;5;5;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;30;-3535.75,-720.3983;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;218;-3457.631,-1094.252;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;217;-3292.941,-765.7766;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;251;-3110.702,-632.8203;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;614;-4887.686,-271.9952;Float;False;distanceDepth1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;253;-2892.186,-636.9329;Float;False;screenDepthWarped;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;634;-4026.69,832.6463;Inherit;False;1293.419;380.3219;;10;615;612;610;613;616;617;618;74;619;620;Warped Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;612;-3970.931,960.5716;Inherit;False;253;screenDepthWarped;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;615;-3976.69,1081.431;Inherit;False;614;distanceDepth1;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;417;-5605.121,1496.852;Inherit;False;1067.956;621.6826;;10;406;402;416;413;405;403;404;418;451;667;Foam Distance;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;610;-3731.578,1097.968;Inherit;False;225;depthDistance;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;613;-3669.931,975.5716;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;210;-4008.113,-494.6009;Inherit;False;1986.277;373.8134;For specular AA;7;209;601;598;7;202;674;675;Geometric Roughness Factor;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;616;-3485.69,1016.43;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;522;-4973.828,2177.976;Inherit;False;933;309;;7;514;521;469;520;515;525;513;Scroll Foam by Wave Speed (vertex);1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-3152.703,79.05662;Float;False;mainUVScrollXY;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;413;-5563.608,1592.931;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;667;-5559.267,1736.247;Inherit;False;639;screenDepth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;617;-3248.969,911.6463;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;515;-4941.457,2362.583;Inherit;False;141;mainUVScrollXY;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldNormalVector;202;-3967.766,-347.2054;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureTransformNode;603;-6003.757,552.6655;Inherit;False;42;False;1;0;SAMPLER2D;_Sampler0603;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.RangedFloatNode;520;-4934.296,2270.392;Float;False;Property;_FoamSpeed;Foam Speed;21;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.UnityObjToViewPosHlpNode;416;-5337.121,1593.19;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScaleAndOffsetNode;584;-5409.25,531.755;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;521;-4662.296,2338.392;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;402;-5105.894,1642.369;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;193;-4504.498,1492.042;Inherit;False;1788.7;647.8003;;22;191;53;280;379;42;94;93;444;43;328;593;329;327;91;445;92;97;457;446;95;96;443;Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;618;-3096.969,882.6463;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;587;-5432.229,398.7802;Inherit;False;0;42;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;675;-3781.875,-237.0305;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;-1,-1,-1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-3967.599,-428.903;Float;False;Property;_Smoothness;Smoothness;2;0;Create;True;0;0;False;0;False;0.95;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-2959.271,1019.981;Float;False;depthFadeWarped;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;333;-3099.65,-933.2516;Float;False;Constant;_Float1;Float 1;29;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;525;-4567.835,2239.642;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;674;-3565.715,-239.4301;Inherit;False;Constant;_Vector0;Vector 0;35;0;Create;True;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ScreenColorNode;29;-3121.135,-822.2603;Float;False;Global;_GrabTextureWater;GrabTextureWater;5;0;Create;True;0;0;False;0;False;Object;-1;True;True;1;0;FLOAT4;0,0,0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;443;-4485.905,2033.118;Inherit;False;170;finalNormals;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;586;-5208.229,458.7802;Float;False;Property;_IgnoreUVs;Ignore UVs;35;0;Create;True;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwitchByFaceNode;601;-3661.686,-336.2182;Inherit;False;2;0;FLOAT3;1,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;418;-4874.012,1597.278;Float;False;foamFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;629;-5012.966,463.4027;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;446;-4467.905,1938.118;Float;False;Property;_FoamDistortion;Foam Distortion;20;1;[Gamma];Create;True;0;0;False;0;False;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;598;-3325.84,-404.3094;Float;False;#define PerceptualSmoothnessToRoughness(perceptualSmoothness) (1.0 - perceptualSmoothness) * (1.0 - perceptualSmoothness)$#define RoughnessToPerceptualSmoothness(roughness) 1.0 - sqrt(roughness)$float3 deltaU = ddx(geometricNormalWS)@$float3 deltaV = ddy(geometricNormalWS)@$float variance = screenSpaceVariance * (dot(deltaU, deltaU) + dot(deltaV, deltaV))@$float roughness = PerceptualSmoothnessToRoughness(perceptualSmoothness)@$// Ref: Geometry into Shading - http://graphics.pixar.com/library/BumpRoughness/paper.pdf - equation (3)$float squaredRoughness = saturate(roughness * roughness + min(2.0 * variance, threshold * threshold))@ // threshold can be really low, square the value for easier$return RoughnessToPerceptualSmoothness(sqrt(squaredRoughness))@;1;False;4;True;perceptualSmoothness;FLOAT;0;In;;Float;False;True;geometricNormalWS;FLOAT3;0,0,0;In;;Float;False;True;screenSpaceVariance;FLOAT;0.5;In;;Float;False;True;threshold;FLOAT;0.5;In;;Float;False;GetGeometricNormalVariance;False;True;0;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0.5;False;3;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;547;111.8516,586.1957;Inherit;False;967.1704;628.1531;FresnelLerpFast is called in Unity GI to adjust the intensity of the reflection towards the horizon for the fresnel effect. Inverting it fixes the brightness at the horizon. ;10;544;539;543;541;542;555;557;559;558;212;Reduce At Horizon;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;95;-4428.968,1562.051;Inherit;False;418;foamFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;469;-4501.967,2384.728;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;457;-4293.94,2027.337;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;332;-2923.538,-955.4619;Float;False;Property;_Addpass;Add pass?;29;0;Fetch;True;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;624;-843.7135,-387.1364;Inherit;False;74;depthFadeWarped;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-4473.968,1655.051;Float;False;Property;_foamMin;Foam End;19;0;Create;False;0;0;False;0;False;0;0;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-4485.968,1741.052;Float;False;Property;_foamMax;Foam Start;18;0;Create;False;0;0;False;0;False;0;0;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;541;127.9156,983.1875;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;92;-4169.469,1678.136;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-2605.756,-965.3028;Float;False;grabPass1;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;588;-4789.229,457.7802;Float;False;FoamUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;514;-4261.548,2364.161;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;327;-4174.406,1773.733;Inherit;False;295;closeness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;622;-646.6536,-382.8068;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;209;-2868.863,-365.4456;Float;False;geomRoughnessFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;91;-4175.14,1588.359;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;542;140.6899,830.8312;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;513;-4250.548,2253.161;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;445;-4092.906,2001.118;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;212;134.1459,740.1124;Inherit;False;209;geomRoughnessFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;241;-1073.95,-349.3615;Inherit;False;49;grabPass1;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;543;344.5902,910.0241;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;444;-3913.706,1994.618;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;328;-3927.407,1562.733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;52;-744.4904,-100.9101;Float;False;Property;_DepthColour;Depth Colour;22;0;Create;True;0;0;False;0;False;0,0,0,1;0.3411765,0.6627451,0.6196077,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-3925.407,1683.733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;593;-4470.889,1837.111;Inherit;False;588;FoamUVs;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;623;-498.8185,-380.757;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-345.3701,-347.5782;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-759.7742,-199.0846;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;379;-3810.581,1768.151;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;93;-3740.472,1559.137;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;539;480.8473,739.1757;Float;False;return FresnelLerpFast (specColor, grazingTerm, nv)@;1;False;3;True;specColor;FLOAT3;0,0,0;In;;Float;True;True;grazingTerm;FLOAT3;0,0,0;In;;Float;True;True;nv;FLOAT;0;In;;Float;True;CallFresnelLerpFast;False;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;558;322.076,1015.087;Inherit;False;221;finalOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;280;-3607.855,1858.493;Float;False;Property;_FoamColour;Foam Colour;16;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;56;-70.0676,-169.0185;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-73.87343,-242.9076;Inherit;False;49;grabPass1;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;559;708.27,907.8354;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;136;-104.574,-24.37431;Inherit;False;314;depthFadeToSurface;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;94;-3590.473,1557.137;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;42;-3620.871,1646.634;Inherit;True;Property;_Foam;Foam;15;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;53;-3350,1968.844;Inherit;False;518;behindDepth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;544;718.5667,734.0386;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-3157.026,1744.556;Inherit;False;6;6;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;128;157.9467,-190.747;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;367;164.1796,-298.3802;Float;False;Constant;_Float4;Float 4;31;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;557;877.6531,705.1884;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;366;375.9932,-219.0883;Float;False;Property;_Keyword4;Keyword 4;31;0;Create;True;0;0;True;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;294.9114,81.9748;Float;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;534;889.9448,-392.2763;Inherit;False;650.6366;270.0464;Displays the geometry behind the water at the edges. ;4;282;261;283;535;Edge Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;631;366.9796,177.0699;Float;False;Property;_Tint;Tint;1;0;Fetch;True;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;191;-2993.807,1746.681;Float;False;finalFoam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;632;595.8674,90.01294;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;192;1012.891,-77.82905;Inherit;False;191;finalFoam;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;174;2783.594,-431.3829;Inherit;False;2204.06;1397.66;;36;82;86;85;88;37;81;36;76;87;67;9;25;70;69;100;99;98;102;168;177;179;155;178;180;197;199;173;186;183;185;184;187;182;336;337;315;Scraps;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;630;336.1582,-5.296143;Inherit;False;209;geomRoughnessFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;548;1023.975,140.9614;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;537;1004.996,-725.8627;Inherit;False;518.6591;270.5648;;3;340;338;339;Shadow Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;973.4416,11.15346;Inherit;False;170;finalNormals;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;535;922.8543,-253.0489;Inherit;False;221;finalOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;176;2791.841,1052.794;Inherit;False;1999.3;776;;16;145;175;144;80;148;157;153;154;149;77;146;147;165;142;166;284;Vertex offset for waves;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;283;1146.857,-216.5001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;340;1054.1,-663.0377;Float;False;Constant;_Float3;Float 3;30;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;213;1236.381,-0.5181403;Inherit;False;Specular;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;173;2812.703,433.4312;Inherit;False;1286.072;237.254;;7;38;28;27;26;233;232;370;Camera Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;261;934.1423,-340.0195;Inherit;False;49;grabPass1;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;339;1048.792,-571.1691;Float;False;Constant;_Float2;Float 2;30;0;Create;True;0;0;False;0;False;0.125;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;536;1266.519,-101.4819;Inherit;False;221;finalOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;25;3087.745,-323.5251;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;336;3639.78,-333.8338;Float;False;return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f@;0;False;0;inInMirror;False;False;0;0;1;INT;0
Node;AmplifyShaderEditor.GetLocalVarNode;284;4331.959,1099.042;Inherit;False;38;cameraDepthFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;182;3796.747,64.28912;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;168;3931.907,-73.51649;Float;False;Property;_UseTessellation;Use Tessellation;26;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;142;3429.136,1338.971;Inherit;True;Property;_Height;Height;24;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;144;3790.502,1182.188;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;607;1287.48,381.1588;Inherit;False;74;depthFadeWarped;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;549;1583.621,0.3096147;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.EdgeLengthTessNode;180;4139.909,591.4836;Inherit;False;1;0;FLOAT;1024;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;554;126.063,457.2227;Float;False;surfaceColourUsedForFresnel;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;315;3645.071,-255.8926;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;2858.18,1512.912;Inherit;False;141;mainUVScrollXY;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PowerNode;405;-5233.921,1977.734;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;247;-5258.411,1156.382;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;99;3259.436,757.4616;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;2840.962,826.7126;Float;False;Property;_VertexDistortionStrength;Vertex Distortion Strength;27;0;Create;True;0;0;False;0;False;0;0.016;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;183;3988.747,64.28912;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleNode;67;3808.843,238.5665;Inherit;False;3;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;153;3144.891,1368.474;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.1;False;1;FLOAT;0.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;2920.227,122.6717;Inherit;False;74;depthFadeWarped;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;619;-3364.225,1086.086;Float;False;return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f@;0;False;0;inInMirror;False;False;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;157;3736.908,1381.029;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;186;4584.458,71.87714;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;451;-4974.421,1786.842;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;3079.735,715.0616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;3083.743,156.2577;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComputeScreenPosHlpNode;184;4196.747,64.28912;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LengthOpNode;244;-5390.411,1152.382;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;671;-4635.302,-286.7356;Float;False;zeroOneDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;166;3749.136,1610.972;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;197;3754.494,801.836;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;606;1681.596,176.3974;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;148;3537.017,1538.169;Float;False;Property;_HeightScale;Height Scale;25;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;26;3198.639,508.2194;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;555;129.9802,660.2508;Inherit;False;554;surfaceColourUsedForFresnel;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;232;2827.861,484.9715;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;80;3537.364,1610.972;Inherit;False;38;cameraDepthFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;149;2842.829,1386.791;Inherit;False;0;142;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;175;4519.01,1243.723;Float;False;finalHeightVertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;-5070.212,1965.361;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;69;3433.088,-311.6137;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;282;1345.716,-326.2784;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;370;3635.121,579.0336;Float;False;Constant;_Float5;Float 5;31;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;146;4037.133,1354.971;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;185;4406.457,210.8772;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;3485.736,783.0616;Float;False;waveOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;177;3659.909,-41.51652;Inherit;False;175;finalHeightVertexOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;337;4161.388,-362.5999;Inherit;False;2;2;0;FLOAT;0;False;1;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;199;4099.492,751.836;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;70;3279.342,-310.4996;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;178;4427.907,527.4837;Float;False;Property;_UseTessellation;Use Tessellation;26;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;326;-1018.144,-493.8309;Float;False;Property;_CullMode;Cull Mode;28;1;[Enum];Create;True;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;1;INT;0
Node;AmplifyShaderEditor.SaturateNode;403;-4917.176,1965.771;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;406;-5395.321,1973.233;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;145;4208.133,1203.97;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;36;3376.871,112.8667;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;291;-4862.209,1351.274;Float;False;return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f@;0;False;0;inInMirror;False;False;0;0;1;INT;0
Node;AmplifyShaderEditor.EdgeLengthTessNode;155;4139.909,463.4835;Inherit;False;1;0;FLOAT;32;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FresnelNode;9;2848.427,-341.4815;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;637;-6435.689,1052.78;Float;False; ;1;False;1;True;In0;FLOAT;0;In;;Float;False;My Custom Expression;True;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;3245.253,109.4427;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;3662.416,178.9606;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;3909.134,1418.971;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;165;3909.134,1594.972;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.75;False;2;FLOAT;0.85;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;88;3445.834,274.1714;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;233;3043.889,535.8615;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;243;-5567.409,1151.382;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;154;2903.081,1625.852;Inherit;False;141;mainUVScrollXY;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;3469.073,487.9144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;87;3229.244,253.3494;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;10;False;2;FLOAT;-9;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;86;3574.416,276.9604;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;3660.609,501.0656;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;338;1218.791,-628.1691;Float;False;Property;_Shadow;Shadow?;28;0;Fetch;True;0;0;False;0;False;0;0;0;False;UNITY_PASS_SHADOWCASTER;Toggle;2;Key0;Key1;Fetch;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;187;4770.458,108.8772;Inherit;False;False;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;2807.488,283.1704;Float;False;Property;_FoamPower;Foam Power;17;0;Create;True;0;0;False;0;False;3;3;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;179;3483.909,-137.5165;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;620;-3124.946,1076.077;Inherit;False;2;2;0;FLOAT;0;False;1;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;3839.094,500.5746;Float;False;cameraDepthFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1823.609,-243.7619;Float;False;True;-1;4;ASEMaterialInspector;0;0;CustomLighting;Silent/Clear Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;True;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;-10;True;Custom;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;32;497.4;1100;False;0.5;True;2;5;False;-1;10;False;-1;1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;0;-1;-1;-1;0;False;0;0;True;326;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;653;0;649;0
WireConnection;653;1;650;0
WireConnection;651;0;653;0
WireConnection;652;0;651;0
WireConnection;655;0;476;0
WireConnection;655;1;656;0
WireConnection;479;0;477;0
WireConnection;478;0;655;0
WireConnection;478;1;479;0
WireConnection;636;0;478;0
WireConnection;225;0;58;0
WireConnection;642;0;641;0
WireConnection;642;1;225;0
WireConnection;371;0;642;0
WireConnection;371;1;642;0
WireConnection;639;0;655;0
WireConnection;372;1;371;0
WireConnection;497;0;109;0
WireConnection;497;1;496;0
WireConnection;497;2;117;0
WireConnection;657;0;663;3
WireConnection;657;1;658;0
WireConnection;500;0;113;0
WireConnection;500;1;496;0
WireConnection;500;2;118;0
WireConnection;498;0;111;0
WireConnection;498;1;496;0
WireConnection;498;2;117;0
WireConnection;563;0;560;1
WireConnection;563;1;560;3
WireConnection;499;0;112;0
WireConnection;499;1;496;0
WireConnection;499;2;118;0
WireConnection;501;0;497;0
WireConnection;501;1;498;0
WireConnection;568;0;563;0
WireConnection;568;1;605;0
WireConnection;568;2;605;1
WireConnection;621;0;372;0
WireConnection;648;0;643;0
WireConnection;648;1;657;0
WireConnection;502;0;499;0
WireConnection;502;1;500;0
WireConnection;569;0;563;0
WireConnection;569;1;604;0
WireConnection;569;2;604;1
WireConnection;572;0;570;0
WireConnection;572;1;568;0
WireConnection;575;0;571;0
WireConnection;575;1;569;0
WireConnection;487;0;501;0
WireConnection;487;2;502;0
WireConnection;644;0;648;0
WireConnection;627;0;575;0
WireConnection;645;0;644;0
WireConnection;51;0;75;0
WireConnection;489;0;487;0
WireConnection;628;0;572;0
WireConnection;374;0;51;0
WireConnection;466;0;489;0
WireConnection;574;0;627;0
WireConnection;552;0;645;0
WireConnection;573;0;628;0
WireConnection;488;0;466;0
WireConnection;486;0;466;0
WireConnection;314;0;552;0
WireConnection;10;0;5;4
WireConnection;10;1;374;0
WireConnection;349;0;348;4
WireConnection;505;0;488;0
WireConnection;505;1;577;0
WireConnection;267;0;269;0
WireConnection;267;1;270;0
WireConnection;41;0;10;0
WireConnection;503;0;486;0
WireConnection;503;1;576;0
WireConnection;22;1;505;0
WireConnection;22;5;123;0
WireConnection;279;1;267;0
WireConnection;608;0;478;0
WireConnection;214;0;553;0
WireConnection;214;1;41;0
WireConnection;214;2;349;0
WireConnection;8;1;503;0
WireConnection;8;5;122;0
WireConnection;19;0;8;0
WireConnection;19;1;22;0
WireConnection;424;0;423;0
WireConnection;272;0;279;0
WireConnection;221;0;214;0
WireConnection;518;0;608;0
WireConnection;425;0;424;0
WireConnection;170;0;19;0
WireConnection;295;0;272;0
WireConnection;223;0;222;0
WireConnection;223;1;219;0
WireConnection;223;2;295;0
WireConnection;223;3;425;0
WireConnection;223;4;519;0
WireConnection;218;1;215;0
WireConnection;218;2;223;0
WireConnection;217;0;218;0
WireConnection;217;1;30;0
WireConnection;251;0;217;0
WireConnection;614;0;479;0
WireConnection;253;0;251;0
WireConnection;613;0;612;0
WireConnection;613;1;615;0
WireConnection;616;0;613;0
WireConnection;616;1;610;0
WireConnection;141;0;487;0
WireConnection;617;0;616;0
WireConnection;617;1;616;0
WireConnection;416;0;413;0
WireConnection;584;0;563;0
WireConnection;584;1;603;0
WireConnection;584;2;603;1
WireConnection;521;0;520;0
WireConnection;521;1;515;0
WireConnection;402;0;416;3
WireConnection;402;1;667;0
WireConnection;618;1;617;0
WireConnection;675;0;202;0
WireConnection;74;0;618;0
WireConnection;525;0;521;0
WireConnection;29;0;217;0
WireConnection;586;0;587;0
WireConnection;586;1;584;0
WireConnection;601;0;202;0
WireConnection;601;1;675;0
WireConnection;418;0;402;0
WireConnection;629;0;586;0
WireConnection;598;0;7;0
WireConnection;598;1;601;0
WireConnection;598;2;674;1
WireConnection;598;3;674;2
WireConnection;469;0;525;0
WireConnection;457;0;443;0
WireConnection;332;1;29;0
WireConnection;332;0;333;0
WireConnection;92;0;97;0
WireConnection;92;1;96;0
WireConnection;49;0;332;0
WireConnection;588;0;629;0
WireConnection;514;0;469;0
WireConnection;622;0;624;0
WireConnection;209;0;598;0
WireConnection;91;0;95;0
WireConnection;91;1;96;0
WireConnection;513;0;469;0
WireConnection;445;0;457;0
WireConnection;445;1;446;0
WireConnection;543;0;542;0
WireConnection;543;1;541;0
WireConnection;444;0;445;0
WireConnection;444;1;513;0
WireConnection;444;2;514;0
WireConnection;328;0;91;0
WireConnection;328;1;327;0
WireConnection;329;0;92;0
WireConnection;329;1;327;0
WireConnection;623;0;622;0
WireConnection;66;0;52;4
WireConnection;66;1;623;0
WireConnection;31;0;241;0
WireConnection;31;1;5;0
WireConnection;379;0;593;0
WireConnection;379;2;444;0
WireConnection;93;0;328;0
WireConnection;93;1;329;0
WireConnection;539;1;212;0
WireConnection;539;2;543;0
WireConnection;56;0;31;0
WireConnection;56;1;52;0
WireConnection;56;2;66;0
WireConnection;559;0;539;0
WireConnection;559;1;558;0
WireConnection;94;0;93;0
WireConnection;42;1;379;0
WireConnection;544;0;559;0
WireConnection;43;0;94;0
WireConnection;43;1;42;0
WireConnection;43;2;280;0
WireConnection;43;3;42;4
WireConnection;43;4;280;4
WireConnection;43;5;53;0
WireConnection;128;0;50;0
WireConnection;128;1;56;0
WireConnection;128;2;136;0
WireConnection;557;1;544;0
WireConnection;366;1;128;0
WireConnection;366;0;367;0
WireConnection;191;0;43;0
WireConnection;632;0;6;0
WireConnection;632;1;631;0
WireConnection;548;0;366;0
WireConnection;548;1;557;0
WireConnection;283;0;535;0
WireConnection;213;0;192;0
WireConnection;213;1;171;0
WireConnection;213;2;548;0
WireConnection;213;3;632;0
WireConnection;213;4;630;0
WireConnection;25;0;9;0
WireConnection;168;1;179;0
WireConnection;168;0;177;0
WireConnection;142;1;153;0
WireConnection;549;0;213;0
WireConnection;549;1;536;0
WireConnection;405;0;406;0
WireConnection;247;0;244;0
WireConnection;247;1;225;0
WireConnection;99;0;98;0
WireConnection;99;1;102;0
WireConnection;183;0;182;0
WireConnection;67;0;85;0
WireConnection;153;0;149;0
WireConnection;153;2;77;0
WireConnection;153;1;154;0
WireConnection;157;0;142;1
WireConnection;186;0;184;0
WireConnection;186;1;185;0
WireConnection;451;0;402;0
WireConnection;81;0;76;0
WireConnection;81;1;82;0
WireConnection;184;0;183;0
WireConnection;244;0;243;2
WireConnection;671;0;476;0
WireConnection;166;0;80;0
WireConnection;606;0;607;0
WireConnection;26;0;233;0
WireConnection;175;0;145;0
WireConnection;404;0;405;0
WireConnection;69;0;70;0
WireConnection;282;0;283;0
WireConnection;282;1;261;0
WireConnection;146;0;144;2
WireConnection;146;1;147;0
WireConnection;185;0;184;0
WireConnection;100;0;99;0
WireConnection;337;0;315;0
WireConnection;337;1;336;0
WireConnection;199;0;197;0
WireConnection;70;0;25;0
WireConnection;178;1;180;0
WireConnection;178;0;155;0
WireConnection;403;0;404;0
WireConnection;145;0;144;1
WireConnection;145;1;146;0
WireConnection;145;2;144;3
WireConnection;36;0;37;0
WireConnection;37;0;81;0
WireConnection;85;0;36;0
WireConnection;85;1;86;0
WireConnection;147;0;157;0
WireConnection;147;1;148;0
WireConnection;147;2;165;0
WireConnection;165;0;166;0
WireConnection;88;0;87;0
WireConnection;233;0;232;2
WireConnection;27;0;26;0
WireConnection;87;0;36;0
WireConnection;86;0;88;0
WireConnection;28;0;27;0
WireConnection;338;1;340;0
WireConnection;338;0;339;0
WireConnection;187;0;186;0
WireConnection;620;0;618;0
WireConnection;620;1;619;0
WireConnection;38;0;370;0
WireConnection;0;2;282;0
WireConnection;0;9;338;0
WireConnection;0;13;549;0
ASEEND*/
//CHKSM=C7F587C405F2D907160D58D4FA2082D8D472C709