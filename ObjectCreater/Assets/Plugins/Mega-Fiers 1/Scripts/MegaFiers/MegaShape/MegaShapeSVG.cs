
using UnityEngine;
using System;
using System.Collections.Generic;
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
using System.Text.RegularExpressions;
#endif

// Part of MegaShape?
public class MegaShapeSVG
{
	public void LoadXML(string svgdata, MegaShape shape, bool clear, int start)
	{
		MegaXMLReader xml = new MegaXMLReader();
		MegaXMLNode node = xml.read(svgdata);

		if ( !clear )
			shape.splines.Clear();

		shape.selcurve = start;
		splineindex = start;
		ParseXML(node, shape);
	}

	int splineindex = 0;

	public void ParseXML(MegaXMLNode node, MegaShape shape)
	{
		foreach ( MegaXMLNode n in node.children )
		{
			switch ( n.tagName )
			{
				case "circle":	ParseCircle(n, shape); break;
				case "path": ParsePath(n, shape); break;
				case "ellipse": ParseEllipse(n, shape); break;
				case "rect": ParseRect(n, shape); break;
				case "polygon": ParsePolygon(n, shape); break;
				default:	break;
			}

			ParseXML(n, shape);
		}
	}

	MegaSpline GetSpline(MegaShape shape)
	{
		MegaSpline spline;

		if ( splineindex < shape.splines.Count )
			spline = shape.splines[splineindex];
		else
		{
			spline = new MegaSpline();
			shape.splines.Add(spline);
		}

		splineindex++;
		return spline;
	}

	Vector3 SwapAxis(Vector3 val, MegaAxis axis)
	{
		float v = 0.0f;
		switch ( axis )
		{
			case MegaAxis.X:
				v = val.x;
				val.x = val.y;
				val.y = v;
				break;

			case MegaAxis.Y:
				break;

			case MegaAxis.Z:
				v = val.y;
				val.y = val.z;
				val.z = v;
				break;
		}

		return val;
	}

	void AddKnot(MegaSpline spline, Vector3 p, Vector3 invec, Vector3 outvec, MegaAxis axis)
	{
		spline.AddKnot(SwapAxis(p, axis), SwapAxis(invec, axis), SwapAxis(outvec, axis));
	}


	void ParseCircle(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);

