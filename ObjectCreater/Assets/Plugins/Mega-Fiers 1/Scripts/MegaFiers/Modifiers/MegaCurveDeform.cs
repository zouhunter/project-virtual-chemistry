
using UnityEngine;
using System.Threading;

[AddComponentMenu("Modifiers/Curve Deform")]
public class MegaCurveDeform : MegaModifier
{
	public MegaAxis			axis = MegaAxis.X;
	public AnimationCurve	defCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.5f, 0.0f), new Keyframe(1.0f, 0.0f));
	public float			MaxDeviation = 1.0f;
	float					width	= 0.0f;
	int						ax;

	public float			Pos = 0.0f;
	public bool				UsePos = false;

	Keyframe key = new Keyframe();

	public override string ModName()	{ return "CurveDeform"; }
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
		p = tm.MultiplyPoint3x4(p);

		float alpha = (p[ax] - bbox.min[ax]) / width;
		if ( UsePos )
			alpha += Pos;
		Monitor.Enter(resourceLock);
		p.y += defCurve.Evaluate(alpha) * MaxDeviation;
		Monitor.Exit(resourceLock);

		return invtm.MultiplyPoint3x4(p);
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		float alpha = (p[ax] - bbox.min[ax]) / width;
		if ( UsePos )
			alpha += Pos;

		p.y += defCurve.Evaluate(alpha) * MaxDeviation;

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		ax = (int)axis;
		width = bbox.max[ax] - bbox.min[ax];
		return true;
	}

	public float GetPos(float alpha)
	{
		float y = defCurve.Evaluate(alpha);
		return y;
	}

	public void SetKey(int index, float t, float v, float intan, float outtan)
	{
		key.time = t;
		key.value = v;
		key.inTangent = intan;
		key.outTangent = outtan;
		defCurve.MoveKey(index, key);
	}
}