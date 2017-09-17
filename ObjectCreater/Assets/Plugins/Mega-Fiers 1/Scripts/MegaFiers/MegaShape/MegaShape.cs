
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class MegaKnotAnimCurve
{
	public AnimationCurve	px = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	py = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	pz = new AnimationCurve(new Keyframe(0, 0));

	public AnimationCurve	ix = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	iy = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	iz = new AnimationCurve(new Keyframe(0, 0));

	public AnimationCurve	ox = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	oy = new AnimationCurve(new Keyframe(0, 0));
	public AnimationCurve	oz = new AnimationCurve(new Keyframe(0, 0));

	public void GetState(MegaKnot knot, float t)
	{
		knot.p.x = px.Evaluate(t);
		knot.p.y = py.Evaluate(t);
		knot.p.z = pz.Evaluate(t);

		knot.invec.x = ix.Evaluate(t);
		knot.invec.y = iy.Evaluate(t);
		knot.invec.z = iz.Evaluate(t);

		knot.outvec.x = ox.Evaluate(t);
		knot.outvec.y = oy.Evaluate(t);
		knot.outvec.z = oz.Evaluate(t);
	}

	public void AddKey(MegaKnot knot, float t)
	{
		px.AddKey(new Keyframe(t, knot.p.x));
		py.AddKey(new Keyframe(t, knot.p.y));
		pz.AddKey(new Keyframe(t, knot.p.z));

		ix.AddKey(new Keyframe(t, knot.invec.x));
		iy.AddKey(new Keyframe(t, knot.invec.y));
		iz.AddKey(new Keyframe(t, knot.invec.z));

		ox.AddKey(new Keyframe(t, knot.outvec.x));
		oy.AddKey(new Keyframe(t, knot.outvec.y));
		oz.AddKey(new Keyframe(t, knot.outvec.z));
	}

	public void MoveKey(MegaKnot knot, float t, int k)
	{
		px.MoveKey(k, new Keyframe(t, knot.p.x));
		py.MoveKey(k, new Keyframe(t, knot.p.y));
		pz.MoveKey(k, new Keyframe(t, knot.p.z));

		ix.MoveKey(k, new Keyframe(t, knot.invec.x));
		iy.MoveKey(k, new Keyframe(t, knot.invec.y));
		iz.MoveKey(k, new Keyframe(t, knot.invec.z));

		ox.MoveKey(k, new Keyframe(t, knot.outvec.x));
		oy.MoveKey(k, new Keyframe(t, knot.outvec.y));
		oz.MoveKey(k, new Keyframe(t, knot.outvec.z));
	}

	public void RemoveKey(int k)
	{
		px.RemoveKey(k);
		py.RemoveKey(k);
		pz.RemoveKey(k);

		ix.RemoveKey(k);
		iy.RemoveKey(k);
		iz.RemoveKey(k);

		ox.RemoveKey(k);
		oy.RemoveKey(k);
		oz.RemoveKey(k);
	}
}

[System.Serializable]
public class MegaSplineAnim
{
	public bool	Enabled = false;
	public List<MegaKnotAnimCurve> knots = new List<MegaKnotAnimCurve>();

	public void SetState(MegaSpline spline, float t)
	{
	}

	public void GetState1(MegaSpline spline, float t)
	{
		for ( int i = 0; i < knots.Count; i++ )
		{
			knots[i].GetState(spline.knots[i], t);
		}
	}

	int FindKey(float t)
	{
		if ( knots.Count > 0 )
		{
			Keyframe[] keys = knots[0].px.keys;

			for ( int i = 0; i < keys.Length; i++ )
			{
				if ( keys[i].time == t )
					return i;
			}
		}

		return -1;
	}

	public void AddState(MegaSpline spline, float t)
	{
		if ( knots.Count == 0 )
		{
			Init(spline);
		}
		// if we have a match for time then replace
		int k = FindKey(t);

		if ( k == -1 )
		{
			// add new keys
			for ( int i = 0; i < spline.knots.Count; i++ )
				knots[i].AddKey(spline.knots[i], t);
		}
		else
		{
			// Move existing key with new values
			for ( int i = 0; i < spline.knots.Count; i++ )
				knots[i].MoveKey(spline.knots[i], t, k);
		}
	}

	public void Remove(float t)
	{
		int k = FindKey(t);

		if ( k != -1 )
		{
			for ( int i = 0; i < knots.Count; i++ )
				knots[i].RemoveKey(k);
		}
	}

	public void RemoveKey(int k)
	{
		if ( k < NumKeys() )
		{
			for ( int i = 0; i < knots.Count; i++ )
				knots[i].RemoveKey(k);
		}
	}

	public void Init(MegaSpline spline)
	{
		knots.Clear();

		for ( int i = 0; i < spline.knots.Count; i++ )
		{
			MegaKnotAnimCurve kc = new MegaKnotAnimCurve();

			kc.MoveKey(spline.knots[i], 0.0f, 0);
			knots.Add(kc);
		}
	}

	public int NumKeys()
	{
		if ( knots == null || knots.Count == 0 )
			return 0;

		return knots[0].px.keys.Length;
	}

	public float GetKeyTime(int k)
	{
		if ( knots == null || knots.Count == 0 )
			return 0;

		Keyframe[] f = knots[0].px.keys;
		if ( k < f.Length )
		{
			return f[k].time;
		}
		return 0.0f;
	}

	public void SetKeyTime(MegaSpline spline, int k, float t)
	{
		if ( knots == null || knots.Count == 0 )
			return;

		for ( int i = 0; i < spline.knots.Count; i++ )
			knots[i].MoveKey(spline.knots[i], t, k);
	}

	public void GetKey(MegaSpline spline, int k)
	{
		float t = GetKeyTime(k);
		GetState1(spline, t);
		spline.CalcLength();	//(10);	// could use less here
	}

	public void UpdateKey(MegaSpline spline, int k)
	{
		float t = GetKeyTime(k);
		for ( int i = 0; i < spline.knots.Count; i++ )
			knots[i].MoveKey(spline.knots[i], t, k);
	}
}

// option for spline profile for border?
// TODO: Add option for border strip, so get edge, duplicate points, move originals in by amount and normal of tangent
// if we do meshes with edge loops then easy to do borders, bevels, extrudes
// TODO: Split code to shape, spline, knot, and same for edit code
// TODO: Each spline in a shape should have its own transform
// TODO: split knot and spline out to files
// Need to draw and edit multiple splines, and work on them, then mesh needs to work on those indi
[System.Serializable]
public class MegaKnotAnim
{
	public int	p;	// point index
	public int	t;	// handle or val
	public int	s;	// spline

	public MegaBezVector3KeyControl	con;
}

public enum MegaHandleType
{
	Position,
	Free,
}

[System.Serializable]
public class MegaKnot
{
	public Vector3	p;
	public Vector3	invec;
	public Vector3	outvec;
	public float	seglength;
	public float	length;
	public bool		notlocked;
	public float	twist;
	public int		id;

	public float[]		lengths;
	public Vector3[]	points;

	public MegaKnot()
	{
		p = new Vector3();
		invec = new Vector3();
		outvec = new Vector3();
		length = 0.0f;
		seglength = 0.0f;
	}

	public Vector3 Interpolate(float t, MegaKnot k)
	{
		float omt = 1.0f - t;

		float omt2 = omt * omt;
		float omt3 = omt2 * omt;

		float t2 = t * t;
		float t3 = t2 * t;

		omt2 = 3.0f * omt2 * t;
		omt = 3.0f * omt * t2;

		Vector3 tp = Vector3.zero;

		tp.x = (omt3 * p.x) + (omt2 * outvec.x) + (omt * k.invec.x) + (t3 * k.p.x);
		tp.y = (omt3 * p.y) + (omt2 * outvec.y) + (omt * k.invec.y) + (t3 * k.p.y);
		tp.z = (omt3 * p.z) + (omt2 * outvec.z) + (omt * k.invec.z) + (t3 * k.p.z);

		return tp;
	}

#if false
	public Vector3 InterpolateCS(float t, MegaKnot k)
	{
		if ( lengths == null || lengths.Length == 0 )
			return Interpolate(t, k);

		float u = (t * seglength) + lengths[0];
		int i = 0;
		for ( i = 0; i < lengths.Length - 1; i++ )
		{
			if ( u < lengths[i] )
			{
				break;
			}
		}

		float alpha = (u - lengths[i - 1]) / (lengths[i] - lengths[i - 1]);
		return Vector3.Lerp(points[i - 1], points[i], alpha);
	}
#else
	public Vector3 InterpolateCS(float t, MegaKnot k)
	{
		if ( lengths == null || lengths.Length == 0 )
			return Interpolate(t, k);

		float u = (t * seglength) + lengths[0];
		
		
		int high = lengths.Length - 1;
		int low = -1;
		int probe = 0;
		//int i = lengths.Length / 2;

		while ( high - low > 1 )
		{
			probe = (high + low) / 2;

			if ( u >= lengths[probe] )
			{
				if ( u < lengths[probe + 1] )
					break;
				low = probe;
			}
			else
				high = probe;
		}
		//for ( i = 0; i < lengths.Length - 1; i++ )
		//{
		//	if ( u < lengths[i] )
		//	{
		//		break;
		//	}
		//}

		float alpha = (u - lengths[probe]) / (lengths[probe + 1] - lengths[probe]);
		return Vector3.Lerp(points[probe], points[probe + 1], alpha);
	}
#endif

	public Vector3 Tangent(float t, MegaKnot k)
	{
		Vector3 vel;

		float a = t;
		float b = 1.0f - t;

		float b2 = b * b;
		float a2 = a * a;

		vel.x = (-3.0f * p.x * b2) + (3.0f * outvec.x * b * (b - 2.0f * a)) + (3.0f * k.invec.x * a * (2.0f * b - a)) + (k.p.x * 3.0f * a2);
		vel.y = (-3.0f * p.y * b2) + (3.0f * outvec.y * b * (b - 2.0f * a)) + (3.0f * k.invec.y * a * (2.0f * b - a)) + (k.p.y * 3.0f * a2);
		vel.z = (-3.0f * p.z * b2) + (3.0f * outvec.z * b * (b - 2.0f * a)) + (3.0f * k.invec.z * a * (2.0f * b - a)) + (k.p.z * 3.0f * a2);

		//float d = vel.sqrMagnitude;

		return vel;
	}
}

public enum MegaShapeEase
{
	Linear,
	Sine,
}

[System.Serializable]
public class MegaSpline
{
	public float				length;
	public bool					closed;
	public List<MegaKnot>		knots = new List<MegaKnot>();
	public List<MegaKnotAnim>	animations;
	public Vector3				offset = Vector3.zero;
	public Vector3				rotate = Vector3.zero;
	public Vector3				scale = Vector3.one;
	public bool					reverse = false;


	public int					outlineSpline = -1;
	public float				outline = 0.0f;

	public bool					constantSpeed = false;
	public int					subdivs = 10;
	public MegaShapeEase		twistmode = MegaShapeEase.Linear;

	// New animation
	public MegaSplineAnim		splineanim = new MegaSplineAnim();

	static public MegaSpline Copy(MegaSpline src)
	{
		MegaSpline spl = new MegaSpline();

		spl.closed = src.closed;
		spl.offset = src.offset;
		spl.rotate = src.rotate;
		spl.scale = src.scale;

		spl.length = src.length;

		spl.knots = new List<MegaKnot>();	//src.knots);

		spl.constantSpeed = src.constantSpeed;
		spl.subdivs = src.subdivs;

		for ( int i = 0; i < src.knots.Count; i++ )
		{
			MegaKnot knot = new MegaKnot();
			knot.p = src.knots[i].p;
			knot.invec = src.knots[i].invec;
			knot.outvec = src.knots[i].outvec;
			knot.seglength = src.knots[i].seglength;
			knot.length = src.knots[i].length;
			knot.notlocked = src.knots[i].notlocked;

			spl.knots.Add(knot);
		}

		spl.animations = new List<MegaKnotAnim>(src.animations);

		return spl;
	}

	public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec)
	{
		MegaKnot knot = new MegaKnot();
		knot.p = p;
		knot.invec = invec;
		knot.outvec = outvec;
		knots.Add(knot);
	}

	public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec, Matrix4x4 tm)
	{
		MegaKnot knot = new MegaKnot();
		knot.p = tm.MultiplyPoint3x4(p);
		knot.invec = tm.MultiplyPoint3x4(invec);
		knot.outvec = tm.MultiplyPoint3x4(outvec);
		knots.Add(knot);
	}

	// Assumes minor axis to be y
	public bool Contains(Vector3 p)
	{
		if ( !closed )
			return false;

		int		j = knots.Count - 1;
		bool	oddNodes = false;

		for ( int i = 0; i < knots.Count; i++ )
		{
			if ( knots[i].p.z < p.z && knots[j].p.z >= p.z || knots[j].p.z < p.z && knots[i].p.z >= p.z )
			{
				if ( knots[i].p.x + (p.z - knots[i].p.z) / (knots[j].p.z - knots[i].p.z) * (knots[j].p.x - knots[i].p.x) < p.x )
					oddNodes = !oddNodes;
			}

			j = i;
		}

		return oddNodes;
	}

	// Assumes minor axis to be y
	public float Area()
	{
		float area = 0.0f;

		if ( closed )
		{
			for ( int i = 0; i < knots.Count; i++ )
			{
				int i1 = (i + 1) % knots.Count;
				area += (knots[i].p.z + knots[i1].p.z) * (knots[i1].p.x - knots[i].p.x);
			}
		}

		return area * 0.5f;
	}

	// Should actually go through segments, what about scale?
#if false	// old
	public float CalcLength(int steps)
	{
		length = 0.0f;

		int kend = knots.Count - 1;

		if ( closed )
			kend++;

		for ( int knot = 0; knot < kend; knot++ )
		{
			int k1 = (knot + 1) % knots.Count;

			Vector3 p1 = knots[knot].p;
			float step = 1.0f / (float)steps;
			float pos = step;

			knots[knot].seglength = 0.0f;

			for ( int i = 1; i < steps; i++ )
			{
				Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

				knots[knot].seglength += Vector3.Magnitude(p2 - p1);
				p1 = p2;
				pos += step;
			}

			knots[knot].seglength += Vector3.Magnitude(knots[k1].p - p1);

			length += knots[knot].seglength;

			knots[knot].length = length;
			length = knots[knot].length;
		}

		//AdjustSpline();

		return length;
	}
