
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Squeeze")]
public class MegaSqueezeWarp : MegaWarp
{
	public float			amount		= 0.0f;
	public float			crv			= 0.0f;
	public float			radialamount = 0.0f;
	public float			radialcrv	= 0.0f;
	public bool				doRegion	= false;
	public float			to			= 0.0f;
	public float			from		= 0.0f;
	public MegaAxis			axis		= MegaAxis.Y;
	Matrix4x4				mat			= new Matrix4x4();
	float k1;
	float k2;
	float k3;
	float k4;
	float l;
	float l2;
	float ovl;
	float ovl2;

	void SetK(float K1, float K2, float K3, float K4)
	{
		k1 = K1;
		k2 = K2;
		k3 = K3;
		k4 = K4;
	}

	public override string WarpName() { return "Squeeze"; }
	public override string GetIcon() { return "MegaStretch icon.png"; }
	public override string GetHelpURL() { return "?page_id=338"; }

	// Radial amount works on distance from pivot on the vertical axis, the lower the value the more effect
	// the other one works on distance from the vertical axis, the lower the value the more the effect
	public override Vector3 Map(int i, Vector3 p)
	{
		float z;

		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		if ( l != 0.0f )
		{
			if ( doRegion )
			{
				if ( p.y < from )
					z = from * ovl;
				else
				{
					if ( p.y > to )
						z = to * ovl;
					else
						z = p.y * ovl;
				}
			}
			else
				z = Mathf.Abs(p.y * ovl);


			float f =  1.0f + z * k1 + k2 * z * (1.0f - z);

			p.y *= f;
		}

		if ( l2 != 0.0f )
		{
			float dist1 = Mathf.Sqrt(p.x * p.x + p.z * p.z);
			float xy = dist1 * ovl2;
			float f1 =  1.0f + xy * k3 + k4 * xy * (1.0f - xy);
			p.x *= f1;
			p.z *= f1;
		}

		p = Vector3.Lerp(ip, p, dcy);

		return invtm.MultiplyPoint3x4(p);
	}

	public override bool Prepare(float decay)
	{
		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;
		mat = Matrix4x4.identity;
		SetAxis(mat);
		SetK(amount, crv, radialamount, radialcrv);
		Vector3 size = Vector3.zero;	//bbox.Size();
		size.x = Width;
		size.y = Height;
		size.z = Length;

		switch ( axis )
		{
			case MegaAxis.X:
				l = size[0];	//bbox.max[1] - bbox.min[1];
				l2 = Mathf.Sqrt(size[1] * size[1] + size[2] * size[2]);
				break;

			case MegaAxis.Y:
				l = size[1];	//bbox.max[1] - bbox.min[1];
				l2 = Mathf.Sqrt(size[0] * size[0] + size[2] * size[2]);
				break;

			case MegaAxis.Z:
				l = size[2];	//bbox.max[1] - bbox.min[1];
				l2 = Mathf.Sqrt(size[1] * size[1] + size[0] * size[0]);
				break;

		}

		//l = size.y;	//bbox.max[1] - bbox.min[1];
		//l2 = Mathf.Sqrt(size.x * size.x + size.z * size.z);

		if ( l != 0.0f )
			ovl = 1.0f / l;

		if ( l2 != 0.0f )
			ovl2 = 1.0f / l2;

		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		return true;
	}

	public override void ExtraGizmo()
	{
		if ( doRegion )
			DrawFromTo(MegaAxis.Z, from, to);
	}

#if false
	public float	amount		= 0.0f;
	public bool		doRegion	= false;
	public float	to			= 0.0f;
	public float	from		= 0.0f;
	public float	amplify		= 0.0f;
	public MegaAxis	axis		= MegaAxis.X;
	float			heightMax	= 0.0f;
	float			heightMin	= 0.0f;
	float			amplifier	= 0.0f;
	Matrix4x4		mat			= new Matrix4x4();

	public override string WarpName() { return "Squeeze"; }
	public override string GetIcon() { return "MegaStretch icon.png"; }
	public override string GetHelpURL() { return "?page_id=2560"; }

	void CalcBulge(MegaAxis axis, float stretch, float amplify)
	{
		amount = stretch;
		amplifier = (amplify >= 0.0f) ? amplify + 1.0f : 1.0f / (-amplify + 1.0f);

		if ( !doRegion )
		{
			switch ( axis )
			{
				case MegaAxis.X:
					heightMin = -Width * 0.5f;
					heightMax = Width * 0.5f;
					break;

				case MegaAxis.Z:
					heightMin = 0.0f;
					heightMax = Height;
					break;

				case MegaAxis.Y:
					heightMin = -Length * 0.5f;
					heightMax = Length * 0.5f;
					break;
			}
		}
		else
		{
			heightMin = from;
			heightMax = to;
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		float normHeight;
		float xyScale, zScale;

		if ( amount == 0.0f || (heightMax - heightMin == 0) )
			return p;

		if ( (doRegion) && (to - from == 0.0f) )
			return p;

		p = tm.MultiplyPoint3x4(p);

		Vector3 ip = p;
		float dist = p.magnitude;
		float dcy = Mathf.Exp(-totaldecay * Mathf.Abs(dist));

		if ( doRegion && p.y > to )
			normHeight = (to - heightMin) / (heightMax - heightMin);
		else if ( doRegion && p.y < from )
			normHeight = (from - heightMin) / (heightMax - heightMin);
		else
			normHeight = (p.y - heightMin) / (heightMax - heightMin);

		if ( amount < 0.0f )
		{
			xyScale = (amplifier * -amount + 1.0F);
			zScale = (-1.0f / (amount - 1.0F));
		}
		else
		{
			xyScale = 1.0f / (amplifier * amount + 1.0f);
			zScale = amount + 1.0f;
		}

		float a = 4.0f * (1.0f - xyScale);
		float b = -4.0f * (1.0f - xyScale);
		float c = 1.0f;
		float fraction = (((a * normHeight) + b) * normHeight) + c;
		p.x *= fraction;
		p.z *= fraction;

		if ( doRegion && p.y < from )
			p.y += (zScale - 1.0f) * from;
		else if ( doRegion && p.y <= to )
			p.y *= zScale;
		else if ( doRegion && p.y > to )
			p.y += (zScale - 1.0f) * to;
		else
			p.y *= zScale;

		p = Vector3.Lerp(ip, p, dcy);

		p = invtm.MultiplyPoint3x4(p);

		return p;
	}

	public override bool Prepare(float decay)
	{
		tm = transform.worldToLocalMatrix;
		invtm = tm.inverse;

		mat = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		SetAxis(mat);
		CalcBulge(axis, amount, amplify);

		totaldecay = Decay + decay;
		if ( totaldecay < 0.0f )
			totaldecay = 0.0f;

		return true;
	}

	public override void ExtraGizmo()
	{
		if ( doRegion )
			DrawFromTo(axis, from, to);
	}
#endif
}