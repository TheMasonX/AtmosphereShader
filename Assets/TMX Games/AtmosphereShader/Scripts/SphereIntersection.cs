using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereIntersection : MonoBehaviour
{
	public Transform dummyCamera;
	public Transform dummyPlanet;
	public float radius;

	[ContextMenu("Run Test")]
	public void RunTest ()
	{
		dummyPlanet.localScale = Vector3.one * radius * 2f;
		float i1 = 0f;
		float i2 = 0f;

		bool hit = rayIntersect(dummyCamera.position, dummyCamera.forward, dummyPlanet.position, radius, out i1, out i2);
		if (hit)
		{
			Vector3 hit1 = dummyCamera.position + dummyCamera.forward * i1;
			Vector3 hit2 = dummyCamera.position + dummyCamera.forward * i2;
			Debug.DrawLine(dummyCamera.position, hit1, Color.blue, 2f);
			Debug.DrawLine(hit1, hit2, Color.red, 2f);
			Debug.DrawRay(hit2, dummyCamera.forward * 3f, Color.white, 2f);
		}
		Debug.LogFormat("{0} | I1: {1}, I2 {2}", hit ? "BULLSEYE! RAY HIT!" : "DARN, RAY MISSED...", i1, i2);
	}


	bool rayIntersect
	(
		// Ray
		Vector3 O, // Origin
		Vector3 D, // Direction

		// Sphere
		Vector3 C, // Centre
		float R, // Radius
		out float AO, // First intersection time
		out float BO  // Second intersection time
	)
	{
		AO = 0f;
		BO = 0f;
		Vector3 L = C - O;
		float DT = Vector3.Dot(L, D);
		float R2 = R * R;

		float CT2 = Vector3.Dot(L,L) - DT*DT;

		// Intersection point outside the circle
		if (CT2 > R2)
			return false;

		float AT = Mathf.Sqrt(R2 - CT2);
		float BT = AT;

		AO = DT - AT;
		BO = DT + BT;
		return true;
	}
}