#else
	public float CalcLength(int steps)
	{
		if ( steps < 1 )
			steps = 1;
		subdivs = steps;
		return CalcLength();
	}

	public float CalcLength()
	{
		length = 0.0f;

		int kend = knots.Count - 1;

		if ( closed )
			kend++;

		for ( int knot = 0; knot < kend; knot++ )
		{
			int k1 = (knot + 1) % knots.Count;

			Vector3 p1 = knots[knot].p;
			float step = 1.0f / (float)subdivs;
			float pos = step;

			knots[knot].seglength = 0.0f;

			if ( knots[knot].lengths == null || knots[knot].lengths.Length != subdivs + 1 )
			{
				knots[knot].lengths = new float[subdivs + 1];
				knots[knot].points = new Vector3[subdivs + 1];
			}

			knots[knot].lengths[0] = length;
			knots[knot].points[0] = knots[knot].p;

			float dist = 0.0f;
			for ( int i = 1; i < subdivs; i++ )
			{
				Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

				knots[knot].points[i] = p2;
				dist = Vector3.Magnitude(p2 - p1);
				knots[knot].seglength += dist;
				p1 = p2;
				pos += step;

				length += dist;
				knots[knot].lengths[i] = length;
			}

			dist = Vector3.Magnitude(knots[k1].p - p1);
			knots[knot].seglength += dist;	//Vector3.Magnitude(knots[k1].p - p1);

			length += dist;	//knots[knot].seglength;

			knots[knot].lengths[subdivs] = length;
			knots[knot].points[subdivs] = knots[k1].p;

			knots[knot].length = length;
			length = knots[knot].length;
		}

		//AdjustSpline();

		return length;
	}
#endif

	//List<Vector3>	samples = new List<Vector3>();
	//public List<float>		alphas = new List<float>();

	//public Vector3 InterpCurve3DSampled(float alpha)
	//{
	//	if ( alpha == 1.0f )
	//		return samples[samples.Count - 1];
	//	if ( alpha  == 0.0f )
	//		return samples[0];

	//	float findex = (float)samples.Count * alpha;
	//	int index = (int)findex;
	//	findex -= index;

	//	return Vector3.Lerp(samples[index], samples[index + 1], findex);
	//}

#if false
	public void AdjustSpline()
	{
		int k = 0;
		float lindist = length / 100.0f;

		float dist = 0.0f;
		Vector3 last = knots[0].p;

		alphas.Clear();
		alphas.Add(1.0f);
		for ( int i = 1; i < 100; i++ )
		{
			float alpha = (float)i / 100.0f;

			Vector3 p = InterpCurve3D(alpha, true, ref k);
			float d = (p - last).magnitude;
			dist += d;

			float sa = (length * alpha) / dist;
			float dev = alpha / sa;
			alphas.Add(sa);	//dev);
			last = p;
		}

		alphas.Add(1.0f);
	}
#endif

#if false
	// Could pass start and end alpha
	public float CalcSampleTable(int steps)
	{
		float delta = length / (float)steps;

		samples.Clear();

		int k = 0;

		samples.Add(InterpCurve3D(0.0f, true, ref k));

		float alpha = 0.0f;

		Vector3 last = samples[0];
		while ( alpha < 1.0f )
		{
			float dist = 0.0f;


		}



		samples.Add(InterpCurve3D(1.0f, true, ref k));


		length = 0.0f;

		int kend = knots.Count - 1;

		if ( closed )
			kend++;

		for ( int knot = 0; knot < kend; knot++ )
		{
			int k1 = (knot + 1) % knots.Count;

			Vector3 p1 = knots[knot].p;
			float step = 1.0f / (float)steps;
			float pos = step;

			knots[knot].seglength = 0.0f;

			for ( int i = 1; i < steps; i++ )
			{
				Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

				knots[knot].seglength += Vector3.Magnitude(p2 - p1);
				p1 = p2;
				pos += step;
			}

			knots[knot].seglength += Vector3.Magnitude(knots[k1].p - p1);

			length += knots[knot].seglength;

			knots[knot].length = length;
			length = knots[knot].length;
		}

		return length;
	}
#endif

	public float GetTwist(float alpha)
	{
		int	seg = 0;

		if ( closed )
		{
			alpha = Mathf.Repeat(alpha, 1.0f);
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				//return Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				//return Mathf.Lerp(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				return TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			if ( seg < knots.Count - 1 )
			{
				//return Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				//return Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
				return TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
			}
			else
			{
				//return Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
				//return Mathf.Lerp(knots[seg].twist, knots[0].twist, alpha);
				return TwistVal(knots[seg].twist, knots[0].twist, alpha);
			}
		}
		else
		{
			float dist = alpha * length;

			for ( seg = 0; seg < knots.Count; seg++ )
			{
				if ( dist <= knots[seg].length )
					break;
			}

			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				//return Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				return TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
			}
			else
				return knots[seg].twist;
		}
	}

	/*  So this should work for curves or splines, no sep code for curve, derive from common base */
	/*  Could save a hint for next time through, ie spline and seg */
	public Vector3 Interpolate(float alpha, bool type, ref int k)
	{
		int	seg = 0;

		//if ( alphas != null && alphas.Count > 0 )
		//{
		//	int ix = (int)(alpha * (float)alphas.Count);
		//	alpha *= alphas[ix];
		//	if ( alpha >= 1.0f )
		//		alpha = 0.99999f;
		//	if ( alpha < 0.0f )
		//		alpha = 0.0f;
		//}

		if ( constantSpeed )
			return InterpolateCS(alpha, type, ref k);

		// Special case if closed
		if ( closed )
		{
			if ( type )
			{
				float dist = alpha * length;

				if ( dist > knots[knots.Count - 1].length )
				{
					k = knots.Count - 1;
					alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
					return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
				}
				else
				{
					for ( seg = 0; seg < knots.Count; seg++ )
					{
						if ( dist <= knots[seg].length )
							break;
					}
				}
				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			if ( seg < knots.Count - 1 )
			{
				k = seg;

				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;

				return knots[seg].Interpolate(alpha, knots[0]);
			}

			//return knots[0].p;
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				return knots[seg].p;

				//return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
		}
	}

	public Vector3 InterpolateCS(float alpha, bool type, ref int k)
	{
		int	seg = 0;

		// Special case if closed
		if ( closed )
		{
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				k = knots.Count - 1;
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			if ( seg < knots.Count - 1 )
			{
				k = seg;
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;
				return knots[seg].InterpolateCS(alpha, knots[0]);
			}
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				return knots[seg].p;

				//return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
#if false
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				k = knots.Count - 1;
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				return knots[seg].p;
			}
#endif
		}
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	float TwistVal(float v1, float v2, float alpha)
	{
		if ( twistmode == MegaShapeEase.Linear )
			return Mathf.Lerp(v1, v2, alpha);

		return easeInOutSine(v1, v2, alpha);
	}

	public Vector3 Interpolate(float alpha, bool type, ref int k, ref float twist)
	{
		int	seg = 0;

		if ( constantSpeed )
			return InterpolateCS(alpha, type, ref k, ref twist);

		// Special case if closed
		if ( closed )
		{
			if ( type )
			{
				float dist = alpha * length;

				if ( dist > knots[knots.Count - 1].length )
				{
					k = knots.Count - 1;
					alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
					//twist = Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					//twist = Mathf.Lerp(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
				}
				else
				{
					for ( seg = 0; seg < knots.Count; seg++ )
					{
						if ( dist <= knots[seg].length )
							break;
					}
				}
				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			if ( seg < knots.Count - 1 )
			{
				k = seg;

				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;

				//twist = Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[0]);
			}
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				twist = knots[seg].twist;
				return knots[seg].p;
			}
		}
	}

	public Vector3 InterpolateCS(float alpha, bool type, ref int k, ref float twist)
	{
		int	seg = 0;

		// Special case if closed
		if ( closed )
		{
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				k = knots.Count - 1;
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				//twist = Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[0]);
			}
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				twist = knots[seg].twist;
				return knots[seg].p;

				//return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
		}
	}

	// New method that handles open splines better
	public Vector3 InterpCurve3D(float alpha, bool type, ref int k)
	{
		Vector3	ret;
		k = 0;

		if ( alpha < 0.0f )
		{
			if ( closed )
				alpha = Mathf.Repeat(alpha, 1.0f);
			else
			{
				Vector3 ps = Interpolate(0.0f, type, ref k);

				// Need a proper tangent function
				Vector3 ps1 = Interpolate(0.01f, type, ref k);

				// Calc the spline in out vecs
				Vector3	delta = ps1 - ps;
				delta.Normalize();
				return ps + ((length * alpha) * delta);
			}
		}
		else
		{
			if ( alpha > 1.0f )
			{
				if ( closed )
					alpha = alpha % 1.0f;
				else
				{
					Vector3 ps = Interpolate(1.0f, type, ref k);

					// Need a proper tangent function
					Vector3 ps1 = Interpolate(0.99f, type, ref k);

					// Calc the spline in out vecs
					Vector3	delta = ps1 - ps;
					delta.Normalize();
					return ps + ((length * (1.0f - alpha)) * delta);
				}
			}
		}

		ret = Interpolate(alpha, type, ref k);

		return ret;
	}

	public Vector3 InterpBezier3D(int knot, float a)
	{
		if ( knot < knots.Count )
		{
			int k1 = knot + 1;
			if ( k1 == knots.Count && closed )
			{
				k1 = 0;
			}

			return knots[knot].Interpolate(a, knots[k1]);
		}

		return Vector3.zero;
	}

	// Should be spline methods
	public void Centre(float scale)
	{
		Vector3 p = Vector3.zero;

		for ( int i = 0; i < knots.Count; i++ )
			p += knots[i].p;

		p /= (float)knots.Count;

		for ( int i = 0; i < knots.Count; i++ )
		{
			knots[i].p -= p;
			knots[i].invec -= p;
			knots[i].outvec -= p;

			knots[i].p *= scale;
			knots[i].invec *= scale;
			knots[i].outvec *= scale;
		}
	}

	public void Reverse()
	{
		List<MegaKnot> newknots = new List<MegaKnot>();

		for ( int i = knots.Count - 1; i >= 0; i-- )
		{
			MegaKnot k = new MegaKnot();
			k.p = knots[i].p;
			k.invec = knots[i].outvec;
			k.outvec = knots[i].invec;
			newknots.Add(k);
		}

		knots = newknots;
		CalcLength();	//(10);
	}

	public void SetHeight(float y)
	{
		for ( int i = 0; i < knots.Count; i++ )
		{
			knots[i].p.y = y;
			knots[i].outvec.y = y;
			knots[i].invec.y = y;
		}
	}

	public void SetTwist(float twist)
	{
		for ( int i = 0; i < knots.Count; i++ )
		{
			knots[i].twist = twist;
		}
	}
}

public enum MeshShapeType
{
	Fill,	// options, height, doublesided
	Tube,	// sides, cap ends, start, end, step
	Box,
	Ribbon,
	//Line,	// height, rotate, offset, segments for height
	//Lathe,	// segs
}

[ExecuteInEditMode]
public class MegaShape : MonoBehaviour
{
	public MegaAxis	axis				= MegaAxis.Y;
	public Color	col1				= new Color(1.0f, 1.0f, 1.0f, 1.0f);
	public Color	col2				= new Color(0.1f, 0.1f, 0.1f, 1.0f);
	public Color	KnotCol				= new Color(0.0f, 1.0f, 0.0f, 1.0f);
	public Color	HandleCol			= new Color(1.0f, 0.0f, 0.0f, 1.0f);
	public Color	VecCol				= new Color(0.1f, 0.1f, 0.2f, 0.5f);
	public float	KnotSize			= 10.0f;
	public float	stepdist			= 1.0f;	// Distance along whole shape
	public bool		normalizedInterp	= true;
	public bool		drawHandles			= true;
	public bool		drawKnots			= true;
	public bool		drawspline			= true;
	public bool		drawTwist			= false;
	public bool		lockhandles			= true;

	public MegaHandleType	handleType = MegaHandleType.Position;
	public float	CursorPos = 0.0f;
	public List<MegaSpline>	splines = new List<MegaSpline>();

	public bool showanimations = false;
	public float keytime = 0.0f;

	//public virtual void MakeShape() { }

	public float defRadius = 1.0f;

	const float CIRCLE_VECTOR_LENGTH = 0.5517861843f;

	public virtual void MakeShape()
	{
		Matrix4x4 tm = GetMatrix();

		float vector = CIRCLE_VECTOR_LENGTH * defRadius;

		MegaSpline spline = NewSpline();

		for ( int ix = 0; ix < 4; ++ix )
		{
			float angle = (Mathf.PI * 2.0f) * (float)ix / (float)4;
			float sinfac = Mathf.Sin(angle);
			float cosfac = Mathf.Cos(angle);
			Vector3 p = new Vector3(cosfac * defRadius, sinfac * defRadius, 0.0f);
			Vector3 rotvec = new Vector3(sinfac * vector, -cosfac * vector, 0.0f);
			spline.AddKnot(p, p + rotvec, p - rotvec, tm);
		}

		spline.closed = true;
		CalcLength();	//10);
	}

	//public List<MegaKnotAnim>	animations;
	public float	testtime = 0.0f;
	public float	time = 0.0f;
	public bool		animate;
	public float	speed = 1.0f;
	public int		selcurve = 0;
	public bool		imported = false;
	//public int		CurrentCurve = 0;

	//public int		CursorKnot = 0;
	public float		CursorPercent = 0.0f;


