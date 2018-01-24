Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Noise ("Noise", 2D) = "white" {}

		[Space(10)]
		_Color ("Color", Color) = (0.2, 0.2, 1.0, 1.0)

		[Space(10)]
		_BaseHeight ("Base Height", Range(0.0, 100.0)) = 10
		_Height ("Height", Range(0.0, 100.0)) = 50
		_Density ("Density", Range(0.0, 0.1)) = .01
		_DensityFalloff ("Density Falloff", Range(0.01, 15.0)) = 2.0

		[Space(10)]
		_NoiseAmount ("Noise Amount", Range(0.0, 2.0)) = .05
		_NoiseCoords ("Noise Coords", Vector) = (0, 0, 0, 0)

		[Space(10)]
		_MaxSamples ("Max Samples", Range(1, 64)) = 16

		[Space(10)]
		_SunPos ("SunPos", Vector) = (1000, 0, 0, 0) 
		_PlanetPos ("PlanetPos", Vector) = (0, 0, 0, 0)
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	static float PI = 3.141592653589793238462;

	sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	sampler2D _Noise;
	float4 _NoiseCoords;
	
	uniform sampler2D_float _CameraDepthTexture;

	// for fast world space reconstruction
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;

	float4 _Color;

	float _BaseHeight;
	float _Height;
	#define TotalHeight (_BaseHeight + _Height)

	float _Density;
	float _DensityFalloff;

	float _NoiseAmount;
	float _NoiseTiling;

	int _MaxSamples;

	float4 _SunPos;
	float4 _PlanetPos;

	struct appdata_fog
	{
		float4 vertex : POSITION;
		half2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
	};
	
	v2f vert (appdata_fog v)
	{
		v2f o;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1.0-o.uv.y;
		#endif				
		
		int frustumIndex = v.texcoord.x + (2.0 * o.uv.y);
		o.interpolatedRay = _FrustumCornersWS[frustumIndex];
		o.interpolatedRay.w = frustumIndex;
		
		return o;
	}

	float ScatterAmount (float wavelength, float angle, float height)
	{
		
	}

	float ComputeDistanceInside (float angle, float height)
	{
		float centerAngle = PI - (asin(height * sin(angle) / TotalHeight) + angle);
		return sqrt(TotalHeight * TotalHeight + height * height - 2.0 * TotalHeight * height * cos(centerAngle)); 
	}

	float RaySample (float3 pos, float distPerSegment)
	{
		float sampleHeight = length(pos);
		float sampleHeightPercent = saturate((sampleHeight - _BaseHeight) / _Height);
		float blocking = _Density * exp(-sampleHeightPercent * _DensityFalloff) * distPerSegment;

		return blocking;
	}

	half4 ComputeFog (v2f i) : SV_Target
	{
		half4 sceneColor = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
//		float2 noiseAnim = float2(_Time.x * 2.13251, _Time.x * -3.123213);
//		float noise = tex2D(_Noise, (UnityStereoTransformScreenSpaceTex(i.uv) + noiseAnim) * _NoiseCoords.xy * _ScreenParams.xy).a;
		float noise = tex2D(_Noise, UnityStereoTransformScreenSpaceTex(i.uv) * _NoiseCoords.xy * _ScreenParams.xy).a;
		noise = mad(noise, 2.0, -1.0) * _NoiseAmount;


		// Reconstruct world space position & direction
		// towards this screen pixel.
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv_depth));
		float dpth = Linear01Depth(rawDepth);

		float3 camOffset = _CameraWS.xyz - _PlanetPos.xyz;
		float camHeight = length(camOffset);
		float3 camOffsetDir = camOffset / camHeight;
		float3 sunDir = _SunPos.xyz - _CameraWS.xyz;

		float4 wsDir;
		float dist;

		if(camHeight >= TotalHeight)
		{
			if(dpth >= .9999f)
			{

			}
			else
			{

			}
		}
		else
		{
			//ray doesn't intersect the planet
			if(dpth >= .9999f)
			{
				float4 normRay = normalize(i.interpolatedRay);
				float viewDot = dot(-normRay.xyz, camOffsetDir);
				float angle = acos(viewDot);
				dist = ComputeDistanceInside(angle, camHeight);
				wsDir = dist * normRay;
			}
			else
			{
				wsDir = dpth * i.interpolatedRay;
				dist = length(wsDir);
			}
		}
		float3 endPointPos = _CameraWS.xyz + wsDir.xyz;
//		dist -= _ProjectionParams.y;
		

		float samplePercent = dist / (TotalHeight * 1.5);
		int samples = max(samplePercent * _MaxSamples, 1);

		float3 jumpVector = (endPointPos - _CameraWS.xyz) / (samples + 1.0);
		float jumpDist = length(jumpVector);
		float3 samplePoint = _CameraWS.xyz;
		float opticalDepth = 0.0;
		for (int i = 0; i < samples; i++)
		{
			samplePoint += jumpVector * (noise + 1.0);
			opticalDepth += RaySample(samplePoint, jumpDist * (noise + 1.0));
			noise *= -1.0;
		}

		opticalDepth = saturate(opticalDepth);

		// Compute fog amount
//		half fogFac = ComputeFogFactor (max(0.0,dist));
		
		// Lerp between fog color & original scene color
		// by fog amount
		return lerp (sceneColor, _Color, opticalDepth);
	}

ENDCG


	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag



			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = ComputeFog(i);

				return col;
			}
			ENDCG
		}
	}
}
