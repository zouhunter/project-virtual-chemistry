using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Globe")]
public class MegaGlobeWarp : MegaWarp
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

	public override string WarpName() { return "Globe"; }
	public override string GetHelpURL() { return "?page_id=3752"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		if ( r == 0.0f )
			return p;

		p = tm.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc
		Vector3 ip = p;

		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		float x = p.x;
		float y = p.y;
		//float z = p.z;

		float yr = (y / r);	// * amplify;

		float c = Mathf.Cos(Mathf.PI - yr);
		float s = Mathf.Sin(Mathf.PI - yr);
		float px = r * c + r - x * c;
		p.x = px;
		float pz = r * s - x * s;
		p.y = pz;

		p = Vector3.Lerp(ip, p, dcy);

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

			p = Vector3.Lerp(ip, p, dcy);

			p = invtm1.MultiplyPoint3x4(p);
		}
		return p;
	}

	void Calc()
	{
		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

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

	public override bool Prepare(float decay)
	{
		Calc();

		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		return true;
	}
}