	public virtual string GetHelpURL() { return "?page_id=390"; }

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/" + GetHelpURL());
	}

	[ContextMenu("Reset Mesh Info")]
	public void ResetMesh()
	{
		shapemesh = null;
		BuildMesh();
	}

	public Matrix4x4 GetMatrix()
	{
		Matrix4x4 tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X:	MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f);
				MegaMatrix.Scale(ref tm, -Vector3.one, false);
				break;
			//case MegaAxis.X: MegaMatrix.RotateY(ref tm, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;	//Matrix.RotateY(ref tm, Mathf.PI * 0.5f); break;
		}

		return tm;
	}

	public void Reverse(int c)
	{
		if ( c >= 0 && c < splines.Count )
		{
			splines[c].Reverse();
		}
	}

	public void SetHeight(int c, float y)
	{
		if ( c >= 0 && c < splines.Count )
		{
			splines[c].SetHeight(y);
		}
	}

	public void SetTwist(int c, float twist)
	{
		if ( c >= 0 && c < splines.Count )
		{
			splines[c].SetTwist(twist);
		}
	}

	// Should be in MegaShape
	public MegaSpline NewSpline()
	{
		if ( splines.Count == 0 )
		{
			MegaSpline newspline = new MegaSpline();
			splines.Add(newspline);
		}

		MegaSpline spline = splines[0];

		spline.knots.Clear();
		spline.closed = false;
		return spline;
	}

	void Reset()
	{
		MakeShape();
	}

	void Awake()
	{
		if ( splines.Count == 0 )
		{
			MakeShape();
		}
	}

	float t = 0.0f;
	public float MaxTime = 1.0f;
	public MegaRepeatMode	LoopMode;
	public bool dolateupdate = false;

	void Update()
	{
		if ( !dolateupdate )
		{
			DoUpdate();
		}
	}

	void LateUpdate()
	{
		if ( dolateupdate )
		{
			DoUpdate();
		}
	}

	void DoUpdate()
	{
		if ( animate )
		{
			BuildMesh();

			time += Time.deltaTime * speed;

			switch ( LoopMode )
			{
				case MegaRepeatMode.Loop:		t = Mathf.Repeat(time, MaxTime); break;
				case MegaRepeatMode.PingPong:	t = Mathf.PingPong(time, MaxTime); break;
				case MegaRepeatMode.Clamp:		t = Mathf.Clamp(time, 0.0f, MaxTime); break;
			}

			for ( int s = 0; s < splines.Count; s++ )
			{
				if ( splines[s].splineanim != null && splines[s].splineanim.Enabled )
				{
					//Debug.Log("getstate");
					splines[s].splineanim.GetState1(splines[s], t);
					splines[s].CalcLength();	//(10);	// could use less here
				}
				else
				{
					if ( splines[s].animations != null && splines[s].animations.Count > 0 )
					{
						for ( int i = 0; i < splines[s].animations.Count; i++ )
						{
							Vector3 pos = splines[s].animations[i].con.GetVector3(t);

							switch ( splines[s].animations[i].t )
							{
								case 0:	splines[splines[s].animations[i].s].knots[splines[s].animations[i].p].invec		= pos;	break;
								case 1:	splines[splines[s].animations[i].s].knots[splines[s].animations[i].p].p			= pos;	break;
								case 2:	splines[splines[s].animations[i].s].knots[splines[s].animations[i].p].outvec	= pos;	break;
							}
						}

						splines[s].CalcLength();	//(10);	// could use less here
					}
				}
			}
		}

		// Options here:
		// Uv scale, offset, rotate, physuv, genuv
		// Optimize
		// recalcnorms
		// tangents
		// fill in shape
		// pipe along shape
		// wall along shape
		// double sided
		// extrude on fill
		//BuildMesh();
#if false
		if ( makeMesh )
		{
			//makeMesh = false;
			List<Vector3>	verts = new List<Vector3>();
			List<Vector2>	uvs = new List<Vector2>();

			float sdist = stepdist * 0.1f;
			if ( splines[0].length / sdist > 1500.0f )
				sdist = splines[0].length / 1500.0f;

			int[] tris = MegaTriangulator.Triangulate(this, splines[0], sdist, ref verts, ref uvs);

			if ( shapemesh == null )
			{
				MeshFilter mf = gameObject.GetComponent<MeshFilter>();

				if ( mf == null )
					mf = gameObject.AddComponent<MeshFilter>();

				mf.sharedMesh = new Mesh();
				MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
				if ( mr == null )
				{
					mr = gameObject.AddComponent<MeshRenderer>();
				}

				Material[] mats = new Material[1];

				mr.sharedMaterials = mats;

				shapemesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);
			}

			//Vector3[] verts = new Vector3[splines[0].knots.Count];
			//for ( int i = 0; i < splines[0].knots.Count; i++ )
			//{
			//	verts[i] = splines[0].knots[i].p;
			//}

			shapemesh.Clear();
			shapemesh.vertices = verts.ToArray();
			shapemesh.uv = uvs.ToArray();

			shapemesh.triangles = tris;
			shapemesh.RecalculateNormals();
			shapemesh.RecalculateBounds();
		}
#endif
	}

	// Meshing options
	public bool				makeMesh = false;
	public MeshShapeType	meshType = MeshShapeType.Fill;
	public bool				DoubleSided = true;
	public bool				CalcTangents = false;
	public bool				GenUV = true;
	public bool				PhysUV = false;
	public float			Height = 0.0f;
	public int				HeightSegs = 1;

	public int				Sides = 4;
	public float			TubeStep = 0.1f;
	public float			Start = 0.0f;
	public float			End = 100.0f;
	public float			Rotate = 0.0f;
	public Vector3			Pivot = Vector3.zero;

	// Material 1
	public Vector2			UVOffset = Vector2.zero;
	public Vector2			UVRotate = Vector2.zero;
	public Vector2			UVScale = Vector2.one;

	public Vector2			UVOffset1 = Vector2.zero;
	public Vector2			UVRotate1 = Vector2.zero;
	public Vector2			UVScale1 = Vector2.one;

	public Vector2			UVOffset2 = Vector2.zero;
	public Vector2			UVRotate3 = Vector2.zero;
	public Vector2			UVScale3 = Vector2.one;

	public bool				autosmooth = false;
	public float			smoothness = 1.1f;
	// TODO:
	// Pivot
	// UV info per submesh
	// work on multiple splines
	// new poly to tri to handle holes
	// merge to one mesh
	// height segs
	public Material mat1;
	public Material mat2;
	public Material mat3;


	public bool				UseHeightCurve = false;
	public AnimationCurve	heightCrv = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public float			heightOff = 0.0f;
	// May need wall UV info as well


	public Mesh shapemesh;


	// New equal step system for faster interps etc
#if false
	//Create table to convert u to distance
	std::pair<float, float> u_distance_map[LENGTH_DIVISIONS + 1];

	//Calculate u value and distance map
	public void CreateDistMap(int divs)
	{
		for ( int i = 0; i <= divs; i++ )
		{
			float u = (float)i / (float)divs;
			u_distance_map[i].first = u;
			u_distance_map[i].second = track_path->Get_Path_Length(u);
		}

		//Determine the u values of the ties
		std::vector<float> tie_u_values;

		for ( int i = 0; i < num_ties; i++ )
		{
			bool found = false;
			for ( int j = 1; j <= divs && !found; j++ )
			{
				/*The correct u value is less than this distance and greater than the last
				so just interpolate from here.*/
				if ( u_distance_map[j].second >= (i + 1) * tie_distance )
				{
					//Calculate percentage between u values
					float distance_between_nodes = u_distance_map[j].second - u_distance_map[j - 1].second;
					float distance_past_previous = ((i + 1) * tie_distance) - u_distance_map[j - 1].second;
					float percent = distance_past_previous / distance_between_nodes;

					//Calculate u value
					float u = ((u_distance_map[j].first - u_distance_map[j - 1].first) * percent) + u_distance_map[j - 1].first;
					found = true;
					tie_u_values.push_back(u);
				}
			}
		}
	}
