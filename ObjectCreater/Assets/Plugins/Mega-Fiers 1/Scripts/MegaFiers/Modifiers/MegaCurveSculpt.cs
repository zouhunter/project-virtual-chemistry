
using UnityEngine;
using System.Threading;

[AddComponentMenu("Modifiers/Curve Sculpt")]
public class MegaCurveSculpt : MegaModifier
{
	public AnimationCurve	defCurveX = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
	public AnimationCurve	defCurveY = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
	public AnimationCurve	defCurveZ = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

	public AnimationCurve	defCurveSclX = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));
	public AnimationCurve	defCurveSclY = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));
	public AnimationCurve	defCurveSclZ = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));


	public Vector3			OffsetAmount = Vector3.one;
	public Vector3			ScaleAmount = Vector3.one;

	Vector3					size = Vector3.zero;

	public MegaAxis	offsetX = MegaAxis.X;
	public MegaAxis	offsetY = MegaAxis.Y;
	public MegaAxis	offsetZ = MegaAxis.Z;

	public MegaAxis	scaleX = MegaAxis.X;
	public MegaAxis	scaleY = MegaAxis.Y;
	public MegaAxis	scaleZ = MegaAxis.Z;

	public bool		symX = false;
	public bool		symY = false;
	public bool		symZ = false;

	public override string ModName() { return "CurveSculpt"; }
	public override string GetHelpURL() { return "?page_id=655"; }

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
		float alpha = 0.0f;
		p = tm.MultiplyPoint3x4(p);

		alpha = (p.x - bbox.min.x) / size.x;
		Monitor.Enter(resourceLock);

		p[(int)scaleX] *= defCurveSclX.Evaluate(alpha) * ScaleAmount.x;
		p[(int)offsetX] += defCurveX.Evaluate(alpha) * OffsetAmount.x;

		alpha = (p.y - bbox.min.y) / size.y;
		p[(int)scaleY] *= defCurveSclY.Evaluate(alpha) * ScaleAmount.y;
		p[(int)offsetY] += defCurveY.Evaluate(alpha) * OffsetAmount.y;

		alpha = (p.z - bbox.min.z) / size.z;
		p[(int)scaleZ] *= defCurveSclZ.Evaluate(alpha) * ScaleAmount.z;
		p[(int)offsetZ] += defCurveZ.Evaluate(alpha) * OffsetAmount.z;
		Monitor.Exit(resourceLock);

		return invtm.MultiplyPoint3x4(p);
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		float alpha = 0.0f;
		p = tm.MultiplyPoint3x4(p);

		alpha = (p.x - bbox.min.x) / size.x;
		p[(int)scaleX] *= defCurveSclX.Evaluate(alpha) * ScaleAmount.x;
		p[(int)offsetX] += defCurveX.Evaluate(alpha) * OffsetAmount.x;

		alpha = (p.y - bbox.min.y) / size.y;
		p[(int)scaleY] *= defCurveSclY.Evaluate(alpha) * ScaleAmount.y;
		p[(int)offsetY] += defCurveY.Evaluate(alpha) * OffsetAmount.y;

		alpha = (p.z - bbox.min.z) / size.z;
		p[(int)scaleZ] *= defCurveSclZ.Evaluate(alpha) * ScaleAmount.z;
		p[(int)offsetZ] += defCurveZ.Evaluate(alpha) * OffsetAmount.z;

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		size = bbox.max - bbox.min;
		return true;
	}
}

