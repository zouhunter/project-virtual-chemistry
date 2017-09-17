
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public enum MegaAlter
{
	Offset,
	Scale,
	Both,
}

public enum MegaAffect
{
	X,
	Y,
	Z,
	XY,
	XZ,
	YZ,
	XYZ,
	None,
}

// TODO: define box for effect, origin and sizes, and a falloff

[System.Serializable]
public class MegaSculptCurve
{
	public MegaSculptCurve()
	{
		curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

		offamount = Vector3.one;
		sclamount = Vector3.one;
		axis = MegaAxis.X;
		affectOffset = MegaAffect.Y;
		affectScale = MegaAffect.None;
		enabled = true;
		weight = 1.0f;
		name = "None";
		uselimits = false;
	}

	public AnimationCurve	curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

	public Vector3		offamount = Vector3.one;
	public Vector3		sclamount = Vector3.one;
	public MegaAxis		axis = MegaAxis.X;
	public MegaAffect	affectOffset = MegaAffect.Y;
	public MegaAffect	affectScale = MegaAffect.None;
	public bool			enabled = true;
	public float		weight = 1.0f;
	public string		name = "None";
	public Color		regcol = Color.yellow;

	public Vector3		origin = Vector3.zero;
	public Vector3		boxsize = Vector3.one;

	public bool			uselimits = false;

	public Vector3 size = Vector3.zero;

	static public MegaSculptCurve Create()
	{
		MegaSculptCurve crv = new MegaSculptCurve();
		return crv;
	}
}

[AddComponentMenu("Modifiers/Curve Sculpt Layered")]
public class MegaCurveSculptLayered : MegaModifier
{
	public List<MegaSculptCurve>	curves = new List<MegaSculptCurve>();
	Vector3					size = Vector3.zero;

	public override string ModName() { return "CurveSculpLayered"; }
	public override string GetHelpURL() { return "?page_id=2411"; }

	static object resourceLock = new object();


	public override void DoWork(MegaModifiers mc, int index, int start, int end, int cores)
	{
		//if ( useWeights )

		if ( selection != null )
		{
			DoWorkWeighted(mc, index, start, end, cores);
			return;
		}

		for ( int i = start; i < end; i++ )
			sverts[i] = MapMT(i, verts[i]);
	}

	public override void DoWorkWeighted(MegaModifiers mc, int index, int start, int end, int cores)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = verts[i];

			float w = selection[i];	//[(int)weightChannel];