#endif



	public void Centre(float scale, Vector3 axis)
	{
		Vector3 p = Vector3.zero;

		int count = 0;
		for ( int s = 0; s < splines.Count; s++ )
		{
			count += splines[s].knots.Count;

			for ( int i = 0; i < splines[s].knots.Count; i++ )
				p += splines[s].knots[i].p;
		}

		p /= (float)count;	//knots.Count;

		for ( int s = 0; s < splines.Count; s++ )
		{
			for ( int i = 0; i < splines[s].knots.Count; i++ )
			{
				splines[s].knots[i].p -= p;
				splines[s].knots[i].invec -= p;
				splines[s].knots[i].outvec -= p;

				splines[s].knots[i].p *= scale;
				splines[s].knots[i].invec *= scale;
				splines[s].knots[i].outvec *= scale;

				splines[s].knots[i].p = Vector3.Scale(splines[s].knots[i].p, axis);
				splines[s].knots[i].invec = Vector3.Scale(splines[s].knots[i].invec, axis);
				splines[s].knots[i].outvec = Vector3.Scale(splines[s].knots[i].outvec, axis);
			}
		}
	}

	public void Centre(float scale, Vector3 axis, int start)
	{
		Vector3 p = Vector3.zero;

		int count = 0;
		for ( int s = start; s < splines.Count; s++ )
		{
			count += splines[s].knots.Count;

			for ( int i = 0; i < splines[s].knots.Count; i++ )
				p += splines[s].knots[i].p;
		}

		p /= (float)count;	//knots.Count;

		for ( int s = start; s < splines.Count; s++ )
		{
			for ( int i = 0; i < splines[s].knots.Count; i++ )
			{
				splines[s].knots[i].p -= p;
				splines[s].knots[i].invec -= p;
				splines[s].knots[i].outvec -= p;

				splines[s].knots[i].p *= scale;
				splines[s].knots[i].invec *= scale;
				splines[s].knots[i].outvec *= scale;

				splines[s].knots[i].p = Vector3.Scale(splines[s].knots[i].p, axis);
				splines[s].knots[i].invec = Vector3.Scale(splines[s].knots[i].invec, axis);
				splines[s].knots[i].outvec = Vector3.Scale(splines[s].knots[i].outvec, axis);
			}
		}
	}

	// Need a scale method?
	public void Scale(float scale)
	{
		for ( int i = 0; i < splines.Count; i++ )
		{
			for ( int k = 0; k < splines[i].knots.Count; k++ )
			{
				splines[i].knots[k].invec *= scale;
				splines[i].knots[k].p *= scale;
				splines[i].knots[k].outvec *= scale;
			}

			if ( splines[i].animations != null )
			{
				for ( int a = 0; a < splines[i].animations.Count; a++ )
				{
					if ( splines[i].animations[a].con != null )
						splines[i].animations[a].con.Scale(scale);
				}
			}
		}

		CalcLength();	//(10);
	}

	public void Scale(float scale, int start)
	{
		for ( int i = start; i < splines.Count; i++ )
		{
			for ( int k = 0; k < splines[i].knots.Count; k++ )
			{
				splines[i].knots[k].invec *= scale;
				splines[i].knots[k].p *= scale;
				splines[i].knots[k].outvec *= scale;
			}

			if ( splines[i].animations != null )
			{
				for ( int a = 0; a < splines[i].animations.Count; a++ )
				{
					if ( splines[i].animations[a].con != null )
						splines[i].animations[a].con.Scale(scale);
				}
			}
		}

		CalcLength();	//(10);
	}

	public void Scale(Vector3 scale)
	{
		for ( int i = 0; i < splines.Count; i++ )
		{
			for ( int k = 0; k < splines[i].knots.Count; k++ )
			{
				splines[i].knots[k].invec.x *= scale.x;
				splines[i].knots[k].invec.y *= scale.y;
				splines[i].knots[k].invec.z *= scale.z;

				splines[i].knots[k].p.x *= scale.x;
				splines[i].knots[k].p.y *= scale.y;
				splines[i].knots[k].p.z *= scale.z;

				splines[i].knots[k].outvec.x *= scale.x;
				splines[i].knots[k].outvec.y *= scale.y;
				splines[i].knots[k].outvec.z *= scale.z;
			}

			if ( splines[i].animations != null )
			{
				for ( int a = 0; a < splines[i].animations.Count; a++ )
				{
					if ( splines[i].animations[a].con != null )
						splines[i].animations[a].con.Scale(scale);
				}
			}
		}

		CalcLength();	//(10);
	}

	public void MoveSpline(Vector3 delta)
	{
		for ( int i = 0; i < splines.Count; i++ )
		{
			MoveSpline(delta, i, false);
		}

		CalcLength();	//(10);
	}

	public void MoveSpline(Vector3 delta, int c, bool calc)
	{
		for ( int k = 0; k < splines[c].knots.Count; k++ )
		{
			splines[c].knots[k].invec += delta;
			splines[c].knots[k].p += delta;
			splines[c].knots[k].outvec += delta;
		}

		if ( splines[c].animations != null )
		{
			for ( int a = 0; a < splines[c].animations.Count; a++ )
			{
				if ( splines[c].animations[a].con != null )
					splines[c].animations[a].con.Move(delta);
			}
		}

		if ( calc )
			CalcLength(c);	//(10);
	}

	public void RotateSpline(Vector3 rot, int c, bool calc)
	{
		Matrix4x4 tm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rot), Vector3.one);

		for ( int k = 0; k < splines[c].knots.Count; k++ )
		{
			splines[c].knots[k].invec = tm.MultiplyPoint3x4(splines[c].knots[k].invec);
			splines[c].knots[k].outvec = tm.MultiplyPoint3x4(splines[c].knots[k].outvec);
			splines[c].knots[k].p = tm.MultiplyPoint3x4(splines[c].knots[k].p);
		}

		if ( splines[c].animations != null )
		{
			for ( int a = 0; a < splines[c].animations.Count; a++ )
			{
				if ( splines[c].animations[a].con != null )
					splines[c].animations[a].con.Rotate(tm);
			}
		}

		if ( calc )
			CalcLength(c);	//(10);
	}

	public int GetSpline(int p, ref MegaKnotAnim ma)	//int spl, ref int sp, ref int pt)
	{
		int index = 0;
		int pn = p / 3;
		for ( int i = 0; i < splines.Count; i++ )
		{
			int nx = index + splines[i].knots.Count;

			if ( pn < nx )
			{
				ma.s = i;
				ma.p = pn - index;
				ma.t = p % 3;
				return i;
			}

			index = nx;
		}

		Debug.Log("Cant find point in spline");
		return 0;
	}


	public float GetCurveLength(int curve)
	{
		if ( curve < splines.Count )
			return splines[curve].length;

		return splines[0].length;
	}

	public float CalcLength(int curve, int step)
	{
		if ( curve < splines.Count )
			return splines[curve].CalcLength(step);

		return 0.0f;
	}

	//public float CalcLength(int curve)
	//{
	//	if ( curve < splines.Count )
	//		return splines[curve].CalcLength();

	//	return 0.0f;
	//}

	[ContextMenu("Recalc Length")]
	public void ReCalcLength()
	{
		CalcLength();	//10);
	}

	//public float CalcLength(int step)
	//{
	//	float length = 0.0f;
	//	for ( int i = 0; i < splines.Count; i++ )
	//		length += CalcLength(i, step);

	//	return length;
	//}

	public float CalcLength()
	{
		float length = 0.0f;
		for ( int i = 0; i < splines.Count; i++ )
			length += splines[i].CalcLength();

		return length;
	}

	public float CalcLength(int curve)
	{
		return splines[curve].CalcLength();
	}

	public Vector3 GetKnotPos(int curve, int knot)
	{
		return splines[curve].knots[knot].p;
	}

	public Vector3 GetKnotInVec(int curve, int knot)
	{
		return splines[curve].knots[knot].invec;
	}

	public Vector3 GetKnotOutVec(int curve, int knot)
	{
		return splines[curve].knots[knot].outvec;
	}

	public void SetKnotPos(int curve, int knot, Vector3 p)
	{
		splines[curve].knots[knot].p = p;
		CalcLength();	//10);
	}

	public void SetKnot(int curve, int knot, Vector3 p, Vector3 intan, Vector3 outtan)
	{
		splines[curve].knots[knot].p = p;
		splines[curve].knots[knot].invec = intan;
		splines[curve].knots[knot].outvec = outtan;
		CalcLength();	//10);
	}

	public void MoveKnot(int curve, int knot, Vector3 p)
	{
		Vector3	delta = p - splines[curve].knots[knot].p;
		splines[curve].knots[knot].p = p;
		splines[curve].knots[knot].invec += delta;
		splines[curve].knots[knot].outvec += delta;
		CalcLength();
	}

	public Quaternion GetRotate(int curve, float alpha)
	{
		Vector3 p = InterpCurve3D(curve, alpha, normalizedInterp);
		Vector3 p1 = InterpCurve3D(curve, alpha + 0.001f, normalizedInterp);
		return Quaternion.LookRotation(p - p1);
	}

	public Vector3 InterpCurve3D(int curve, float alpha, bool type)
	{
		Vector3	ret;
		int k = 0;

		if ( curve < splines.Count )
		{
			if ( alpha < 0.0f )
			{
				if ( splines[curve].closed )
					alpha = Mathf.Repeat(alpha, 1.0f);
				else
				{
					Vector3 ps = splines[curve].Interpolate(0.0f, type, ref k);

					// Need a proper tangent function
					Vector3 ps1 = splines[curve].Interpolate(0.01f, type, ref k);

					// Calc the spline in out vecs
					Vector3	delta = ps1 - ps;
					delta.Normalize();
					return ps + ((splines[curve].length * alpha) * delta);
				}
			}
			else
			{
				if ( alpha > 1.0f )
				{
					if ( splines[curve].closed )
						alpha = alpha % 1.0f;
					else
					{
						Vector3 ps = splines[curve].Interpolate(1.0f, type, ref k);

						// Need a proper tangent function
						Vector3 ps1 = splines[curve].Interpolate(0.99f, type, ref k);

						// Calc the spline in out vecs
						Vector3	delta = ps1 - ps;
						delta.Normalize();
						return ps + ((splines[curve].length * (1.0f - alpha)) * delta);
					}
				}
			}

			ret = splines[curve].Interpolate(alpha, type, ref k);
		}
		else
			ret = splines[0].Interpolate(1.0f, type, ref k);

		return ret;
	}

	public Vector3 InterpCurve3D(int curve, float alpha, bool type, ref float twist)
	{
		Vector3	ret;
		int k = 0;

		if ( curve < splines.Count )
		{
			if ( alpha < 0.0f )
			{
				if ( splines[curve].closed )
					alpha = Mathf.Repeat(alpha, 1.0f);
				else
				{
					Vector3 ps = splines[curve].Interpolate(0.0f, type, ref k, ref twist);

					// Need a proper tangent function
					Vector3 ps1 = splines[curve].Interpolate(0.01f, type, ref k, ref twist);

					// Calc the spline in out vecs
					Vector3	delta = ps1 - ps;
					delta.Normalize();
					return ps + ((splines[curve].length * alpha) * delta);
				}
			}
			else
			{
				if ( alpha > 1.0f )
				{
					if ( splines[curve].closed )
						alpha = alpha % 1.0f;
					else
					{
						Vector3 ps = splines[curve].Interpolate(1.0f, type, ref k, ref twist);

						// Need a proper tangent function
						Vector3 ps1 = splines[curve].Interpolate(0.99f, type, ref k, ref twist);

						// Calc the spline in out vecs
						Vector3	delta = ps1 - ps;
						delta.Normalize();
						return ps + ((splines[curve].length * (1.0f - alpha)) * delta);
					}
				}
			}

			ret = splines[curve].Interpolate(alpha, type, ref k, ref twist);
		}
		else
			ret = splines[0].Interpolate(1.0f, type, ref k, ref twist);

		return ret;
	}


	static float lastout = 0.0f;
	static float lastin = -9999.0f;

	static public float veccalc(float angstep)
	{
		if ( lastin == angstep )
			return lastout;

		float totdist;
		float sinfac = Mathf.Sin(angstep);
		float cosfac = Mathf.Cos(angstep);
		float test;
		int ix;
		MegaSpline work = new MegaSpline();
		Vector3 k1 = new Vector3(Mathf.Cos(0.0f), Mathf.Sin(0.0f), 0.0f);
		Vector3 k2 = new Vector3(cosfac, sinfac, 0.0f);

		float hi = 1.5f;
		float lo = 0.0f;
		int count = 200;

		// Loop thru test vectors
	loop:
		work.knots.Clear();
		test = (hi + lo) / 2.0f;
		Vector3 outv = k1 + new Vector3(0.0f, test, 0.0f);
		Vector3 inv = k2 + new Vector3(sinfac * test, -cosfac * test, 0.0f);

		work.AddKnot(k1, k1, outv);
		work.AddKnot(k2, inv, k2);

		totdist = 0.0f;
		int k = 0;
		//totdist = work.CalcLength(10);
		for ( ix = 0; ix < 10; ++ix )
		{
			Vector3 terp = work.Interpolate((float)ix / 10.0f, false, ref k);
			totdist += Mathf.Sqrt(terp.x * terp.x + terp.y * terp.y);
		}

		totdist /= 10.0f;
		count--;

		if ( totdist == 1.0f || count <= 0 )
			goto done;

		if ( totdist > 1.0f )
		{
			hi = test;
			goto loop;
		}
		lo = test;
		goto loop;

	done:
		lastin = angstep;
		lastout = test;
		return test;
	}

	public Vector3 FindNearestPointWorld(Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		Vector3 pos = transform.TransformPoint(FindNearestPoint(transform.worldToLocalMatrix.MultiplyPoint(p), iterations, ref kn, ref tangent, ref alpha));

		tangent = transform.TransformPoint(tangent);
		return pos;
	}

	// Find nearest point
	public Vector3 FindNearestPoint(Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		//Vector3 np = Vector3.zero;

		float positiveInfinity = float.PositiveInfinity;
		float num2 = 0.0f;
		iterations = Mathf.Clamp(iterations, 0, 5);
		int kt = 0;

		int crv = selcurve;
		if ( crv >= splines.Count )
			crv = splines.Count - 1;

		for ( float i = 0.0f; i <= 1.0f; i += 0.01f )
		{
			//Vector3 vector = this.GetPositionOnSpline(i) - p;
			//Vector3 vector = InterpCurve3D(0, i, true) - p;	//this.GetPositionOnSpline(i) - p;
			Vector3 vector = splines[crv].Interpolate(i, true, ref kt) - p;	//this.GetPositionOnSpline(i) - p;
			float sqrMagnitude = vector.sqrMagnitude;
			if ( positiveInfinity > sqrMagnitude )
			{
				positiveInfinity = sqrMagnitude;
				num2 = i;
			}
		}

		for ( int j = 0; j < iterations; j++ )
		{
			float num6 = 0.01f * Mathf.Pow(10.0f, -((float)j));
			float num7 = num6 * 0.1f;
			for ( float k = Mathf.Clamp01(num2 - num6); k <= Mathf.Clamp01(num2 + num6); k += num7 )
			{
				//Vector3 vector2 = InterpCurve3D(0, k, true) - p;	//this.GetPositionOnSpline(k) - p;
				Vector3 vector2 = splines[crv].Interpolate(k, true, ref kt) - p;	//this.GetPositionOnSpline(k) - p;
				float num9 = vector2.sqrMagnitude;

				if ( positiveInfinity > num9 )
				{
					positiveInfinity = num9;
					num2 = k;
				}
			}
		}

		kn = kt;
		tangent = InterpCurve3D(crv, num2 + 0.01f, true);
		alpha = num2;
		return InterpCurve3D(crv, num2, true);	//num2;
		//return np;
	}

	public Vector3 FindNearestPoint(int crv, Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		//Vector3 np = Vector3.zero;

		float positiveInfinity = float.PositiveInfinity;
		float num2 = 0.0f;
		iterations = Mathf.Clamp(iterations, 0, 5);
		int kt = 0;

		if ( crv >= splines.Count )
			crv = splines.Count - 1;

		for ( float i = 0.0f; i <= 1.0f; i += 0.01f )
		{
			//Vector3 vector = this.GetPositionOnSpline(i) - p;
			//Vector3 vector = InterpCurve3D(0, i, true) - p;	//this.GetPositionOnSpline(i) - p;
			Vector3 vector = splines[crv].Interpolate(i, true, ref kt) - p;	//this.GetPositionOnSpline(i) - p;
			float sqrMagnitude = vector.sqrMagnitude;
			if ( positiveInfinity > sqrMagnitude )
			{
				positiveInfinity = sqrMagnitude;
				num2 = i;
			}
		}

		for ( int j = 0; j < iterations; j++ )
		{
			float num6 = 0.01f * Mathf.Pow(10.0f, -((float)j));
			float num7 = num6 * 0.1f;
			for ( float k = Mathf.Clamp01(num2 - num6); k <= Mathf.Clamp01(num2 + num6); k += num7 )
			{
				//Vector3 vector2 = InterpCurve3D(0, k, true) - p;	//this.GetPositionOnSpline(k) - p;
				Vector3 vector2 = splines[crv].Interpolate(k, true, ref kt) - p;	//this.GetPositionOnSpline(k) - p;
				float num9 = vector2.sqrMagnitude;

				if ( positiveInfinity > num9 )
				{
					positiveInfinity = num9;
					num2 = k;
				}
			}
		}

		kn = kt;
		tangent = InterpCurve3D(crv, num2 + 0.01f, true);
		alpha = num2;
		return InterpCurve3D(crv, num2, true);	//num2;
		//return np;
	}

	public void BuildSplineWorld(int curve, Vector3[] points, bool closed)
	{
		if ( curve >= 0 && curve < splines.Count )
		{
			MegaSpline spline = splines[curve];

			spline.knots = new List<MegaKnot>(points.Length);

			for ( int i = 0; i < points.Length; i++ )
			{
				MegaKnot knot = new MegaKnot();
				knot.p = transform.worldToLocalMatrix.MultiplyPoint(points[i]);
				spline.knots.Add(knot);
			}

			spline.closed = closed;
			AutoCurve(spline);
		}
	}

	public void BuildSpline(int curve, Vector3[] points, bool closed)
	{
		if ( curve >= 0 && curve < splines.Count )
		{
			MegaSpline spline = splines[curve];

			spline.knots = new List<MegaKnot>(points.Length);

			for ( int i = 0; i < points.Length; i++ )
			{
				MegaKnot knot = new MegaKnot();
				knot.p = points[i];
				spline.knots.Add(knot);
			}

			spline.closed = closed;
			AutoCurve(spline);
		}
	}

	public void BuildSpline(Vector3[] points, bool closed)
	{
		MegaSpline spline = new MegaSpline();

		spline.knots = new List<MegaKnot>(points.Length);

		for ( int i = 0; i < points.Length; i++ )
		{
			MegaKnot knot = new MegaKnot();
			knot.p = points[i];
			spline.knots.Add(knot);
		}

		spline.closed = closed;
		splines.Add(spline);
		AutoCurve(spline);
	}

	public void AddToSpline(int curve, Vector3[] points)
	{
		if ( curve >= 0 && curve < splines.Count )
		{
			MegaSpline spline = splines[curve];
			int fk = spline.knots.Count;

			for ( int i = 0; i < points.Length; i++ )
			{
				MegaKnot knot = new MegaKnot();
				knot.p = points[i];
				spline.knots.Add(knot);
			}

			AutoCurve(spline, fk, fk + points.Length);
		}
	}

	public void AddToSpline(int curve, Vector3 point)
	{
		if ( curve >= 0 && curve < splines.Count )
		{
			MegaSpline spline = splines[curve];

			MegaKnot knot = new MegaKnot();
			knot.p = point;
			spline.knots.Add(knot);

			AutoCurve(spline, spline.knots.Count - 2, spline.knots.Count - 1);
		}
	}

	public void AutoCurve(int s)
	{
		AutoCurve(splines[s]);
	}

	// Calc tangents for knots
	public void AutoCurve(MegaSpline spline)
	{
		if ( spline.closed )
		{
			Vector3 premid = (spline.knots[spline.knots.Count - 1].p + spline.knots[0].p) * 0.5f;

			for ( int k = 0; k < spline.knots.Count; k++ )
			{
				int nk = (k + 1) % spline.knots.Count;
				Vector3 mid = (spline.knots[nk].p + spline.knots[k].p) * 0.5f;

				Vector3 mp = (mid + premid) * 0.5f;
				spline.knots[k].invec = spline.knots[k].p + ((premid - mp) * smoothness);
				spline.knots[k].outvec = spline.knots[k].p + ((mid - mp) * smoothness);

				premid = mid;
			}
		}
		else
		{
			Vector3 premid = (spline.knots[1].p + spline.knots[0].p) * 0.5f;

			for ( int k = 1; k < spline.knots.Count - 1; k++ )
			{
				Vector3 mid = (spline.knots[k + 1].p + spline.knots[k].p) * 0.5f;

				Vector3 mp = (mid + premid) * 0.5f;
				spline.knots[k].invec = spline.knots[k].p + ((premid - mp) * smoothness);
				spline.knots[k].outvec = spline.knots[k].p + ((mid - mp) * smoothness);

				premid = mid;
			}
		}

		spline.CalcLength();	//10);
	}

	public void AutoCurve(MegaSpline spline, int start, int end)
	{
		if ( spline.closed )
		{
			int pk = (start - 1) % spline.knots.Count;

			Vector3 premid = (spline.knots[pk].p + spline.knots[start].p) * 0.5f;

			for ( int k = start; k < end; k++ )
			{
				int nk = (k + 1) % spline.knots.Count;
				Vector3 mid = (spline.knots[nk].p + spline.knots[k].p) * 0.5f;

				Vector3 mp = (mid + premid) * 0.5f;
				spline.knots[k].invec = spline.knots[k].p + ((premid - mp) * smoothness);
				spline.knots[k].outvec = spline.knots[k].p + ((mid - mp) * smoothness);

				premid = mid;
			}
		}
		else
		{
			int pk = (start - 1) % spline.knots.Count;
			Vector3 premid = (spline.knots[pk].p + spline.knots[start].p) * 0.5f;

			for ( int k = start; k < end - 1; k++ )
			{
				Vector3 mid = (spline.knots[k + 1].p + spline.knots[k].p) * 0.5f;

				Vector3 mp = (mid + premid) * 0.5f;
				spline.knots[k].invec = spline.knots[k].p + ((premid - mp) * smoothness);
				spline.knots[k].outvec = spline.knots[k].p + ((mid - mp) * smoothness);

				premid = mid;
			}
		}

		spline.CalcLength();	//10);
	}


	public void AutoCurve()
	{
		for ( int s = 0; s < splines.Count; s++ )
		{
			MegaSpline spline = splines[s];

			AutoCurve(spline);
#if false
			if ( spline.closed )
			{
				Vector3 premid = (spline.knots[spline.knots.Count - 1].p + spline.knots[0].p) * 0.5f;

				for ( int k = 0; k < spline.knots.Count; k++ )
				{
					int nk = (k + 1) % spline.knots.Count;
					Vector3 mid = (spline.knots[nk].p + spline.knots[k].p) * 0.5f;

					Vector3 mp = (mid + premid) * 0.5f;
					Vector3 delta = spline.knots[k].p - mp;

					spline.knots[k].invec = spline.knots[k].p + (premid - mp);	//premid + delta;
					spline.knots[k].outvec = spline.knots[k].p + (mid - mp);	//mid + delta;

					premid = mid;
				}
			}
			else
			{
				Vector3 premid = (spline.knots[1].p + spline.knots[0].p) * 0.5f;

				for ( int k = 1; k < spline.knots.Count - 1; k++ )
				{
					Vector3 mid = (spline.knots[k + 1].p + spline.knots[k].p) * 0.5f;

					Vector3 mp = (mid + premid) * 0.5f;
					Vector3 delta = spline.knots[k].p - mp;

					spline.knots[k].invec = spline.knots[k].p + (premid - mp);	//premid + delta;
					spline.knots[k].outvec = spline.knots[k].p + (mid - mp);	//mid + delta;

					premid = mid;
				}
			}
#endif
		}

		//CalcLength(10);
	}

	List<Vector3>	verts = new List<Vector3>();
	List<Vector2>	uvs = new List<Vector2>();
	List<int>		tris = new List<int>();
	List<int>		tris1 = new List<int>();
	List<int>		tris2 = new List<int>();

