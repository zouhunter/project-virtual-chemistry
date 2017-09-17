
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Cylindrify")]
public class MegaCylindrifyWarp : MegaWarp
{
	public float Percent = 0.0f;
	//public float Decay = 0.0f;

	public override string WarpName() { return "Cylindrify"; }
	public override string GetHelpURL() { return "?page_id=166"; }

	float size1;
	float per;

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		//float dcy = Mathf.Exp(-Decay * p.magnitude);

		float k = ((size1 / Mathf.Sqrt(p.x * p.x + p.z * p.z) / 2.0f - 1.0f) * per * dcy) + 1.0f;
		p.x *= k;
		p.z *= k;

		p = Vector3.Lerp(ip, p, dcy);

		return invtm.MultiplyPoint3x4(p);
	}

	//public override bool ModLateUpdate(Modifiers mc)
	void Update()
	{
		Prepare(Decay);
	}

	public MegaAxis axis;
	Matrix4x4		mat = new Matrix4x4();

	public override bool Prepare(float decay)
	{
		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

		//size = bbox.Size();
		//size.x = Width;
		//size.y = Height;
		//size.z = Length;

		mat = Matrix4x4.identity;

		//SetTM1();
		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		SetAxis(mat);

		float xsize = Width;	//bbox.max.x - bbox.min.x;
		float zsize = Length;	//bbox.max.z - bbox.min.z;
		size1 = (xsize > zsize) ? xsize : zsize;

		// Get the percentage to spherify at this time
		per = Percent / 100.0f;

		return true;
	}
}
