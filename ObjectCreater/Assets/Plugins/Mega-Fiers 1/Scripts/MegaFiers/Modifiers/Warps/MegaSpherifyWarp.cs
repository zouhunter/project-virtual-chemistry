
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Spherify")]
public class MegaSpherifyWarp : MegaWarp
{
	public float		percent = 0.0f;
	public float		FallOff = 0.0f;
	float per;
	float xsize,ysize,zsize;
	float size;
	//float cx,cy,cz;
	public override string WarpName() { return "Spherify"; }
	public override string GetHelpURL() { return "?page_id=322"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy1 = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		//float xw,yw,zw;

		//xw = p.x - cx; yw = p.y - cy; zw = p.z - cz;
		//if ( xw == 0.0f && yw == 0.0f && zw == 0.0f )
		//	xw = yw = zw = 1.0f;
		float vdist = dist;	//Mathf.Sqrt(xw * xw + yw * yw + zw * zw);
		float mfac = size / vdist;

		float dcy = Mathf.Exp(-FallOff * Mathf.Abs(vdist));

		p.x = p.x + (Mathf.Sign(p.x) * ((Mathf.Abs(p.x * mfac) - Mathf.Abs(p.x)) * per) * dcy);
		p.y = p.y + (Mathf.Sign(p.y) * ((Mathf.Abs(p.y * mfac) - Mathf.Abs(p.y)) * per) * dcy);
		p.z = p.z + (Mathf.Sign(p.z) * ((Mathf.Abs(p.z * mfac) - Mathf.Abs(p.z)) * per) * dcy);

		p = Vector3.Lerp(ip, p, dcy1);

		return invtm.MultiplyPoint3x4(p);
	}

	//public override bool ModLateUpdate(Modifiers mc)
	void Update()
	{
		Prepare(Decay);
	}

	public override bool Prepare(float decay)
	{
		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		xsize = Width;	//bbox.max.x - bbox.min.x;
		ysize = Height;	//bbox.max.y - bbox.min.y;
		zsize = Length;	//bbox.max.z - bbox.min.z;
		size = (xsize > ysize) ? xsize : ysize;
		size = (zsize > size) ? zsize : size;
		size /= 2.0f;
		//cx = 0.0f;	//bbox.center.x;
		//cy = 0.0f;	//bbox.center.y;
		//cz = 0.0f;	//bbox.center.z;

		// Get the percentage to spherify at this time
		per = percent / 100.0f;

		return true;
	}
}