#if true	// tube mesh
	//Vector3[]	verts;
	//Vector2[]	uvs;
	//int[]			tris;
	Vector3[] cross;

	public int tsides = 8;

	void BuildCrossSection(float rad)
	{
		if ( cross == null || cross.Length != tsides )
			cross = new Vector3[tsides];

		float sang = rotate * Mathf.Deg2Rad;

		for ( int i = 0; i < tsides; i++ )
		{
			float ang = sang + (((float)i / (float)tsides) * Mathf.PI * 2.0f);

			cross[i] = new Vector3(Mathf.Sin(ang) * rad, 0.0f, Mathf.Cos(ang) * rad);
		}
	}

	public void BuildTubeMesh()
	{
		// Start, length
		BuildMultiStrandMesh();
	}

	public enum CrossSectionType
	{
		Circle,
		Box,
	}

	public CrossSectionType crossType = CrossSectionType.Circle;

	public float Twist = 0.0f;
	public int strands = 1;
	public float tradius = 0.1f;
	public float offset = 0.0f;
	public float uvtilex = 1.0f;
	public float uvtiley = 1.0f;
	public float uvtwist = 0.0f;
	public float TubeLength = 1.0f;
	public float TubeStart = 0.0f;
	public float SegsPerUnit = 20.0f;
	public float TwistPerUnit = 0.0f;
	public float strandRadius = 0.0f;
	public float startAng = 0.0f;
	public float rotate = 0.0f;
	int segments = 0;
	public bool cap = false;
	Vector3[]	tverts;
	Vector2[]	tuvs;
	int[]	ttris;
	Matrix4x4 tm;
	Matrix4x4 mat;
	Matrix4x4 wtm;
	public MegaAxis RopeUp = MegaAxis.Y;
	Vector3 ropeup = Vector3.up;

	public AnimationCurve scaleX = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public AnimationCurve scaleY = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public bool		unlinkScale = false;

	// Add in twist etc in here to get proper matrix
	Matrix4x4 GetDeformMat(float percent)
	{
		float alpha = percent;
		float twist = 0.0f;

		Vector3 ps	= InterpCurve3D(selcurve, alpha, normalizedInterp, ref twist);
		Vector3 ps1	= InterpCurve3D(selcurve, alpha + 0.001f, normalizedInterp, ref twist);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		//Vector3 scl = ps * 0.99f;

		Quaternion rotation = Quaternion.LookRotation(relativePos, ropeup);	//vertices[p + 1].point - vertices[p].point);
		Quaternion twistrot = Quaternion.Euler(0.0f, 0.0f, twist);
		//wtm.SetTRS(ps, rotation, Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, rotation * twistrot);

		wtm = mat * wtm;	// * roll;
		return wtm;
	}

	public float boxwidth = 0.2f;
	public float boxheight = 0.2f;

	float[] boxuv = new float[8];

	public void BuildBoxCrossSection(float width, float height)
	{
		if ( cross == null || cross.Length != 8 )
			cross = new Vector3[8];

		float sang = rotate * Mathf.Deg2Rad;

		Matrix4x4 mat = Matrix4x4.identity;
		MegaMatrix.RotateY(ref mat, sang);	//rotate);

		cross[0] = new Vector3(width * 0.5f, 0.0f, height * 0.5f);
		cross[1] = new Vector3(width * 0.5f, 0.0f, -height * 0.5f);

		cross[2] = new Vector3(width * 0.5f, 0.0f, -height * 0.5f);
		cross[3] = new Vector3(-width * 0.5f, 0.0f, -height * 0.5f);

		cross[4] = new Vector3(-width * 0.5f, 0.0f, -height * 0.5f);
		cross[5] = new Vector3(-width * 0.5f, 0.0f, height * 0.5f);

		cross[6] = new Vector3(-width * 0.5f, 0.0f, height * 0.5f);
		cross[7] = new Vector3(width * 0.5f, 0.0f, height * 0.5f);

		for ( int i = 0; i < 8; i++ )
		{
			cross[i] = mat.MultiplyPoint(cross[i]);
		}

		float uvlen = (2.0f * boxwidth) + (2.0f * boxheight);
		float ux = 0.0f;

		boxuv[0] = 0.0f;
		ux += boxheight;
		boxuv[1] = ux / uvlen;
		boxuv[2] = boxuv[1];
		ux += boxwidth;
		boxuv[3] = ux / uvlen;
		boxuv[4] = boxuv[3];
		ux += boxheight;
		boxuv[5] = ux / uvlen;
		boxuv[6] = boxuv[5];
		ux += boxwidth;
		boxuv[7] = ux / uvlen;
	}

	public MegaAxis raxis = MegaAxis.X;
	public int ribsegs = 1;

	public void BuildRibbonCrossSection(float width)
	{
		if ( cross == null || cross.Length != ribsegs + 1 )
			cross = new Vector3[ribsegs + 1];

		float sang = rotate * Mathf.Deg2Rad;

		for ( int i = 0; i <= ribsegs; i++ )
		{
			float x = (((float)i / (float)ribsegs) * width) - (width * 0.5f);

			switch ( raxis )
			{
				case MegaAxis.X: cross[i] = new Vector3(x, 0.0f, 0.0f);	break;
				case MegaAxis.Y: cross[i] = new Vector3(0.0f, x, 0.0f); break;
				case MegaAxis.Z: cross[i] = new Vector3(0.0f, 0.0f, x); break;
			}
		}

		Matrix4x4 mat = Matrix4x4.identity;
		MegaMatrix.RotateY(ref mat, sang);	//rotate);

		for ( int i = 0; i < cross.Length; i++ )
		{
			cross[i] = mat.MultiplyPoint(cross[i]);
		}
	}

	// Width, segs

	void BuildRibbonMesh()
	{
		//float lengthuvtile = uvtiley * TubeLength;

		TubeLength = Mathf.Clamp01(TubeLength);
		if ( TubeLength == 0.0f || strands < 1 )
		{
			shapemesh.Clear();
			return;
		}

		//float sradius = (tradius * 0.5f) + strandRadius;

		BuildRibbonCrossSection(boxwidth);

		segments = (int)((splines[0].length * TubeLength) / (stepdist * 0.1f));

		Twist = TwistPerUnit;	// * TubeLength;
		float sang = startAng * Mathf.Deg2Rad;

		int vcount = ((segments + 1) * (ribsegs + 1)) * strands;
		int tcount = (ribsegs * 2 * segments) * strands;

		//Debug.Log("segs " + segments);
		//Debug.Log("verts " + vcount);
		//Debug.Log("tris " + tcount);

		float off = (tradius * 0.5f) + offset;

		//float off = offset;
		if ( strands == 1 )
		{
			off = offset;
		}

		if ( tverts == null || tverts.Length != vcount )
			tverts = new Vector3[vcount];

		//bool builduvs = false;

		if ( GenUV && (tuvs == null || tuvs.Length != vcount) )
		{
			tuvs = new Vector2[vcount];
			//builduvs = true;
		}

		if ( ttris == null || ttris.Length != tcount * 3 )
		{
			ttris = new int[tcount * 3];
		}

		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		MegaMatrix.SetTrans(ref tm, Pivot);

		switch ( RopeUp )
		{
			case MegaAxis.X: ropeup = Vector3.right; break;
			case MegaAxis.Y: ropeup = Vector3.up; break;
			case MegaAxis.Z: ropeup = Vector3.forward; break;
		}

		// We only need to refresh the verts, tris and uvs are done once
		int vi = 0;
		int ti = 0;

		Vector2 uv = Vector2.zero;
		Vector3 soff = Vector3.zero;
		Vector3 scl = Vector3.one;

		for ( int s = 0; s < strands; s++ )
		{
			//rollingquat = Quaternion.identity;

			float ang = ((float)s / (float)strands) * Mathf.PI * 2.0f;

			soff.x = Mathf.Sin(ang) * off;
			soff.z = Mathf.Cos(ang) * off;
			//Matrix.SetTrans(ref tm, soff);

			int vo = vi;

			vo = vi;

			for ( int i = 0; i <= segments; i++ )
			{
				float alpha = TubeStart + (((float)i / (float)segments) * TubeLength);

				wtm = GetDeformMat(alpha);

				float uvt = alpha * uvtwist;

				float tst = sang + ((alpha - TubeStart) * Twist * Mathf.PI * 2.0f);	// + rollang;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);

				float cuv = (float)(cross.Length - 1);
				for ( int v = 0; v < cross.Length; v++ )
				{
					Vector3 cp = cross[v];
					cp.x *= scl.x;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( GenUV )	//builduvs )
					{
						uv.y = ((alpha - TubeStart) * splines[0].length * uvtiley) + UVOffset.y;
						uv.x = (((float)v / cuv) * uvtilex) + uvt + UVOffset.x;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}
				// Uv is - to 1 around and alpha along
			}

			if ( GenUV )	//builduvs )
			{
				int sc = ribsegs + 1;
				for ( int i = 0; i < segments; i++ )
				{
					for ( int v = 0; v < cross.Length - 1; v++ )
					{
						ttris[ti++] = ((i + 1) * sc) + v + vo;
						ttris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
						ttris[ti++] = (i * sc) + v + vo;

						ttris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
						ttris[ti++] = (i * sc) + ((v + 1) % sc) + vo;
						ttris[ti++] = (i * sc) + v + vo;
					}
				}
			}
		}

		// Conform
		if ( conform )
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}

		//Mesh mesh = MegaUtils.GetMesh(gameObject);
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;

		if ( GenUV )	//builduvs )
		{
			shapemesh.uv = tuvs;
		}
		else
		{
			//shapemesh.vertices = tverts;
		}

		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		if ( CalcTangents )
			MegaUtils.BuildTangents(shapemesh);
	}

	void BuildBoxMesh()
	{
		//float lengthuvtile = uvtiley * TubeLength;

		TubeLength = Mathf.Clamp01(TubeLength);
		if ( TubeLength == 0.0f || strands < 1)
		{
			shapemesh.Clear();
			return;
		}

		//float sradius = (tradius * 0.5f) + strandRadius;

		BuildBoxCrossSection(boxwidth, boxheight);

		segments = (int)((splines[0].length * TubeLength) / (stepdist * 0.1f));

		Twist = TwistPerUnit;	// * TubeLength;
		float sang = startAng * Mathf.Deg2Rad;

		int vcount = 9 * (segments + 1) * strands;
		int tcount = (8 * segments) * strands;

		float off = (tradius * 0.5f) + offset;

		//float off = offset;
		if ( strands == 1 )
		{
			off = offset;
		}
		if ( cap )
		{
			vcount += 8 * strands;
			tcount += 4 * strands;
		}

		if ( tverts == null || tverts.Length != vcount )
			tverts = new Vector3[vcount];

		bool builduvs = false;

		if ( GenUV && (tuvs == null || tuvs.Length != vcount) )
		{
			tuvs = new Vector2[vcount];
			builduvs = true;
		}

		if ( ttris == null || ttris.Length != tcount * 3 )
		{
			ttris = new int[tcount * 3];
		}

		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		switch ( RopeUp )
		{
			case MegaAxis.X: ropeup = Vector3.right; break;
			case MegaAxis.Y: ropeup = Vector3.up; break;
			case MegaAxis.Z: ropeup = Vector3.forward; break;
		}

		MegaMatrix.SetTrans(ref tm, Pivot);

		// We only need to refresh the verts, tris and uvs are done once
		int vi = 0;
		int ti = 0;

		Vector2 uv = Vector2.zero;
		Vector3 soff = Vector3.zero;
		Vector3 scl = Vector3.one;

		for ( int s = 0; s < strands; s++ )
		{
			//rollingquat = Quaternion.identity;

			float ang = ((float)s / (float)strands) * Mathf.PI * 2.0f;

			soff.x = Mathf.Sin(ang) * off;
			soff.z = Mathf.Cos(ang) * off;
			//Matrix.SetTrans(ref tm, soff);

			int vo = vi;

			// Cap maybe needs to be submesh, at least needs seperate verts
			if ( cap )
			{
				// Add slice at 0
				float alpha = TubeStart;	//0.0f;
				wtm = GetDeformMat(alpha);

				//float uvt = alpha * uvtwist;

				float tst = sang + (0.0f * Twist * Mathf.PI * 2.0f);
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				for ( int v = 0; v < 4; v++ )
				{
					Vector3 cp = cross[v * 2];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}

				//if ( GenUV )	//builduvs )
				{
					//for ( int sd = 1; sd < 2; sd++ )
					{
						ttris[ti++] = vo;
						ttris[ti++] = vo + 2;
						ttris[ti++] = vo + 1;

						ttris[ti++] = vo;
						ttris[ti++] = vo + 3;
						ttris[ti++] = vo + 2;
					}
				}

				vo = vi;

				// Other end
				alpha = TubeStart + TubeLength;	//.0f;
				wtm = GetDeformMat(alpha);

				//uvt = alpha * uvtwist;

				tst = sang + (TubeLength * Twist * Mathf.PI * 2.0f);
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				for ( int v = 0; v < 4; v++ )
				{
					Vector3 cp = cross[v * 2];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( GenUV )	//builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}

				//if ( GenUV )	//builduvs )
				{
					//for ( int sd = 1; sd < 2; sd++ )
					{
						ttris[ti++] = vo;
						ttris[ti++] = vo + 1;
						ttris[ti++] = vo + 2;

						ttris[ti++] = vo;
						ttris[ti++] = vo + 2;
						ttris[ti++] = vo + 3;
					}
				}
			}

			vo = vi;

			for ( int i = 0; i <= segments; i++ )
			{
				float alpha = TubeStart + (((float)i / (float)segments) * TubeLength);

				wtm = GetDeformMat(alpha);

				float uvt = alpha * uvtwist;

				float tst = sang + ((alpha - TubeStart) * Twist * Mathf.PI * 2.0f);	// + rollang;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				for ( int v = 0; v < cross.Length; v++ )
				{
					Vector3 cp = cross[v];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( GenUV )	//builduvs )
					{
						//uv.y = (alpha - TubeStart) * lengthuvtile * splines[0].length;	//uvtiley;
						uv.y = ((alpha - TubeStart) * splines[0].length * uvtiley) + UVOffset.y;
						//uv.x = (((float)v / (float)cross.Length) * uvtilex) + uvt;

						uv.x = (boxuv[v] * uvtilex) + uvt + UVOffset.x;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}
				// Uv is - to 1 around and alpha along
			}

			if ( GenUV )	//builduvs )
			{
				int sc = 8;
				for ( int i = 0; i < segments; i++ )
				{
					for ( int v = 0; v < 4; v++ )
					{
						int v2 = v * 2;
						ttris[ti++] = (i * sc) + v2 + vo;
						ttris[ti++] = ((i + 1) * sc) + (v2 + 1) + vo;
						ttris[ti++] = ((i + 1) * sc) + v2 + vo;

						ttris[ti++] = (i * sc) + v2 + vo;
						ttris[ti++] = (i * sc) + (v2 + 1) + vo;
						ttris[ti++] = ((i + 1) * sc) + (v2 + 1) + vo;
					}
				}
			}
		}

		// Conform
		if ( conform )
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}

		//Mesh mesh = MegaUtils.GetMesh(gameObject);
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;

		if ( GenUV )	//builduvs )
		{
			shapemesh.uv = tuvs;
		}
		else
		{
			//shapemesh.vertices = tverts;
		}

		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		if ( CalcTangents )
			MegaUtils.BuildTangents(shapemesh);
	}

	void BuildMultiStrandMesh()
	{
		//float lengthuvtile = uvtiley * TubeLength;

		TubeLength = Mathf.Clamp01(TubeLength);
		if ( TubeLength == 0.0f || strands < 1 )
		{
			shapemesh.Clear();
			return;
		}
		
		Twist = TwistPerUnit;	// * TubeLength;
		//segments = (int)(RopeLength * SegsPerUnit);
		segments = (int)((splines[selcurve].length * TubeLength) / (stepdist * 0.1f));

		float sang = startAng * Mathf.Deg2Rad;

		float off = (tradius * 0.5f) + offset;

		if ( strands == 1 )
		{
			off = offset;
		}

		float sradius = (tradius * 0.5f) + strandRadius;
		BuildCrossSection(sradius);

		int vcount = ((segments + 1) * (tsides + 1)) * strands;
		int tcount = ((tsides * 2) * segments) * strands;

		//Debug.Log("segs " + segments);
		//Debug.Log("verts " + vcount);
		//Debug.Log("tris " + tcount);

		if ( cap )
		{
			vcount += ((tsides + 1) * 2) * strands;
			tcount += (tsides * 2) * strands;
		}

		if ( tverts == null || tverts.Length != vcount )
		{
			tverts = new Vector3[vcount];
		}

		bool builduvs = false;

		if ( GenUV && (tuvs == null || tuvs.Length != vcount) )
		{
			tuvs = new Vector2[vcount];
			builduvs = true;
		}

		if ( ttris == null || ttris.Length != tcount * 3 )
		{
			ttris = new int[tcount * 3];
		}
		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		MegaMatrix.SetTrans(ref tm, Pivot);

		switch ( RopeUp )
		{
			case MegaAxis.X: ropeup = Vector3.right; break;
			case MegaAxis.Y: ropeup = Vector3.up; break;
			case MegaAxis.Z: ropeup = Vector3.forward; break;
		}
		// We only need to refresh the verts, tris and uvs are done once
		int vi = 0;
		int ti = 0;

		Vector2 uv = Vector2.zero;
		Vector3 soff = Vector3.zero;

		Vector3 scl = Vector3.one;

		for ( int s = 0; s < strands; s++ )
		{
			//rollingquat = Quaternion.identity;

			float ang = ((float)s / (float)strands) * Mathf.PI * 2.0f;

			soff.x = Mathf.Sin(ang) * off;
			soff.z = Mathf.Cos(ang) * off;
			//Matrix.SetTrans(ref tm, soff);

			int vo = vi;

			// Cap maybe needs to be submesh, at least needs seperate verts
			if ( cap )
			{
				// Add slice at 0
				float alpha = TubeStart;	//0.0f;
				wtm = GetDeformMat(alpha);

				//float uvt = alpha * uvtwist;

				float tst = sang + ((alpha - TubeStart) * Twist * Mathf.PI * 2.0f);
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 cp = cross[v % cross.Length];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}

				if ( GenUV )	//builduvs )
				{
					for ( int sd = 1; sd < tsides; sd++ )
					{
						ttris[ti++] = vo;
						ttris[ti++] = vo + sd + 1;
						ttris[ti++] = vo + sd;
					}
				}

				vo = vi;

				// Other end
				alpha = TubeStart + TubeLength;	//.0f;
				wtm = GetDeformMat(alpha);

				//uvt = alpha * uvtwist;

				tst = sang + ((alpha - TubeStart) * Twist * Mathf.PI * 2.0f);
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 cp = cross[v % cross.Length];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( GenUV )	//builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}

				if ( GenUV )	//builduvs )
				{
					for ( int sd = 1; sd < tsides; sd++ )
					{
						ttris[ti++] = vo;
						ttris[ti++] = vo + sd;
						ttris[ti++] = vo + sd + 1;
					}
				}
			}

			vo = vi;

			for ( int i = 0; i <= segments; i++ )
			{
				float alpha = TubeStart + (((float)i / (float)segments) * TubeLength);

				scl.x = scaleX.Evaluate(alpha);
				if ( unlinkScale )
					scl.z = scaleY.Evaluate(alpha);
				else
					scl.z = scl.x;

				wtm = GetDeformMat(alpha);

				float uvt = alpha * uvtwist;

				float tst = sang + ((alpha - TubeStart) * Twist * Mathf.PI * 2.0f);	// + rollang;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 cp = cross[v % cross.Length];
					cp.x *= scl.x;
					cp.z *= scl.z;

					Vector3 p = tm.MultiplyPoint3x4(cp + soff);	//cross[v % cross.Length] + soff);
					tverts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( GenUV )	//builduvs )
					{
						//uv.y = alpha * lengthuvtile;	//uvtiley;
						uv.y = ((alpha - TubeStart) * splines[0].length * uvtiley) + UVOffset.y;
						uv.x = (((float)v / (float)cross.Length) * uvtilex) + uvt + UVOffset.x;

						tuvs[vi++] = uv;
					}
					else
						vi++;
				}
				// Uv is - to 1 around and alpha along
			}

			if ( GenUV )	//builduvs )
			{
				int sc = tsides + 1;
				for ( int i = 0; i < segments; i++ )
				{
					for ( int v = 0; v < cross.Length; v++ )
					{
						ttris[ti++] = (i * sc) + v + vo;
						ttris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
						ttris[ti++] = ((i + 1) * sc) + v + vo;

						ttris[ti++] = (i * sc) + v + vo;
						ttris[ti++] = (i * sc) + ((v + 1) % sc) + vo;
						ttris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
					}
				}
			}
		}

		// Conform
		if ( conform )
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}

		//Mesh mesh = MegaUtils.GetMesh(gameObject);
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;

		if ( GenUV )	//builduvs )
		{
			shapemesh.uv = tuvs;
		}
		else
		{
			//shapemesh.vertices = tverts;
		}

		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		//MeshConstructor.BuildTangents(mesh);
		if ( CalcTangents )
			MegaUtils.BuildTangents(shapemesh);
	}


