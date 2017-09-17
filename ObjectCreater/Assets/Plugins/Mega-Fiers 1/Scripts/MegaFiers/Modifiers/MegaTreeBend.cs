
using UnityEngine;

[AddComponentMenu("Modifiers/Tree Bend")]
public class MegaTreeBend : MegaModifier
{
	public float fBendScale = 1.0f;
	public Vector3 vWind = Vector3.zero;
	public float WindDir = 0.0f;
	public float WindSpeed = 0.0f;
	Vector2 waveIn = Vector2.zero;
	Vector2 waves = Vector2.zero;

	public override string ModName() { return "Tree Bend"; }
	public override string GetHelpURL() { return "?page_id=41"; }

	float frac(float val)
	{
		int v = (int)val;
		return val - v;
	}

	Vector2 smoothCurve(Vector2 x)
	{
		x.x = x.x * x.x * (3.0f - 2.0f * x.x);
		x.y = x.y * x.y * (3.0f - 2.0f * x.y);
		return x;
	}

	Vector2 triangleWave(Vector2 x)
	{
		x.x = Mathf.Abs(frac(x.x + 0.5f) * 2.0f - 1.0f);
		x.y = Mathf.Abs(frac(x.y + 0.5f) * 2.0f - 1.0f);
		return x;
	}

	Vector2 smoothTriangleWave(Vector2 x)
	{
		return smoothCurve(triangleWave(x));
	}

	float windTurbulence(float bbPhase, float frequency, float strength)
	{
		// We create the input value for wave generation from the frequency and phase.
		waveIn.x = bbPhase + frequency;
		waveIn.y = bbPhase + frequency;

		// We use two square waves to generate the effect which
		// is then scaled by the overall strength.
		waves.x = (frac(waveIn.x * 1.975f) * 2.0f - 1.0f);
		waves.y = (frac(waveIn.y * 0.793f) * 2.0f - 1.0f);
		waves = smoothTriangleWave(waves);

		// Sum up the two waves into a single wave.
		return (waves.x + waves.y) * strength;
	}

	Vector2 windEffect(float bbPhase, Vector2 windDirection, float gustLength, float gustFrequency, float gustStrength, float turbFrequency, float turbStrength)
	{
		// Calculate the ambient wind turbulence.
		float turbulence = windTurbulence(bbPhase, turbFrequency, turbStrength);

		// We simulate the overall gust via a sine wave.
		float gustPhase = Mathf.Clamp01(Mathf.Sin((bbPhase - gustFrequency) / gustLength));
		float gustOffset = (gustPhase * gustStrength) + ((0.2f + gustPhase) * turbulence);

		// Return the final directional wind effect.
		return gustOffset * windDirection;
	}

	// Virtual method for all mods
	public override void SetValues(MegaModifier mod)
	{
		//MegaTreeBend bm = (MegaTreeBend)mod;
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc

		float fLength = p.magnitude;

		float fBF = p.y * fBendScale;
		fBF += 1.0f;
		fBF *= fBF;
		fBF = fBF * fBF - fBF;
		Vector3 vNewPos = p;
		vNewPos.x += vWind.x * fBF;
		vNewPos.z += vWind.y * fBF;
		p = vNewPos.normalized * fLength;  
		p = invtm.MultiplyPoint3x4(p);
		return p;
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		vWind.x = Mathf.Sin(WindDir * Mathf.Deg2Rad) * WindSpeed;
		vWind.y = Mathf.Cos(WindDir * Mathf.Deg2Rad) * WindSpeed;

		return true;
	}
}
