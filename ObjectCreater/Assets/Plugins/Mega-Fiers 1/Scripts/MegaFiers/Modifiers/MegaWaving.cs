
using UnityEngine;

[AddComponentMenu("Modifiers/Waving")]
public class MegaWaving : MegaModifier
{
	public float	amp			= 0.01f;
	public float	flex		= 1.0f;
	public float	wave		= 1.0f;
	public float	phase		= 0.0f;
	public float	Decay		= 0.0f;
	public bool		animate		= false;
	public float	Speed		= 1.0f;
	public MegaAxis	waveaxis	= MegaAxis.X;
	float time	= 0.0f;
	float dy	= 0.0f;
	int ix = 0;
	int iz = 2;
	float t = 0.0f;

	public override string ModName() { return "Waving"; }
	public override string GetHelpURL() { return "?page_id=308"; }

	static public float WaveFunc(float radius, float t, float amp, float waveLen, float phase, float decay)
	{
		float ang = Mathf.PI * 2.0f * (radius / waveLen + phase);
		return amp * Mathf.Sin(ang) * Mathf.Exp(-decay * Mathf.Abs(radius));
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		float u = Mathf.Abs(2.0f * p[iz]);	//.z);	// / dist);
		u = u * u;
		p[ix] += flex * WaveFunc(p[iz], time, amp * u, wave, phase, dy);
		return invtm.MultiplyPoint3x4(p);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( animate )
		{
			float dt = Time.deltaTime;
			if ( dt == 0.0f )
				dt = 0.01f;
			t += dt * Speed;
			phase = t;
		}
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( wave == 0.0f )
			wave = 0.000001f;

		dy = Decay / 1000.0f;

		switch ( waveaxis )
		{
			case MegaAxis.X:
				ix = 0;
				iz = 2;
				break;

			case MegaAxis.Y:
				ix = 1;
				iz = 2;
				break;

			case MegaAxis.Z:
				ix = 2;
				iz = 0;
				break;
		}
		return true;
	}
}
