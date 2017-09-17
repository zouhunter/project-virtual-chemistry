
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Hump")]
public class MegaHumpWarp : MegaWarp
{
	public float	amount	= 0.0f;
	public float	cycles	= 1.0f;
	public float	phase	= 0.0f;
	public bool		animate	= false;
	public float	speed	= 1.0f;
	public MegaAxis	axis	= MegaAxis.Z;
	float amt;
	Vector3	size = Vector3.zero;

	public override string WarpName() { return "Hump"; }
	public override string GetHelpURL() { return "?page_id=207"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		switch ( axis )
		{
			case MegaAxis.X: p.x += amt * Mathf.Sin(Mathf.Sqrt(p.x * p.x / size.x) + Mathf.Sqrt(p.y * p.y / size.y) * Mathf.PI / 0.1f * (Mathf.Deg2Rad * cycles) + phase); break;
			case MegaAxis.Y: p.y += amt * Mathf.Sin(Mathf.Sqrt(p.y * p.y / size.y) + Mathf.Sqrt(p.x * p.x / size.x) * Mathf.PI / 0.1f * (Mathf.Deg2Rad * cycles) + phase); break;
			case MegaAxis.Z: p.z += amt * Mathf.Sin(Mathf.Sqrt(p.x * p.x / size.x) + Mathf.Sqrt(p.y * p.y / size.y) * Mathf.PI / 0.1f * (Mathf.Deg2Rad * cycles) + phase); break;
		}

		p = Vector3.Lerp(ip, p, dcy);

		return invtm.MultiplyPoint3x4(p);
	}

	void Update()
	{
		if ( animate )
			phase += Time.deltaTime * speed;
		Prepare(Decay);
	}

	public override bool Prepare(float decay)
	{
		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

		//size = bbox.Size();
		size.x = Width;
		size.y = Height;
		size.z = Length;

		amt = amount / 100.0f;

		return true;
	}
}