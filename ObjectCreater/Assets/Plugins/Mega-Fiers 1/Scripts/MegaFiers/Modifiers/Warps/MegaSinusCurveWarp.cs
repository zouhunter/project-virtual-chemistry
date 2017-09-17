
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Sinus Curve")]
public class MegaSinusCurveWarp : MegaWarp
{
	public float		scale = 1.0f;
	public float		wave = 1.0f;
	public float		speed = 1.0f;
	public float		phase = 0.0f;
	public bool			animate = false;
	Matrix4x4			mat = new Matrix4x4();

	public override string WarpName() { return "Sinus Curve"; }
	public override string GetHelpURL() { return "Bubble.htm"; }

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		p.y += Mathf.Sin(phase + (p.x * wave) + p.y + p.z) * scale;

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

		mat = Matrix4x4.identity;

		SetAxis(mat);
		return true;
	}
}