#endif	// tube mesh

	public void ClearMesh()
	{
		MeshFilter mf = gameObject.GetComponent<MeshFilter>();

		if ( mf != null )
		{
			mf.sharedMesh = null;
			shapemesh = null;
		}
	}

	public void SetMats()
	{
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		if ( mr == null )
		{
			mr = gameObject.AddComponent<MeshRenderer>();
		}

		if ( meshType == MeshShapeType.Fill )
		{
			Material[] mats = new Material[3];

			mats[0] = mat1;
			mats[1] = mat2;
			mats[2] = mat3;

			mr.sharedMaterials = mats;
		}
		else
		{
			Material[] mats = new Material[1];

			mats[0] = mat1;

			mr.sharedMaterials = mats;
		}
	}

	// Best if we calc the normals to avoid issues at join
	public void BuildMesh()
	{
		if ( makeMesh )
		{
			if ( shapemesh == null )
			{
				MeshFilter mf = gameObject.GetComponent<MeshFilter>();

				if ( mf == null )
					mf = gameObject.AddComponent<MeshFilter>();

				mf.sharedMesh = new Mesh();
				MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
				if ( mr == null )
				{
					mr = gameObject.AddComponent<MeshRenderer>();
				}

				SetMats();

				shapemesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);
			}

			if ( meshType == MeshShapeType.Tube )
			{
				BuildTubeMesh();
				return;
			}

			if ( meshType == MeshShapeType.Box )
			{
				BuildBoxMesh();
				return;
			}

			if ( meshType == MeshShapeType.Ribbon )
			{
				BuildRibbonMesh();
				return;
			}

			//makeMesh = false;

			float sdist = stepdist * 0.1f;
			if ( splines[selcurve].length / sdist > 1500.0f )
				sdist = splines[selcurve].length / 1500.0f;

			Vector3 size = Vector3.zero;
			verts.Clear();
			uvs.Clear();
			tris.Clear();
			tris1.Clear();
			tris2.Clear();
			tris = MegaTriangulator.Triangulate(this, splines[selcurve], sdist, ref verts, ref uvs, ref tris, Pivot, ref size);


			if ( axis != MegaAxis.Y )
			{
				for ( int i = 0; i < tris.Count; i += 3 )
				{
					int t = tris[i];
					tris[i] = tris[i + 2];
					tris[i + 2] = t;
				}
			}

			int vcount = verts.Count;
			int tcount = tris.Count;

			if ( Height < 0.0f )
				Height = 0.0f;
			float h = Height;	//Mathf.Abs(Height);

			Matrix4x4 tm1 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0.0f), new Vector3(UVScale.x, 1.0f, UVScale.y));

			//Vector3 size = shapemesh.bounds.size;

			if ( GenUV )
			{
				uvs.Clear();	// need to stop triangulator doing uvs
				Vector2 uv = Vector2.zero;
				Vector3 uv1 = Vector3.zero;

				int uvx = 0;
				int uvy = 2;
				switch ( axis )
				{
					case MegaAxis.X:
						uvx = 1;
						break;

					case MegaAxis.Z:
						uvy = 1;
						break;
				}

				for ( int i = 0; i < verts.Count; i++ )
				{
					//uv1.x = (verts[i].x);	// * UVScale.x) + UVOffset.x;	// * UVScale.x;
					//uv1.z = (verts[i].z);	// * UVScale.y) + UVOffset.y;	// * UVScale.y;
					uv1.x = verts[i][uvx];	// * UVScale.x) + UVOffset.x;	// * UVScale.x;
					uv1.z = verts[i][uvy];	// * UVScale.y) + UVOffset.y;	// * UVScale.y;

					if ( !PhysUV )
					{
						uv1.x /= size[uvx];	//.x;
						uv1.z /= size[uvy];	//.z;
					}
					uv1 = tm1.MultiplyPoint3x4(uv1);
					uv.x = uv1.x + UVOffset.x;
					uv.y = uv1.z + UVOffset.y;
					uvs.Add(uv);
				}
			}

			if ( DoubleSided && h != 0.0f )
			{
				//vcount = verts.Count;
				for ( int i = 0; i < vcount; i++ )
				{
					Vector3 p = verts[i];

					if ( UseHeightCurve )
					{
						float alpha = MegaTriangulator.m_points[i].z / splines[selcurve].length;
						//p.y -= h * heightCrv.Evaluate(alpha + heightOff);
						p[(int)axis] -= h * heightCrv.Evaluate(alpha + heightOff);
					}
					else
					{
						//p.y -= h;
						p[(int)axis] -= h;
					}
					verts.Add(p);	//verts[i]);
					uvs.Add(uvs[i]);
				}

				//tcount = tris.Count;
#if false
				switch ( axis )
				{
					case MegaAxis.X:
						for ( int i = tcount - 1; i >= 0; i-- )
						{
							tris1.Add(tris[i] + vcount);
						}
						break;

					case MegaAxis.Y:
						for ( int i = tcount - 1; i >= 0; i-- )
						{
							tris1.Add(tris[i] + vcount);
						}
						break;

					case MegaAxis.Z:
						for ( int i = 0; i < tcount; i++ )
						{
							tris1.Add(tris[i] + vcount);
						}
						break;

				}
#endif
				for ( int i = tcount - 1; i >= 0; i-- )
				{
					tris1.Add(tris[i] + vcount);
				}
			}
