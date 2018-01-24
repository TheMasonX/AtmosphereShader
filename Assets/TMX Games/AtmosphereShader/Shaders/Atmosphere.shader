Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		[Space(10)]
		_Color ("Color", Color) = (0.2, 0.2, 1.0, 1.0)

		[Space(10)]
		_BaseHeight ("Base Height", Range(0.0, 100.0)) = 10
		_Height ("Height", Range(0.0, 100.0)) = 50
		_Density ("Density", Range(0.0, 0.1)) = .01
		_DensityFalloff ("Density Falloff", Range(0.01, 5.0)) = 2.0

		[Space(10)]
		_SunPos ("SunPos", Vector) = (1000, 0, 0, 0) 
		_PlanetPos ("PlanetPos", Vector) = (0, 0, 0, 0)
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

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


	float4 _SunPos;
	float4 _PlanetPos;

	struct appdata_fog
	{
		float4 vertex : POSITION;
		half2 texcoord : TEXCOORD0;
	};

	struct v2f {
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
			o.uv.y = 1-o.uv.y;
		#endif				
		
		int frustumIndex = v.texcoord.x + (2 * o.uv.y);
		o.interpolatedRay = _FrustumCornersWS[frustumIndex];
		o.interpolatedRay.w = frustumIndex;
		
		return o;
	}
	
	// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
	half ComputeFogFactor (float coord)
	{
		float fogFac = 0.0;
		fogFac = _Density * 1.2011224087f * coord;
		fogFac = exp2(-fogFac*fogFac);
		return saturate(fogFac);
	}

	// Distance-based fog
	float ComputeDistance (float3 camDir, float zdepth)
	{
		float dist = length(camDir);
		dist -= _ProjectionParams.y;
		return dist;
	}

	half4 ComputeFog (v2f i) : SV_Target
	{
		half4 sceneColor = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
		
		// Reconstruct world space position & direction
		// towards this screen pixel.
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv_depth));
		float dpth = Linear01Depth(rawDepth);
		//ray doesn't intersect the planet
		if(dpth >= .9999f)
		{
			
		}

		float3 camOffset = _CameraWS.xyz - _PlanetPos;
		float camHeight = length(camOffset);
		float3 sunDir = _SunPos - _CameraWS.xyz;

		float4 wsDir = dpth * i.interpolatedRay;
		float4 endPointPos = _CameraWS + wsDir;

		// Compute fog distance
		float g = ComputeDistance (wsDir, dpth);

		// Compute fog amount
		half fogFac = ComputeFogFactor (max(0.0,g));
		
		// Lerp between fog color & original scene color
		// by fog amount
		return lerp (_Color, sceneColor, fogFac);
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
