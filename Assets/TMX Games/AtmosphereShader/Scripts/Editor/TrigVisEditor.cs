using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrigVisualizer))]
public class TrigVisEditor : Editor
{
	Color camColor = Color.cyan;
	Color aboveCamColor = Color.blue;
	Color circleColor = Color.white;
	Color endPointColor = Color.red;
	Color distColor = Color.white;
	Color centerToEndColor = Color.green;
	Color aboveToEndColor = Color.magenta;

	TrigVisualizer script;

	GUIStyle style;

	void OnSceneGUI ()
	{
		script = target as TrigVisualizer;

		style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.MiddleCenter;
		style.fontStyle = FontStyle.Bold;
		style.richText = true;

		Vector3 center = script.transform.position;
		Vector3 cam = center + Vector3.forward * script.radius * script.height;
		Vector3 aboveCam = center + Vector3.forward * script.radius;
		Vector3 angle = new Vector3(-Mathf.Sin(script.radAngle), 0f, -Mathf.Cos(script.radAngle));
		float dist = Dist(script.radAngle, script.height * script.radius, script.radius);
		Vector3 endPoint = cam + angle * dist;
//		Vector3 endPoint = cam + angle * script.radius;

		//Draw Circle Perimeter
		Handles.color = circleColor;
		Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, script.radius);

		//Draw From Center To Camera
		Handles.color = camColor;
		Handles.DrawLine(center, cam);

		//Draw From Camera To Edge Above Camera
		Handles.color = aboveCamColor;
		Handles.DrawLine(cam, aboveCam);

		//Draw From Camera To End Point
		Handles.color = endPointColor;
		Handles.DrawLine(cam, endPoint);

		//Draw From Center To End Point
		Handles.color = centerToEndColor;
		Handles.DrawLine(center, endPoint);

		//Draw From Center To End Point
		Handles.color = aboveToEndColor;
		Handles.DrawLine(aboveCam, endPoint);
		 
		DrawVertex(center, circleColor, "Center");
		DrawVertex(cam, camColor, "Camera");
		DrawVertex(aboveCam, aboveCamColor, "Above Camera");
		DrawVertex(endPoint, endPointColor, "End Point");

		DrawAngle(center, circleColor, endPoint.normalized, cam.normalized);
		DrawAngle(cam, camColor, (endPoint - cam).normalized, -cam.normalized);
		DrawAngle(endPoint, endPointColor, -endPoint.normalized, (cam - endPoint).normalized);
		DrawAngle(endPoint, endPointColor.Invert(), (cam - endPoint).normalized, (aboveCam - endPoint).normalized);
//		DrawAngle(endPoint, endPointColor, -endPoint.normalized, (aboveCam - endPoint).normalized);
		DrawAngle(aboveCam, aboveCamColor, -aboveCam.normalized, (endPoint - aboveCam).normalized);



		LabelDist(endPoint, cam, distColor);
		LabelDist(endPoint, aboveCam, distColor);
		LabelDist(cam, aboveCam, distColor);
		LabelDist(center, cam, distColor);
	}

//	void OnSceneGUI ()
//	{
//		script = target as TrigVisualizer;
//
//		style = new GUIStyle(GUI.skin.label);
//		style.alignment = TextAnchor.MiddleCenter;
//		style.fontStyle = FontStyle.Bold;
//		style.richText = true;
//
//		Vector3 center = script.transform.position;
//		Vector3 cam = center + Vector3.forward * script.radius * script.height;
//		Vector3 aboveCam = center + Vector3.forward * script.radius;
//		Vector3 angle = new Vector3(-Mathf.Sin(script.radAngle), 0f, Mathf.Cos(script.radAngle));
//		Vector3 centerAngle = new Vector3(-Mathf.Sin(script.radCenterAngle), 0f, Mathf.Cos(script.radCenterAngle));
//		Vector3 endPoint = center + centerAngle * script.radius;
//		//		Vector3 endPoint = cam + angle * script.radius;
//
//		//Draw Circle Perimeter
//		Handles.color = circleColor;
//		Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, script.radius);
//
//		//Draw From Center To Camera
//		Handles.color = camColor;
//		Handles.DrawLine(center, cam);
//
//		//Draw From Camera To Edge Above Camera
//		Handles.color = aboveCamColor;
//		Handles.DrawLine(cam, aboveCam);
//
//		//Draw From Camera To End Point
//		Handles.color = endPointColor;
//		Handles.DrawLine(cam, endPoint);
//
//		//Draw From Center To End Point
//		Handles.color = centerToEndColor;
//		Handles.DrawLine(center, endPoint);
//
//		//Draw From Center To End Point
//		Handles.color = aboveToEndColor;
//		Handles.DrawLine(aboveCam, endPoint);
//
//		DrawVertex(center, circleColor, "Center");
//		DrawVertex(cam, camColor, "Camera");
//		DrawVertex(aboveCam, aboveCamColor, "Above Camera");
//		DrawVertex(endPoint, endPointColor, "End Point");
//
//		DrawAngle(center, circleColor, endPoint.normalized, cam.normalized);
//		DrawAngle(cam, camColor, (endPoint - cam).normalized, -cam.normalized);
//		DrawAngle(endPoint, endPointColor, -endPoint.normalized, (cam - endPoint).normalized);
//		DrawAngle(endPoint, endPointColor.Invert(), (cam - endPoint).normalized, (aboveCam - endPoint).normalized);
//		//		DrawAngle(endPoint, endPointColor, -endPoint.normalized, (aboveCam - endPoint).normalized);
//		DrawAngle(aboveCam, aboveCamColor, -aboveCam.normalized, (endPoint - aboveCam).normalized);
//
//		float a = Vector3.Angle((endPoint - cam), -cam) * Mathf.Deg2Rad;
//		float c = Dist(a, script.height * script.radius, script.radius);
//		Debug.Log(a + " | " + c);
//
//		LabelDist(endPoint, cam, distColor);
//		LabelDist(endPoint, aboveCam, distColor);
//		LabelDist(cam, aboveCam, distColor);
//		LabelDist(center, cam, distColor);
//	}

	public void DrawVertex (Vector3 pos, Color col, string label)
	{
		col.a = script.vertexOpacity;
		style.fontSize = script.vertexLabelSize;

		Handles.color = col;
		Handles.DrawSolidDisc(pos, Vector3.up, script.vertexSize);
		Handles.Label(pos, label.Colorize(col), style);
	}

	public void DrawAngle (Vector3 pos, Color col, Vector3 a, Vector3 b)
	{
		style.fontSize = script.angleLabelSize;

		float angle = Vector3.SignedAngle(a, b, Vector3.up);
		col.a = script.angleOpacity;
		Handles.color = col;
		Handles.DrawWireArc(pos, Vector3.up, a, angle, script.angleSize);
		Handles.Label(pos + (a + b).normalized * script.angleSize * script.angleLabelOffset, Mathf.Abs(angle).Degree("F2").Colorize(col), style);
	}

	public void LabelDist (Vector3 a, Vector3 b, Color col)
	{
		style.fontSize = script.distLabelSize;
		Handles.Label((a + b) * .5f, (a - b).magnitude.Colorize(col, "F3"), style);
	}

	public float Dist (float angle, float height, float radius)
	{
		float centerAngle = Mathf.PI - (Mathf.Asin(height * Mathf.Sin(angle) / radius) + angle);
		return Mathf.Sqrt(radius * radius + height * height - 2f * radius * height * Mathf.Cos(centerAngle)); 
	}
}