#if true
			// Do edge
			if ( h != 0.0f )
			{
				int vc = verts.Count;

				Vector3 ep = Vector3.zero;
				Vector2 euv = Vector2.zero;
				
				tm1 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate1.x, UVRotate1.y, 0.0f), new Vector3(UVScale1.x, 1.0f, UVScale1.y));

				// Top loop
				for ( int i = 0; i < MegaTriangulator.m_points.Count; i++ )
				{
					ep = verts[i];
					//ep.x = MegaTriangulator.m_points[i].x;
					//ep.y = 0.0f;
					//ep.z = MegaTriangulator.m_points[i].y;
					verts.Add(ep);

					//euv.x = (MegaTriangulator.m_points[i].z / splines[0].length) * 4.0f;
					//euv.x = (MegaTriangulator.m_points[i].z * UVScale1.x) + UVOffset1.x;	// / splines[0].length) * 4.0f;
					//euv.y = UVOffset1.y;	//0.0f;

					ep.x = (MegaTriangulator.m_points[i].z);	// * UVScale1.x) + UVOffset1.x;	// / splines[0].length) * 4.0f;
					
					if ( !PhysUV )
					{
						ep.x /= size.x;
					}

					ep.y = 0.0f;
					ep.z = 0.0f;	//UVOffset1.y;	//0.0f;

					ep = tm1.MultiplyPoint3x4(ep);
					euv.x = ep.x + UVOffset1.x;
					euv.y = ep.z + UVOffset1.y;

					uvs.Add(euv);
				}
				// Add first point again
				ep = verts[0];
				//ep.y -= h * heightCrv.Evaluate(0.0f);
				verts.Add(ep);

				//euv.x = 1.0f * 4.0f;	//MegaTriangulator.m_points[0].z / splines[0].length;
				euv.x = (splines[selcurve].length * UVScale1.x) + UVOffset1.x;	//1.0f * 4.0f;	//MegaTriangulator.m_points[0].z / splines[0].length;
				if ( !PhysUV )
				{
					euv.x /= size.x;
				}
				euv.y = 0.0f + UVOffset1.y;
				uvs.Add(euv);

				// Bot loop
				float hd = 1.0f;

				for ( int i = 0; i < MegaTriangulator.m_points.Count; i++ )
				{
					float alpha = MegaTriangulator.m_points[i].z / splines[selcurve].length;

					ep = verts[i];
					if ( UseHeightCurve )
						hd = heightCrv.Evaluate(alpha + heightOff);

					//ep.y -=  h * hd;	//heightCrv.Evaluate(alpha);
					ep[(int)axis] -= h * hd;

					verts.Add(ep);

					ep.x = (MegaTriangulator.m_points[i].z);	// * UVScale1.x) + UVOffset1.x;	// / splines[0].length) * 4.0f;
					ep.z = ep.y;	//0.0f;	//UVOffset1.y;	//0.0f;
					ep.y = 0.0f;

					if ( !PhysUV )
					{
						ep.x /= size.x;
						ep.z /= (h * hd);
					}
					ep = tm1.MultiplyPoint3x4(ep);
					euv.x = ep.x + UVOffset1.x;
					euv.y = ep.z + UVOffset1.y;

					//euv.x = (MegaTriangulator.m_points[i].z / splines[0].length) * 4.0f;
					//euv.x = (MegaTriangulator.m_points[i].z * UVScale1.x) + UVOffset1.x;
					//euv.y = ((h * hd) * UVScale1.y) + UVOffset1.y;	//1.0f;
					uvs.Add(euv);
				}
				// Add first point again
				ep = verts[0];

				if ( UseHeightCurve )
				{
					hd = heightCrv.Evaluate(0.0f + heightOff);
				}

				//ep.y -= h * hd;	//heightCrv.Evaluate(0.0f);
				ep[(int)axis] -= h * hd;
				verts.Add(ep);

				ep.x = (MegaTriangulator.m_points[0].z);	// * UVScale1.x) + UVOffset1.x;	// / splines[0].length) * 4.0f;
				ep.z = ep.y;	//0.0f;	//UVOffset1.y;	//0.0f;
				ep.y = 0.0f;

				if ( !PhysUV )
				{
					ep.x /= size.x;
					ep.z /= (h * hd);
				}
				ep = tm1.MultiplyPoint3x4(ep);
				euv.x = ep.x + UVOffset1.x;
				euv.y = ep.z + UVOffset1.y;

				//euv.x = (MegaTriangulator.m_points[i].z / splines[0].length) * 4.0f;
				//euv.x = (MegaTriangulator.m_points[i].z * UVScale1.x) + UVOffset1.x;
				//euv.y = ((h * hd) * UVScale1.y) + UVOffset1.y;	//1.0f;
				uvs.Add(euv);

				//euv.x = 1.0f;	//MegaTriangulator.m_points[0].z / splines[0].length;
				//euv.x = (splines[0].length * UVScale1.x) + UVOffset1.x;	//MegaTriangulator.m_points[0].z / splines[0].length;
				//euv.y = (h * hd * UVScale1.y) + UVOffset1.y;	//1.0f;

				//if ( !PhysUV )
				//{
				//	euv.x /= size.x;
				//	euv.y /= 
				//}
				//uvs.Add(euv);

				// Faces
				int ecount = MegaTriangulator.m_points.Count + 1;

				int ip = 0;
				if ( splines[selcurve].reverse )
				{
					for ( ip = 0; ip < MegaTriangulator.m_points.Count; ip++ )
					{
						tris2.Add(ip + vc + 1);
						tris2.Add(ip + vc + ecount);
						tris2.Add(ip + vc);

						tris2.Add(ip + vc + ecount + 1);
						tris2.Add(ip + vc + ecount);
						tris2.Add(ip + vc + 1);
					}
				}
				else
				{
					for ( ip = 0; ip < MegaTriangulator.m_points.Count; ip++ )
					{
						tris2.Add(ip + vc);
						tris2.Add(ip + vc + ecount);
						tris2.Add(ip + vc + 1);

						tris2.Add(ip + vc + 1);
						tris2.Add(ip + vc + ecount);
						tris2.Add(ip + vc + ecount + 1);
					}
				}
#if false
#else
#endif

#if false
				tris.Add(ip + vc);
				tris.Add(ip + vc + ecount);
				tris.Add(vc);

				tris.Add(vc);
				tris.Add(ip + vc + ecount);
				tris.Add(vc + ecount);
#endif
			}
#endif

			Vector3[] tverts = verts.ToArray();
			// Conform
			if ( conform )
			{
				CalcBounds(tverts);
				DoConform(tverts);
			}

			shapemesh.Clear();

			shapemesh.vertices = tverts;	//verts.ToArray();
			shapemesh.uv = uvs.ToArray();

			shapemesh.subMeshCount = 3;
			shapemesh.SetTriangles(tris.ToArray(), 0);
			shapemesh.SetTriangles(tris1.ToArray(), 1);
			shapemesh.SetTriangles(tris2.ToArray(), 2);

			//shapemesh.triangles = tris.ToArray();
			shapemesh.RecalculateNormals();
			shapemesh.RecalculateBounds();

			if ( CalcTangents )
				MegaUtils.BuildTangents(shapemesh);

			//if ( mesh != null )
			//{
				//BuildMesh(mesh);
				//MegaModifyObject mo = GetComponent<MegaModifyObject>();
				//if ( mo != null )
				//{
				//	mo.MeshUpdated();
				//}
			//}
		}
	}