		float cx = 0.0f;
		float cy = 0.0f;
		float r = 0.0f;

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "cx": cx = float.Parse(val.value); break;
				case "cy": cy = float.Parse(val.value); break;
				case "r": r = float.Parse(val.value); break;
			}
		}

		float vector = CIRCLE_VECTOR_LENGTH * r;

		spline.knots.Clear();
		for ( int ix = 0; ix < 4; ++ix )
		{
			float angle = (Mathf.PI * 2.0f) * (float)ix / (float)4;
			float sinfac = Mathf.Sin(angle);
			float cosfac = Mathf.Cos(angle);
			Vector3 p = new Vector3((cosfac * r) + cx, 0.0f, (sinfac * r) + cy);
			Vector3 rotvec = new Vector3(sinfac * vector, 0.0f, -cosfac * vector);
			//spline.AddKnot(p, p + rotvec, p - rotvec);
			AddKnot(spline, p, p + rotvec, p - rotvec, shape.axis);
		}

		spline.closed = true;
	}

	void ParseEllipse(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);

		float cx = 0.0f;
		float cy = 0.0f;
		float rx = 0.0f;
		float ry = 0.0f;

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "cx": cx = float.Parse(val.value); break;
				case "cy": cy = float.Parse(val.value); break;
				case "rx": rx = float.Parse(val.value); break;
				case "ry": ry = float.Parse(val.value); break;
			}
		}

		ry = Mathf.Clamp(ry, 0.0f, float.MaxValue);
		rx = Mathf.Clamp(rx, 0.0f, float.MaxValue);

		float radius, xmult, ymult;
		if ( ry < rx )
		{
			radius = rx;
			xmult = 1.0f;
			ymult = ry / rx;
		}
		else
		{
			if ( rx < ry )
			{
				radius = ry;
				xmult = rx / ry;
				ymult = 1.0f;
			}
			else
			{
				radius = ry;
				xmult = ymult = 1.0f;
			}
		}


		float vector = CIRCLE_VECTOR_LENGTH * radius;

		Vector3 mult = new Vector3(xmult, ymult, 1.0f);

		for ( int ix = 0; ix < 4; ++ix )
		{
			float angle = 6.2831853f * (float)ix / 4.0f;
			float sinfac = Mathf.Sin(angle);
			float cosfac = Mathf.Cos(angle);
			Vector3 p = new Vector3(cosfac * radius + cx, 0.0f, sinfac * radius + cy);
			Vector3 rotvec = new Vector3(sinfac * vector, 0.0f, -cosfac * vector);
			//spline.AddKnot(Vector3.Scale(p, mult), Vector3.Scale((p + rotvec), mult), Vector3.Scale((p - rotvec), mult));	//, tm);
			AddKnot(spline, Vector3.Scale(p, mult), Vector3.Scale((p + rotvec), mult), Vector3.Scale((p - rotvec), mult), shape.axis);	//, tm);
		}

		spline.closed = true;
	}


	void ParseRect(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);
		Vector3[] ppoints = new Vector3[4];

		float w = 0.0f;
		float h = 0.0f;
		float x = 0.0f;
		float y = 0.0f;

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "x": x = float.Parse(val.value); break;
				case "y": y = float.Parse(val.value); break;
				case "width": w = float.Parse(val.value); break;
				case "height": h = float.Parse(val.value); break;
				case "transform": Debug.Log("SVG Transform not implemented yet");
					break;
			}
		}

		ppoints[0] = new Vector3(x, 0.0f, y);
		ppoints[1] = new Vector3(x, 0.0f, y + h);
		ppoints[2] = new Vector3(x + w, 0.0f, y + h);
		ppoints[3] = new Vector3(x + w, 0.0f, y);

		spline.closed = true;
		spline.knots.Clear();
		//spline.AddKnot(ppoints[0], ppoints[0], ppoints[0]);
		//spline.AddKnot(ppoints[1], ppoints[1], ppoints[1]);
		//spline.AddKnot(ppoints[2], ppoints[2], ppoints[2]);
		//spline.AddKnot(ppoints[3], ppoints[3], ppoints[3]);
		AddKnot(spline, ppoints[0], ppoints[0], ppoints[0], shape.axis);
		AddKnot(spline, ppoints[1], ppoints[1], ppoints[1], shape.axis);
		AddKnot(spline, ppoints[2], ppoints[2], ppoints[2], shape.axis);
		AddKnot(spline, ppoints[3], ppoints[3], ppoints[3], shape.axis);
	}

	void ParsePolygon(MegaXMLNode node, MegaShape shape)
	{
		MegaSpline spline = GetSpline(shape);

		spline.knots.Clear();
		spline.closed = true;

		char[] charSeparators = new char[] { ' ' };

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "points":

					string[] coordinates = val.value.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

					for ( int j = 0; j < coordinates.Length; j++ )
					{
						Vector3 p = ParseV2Split(coordinates[j], 0);

						MegaKnot k = new MegaKnot();
						k.p = SwapAxis(new Vector3(p.x, 0.0f, p.y), shape.axis);
						k.invec = k.p;
						k.outvec = k.p;
						spline.knots.Add(k);
					}

					break;
			}
		}

		if ( spline.closed )
		{
			Vector3 delta1 = spline.knots[0].outvec - spline.knots[0].p;
			spline.knots[0].invec = spline.knots[0].p - delta1;
		}
	}

	char[] commaspace = new char[] { ',', ' ' };

	void ParsePath(MegaXMLNode node, MegaShape shape)
	{
		Vector3 cp = Vector3.zero;
		Vector2 cP1;
		Vector2 cP2;

		char[] charSeparators = new char[] { ',', ' ' };

		MegaSpline spline = null;
		MegaKnot k;
		string[] coord;

		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaXMLValue val = node.values[i];

			//Debug.Log("val name " + val.name);

			switch ( val.name )
			{
				case "d":
#if UNITY_FLASH
					string[] coordinates = null;	//string.Split(val.value, @"(?=[MmLlCcSsZzHhVv])");
#else
					string[] coordinates = Regex.Split(val.value, @"(?=[MmLlCcSsZzHhVv])");
#endif

					for ( int j = 0; j < coordinates.Length; j++ )
					{
						if ( coordinates[j].Length > 0 )
						{
							string v = coordinates[j].Substring(1);
							if ( v != null && v.Length > 0 )
							{
								v = v.Replace("-", ",-");

								while ( v.Length > 0 && (v[0] == ',' || v[0] == ' ') )
									v = v.Substring(1);
							}

							switch ( coordinates[j][0] )
							{
								case 'Z':
								case 'z':
									if ( spline != null )
									{
										spline.closed = true;
#if false
										Vector3 delta1 = spline.knots[0].outvec - spline.knots[0].p;
										spline.knots[0].invec = spline.knots[0].p - delta1;

										if ( spline.knots[0].p == spline.knots[spline.knots.Count - 1].p )
											spline.knots.Remove(spline.knots[spline.knots.Count - 1]);
#else
										int kc = spline.knots.Count - 1;
										spline.knots[0].invec = spline.knots[kc].invec;
										spline.knots.Remove(spline.knots[kc]);

#endif
									}
									break;

								case 'M':
									spline = GetSpline(shape);
									spline.knots.Clear();

									cp = ParseV2Split(v, 0);
									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = k.p;
									k.outvec = k.p;
									spline.knots.Add(k);
									break;

								case 'm':
									spline = GetSpline(shape);
									spline.knots.Clear();

									//Debug.Log("v: " + v);
									coord = v.Split(" "[0]);
									//Debug.Log("m coords " + coord.Length);
									//Debug.Log("v2 " + coord[0]);
									for ( int k0 = 0; k0 < coord.Length - 1; k0 = k0 + 1 )
									{
										//Debug.Log("v2 " + coord[k0]);
										Vector3 cp1 = ParseV2Split(coord[k0], 0);
										//Debug.Log("cp1 " + cp1);

										cp.x += cp1.x;	//ParseV2Split(coord[k0], 0);	// ParseV2(coord, k0);
										cp.y += cp1.y;

										k = new MegaKnot();
										k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
										k.invec = k.p;
										k.outvec = k.p;
										spline.knots.Add(k);

									}

#if false
									Vector3 cp1 = ParseV2Split(v, 0);
									cp.x += cp1.x;
									cp.y += cp1.y;
									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = k.p;
									k.outvec = k.p;
									spline.knots.Add(k);
#endif
									break;

								case 'l':
									coord = v.Split(","[0]);
									for ( int k0 = 0; k0 < coord.Length; k0 = k0 + 2 )
										cp += ParseV2(coord, k0);

									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'c':
									coord = v.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

									for ( int k2 = 0; k2 < coord.Length; k2 += 6 )
									{
										cP1 = cp + ParseV2(coord, k2);
										cP2 = cp + ParseV2(coord, k2 + 2);
										cp += ParseV2(coord, k2 + 4);

										spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cP1.x, 0.0f, cP1.y), shape.axis);

										k = new MegaKnot();
										k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
										k.invec = SwapAxis(new Vector3(cP2.x, 0.0f, cP2.y), shape.axis);
										k.outvec = k.p - (k.invec - k.p);
										spline.knots.Add(k);
									}
									break;

								case 'L':
									coord = v.Split(","[0]);
									for ( int k3 = 0; k3 < coord.Length; k3 = k3 + 2 )
										cp = ParseV2(coord, k3);

									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'v':
									//Debug.Log("v: " + v);
									coord = v.Split(","[0]);
									for ( int k4 = 0; k4 < coord.Length; k4++ )
										cp.y += float.Parse(coord[k4]);
									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'V':
									coord = v.Split(","[0]);
									for ( int k9 = 0; k9 < coord.Length; k9++ )
										cp.y = float.Parse(coord[k9]);

									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'h':
									coord = v.Split(","[0]);
									for ( int k5 = 0; k5 < coord.Length; k5++ )
										cp.x += float.Parse(coord[k5]);

									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'H':
									coord = v.Split(","[0]);
									for ( int k6 = 0; k6 < coord.Length; k6++ )
										cp.x = float.Parse(coord[k6]);

									spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);

									k = new MegaKnot();
									k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.invec = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
									k.outvec = k.p - (k.invec - k.p);
									spline.knots.Add(k);

									break;

								case 'S':
									coord = v.Split(","[0]);
									for ( int k7 = 0; k7 < coord.Length; k7 = k7 + 4 )
									{
										cp = ParseV2(coord, k7 + 2);
										cP1 = ParseV2(coord, k7);
										k = new MegaKnot();
										k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
										k.invec = SwapAxis(new Vector3(cP1.x, 0.0f, cP1.y), shape.axis);
										k.outvec = k.p - (k.invec - k.p);
										spline.knots.Add(k);
									}
									break;

								case 's':
									coord = v.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

									for ( int k7 = 0; k7 < coord.Length; k7 = k7 + 4 )
									{
										cP1 = cp + ParseV2(coord, k7);
										cp += ParseV2(coord, k7 + 2);

										k = new MegaKnot();
										k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
										k.invec = SwapAxis(new Vector3(cP1.x, 0.0f, cP1.y), shape.axis);
										k.outvec = k.p - (k.invec - k.p);
										spline.knots.Add(k);
									}
									break;

								case 'C':
									coord = v.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

									for ( int k2 = 0; k2 < coord.Length; k2 += 6 )
									{
										cP1 = ParseV2(coord, k2);
										cP2 = ParseV2(coord, k2 + 2);
										cp = ParseV2(coord, k2 + 4);

										spline.knots[spline.knots.Count - 1].outvec = SwapAxis(new Vector3(cP1.x, 0.0f, cP1.y), shape.axis);

										k = new MegaKnot();
										k.p = SwapAxis(new Vector3(cp.x, 0.0f, cp.y), shape.axis);
										k.invec = SwapAxis(new Vector3(cP2.x, 0.0f, cP2.y), shape.axis);
										k.outvec = k.p - (k.invec - k.p);
										spline.knots.Add(k);
									}
									break;

								default:
									break;
							}
						}
					}
					break;
			}
		}
	}

	public void importData(string svgdata, MegaShape shape, float scale, bool clear, int start)
	{
		LoadXML(svgdata, shape, clear, start);
		for ( int i = start; i < splineindex; i++ )
		{
			float area = shape.splines[i].Area();
			if ( area < 0.0f )
				shape.splines[i].reverse = false;
			else
				shape.splines[i].reverse = true;
		}

		//shape.Centre(0.01f, new Vector3(-1.0f, 1.0f, 1.0f));
		shape.Centre(scale, new Vector3(-1.0f, 1.0f, 1.0f), start);
		shape.CalcLength();	//10);
	}

	const float CIRCLE_VECTOR_LENGTH = 0.5517861843f;

	Vector2 ParseV2Split(string str, int i)
	{
		return ParseV2(str.Split(commaspace, StringSplitOptions.RemoveEmptyEntries), i);
	}

	Vector3 ParseV2(string[] str, int i)
	{
		Vector3 p = Vector2.zero;

		p.x = float.Parse(str[i]);
		p.y = float.Parse(str[i + 1]);
		return p;
	}
}
