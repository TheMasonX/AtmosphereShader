using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
	public static string Colorize (this string s, Color col)
	{
		return string.Format("<color=#{0}>{1}</color>", col.ToHex(), s);
	}

	public static string Colorize (this float f, Color col, string format = "")
	{
		return f.ToString(format).Colorize(col);
	}

	public static string Colorize (this object o, Color col)
	{
		return o.ToString().Colorize(col);
	}

	// Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.


	public static string ToHex (this Color color)
	{
		Color32 c32 = color;
		string hex = c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2");
		return hex;
	}

	public static Color HexToColor (string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}

	public static Color Invert (this Color c, bool invertAlpha = false)
	{
		return new Color(1f - c.r, 1f - c.g, 1f - c.b, invertAlpha ? (1f - c.a) : c.a);
	}

	const string degreeSymbol = "\u00B0";

	public static string Degree (this string s)
	{
		return string.Concat(s, degreeSymbol);
	}

	public static string Degree (this float s, string format = "")
	{
		return s.ToString(format) + degreeSymbol;
	}

	public static string Degree (this object s)
	{
		return s.ToString() + degreeSymbol;
	}
}
