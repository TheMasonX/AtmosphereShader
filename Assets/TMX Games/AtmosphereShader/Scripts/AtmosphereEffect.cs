using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Security.Cryptography;

[ExecuteInEditMode]
[AddComponentMenu("TMX Games/Atmosphere")]
[RequireComponent(typeof(Camera))]
public class AtmosphereEffect : PostEffectsBase
{
	public Shader atmosphereShader = null;
	public Material atmosphereMaterial = null;

	[Space(10)]
	public Color color = new Color(0.4f, 0.4f, 1.0f, 1.0f);

	[Space(10)]
	[Range(0f, 100f)]
	public float BaseHeight = 50f;
	[Range(0f, 100f)]
	public float Height = 50f;
	[Range(0f, 0.5f)]
	public float Density = .01f;
	[Range(0.01f, 15f)]
	public float DensityFalloff = 2f;

	[Space(10)]
	public Texture2D Noise;
	[Range(0f, 2.0f)]
	public float NoiseAmount = 0.1f;

	[Range(1, 64)]
	public int MaxSamples = 16;

	[Space(10)]
	public Vector4 SunPos = new Vector4(1000, 0, 0, 0);
	public Vector4	PlanetPos = new Vector4(0, 0, 0, 0);

	public override bool CheckResources ()
	{
		CheckSupport (true);

		atmosphereMaterial = CheckShaderAndCreateMaterial (atmosphereShader, atmosphereMaterial);

//		if (!isSupported)
//			ReportAutoDisable ();
		return isSupported;
	}

	[ImageEffectAllowedInSceneView]
	[ImageEffectOpaque]
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (CheckResources() == false)
		{
			Graphics.Blit(source, destination);
			return;
		}

		Camera cam = GetComponent<Camera>();
		Transform camtr = cam.transform;

		Vector3[] frustumCorners = new Vector3[4];
		cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, cam.stereoActiveEye, frustumCorners);
		var bottomLeft = camtr.TransformVector(frustumCorners[0]);
		var topLeft = camtr.TransformVector(frustumCorners[1]);
		var topRight = camtr.TransformVector(frustumCorners[2]);
		var bottomRight = camtr.TransformVector(frustumCorners[3]);

		Matrix4x4 frustumCornersArray = Matrix4x4.identity;
		frustumCornersArray.SetRow(0, bottomLeft);
		frustumCornersArray.SetRow(1, bottomRight);
		frustumCornersArray.SetRow(2, topLeft);
		frustumCornersArray.SetRow(3, topRight);

		var camPos = camtr.position;
		atmosphereMaterial.SetMatrix("_FrustumCornersWS", frustumCornersArray);
		atmosphereMaterial.SetVector("_CameraWS", camPos);

		atmosphereMaterial.SetFloat("_BaseHeight", BaseHeight);
		atmosphereMaterial.SetFloat("_Height", Height);
		atmosphereMaterial.SetFloat("_Density", Density);
		atmosphereMaterial.SetFloat("_DensityFalloff", DensityFalloff);
		atmosphereMaterial.SetFloat("_NoiseAmount", NoiseAmount);
		atmosphereMaterial.SetInt("_MaxSamples", MaxSamples);

		atmosphereMaterial.SetVector("_SunPos", SunPos);
		atmosphereMaterial.SetVector("_PlanetPos", PlanetPos);

		if (Noise != null)
		{
			atmosphereMaterial.SetVector("_NoiseCoords", new Vector4(1f / Noise.width, 1f / Noise.height, 0f, 0f));
			atmosphereMaterial.SetTexture("_Noise", Noise);
		}

		int pass = 0;
		Graphics.Blit(source, destination, atmosphereMaterial, pass);
	}
}
