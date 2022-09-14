#ifndef SCWS_INCLUDED
#define SCWS_INCLUDED

		// Depth texture
		uniform UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture); uniform float4 _CameraDepthTexture_TexelSize;

		uniform half _Glossiness;
		uniform half _Metallic;
		uniform fixed4 _Tint;
		uniform half _TintMultiply;

		uniform sampler2D _MainTex; uniform float4 _MainTex_ST;

		uniform float _IgnoreUVs;
		uniform float _IgnoreVertexColour;
		uniform float _IgnoreDepthBuffer;

		uniform float _WaveStrength;
		uniform sampler2D _Wave; uniform float4 _Wave_ST;
		uniform float _Wave2Strength;
		uniform sampler2D _Wave2; uniform float4 _Wave2_ST;

		uniform float _WaveScrollSpeed;
		uniform float _WaveScrollX;
		uniform float _WaveScrollY;
		uniform float _Wave2ScrollSpeed;
		uniform float _Wave2ScrollX;
		uniform float _Wave2ScrollY;

		uniform sampler2D _Foam; uniform float4 _Foam_ST;
		uniform fixed4 _FoamColour;
		uniform half _FoamDensity;
		uniform half _FoamMax;
		uniform half _FoamMin;
		uniform half _FoamDistortion;
		uniform half _FoamScrollSpeed;

		uniform float _DepthDensity;
		uniform float _DepthDensityVertical;
		uniform float _DepthNear;
		uniform float _DepthFar;
		uniform float4 _DepthColour;

		struct Input {
			// Manually created
			float4 waveUVs;
			float4 foamUVs;
			float4 rayFromCamera;
			// Auto generated
			float2 uv__Texcoord;
			float4 color : COLOR;
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			half VFace : VFACE;
		};

		struct depthData {
			float backgroundDepth;
			float surfaceDepth;
			float surfaceFade;
			float foamFade;
			float radialDistance;
			float closeness;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

#if defined(SCWS_GRABPASS)
		// These variables are only used in this version.
		// Screen texture
		uniform sampler2D _GrabTextureWater; uniform float4 _GrabTextureWater_TexelSize;
		// Wave refraction warping intensity
		uniform half _WaveReflectionDistortion;
		// Whether the water fog adds or multiplies the refraction
		uniform half _DepthMultiply;

		float4 grabScreenColour(float4 screenPos, float2 offset) {
			float2 uv = (screenPos.xy + offset) / screenPos.w;
			#if UNITY_UV_STARTS_AT_TOP
				if (_CameraDepthTexture_TexelSize.y < 0) {
					uv.y = 1 - uv.y;
				}
			#endif
			uv = (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
			abs(_CameraDepthTexture_TexelSize.xy);
			return tex2D(_GrabTextureWater, uv);
		}
#endif

		// Utility functions

		void debugThis(inout SurfaceOutputStandard o, float4 debug)
		{
			o.Emission = debug.xyz;
			o.Alpha = 1.0; o.Albedo = 0; o.Occlusion = 0; o.Smoothness = 0;
		}
		void debugThis(inout SurfaceOutputStandard o, float3 debug)
		{
			o.Emission = debug.xyz;
			o.Alpha = 1.0; o.Albedo = 0; o.Occlusion = 0; o.Smoothness = 0;
		}
		void debugThis(inout SurfaceOutputStandard o, float debug)
		{
			o.Emission = debug;
			o.Alpha = 1.0; o.Albedo = 0; o.Occlusion = 0; o.Smoothness = 0;
		}

		bool isVR() {
		    // USING_STEREO_MATRICES
		    #if UNITY_SINGLE_PASS_STEREO
		        return true;
		    #else
		        return false;
		    #endif
		}

		bool IsInMirror()
		{
		    return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
		}

		// Oblique projection fix for mirrors.
		// See https://github.com/lukis101/VRCUnityStuffs/blob/master/Shaders/DJL/Overlays/WorldPosOblique.shader
		#define PM UNITY_MATRIX_P

		inline float4 CalculateFrustumCorrection()
		{
			float x1 = -PM._31/(PM._11*PM._34);
			float x2 = -PM._32/(PM._22*PM._34);
			return float4(x1, x2, 0, PM._33/PM._34 + x1*PM._13 + x2*PM._23);
		}
		inline float CorrectedLinearEyeDepth(float z, float B)
		{
			// default Unity is
			// return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
			return 1.0 / (z/PM._34 + B);
		}

		inline float3 BlendNormalsPD(float3 n1, float3 n2) {
			return normalize(float3(n1.xy*n2.z + n2.xy*n1.z, n1.z*n2.z));
		}

		// Ref: "Crafting a Next-Gen Material Pipeline for The Order: 1886".
		float ClampNdotV(float NdotV)
		{
		    return max(NdotV, 0.0001); // Approximately 0.0057 degree bias
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

		float lerpstep( float a, float b, float t)
		{
		    return saturate( ( t - a ) / ( b - a ) );
		}

		float smootherstep(float a, float b, float t) 
		{
		    t = saturate( ( t - a ) / ( b - a ) );
		    return t * t * t * (t * (t * 6. - 15.) + 10.);
		}

		float exp2FogFactor(float x){
  			const float LOG2 = -1.442695;
			return exp2(x * x * LOG2);
		}

		float unityExp2Factor(float x) {
			// input already has strength applied
			return exp2(-x*x);
		}

        // https://iquilezles.org/www/articles/fog/fog.htm
        float fogFactorNonConstant(float3 rayOri, float3 rayDir, float distance, float c, float b)
        {
            return c * exp(-rayOri.y*b) * (1.0-exp( -distance*rayDir.y*b ))/rayDir.y;
        }

		float GetFogDistance(float rawDepth)
		{
		    float distance = (rawDepth * _ProjectionParams.z) - _ProjectionParams.y;
		    return distance;
		}

		// Underwater fog. xyz: color w: alpha
		float4 UnderwaterFog (depthData d, float density) {
			float eyeFogFactor = d.radialDistance;
			eyeFogFactor = smootherstep(_DepthFar, _DepthNear, eyeFogFactor * 0.01);
/*
			eyeFogFactor = exp(-eyeFogFactor * _DepthDensity);
/*
			eyeFogFactor = 1 / dot(eyeFogFactor, eyeFogFactor);
/*
		    eyeFogFactor = d.surfaceFade * _DepthDensity;
		    eyeFogFactor = exp2(-eyeFogFactor * eyeFogFactor);
*/
			float fogFactor = eyeFogFactor;
			
			return float4(_DepthColour.rgb, LerpOneTo(fogFactor, _DepthColour.w));
		}

		float2 getDepthUVs (float4 screenPos, float2 offset)
		{
			float2 uv = (screenPos.xy + offset) / screenPos.w;
			#if UNITY_UV_STARTS_AT_TOP
				if (_CameraDepthTexture_TexelSize.y < 0) {
					uv.y = 1 - uv.y;
				}
			#endif
			return //uv;
			(floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
				abs(_CameraDepthTexture_TexelSize.xy);
		}

		void farDepthReverseFix(inout float bgDepth)
		{
			#if UNITY_REVERSED_Z
				if (bgDepth == 0)
			#else
				if (bgDepth == 1)
			#endif
				bgDepth = 0.0;
				//bgDepth = IsInMirror();
				//bgDepth = (bgDepth);
		}

		depthData getDepthValues (Input IN, depthData d = (depthData)0,
			float2 offset = float2(0, 0), bool useOffset = false)
		{
			if (_IgnoreDepthBuffer == 1)
			{
				d = (depthData)1;
				return d;
			}

			//d.backgroundDepth
			//d.surfaceDepth
			//d.surfaceFade
			//d.foamFade
			//d.radialDistance
			//d.closeness

			float2 uv = getDepthUVs(IN.screenPos, 0);
			float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
			const fixed3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;

			farDepthReverseFix(depth);

			d.backgroundDepth = 
				CorrectedLinearEyeDepth(depth, IN.rayFromCamera.w/IN.screenPos.w);

			d.radialDistance = length(IN.rayFromCamera * d.backgroundDepth);
			float radialCorrection = d.radialDistance / d.backgroundDepth;

			// Needs proper correction for mirrors
			d.surfaceDepth = IsInMirror()? 1.0 : 
				UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);

			d.surfaceFade = d.backgroundDepth - d.surfaceDepth;

			if (useOffset)
			{	
				offset *= saturate(d.surfaceFade*0.001);
			
				uv = getDepthUVs(IN.screenPos, offset);
				depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);

				farDepthReverseFix(depth);

				d.backgroundDepth = 
					CorrectedLinearEyeDepth(depth, IN.rayFromCamera.w/IN.screenPos.w);

				d.surfaceFade = d.backgroundDepth - d.surfaceDepth;
			}

			d.foamFade = abs(d.backgroundDepth + d.surfaceDepth);

			float3 worldPosition = d.backgroundDepth * IN.rayFromCamera 
				/ IN.screenPos.z  + _WorldSpaceCameraPos.xyz;

			d.closeness = 1/distance(_WorldSpaceCameraPos, worldPosition);

			return d;
		}

		// Workaround for Surface shader issues with tessellation
		void vert_getUVs(float2 texcoord, inout Input o, float4 worldPosition)
		{
			const float2 meshUVs = texcoord.xy;
			const float2 planarUVs = worldPosition.xz;

			float2 surfUVs = TRANSFORM_TEX(meshUVs, _MainTex);

			float2 waveBaseUVs = _IgnoreUVs? planarUVs : meshUVs;

			float2 waveScroll = _WaveScrollSpeed * _Time.y 
				* float2(_WaveScrollX, _WaveScrollY);
			float2 wave2Scroll = _Wave2ScrollSpeed * _Time.y
				* float2(_Wave2ScrollX, _Wave2ScrollY);
			float2 waveUVs =  TRANSFORM_TEX(waveBaseUVs, _Wave) + frac(waveScroll);
			float2 wave2UVs = TRANSFORM_TEX(waveBaseUVs, _Wave2) + frac(wave2Scroll);

			o.waveUVs = float4(waveUVs, wave2UVs);

			float2 foamUVs = TRANSFORM_TEX(waveBaseUVs, _Foam)
			 + frac(_FoamScrollSpeed * (waveScroll + wave2Scroll) * _Foam_ST.xy);

			o.foamUVs = float4(foamUVs, surfUVs);
		}

		void vert( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );

			float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
  			float3 worldNormal = UnityObjectToWorldNormal(v.normal);

			vert_getUVs(v.texcoord.xy, o, worldPosition);

			o.rayFromCamera.xyz = worldPosition.xyz - _WorldSpaceCameraPos.xyz;

			o.rayFromCamera.w = 
				dot(UnityObjectToClipPos(v.vertex),CalculateFrustumCorrection());

			o.screenPos = ComputeScreenPos (UnityObjectToClipPos(v.vertex));
		}

		void SilentWaterFunction (Input IN, inout SurfaceOutputStandard o)
		{
			IN.color = _IgnoreVertexColour? 1.0 : IN.color;
			fixed4 surface = tex2D (_MainTex, IN.foamUVs.zw) * _Tint * IN.color;
			fixed3 interior = 0;

			IN.worldNormal = WorldNormalVector( IN, float3( 0, 0, 1 ) );
			IN.rayFromCamera = normalize(IN.rayFromCamera);

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
			float roughness = PerceptualSmoothnessToRoughness(_Glossiness);
			float NdotV = ClampNdotV(dot(IN.VFace * IN.worldNormal, normalize(viewDir)));
			float surfaceReduction = (FresnelLerp(1-_Metallic, roughness, NdotV));
        	surfaceReduction *= 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
        	surfaceReduction = sqrt(surfaceReduction);

			// As we approach the horizon, flatten the normals. 
			// This isn't physically correct, so disabled for now.
			// o.Normal = lerp(float3(0, 0, 1), o.Normal, surfaceReduction);

			// Get the base depth values.
			depthData d = getDepthValues(IN);
			//float edgeFade = 1-saturate(1-(d.surfaceFade)*10);
			float edgeFade2 = smoothstep(0.1, 1, saturate(d.surfaceFade));
			float edgeFade = smoothstep(0.01, .2, saturate(d.surfaceFade));

			// == Screen refraction ==
			#if defined(SCWS_GRABPASS)
			float2 refractOffset = o.Normal * _WaveReflectionDistortion;
			// Fade edges
			refractOffset *= edgeFade;
			// Fade by closeness
			refractOffset *= 1-saturate(d.closeness);
			refractOffset *= saturate(IN.screenPos.w);

			d = getDepthValues(IN, d, refractOffset, true);

			float4 refraction = grabScreenColour(IN.screenPos, refractOffset);
			#endif

#if 1
			// == Underwater fog ==
			float4 underFog = UnderwaterFog(d, _DepthDensity); 
			// For fog, 0 is full intensity; 1 is fully transparent

			// On backfaces, reduce the fog effect.
			float fogBack = saturate(IN.VFace + 1.5);

			// Fade the fog to black where it's more blended,
			// but also remove the surface opacity.
			float fogReduce = saturate(underFog.w + surface.a);

			// Fade out the edges of the water regardless of fog.
			underFog.w = 1-((1-underFog.w) * edgeFade) ;
			fogReduce = 1-((1-fogReduce) * edgeFade) ;

			// Unless using a refraction.
			#if defined(SCWS_GRABPASS)
			fogReduce = saturate(underFog.w);
			#endif

			underFog.rgb *= 1-fogReduce;
			// Fade the surface to black where it isn't fogged.
			surface.rgb *= fogReduce;
			// Add the fog to the interior colour, so it doesn't get lit by add pass lights.
			interior.rgb += underFog.rgb;
			// Replace the surface alpha with the fog alpha where fog is present.
			//surface.a = max(surface.a,(1-underFog.w));
			surface.a = saturate(surface.a+(fogBack-underFog.w));

			// Apply some lighting to the fog for dynamic lighting conditions.
			#if defined(SCWS_GRABPASS)
			interior.rgb *= LerpWhiteTo(envSample, 1-_DepthMultiply);
			#else
			interior.rgb *= envSample;
			#endif
#endif

			surface *= edgeFade;
			interior *= edgeFade;

#if 1
			// == Surface foam ==
			float2 foamUVs = IN.foamUVs.xy + o.Normal * _FoamDistortion;
			float3 surfaceFoam = tex2D(_Foam, foamUVs) * _FoamColour;

			float foamFade = d.surfaceFade; 

			// Remap the edge fade to match the foam range.
			_FoamMin += 0.001;
			foamFade = saturate((foamFade - _FoamMin) / (_FoamMax - _FoamMin));

			surfaceFoam = saturate(foamFade + foamFade - (1-surfaceFoam));
			surfaceFoam *= _FoamDensity;

			surface.rgb = surface.rgb + surfaceFoam.rgb;

			#if defined(SCWS_GRABPASS)
			interior = lerp(refraction.rgb, 
				LerpWhiteTo(refraction.rgb, _DepthMultiply)*interior.rgb, 
				saturate(surface.a + 1-surfaceReduction));
			#endif

			interior += surfaceFoam * envSample;
			interior *= edgeFade;
#endif

			// Fix later: instances where vertex colour alpha is 0, 
			// but output alpha is > 0 so the edge is dark. 
			#if defined(SCWS_GRABPASS)
			o.Emission = surface.rgb * surfaceReduction * LerpWhiteTo(refraction, _TintMultiply);
			o.Emission += interior * IN.color.a;
			o.Emission *= 0.5 * edgeFade;
			o.Alpha = edgeFade * IN.color.a;
			#else
			o.Albedo = surface.rgb * surfaceReduction;
			o.Emission = interior.rgb * surfaceReduction * IN.color.a;
			o.Alpha = saturate(surface.a + 1-surfaceReduction) * IN.color.a * (edgeFade*edgeFade);
			#endif

			// On the edges of water, reflections should fade out. 
			// We also want to fade out specular intensity (metallic) to account 
			// for the premultiplied alpha calculation.
			// The occlusion property affects indirect lighting (i.e. reflections).
			o.Occlusion = edgeFade * IN.color.a;
			o.Metallic = _Metallic * edgeFade * IN.color.a;

			//debugThis(o, __debug);
		}

#if defined(SCWS_TESS)
		uniform sampler2D _Disp; uniform float4 _Disp_ST;
		uniform float _DispStrength;
		uniform float _DispScrollX;
		uniform float _DispScrollY;
		uniform float _DispScrollSpeed;

		void vertTess( inout appdata_full v, out Input o )
		{
			vert(v, o);
			float4 worldPosition = mul(unity_ObjectToWorld, v.vertex);
			const float2 meshUVs = v.texcoord.xy;
			const float2 planarUVs = worldPosition.xz;

			float2 dispScroll = _DispScrollSpeed * _Time.y 
				* float2(_DispScrollX, _DispScrollY);
			float2 dUVs = _IgnoreUVs? planarUVs : meshUVs;
			dUVs = dUVs * _Disp_ST.xy + _Disp_ST.zw;
			dUVs += frac(dispScroll);
			float d = tex2Dlod(_Disp, float4(dUVs,0,0)).r-0.5;
			d = d*_DispStrength*10;

			float3 vPos = v.vertex.xyz;

			vPos += v.normal*d;

			v.vertex.xyz = vPos;
		}

		void vertTess( inout appdata_full v )
		{
			Input o;
			vertTess(v, o);
		}

		#include "Tessellation.cginc"

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
		  return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, 50, 0.01);
		}
#endif

#endif // SCWS_INCLUDED