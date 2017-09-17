using UnityEngine;

// falloff effect
// exaggerate

[AddComponentMenu("Modifiers/Globe")]
public class MegaGlobe : MegaModifier
{
	public float	dir			= -90.0f;
	public float	dir1		= -90.0f;
	public MegaAxis	axis		= MegaAxis.X;
	public MegaAxis	axis1		= MegaAxis.Z;
	Matrix4x4		mat			= new Matrix4x4();

	public bool		twoaxis		= true;
	Matrix4x4		tm1			= new Matrix4x4();
	Matrix4x4		invtm1		= new Matrix4x4();

	public float	r			= 0.0f;
	public float	r1			= 0.0f;
	public float	radius		= 10.0f;
	public bool		linkRadii	= true;
	public float	radius1		= 10.0f;

	public override string ModName()	{ return "Globe"; }
	public override string GetHelpURL() { return "?page_id=3752"; }

	//public float	amplify = 1.0f;

	// Virtual method for all mods
	public override void SetValues(MegaModifier mod)
	{
		MegaBend bm = (MegaBend)mod;
		//angle = bm.angle;
		dir = bm.dir;
		axis = bm.axis;
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		if ( r == 0.0f )
			return p;

		p = tm.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc

		float x = p.x;
		float y = p.y;
		//float z = p.z;

		//Debug.Log("p " + p);
		float yr = (y / r);	// * amplify;

		float c = Mathf.Cos(Mathf.PI - yr);
		float s = Mathf.Sin(Mathf.PI - yr);
		float px = r * c + r - x * c;
		p.x = px;
		float pz = r * s - x * s;
		p.y = pz;

		p = invtm.MultiplyPoint3x4(p);

		if ( twoaxis )
		{
			p = tm1.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc

			x = p.x;
			y = p.y;
			//z = p.z;

			yr = (y / r1);	// * amplify;

			c = Mathf.Cos(Mathf.PI - yr);
			s = Mathf.Sin(Mathf.PI - yr);
			px = r1 * c + r1 - x * c;
			p.x = px;
			pz = r1 * s - x * s;
			p.y = pz;

			p = invtm1.MultiplyPoint3x4(p);
		}
		return p;
	}

	void Calc()
	{
		mat = Matrix4x4.identity;

		tm1 = tm;
		invtm1 = invtm;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		MegaMatrix.RotateY(ref mat, Mathf.Deg2Rad * dir);
		SetAxis(mat);

		mat = Matrix4x4.identity;

		switch ( axis1 )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		MegaMatrix.RotateY(ref mat, Mathf.Deg2Rad * dir1);
		//SetAxis(mat);
		Matrix4x4 itm = mat.inverse;
		tm1 = mat * tm1;
		invtm1 = invtm1 * itm;

		r = -radius;

		if ( linkRadii )
			r1 = -radius;
		else
			r1 = -radius1;
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		Calc();
		return true;
	}
}