			if ( w > 0.001f )
			{
				Vector3 mp = MapMT(i, verts[i]);

				sverts[i].x = p.x + (mp.x - p.x) * w;
				sverts[i].y = p.y + (mp.y - p.y) * w;
				sverts[i].z = p.z + (mp.z - p.z) * w;
			}
			else
				sverts[i] = p;	//verts[i];
		}
	}

	public Vector3 MapMT(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		for ( int c = 0; c < curves.Count; c++ )
		{
			MegaSculptCurve crv = curves[c];

			if ( crv.enabled )
			{
				int ax = (int)crv.axis;

				if ( crv.uselimits )
				{
					// Is the point in the box
					Vector3 bp = p - crv.origin;
					if ( Mathf.Abs(bp.x) < crv.size.x && Mathf.Abs(bp.y) < crv.size.y && Mathf.Abs(bp.z) < crv.size.z )
					{
						float alpha = 0.5f + ((bp[ax] / crv.size[ax]) * 0.5f);

						if ( alpha >= 0.0f && alpha <= 1.0f )
						{
							Monitor.Enter(resourceLock);
							float a = crv.curve.Evaluate(alpha) * crv.weight;
							Monitor.Exit(resourceLock);

							switch ( crv.affectScale )
							{
								case MegaAffect.X:
									p.x += bp.x * (a * crv.sclamount.x);
									//p.x *= 1.0f + (a * crv.sclamount.x);
									break;

								case MegaAffect.Y:
									p.y += bp.y * (a * crv.sclamount.y);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									break;

								case MegaAffect.Z:
									p.z += bp.z * (a * crv.sclamount.z);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.XY:
									p.x += bp.x * (a * crv.sclamount.x);
									p.y += bp.y * (a * crv.sclamount.y);
									//p.x *= 1.0f + (a * crv.sclamount.x);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									break;

								case MegaAffect.XZ:
									p.x += bp.x * (a * crv.sclamount.x);
									p.z += bp.z * (a * crv.sclamount.z);
									//p.x *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.YZ:
									p.y += bp.y * (a * crv.sclamount.y);
									p.z += bp.z * (a * crv.sclamount.z);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.XYZ:
									p.x += bp.x * (a * crv.sclamount.x);
									p.y += bp.y * (a * crv.sclamount.y);
									p.z += bp.z * (a * crv.sclamount.z);

									//p.x *= 1.0f + (a * crv.sclamount.x);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;
							}

							switch ( crv.affectOffset )
							{
								case MegaAffect.X:
									p.x += a * crv.offamount.x;
									break;

								case MegaAffect.Y:
									p.y += a * crv.offamount.y;
									break;

								case MegaAffect.Z:
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.XY:
									p.x += a * crv.offamount.x;
									p.y += a * crv.offamount.y;
									break;

								case MegaAffect.XZ:
									p.x += a * crv.offamount.x;
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.YZ:
									p.y += a * crv.offamount.y;
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.XYZ:
									p.x += a * crv.offamount.x;
									p.y += a * crv.offamount.y;
									p.z += a * crv.offamount.z;
									break;
							}
						}
					}
				}
				else
				{
					float alpha = (p[ax] - bbox.min[ax]) / size[ax];
					Monitor.Enter(resourceLock);
					float a = crv.curve.Evaluate(alpha) * crv.weight;
					Monitor.Exit(resourceLock);

					switch ( crv.affectScale )
					{
						case MegaAffect.X:
							p.x *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.Y:
							p.y *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.Z:
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.XY:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.y *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.XZ:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.YZ:
							p.y *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.XYZ:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.y *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;
					}

					switch ( crv.affectOffset )
					{
						case MegaAffect.X:
							p.x += a * crv.offamount.x;
							break;

						case MegaAffect.Y:
							p.y += a * crv.offamount.y;
							break;

						case MegaAffect.Z:
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.XY:
							p.x += a * crv.offamount.x;
							p.y += a * crv.offamount.y;
							break;

						case MegaAffect.XZ:
							p.x += a * crv.offamount.x;
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.YZ:
							p.y += a * crv.offamount.y;
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.XYZ:
							p.x += a * crv.offamount.x;
							p.y += a * crv.offamount.y;
							p.z += a * crv.offamount.z;
							break;
					}
				}
			}
		}

		return invtm.MultiplyPoint3x4(p);
	}



	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		for ( int c = 0; c < curves.Count; c++ )
		{
			MegaSculptCurve crv = curves[c];

			if ( crv.enabled )
			{
				int ax = (int)crv.axis;

				if ( crv.uselimits )
				{
					// Is the point in the box
					Vector3 bp = p - crv.origin;
					if ( Mathf.Abs(bp.x) < crv.size.x && Mathf.Abs(bp.y) < crv.size.y && Mathf.Abs(bp.z) < crv.size.z )
					{
						float alpha = 0.5f + ((bp[ax] / crv.size[ax]) * 0.5f);

						if ( alpha >= 0.0f && alpha <= 1.0f )
						{
							float a = crv.curve.Evaluate(alpha) * crv.weight;

							switch ( crv.affectScale )
							{
								case MegaAffect.X:
									p.x += bp.x * (a * crv.sclamount.x);
									//p.x *= 1.0f + (a * crv.sclamount.x);
									break;

								case MegaAffect.Y:
									p.y += bp.y * (a * crv.sclamount.y);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									break;

								case MegaAffect.Z:
									p.z += bp.z * (a * crv.sclamount.z);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.XY:
									p.x += bp.x * (a * crv.sclamount.x);
									p.y += bp.y * (a * crv.sclamount.y);
									//p.x *= 1.0f + (a * crv.sclamount.x);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									break;

								case MegaAffect.XZ:
									p.x += bp.x * (a * crv.sclamount.x);
									p.z += bp.z * (a * crv.sclamount.z);
									//p.x *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.YZ:
									p.y += bp.y * (a * crv.sclamount.y);
									p.z += bp.z * (a * crv.sclamount.z);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;

								case MegaAffect.XYZ:
									p.x += bp.x * (a * crv.sclamount.x);
									p.y += bp.y * (a * crv.sclamount.y);
									p.z += bp.z * (a * crv.sclamount.z);

									//p.x *= 1.0f + (a * crv.sclamount.x);
									//p.y *= 1.0f + (a * crv.sclamount.y);
									//p.z *= 1.0f + (a * crv.sclamount.z);
									break;
							}

							switch ( crv.affectOffset )
							{
								case MegaAffect.X:
									p.x += a * crv.offamount.x;
									break;

								case MegaAffect.Y:
									p.y += a * crv.offamount.y;
									break;

								case MegaAffect.Z:
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.XY:
									p.x += a * crv.offamount.x;
									p.y += a * crv.offamount.y;
									break;

								case MegaAffect.XZ:
									p.x += a * crv.offamount.x;
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.YZ:
									p.y += a * crv.offamount.y;
									p.z += a * crv.offamount.z;
									break;

								case MegaAffect.XYZ:
									p.x += a * crv.offamount.x;
									p.y += a * crv.offamount.y;
									p.z += a * crv.offamount.z;
									break;
							}
						}
					}
				}
				else
				{
					float alpha = (p[ax] - bbox.min[ax]) / size[ax];
					float a = crv.curve.Evaluate(alpha) * crv.weight;

					switch ( crv.affectScale )
					{
						case MegaAffect.X:
							p.x *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.Y:
							p.y *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.Z:
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.XY:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.y *= 1.0f + (a * crv.sclamount.y);
							break;

						case MegaAffect.XZ:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.YZ:
							p.y *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;

						case MegaAffect.XYZ:
							p.x *= 1.0f + (a * crv.sclamount.y);
							p.y *= 1.0f + (a * crv.sclamount.y);
							p.z *= 1.0f + (a * crv.sclamount.z);
							break;
					}

					switch ( crv.affectOffset )
					{
						case MegaAffect.X:
							p.x += a * crv.offamount.x;
							break;

						case MegaAffect.Y:
							p.y += a * crv.offamount.y;
							break;

						case MegaAffect.Z:
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.XY:
							p.x += a * crv.offamount.x;
							p.y += a * crv.offamount.y;
							break;

						case MegaAffect.XZ:
							p.x += a * crv.offamount.x;
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.YZ:
							p.y += a * crv.offamount.y;
							p.z += a * crv.offamount.z;
							break;

						case MegaAffect.XYZ:
							p.x += a * crv.offamount.x;
							p.y += a * crv.offamount.y;
							p.z += a * crv.offamount.z;
							break;
					}
				}
			}
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		size = bbox.max - bbox.min;

		for ( int i = 0; i < curves.Count; i++ )
		{
			curves[i].size = curves[i].boxsize * 0.5f;	//Vector3.Scale(size, curves[i].boxsize);
		}
		return true;
	}

	public override void DrawGizmo(MegaModContext context)
	{
		base.DrawGizmo(context);

		for ( int i = 0; i < curves.Count; i++ )
		{
			if ( curves[i].enabled && curves[i].uselimits )
			{
				Gizmos.color = curves[i].regcol;	//Color.yellow;
				Gizmos.DrawWireCube(curves[i].origin, curves[i].boxsize);	// * 0.5f);
			}
		}
	}
}