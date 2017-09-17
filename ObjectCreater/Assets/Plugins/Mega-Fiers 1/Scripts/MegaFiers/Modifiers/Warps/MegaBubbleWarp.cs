
using UnityEngine;
using System.IO;

[AddComponentMenu("Modifiers/Warps/Bubble")]
public class MegaBubbleWarp : MegaWarp
{
	public float		radius = 0.0f;
	public float		falloff = 20.0f;
	Matrix4x4			mat = new Matrix4x4();

	public override string WarpName() { return "Bubble"; }
	public override string GetHelpURL() { return "?page_id=111"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		float val = ((Vector3.Magnitude(p)) / falloff);
		p += radius * (Vector3.Normalize(p)) / (val * val + 1.0f);

		p = Vector3.Lerp(ip, p, dcy);
		return invtm.MultiplyPoint3x4(p);
	}

	//public override bool ModLateUpdate(Modifiers mc)
	void Update()
	{
		Prepare(Decay);
	}

	public override bool Prepare(float decay)
	{
		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;
		mat = Matrix4x4.identity;

		SetAxis(mat);
		return true;
	}
}