#if true
	static int CURVELENGTHSTEPS = 5;
	static float CurveLength(MegaSpline spline, int knot, float v1, float v2, float size)
	{
		float len = 0.0f;
		if ( size == 0.0f )
		{   // Simple curve length
			Vector3 p1,p2;
			p1 = spline.InterpBezier3D(knot, v1);
			//Debug.Log("p1 " + p1);
			float step = (v2 - v1) / (float)CURVELENGTHSTEPS;
			//Debug.Log("Step " + step);
			float pos;
			int i;
			for ( i = 1, pos = step; i < CURVELENGTHSTEPS; ++i, pos += step )
			{
				p2 = spline.InterpBezier3D(knot, v1 + pos);
				len += Vector3.Magnitude(p2 - p1);
				p1 = p2;
			}
			//Debug.Log("len " + len);
			len += Vector3.Magnitude(spline.InterpBezier3D(knot, v2) - p1);
			//Debug.Log("len " + len);
		}
		else
		{   // Need to figure based on displaced location
			int knots = spline.knots.Count;
			int prev = (knot + knots - 1) % knots;
			int next = (knot + 1) % knots;
			float pv = v1 - 0.01f;
			int pk = knot;
			if ( pv < 0.0f )
			{
				if ( spline.closed )
				{
					pv += 1.0f;
					pk = prev;
				}
				else
					pv = 0.0f;
			}
			float nv = v1 + 0.01f;
			Vector3 direction = Vector3.Normalize(spline.InterpBezier3D(knot, nv) - spline.InterpBezier3D(pk, pv));
			//direction.z = 0.0f;  // Keep it in the XY plane
			//Vector3 perp = new Vector3(direction.y * size, -direction.x * size, 0.0f);
			direction.y = 0.0f;  // Keep it in the XY plane
			Vector3 perp = new Vector3(direction.z * size, 0.0f, -direction.x * size);

			Vector3 p1,p2;
			p1 = spline.InterpBezier3D(knot, v1) + perp;   // Got 1st displaced point

			float step = (v2 - v1) / CURVELENGTHSTEPS;
			float pos;
			int i;
			for ( i = 1, pos = step; i < CURVELENGTHSTEPS; ++i, pos += step )
			{
				pv = v1 + pos - 0.01f;
				nv = v1 + pos + 0.01f;
				direction = Vector3.Normalize(spline.InterpBezier3D(knot, nv) - spline.InterpBezier3D(knot, pv));
				//direction.z = 0.0f;  // Keep it in the XY plane
				//perp = new Vector3(direction.y * size, -direction.x * size, 0.0f);
				direction.y = 0.0f;  // Keep it in the XY plane
				perp = new Vector3(direction.z * size, 0.0f, -direction.x * size);

				p2 = spline.InterpBezier3D(knot, v1 + pos) + perp;
				len += Vector3.Magnitude(p2 - p1);
				p1 = p2;
			}
			pv = v2 - 0.01f;
			int nk = knot;
			nv = v2 + 0.01f;
			if ( nv > 1.0f )
			{
				if ( spline.closed )
				{
					nv -= 1.0f;
					nk = next;
				}
				else
					nv = 1.0f;
			}
			direction = Vector3.Normalize(spline.InterpBezier3D(nk, nv) - spline.InterpBezier3D(knot, pv));
			//direction.z = 0.0f;  // Keep it in the XY plane
			//perp = new Vector3(direction.y * size, -direction.x * size, 0.0f);
			direction.y = 0.0f;  // Keep it in the XY plane
			perp = new Vector3(direction.z * size, 0.0f, -direction.x * size);

			len += Vector3.Magnitude((spline.InterpBezier3D(knot, v2) + perp) - p1);
		}
		return len;
	}

	// Outline test

	public void OutlineSpline(MegaShape shape, int poly, float size, bool centered)
	{
		MegaSpline inSpline = shape.splines[poly];
		MegaSpline outSpline = new MegaSpline();

		OutlineSpline(inSpline, outSpline, size, centered);

		shape.splines.Add(outSpline);
		outSpline.CalcLength();	//10);
	}

	public void OutlineSpline(MegaSpline inSpline, MegaSpline outSpline, float size, bool centered)
	{
		// Do some basic calculations that we'll need regardless
		float size1 = (centered) ? size / 2.0f : 0.0f;  // First phase offset
		//float size2 = (centered) ? -size / 2.0f : -size;   // Second phase offset
		int knots = inSpline.knots.Count;
		//Vector3 knot, invec, outvec;
		int i;
		//Matrix4x4 theMatrix;

		outSpline.knots.Clear();

		// If the input spline is closed, we wind up with two polygons
		if ( inSpline.closed )
		{
			///MegaSpline outSpline2 = new MegaSpline();	//shape->NewSpline();
			// Generate the outline polygons...
			for ( i = 0; i < knots; ++i )
			{
				int prevKnot = (i + knots - 1) % knots;
				float oldInLength = CurveLength(inSpline, prevKnot, 0.5f, 1.0f, 0.0f);
				float oldOutLength = CurveLength(inSpline, i, 0.0f, 0.5f, 0.0f);
				
				//Debug.Log("oldlens " + oldInLength + " " + oldOutLength);

				//int knotType = 0;	//inSpline->GetKnotType(i);
				// Determine the angle of the curve at this knot
				// Get vector from interp before knot to interp after knot
				Vector3 ko = inSpline.knots[i].p;	//->GetKnotPoint(i);
				//Debug.Log("ko " + ko);
				Vector3 bVec = Vector3.Normalize(inSpline.InterpBezier3D(prevKnot, 0.99f) - ko);
				Vector3 fVec = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
				Vector3 direction = Vector3.Normalize(fVec - bVec);
				//direction.z = 0.0f;  // Keep it in the XY plane
				direction.y = 0.0f;  // Keep it in the XY plane
				// Figure the size multiplier for the crotch angle
				float dot = Vector3.Dot(bVec, fVec);
				float angle, wsize1;	//, wsize2;
				if ( dot >= -0.9999939f )
					angle = -Mathf.Acos(dot) / 2.0f;
				else
					angle = Mathf.PI * 0.5f;

				float base1 = size1 / Mathf.Tan(angle);
				float sign1 = (size1 < 0.0f) ? -1.0f : 1.0f;
				wsize1 = Mathf.Sqrt(base1 * base1 + size1 * size1) * sign1;
				//float base2 = size2 / Mathf.Tan(angle);
				//float sign2 = (size2 < 0.0f) ? -1.0f : 1.0f;
				//wsize2 = Mathf.Sqrt(base2 * base2 + size2 * size2) * sign2;

				//Vector3 perp = new Vector3(direction.y * wsize1, -direction.x * wsize1, 0.0f);
				Vector3 perp = new Vector3(direction.z * wsize1, 0.0f, -direction.x * wsize1);
				float newInLength = CurveLength(inSpline, prevKnot, 0.5f, 1.0f, size1);
				float newOutLength = CurveLength(inSpline, i, 0.0f, 0.5f, size1);
				//Debug.Log("newlens " + newInLength + " " + newOutLength);
				//Debug.Log("i " + i + " prev " + prevKnot);
				Vector3 kn = ko + perp;
				//Debug.Log("kn " + kn);
				float inMult = newInLength / oldInLength;
				float outMult = newOutLength / oldOutLength;
				//MegaKnot k(knotType, LTYPE_CURVE, kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
				outSpline.AddKnot(kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
				//perp = new Vector3(direction.y * wsize2, -direction.x * wsize2, 0.0f);
				///perp = new Vector3(direction.z * wsize2, 0.0f, -direction.x * wsize2);
				///newInLength = CurveLength(inSpline, prevKnot, 0.5f, 1.0f, size2);
				///newOutLength = CurveLength(inSpline, i, 0.0f, 0.5f, size2);
				///kn = ko + perp;
				///inMult = newInLength / oldInLength;
				///outMult = newOutLength / oldOutLength;
				//k = MegaKnot(knotType, LTYPE_CURVE, kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
				///outSpline2.AddKnot(kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
			}

			outSpline.closed = true;
			//outSpline.ComputeBezPoints();
			//*inSpline = outSpline;
			///outSpline2.closed = true;
			//outSpline2->ComputeBezPoints();

			///shape.splines.Add(outSpline);
			//shape.splines.Add(outSpline2);
			///shape.CalcLength(10);
		}
		else
		{   // Otherwise, we get one closed polygon
			// Generate the outline polygon...
			for ( i = 0; i < knots; ++i )
			{
				// Determine the angle of the curve at this knot
				// Get vector from interp before knot to interp after knot
				Vector3 direction;
				Vector3 ko = inSpline.knots[i].p;
				float oldInLength = (i == 0) ? 1.0f : CurveLength(inSpline, i - 1, 0.5f, 1.0f, 0.0f);
				float oldOutLength = (i == (knots - 1)) ? 1.0f : CurveLength(inSpline, i, 0.0f, 0.5f, 0.0f);
				float wsize1 = 0.0f;
				if ( i == 0 )
				{
					direction = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
					wsize1 = size1;
				}
				else
				{
					if ( i == (knots - 1) )
					{
						direction = Vector3.Normalize(ko - inSpline.InterpBezier3D(i - 1, 0.99f));
						wsize1 = size1;
					}
					else
					{
						Vector3 bVec = Vector3.Normalize(inSpline.InterpBezier3D(i - 1, 0.99f) - ko);
						Vector3 fVec = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
						direction = Vector3.Normalize(fVec - bVec);
						// Figure the size multiplier for the crotch angle
						float dot = Vector3.Dot(bVec, fVec);
						if ( dot >= -0.9999939f )
						{
							float angle = -Mathf.Acos(dot) / 2.0f;
							float base1 = size1 / Mathf.Tan(angle);
							float sign1 = (size1 < 0.0f) ? -1.0f : 1.0f;
							wsize1 = Mathf.Sqrt(base1 * base1 + size1 * size1) * sign1;
						}
						else
						{
							wsize1 = size1;
						}
					}
				}

				//direction.z = 0.0f;  // Keep it in the XY plane
				//Vector3 perp = new Vector3(direction.y * wsize1, -direction.x * wsize1, 0.0f);
				direction.y = 0.0f;  // Keep it in the XY plane
				Vector3 perp = new Vector3(direction.z * wsize1, 0.0f, -direction.x * wsize1);
				float newInLength = (i == 0) ? 1.0f : CurveLength(inSpline, i - 1, 0.5f, 1.0f, size1);
				float newOutLength = (i == (knots - 1)) ? 1.0f : CurveLength(inSpline, i, 0.0f, 0.5f, size1);
				float inMult = newInLength / oldInLength;
				float outMult = newOutLength / oldOutLength;
				//int knotType = 0;	//inSpline->GetKnotType(i);
				Vector3 kn = ko + perp;
				//MegaKnot k((i==0 || i==(knots-1)) ? KTYPE_BEZIER_CORNER : knotType, LTYPE_CURVE, kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
				outSpline.AddKnot(kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
			}
#if false
			for ( i = knots - 1; i >= 0; --i )
			{
				// Determine the angle of the curve at this knot
				// Get vector from interp before knot to interp after knot
				Vector3 direction;
				Vector3 ko = inSpline.knots[i].p;	//->GetKnotPoint(i);
				float oldInLength = (i == 0) ? 1.0f : CurveLength(inSpline, i - 1, 0.5f, 1.0f, 0.0f);
				float oldOutLength = (i == (knots - 1)) ? 1.0f : CurveLength(inSpline, i, 0.0f, 0.5f, 0.0f);
				float wsize2 = 0.0f;
				if ( i == 0 )
				{
					direction = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
					wsize2 = size2;
				}
				else
				{
					if ( i == (knots - 1) )
					{
						direction = Vector3.Normalize(ko - inSpline.InterpBezier3D(i-1, 0.99f));
						wsize2 = size2;
					}
					else
					{
						Vector3 bVec = Vector3.Normalize(inSpline.InterpBezier3D(i-1, 0.99f) - ko);
						Vector3 fVec = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
						direction = Vector3.Normalize(fVec - bVec);
						// Figure the size multiplier for the crotch angle
						float dot = Vector3.Dot(bVec, fVec);
						if ( dot >= -0.9999939f )
						{
							float angle = -Mathf.Acos(dot) / 2.0f;
							float base2 = size2 / Mathf.Tan(angle);
							float sign2 = (size2 < 0.0f) ? -1.0f : 1.0f;
							wsize2 = Mathf.Sqrt(base2 * base2 + size2 * size2) * sign2;
						}
						else
						{
							wsize2 = size2;
						}
					}
				}
				//direction.z = 0.0f;  // Keep it in the XY plane
				//Vector3 perp = new Vector3(direction.y * wsize2, -direction.x * wsize2, 0.0f);
				direction.y = 0.0f;  // Keep it in the XY plane
				Vector3 perp = new Vector3(direction.z * wsize2, 0.0f, -direction.x * wsize2);
				float newInLength = (i == 0) ? 1.0f : CurveLength(inSpline, i - 1, 0.5f, 1.0f, size2);
				float newOutLength = (i == (knots - 1)) ? 1.0f : CurveLength(inSpline, i, 0.0f, 0.5f, size2);
				float inMult = newInLength / oldInLength;
				float outMult = newOutLength / oldOutLength;
				//int knotType = 0;	//inSpline->GetKnotType(i);
				Vector3 kn = ko + perp;
				//MegaKnot k((i==0 || i==(knots-1)) ? KTYPE_BEZIER_CORNER : knotType, LTYPE_CURVE, kn, kn + (inSpline.knots[i].outvec - ko) * outMult, kn + (inSpline.knots[i].invec - ko) * inMult);
				outSpline.AddKnot(kn, kn + (inSpline.knots[i].outvec - ko) * outMult, kn + (inSpline.knots[i].invec - ko) * inMult);
			}
			int lastPt = outSpline.knots.Count - 1;
			outSpline.knots[0].invec = outSpline.knots[0].p;	//(0, outSpline.GetKnotPoint(0));
			outSpline.knots[lastPt].outvec = outSpline.knots[lastPt].p;	//GetKnotPoint(lastPt));
			outSpline.knots[knots].invec = outSpline.knots[knots].p;	//GetKnotPoint(knots));
			outSpline.knots[knots - 1].outvec = outSpline.knots[knots - 1].p;	//GetKnotPoint(knots - 1));
			outSpline.closed = true;
#endif
			outSpline.closed = false;
		}
	}
#endif

	// Conform
	public bool			conform = false;
	public GameObject	target;
	public Collider		conformCollider;
	public float[]		offsets;
	public float[]		last;
	public float		conformAmount = 1.0f;
	public float		raystartoff = 0.0f;
	public float		raydist = 10.0f;
	public float		conformOffset = 0.0f;
	float minz = 0.0f;


	public void SetTarget(GameObject targ)
	{
		target = targ;

		if ( target )
		{
			conformCollider = target.GetComponent<Collider>();	//GetComponent<MeshCollider>();
		}
	}

	void CalcBounds(Vector3[] verts)
	{
		minz = verts[0].y;
		for ( int i = 1; i < verts.Length; i++ )
		{
			if ( verts[i].y < minz )
				minz = verts[i].y;
		}
	}

	public void InitConform(Vector3[] verts)
	{
		if ( offsets == null || offsets.Length != verts.Length )
		{
			offsets = new float[verts.Length];
			last = new float[verts.Length];

			for ( int i = 0; i < verts.Length; i++ )
				offsets[i] = verts[i].y - minz;
		}

		// Only need to do this if target changes, move to SetTarget
		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<Collider>();	//GetComponent<MeshCollider>();
		}
	}

	// We could do a bary centric thing if we grid up the bounds
	void DoConform(Vector3[] verts)
	{
		InitConform(verts);

		if ( target && conformCollider )
		{
			Matrix4x4 loctoworld = transform.localToWorldMatrix;

			Matrix4x4 tm = loctoworld;	// * worldtoloc;
			Matrix4x4 invtm = tm.inverse;

			Ray ray = new Ray();
			RaycastHit	hit;

			float ca = conformAmount;

			// When calculating alpha need to do caps sep
			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 origin = tm.MultiplyPoint(verts[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				//loftverts[i] = loftverts1[i];

				if ( conformCollider.Raycast(ray, out hit, raydist) )
				{
					Vector3 lochit = invtm.MultiplyPoint(hit.point);

					verts[i].y = Mathf.Lerp(verts[i].y, lochit.y + offsets[i] + conformOffset, ca);	//conformAmount);
					last[i] = verts[i].y;
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					verts[i].y = last[i];	//lochit.z + offsets[i] + offset;
				}
			}
		}
		else
		{
		}
	}


	public float conformWeight = 1.0f;	// 1 is conform only this mesh, 0 only the target
	// Option to conform terrain or mesh to mesh, slider to say how much of each happens
	// so 0.5 would meet in the middle
	void ConformTarget()
	{
		// We will need the vertex paint system as will need to find nearest point and have a falloff
	}
}

// Need to find major axis and flatten to 2d, then revert back to 3d
#if true	//!UNITY_FLASH
public class MegaTriangulator
{
	static public List<Vector3> m_points = new List<Vector3>();

	//public MegaTriangulator(MegaKnot[] points)
	//{
	//	m_points = new List<Vector2>();	//points);
	//
	//}
   
	static public List<int> Triangulate(MegaShape shape, MegaSpline spline, float dist, ref List<Vector3> verts, ref List<Vector2> uvs, ref List<int> indices, Vector3 pivot, ref Vector3 size)
	{
		// Find 
		m_points.Clear();

		List<MegaKnot> knots = spline.knots;

		Vector3 min = knots[0].p;
		Vector3 max = knots[0].p;

		for ( int i = 1; i < knots.Count; i++ )
		{
			Vector3 p1 = knots[i].p;

			if ( p1.x < min.x )	min.x = p1.x;
			if ( p1.y < min.y ) min.y = p1.y;
			if ( p1.z < min.z ) min.z = p1.z;

			if ( p1.x > max.x ) max.x = p1.x;
			if ( p1.y > max.y ) max.y = p1.y;
			if ( p1.z > max.z ) max.z = p1.z;
		}

		size = max - min;

		int removeaxis = 0;

		if ( Mathf.Abs(size.x) < Mathf.Abs(size.y) )
		{
			if ( Mathf.Abs(size.x) < Mathf.Abs(size.z) )
				removeaxis = 0;
			else
				removeaxis = 2;
		}
		else
		{
			if ( Mathf.Abs(size.y) < Mathf.Abs(size.z) )
				removeaxis = 1;
			else
				removeaxis = 2;
		}

		Vector3 tp = Vector3.zero;
#if false
		for ( int i = 0; i < knots.Count; i++ )
		{
			for ( int a = 0; a < steps; a ++ )
			{
				float alpha = (float)a / (float)steps;
				Vector3 p = spline.knots[i].Interpolate(alpha, spline.knots[i]);
				switch ( removeaxis )
				{
					case 0:	tp.x = p.y; tp.y = p.z;	break;
					case 1: tp.x = p.x; tp.y = p.z; break;
					case 2: tp.x = p.x; tp.y = p.y; break;
				}
				verts.Add(p);
				m_points.Add(tp);
			}
		}
#endif
		float ds = spline.length / (spline.length / dist);

		if ( ds > spline.length )
			ds = spline.length;

		//int c	= 0;
		int k	= -1;
		//int lk	= -1;

		//Vector3 first = spline.Interpolate(0.0f, shape.normalizedInterp, ref lk);
		Vector3 p = Vector3.zero;

		for ( float dst = 0.0f; dst < spline.length; dst += ds )
		{
			float alpha = dst / spline.length;
			p = spline.Interpolate(alpha, shape.normalizedInterp, ref k) + pivot;

			switch ( removeaxis )
			{
				case 0: tp.x = p.y; tp.y = p.z; break;
				case 1: tp.x = p.x; tp.y = p.z; break;
				case 2: tp.x = p.x; tp.y = p.y; break;
			}
			tp.z = dst;
			verts.Add(p);
			m_points.Add(tp);

			// Dont need this here as can do in post step
			//tp.x = (tp.x - min.x) / size.x;
			//tp.y = (tp.y - min.z) / size.z;
			tp.x = (tp.x - min.x);	// / size.x;
			tp.y = (tp.y - min.z);	// / size.z;
			uvs.Add(tp);
		}

		//if ( spline.closed )
		//	p = spline.Interpolate(0.0f, shape.normalizedInterp, ref k);
		//else
		//	p = spline.Interpolate(1.0f, shape.normalizedInterp, ref k);

		//switch ( removeaxis )
		//{
		//	case 0: tp.x = p.y; tp.y = p.z; break;
		//	case 1: tp.x = p.x; tp.y = p.z; break;
		//	case 2: tp.x = p.x; tp.y = p.y; break;
		//}

		//verts.Add(p);
		//m_points.Add(tp);

		return Triangulate(indices);
	}

	static public List<int> Triangulate(List<int> indices)
	{
		//List<int> indices = new List<int>();

		int n = m_points.Count;
		if ( n < 3 )
			return indices;	//.ToArray();

		int[] V = new int[n];
		if ( Area() > 0.0f )
		{
			for ( int v = 0; v < n; v++ )
				V[v] = v;
		}
		else
		{
			for ( int v = 0; v < n; v++ )
				V[v] = (n - 1) - v;
		}
       
		int nv = n;
		int count = 2 * nv;
		for ( int m = 0, v = nv - 1; nv > 2; )
		{
			if ( (count--) <= 0 )
				return indices;	//.ToArray();

			int u = v;
			if ( nv <= u )
				u = 0;
			v = u + 1;
			if ( nv <= v )
				v = 0;
			int w = v + 1;
			if ( nv <= w )
				w = 0;

			if ( Snip(u, v, w, nv, V) )
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(c);
				indices.Add(b);
				indices.Add(a);
				m++;
				for ( s = v, t = v + 1; t < nv; s++, t++ )
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}

		//indices.Reverse();
		return indices;	//.ToArray();
	}
   
	static private float Area()
	{
		int n = m_points.Count;
		float A = 0.0f;
		for ( int p = n - 1, q = 0; q < n; p = q++ )
		{
			Vector2 pval = m_points[p];
			Vector2 qval = m_points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}

		return A * 0.5f;
	}
   
	static private bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector2 A = m_points[V[u]];
		Vector2 B = m_points[V[v]];
		Vector2 C = m_points[V[w]];

		if ( Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))) )
			return false;

		for ( int p = 0; p < n; p++ )
		{
			if ( (p == u) || (p == v) || (p == w) )
				continue;
			Vector2 P = m_points[V[p]];

			if ( InsideTriangle(A, B, C, P) )
				return false;
		}
		return true;
	}
   
	static private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax = C.x - B.x;
		float ay = C.y - B.y;
		float bx = A.x - C.x;
		float by = A.y - C.y;
		float cx = B.x - A.x;
		float cy = B.y - A.y;
		float apx = P.x - A.x;
		float apy = P.y - A.y;
		float bpx = P.x - B.x;
		float bpy = P.y - B.y;
		float cpx = P.x - C.x;
		float cpy = P.y - C.y;

		float aCROSSbp = ax * bpy - ay * bpx;
		float cCROSSap = cx * apy - cy * apx;
		float bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}
}
#endif