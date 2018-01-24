using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrigVisualizer : MonoBehaviour
{
	public float radius = 5f;
	[Range(0f, 1f)]
	public float height = .8f;
	[Range(0f, 180f)]
	public float angle = 90f;
	public float radAngle { get { return angle * Mathf.Deg2Rad; } }

	private float heightAbove { get { return 1f - height; } }

	[Space(10f)]
	[Range(0f, .2f)]
	public float vertexSize = .1f;
	[Range(0f, 1f)]
	public float vertexOpacity = .8f;
	[Range(1, 20)]
	public int vertexLabelSize = 8;

	[Space(10f)]
	[Range(0f, 2f)]
	public float angleSize = .5f;
	[Range(.5f, 2f)]
	public float angleLabelOffset = 1.5f;
	[Range(0f, 1f)]
	public float angleOpacity = .8f;
	[Range(1, 20)]
	public int angleLabelSize = 8;

	[Range(1, 20)]
	public int distLabelSize = 8;

}