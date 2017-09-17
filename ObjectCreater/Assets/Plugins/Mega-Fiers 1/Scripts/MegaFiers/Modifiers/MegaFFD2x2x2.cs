
using UnityEngine;

[AddComponentMenu("Modifiers/FFD/FFD 2x2x2")]
public class MegaFFD2x2x2 : MegaFFD
{
	public override string ModName() { return "FFD2x2x2"; }

	public override int GridSize()
	{
		return 2;
	}

	public override Vector3 Map(int ii, Vector3 p)
	{
		Vector3 q = Vector3.zero;

		Vector3 pp = tm.MultiplyPoint3x4(p);

		if ( inVol )
		{
			for ( int i = 0; i < 3; i++ )
			{
				if ( pp[i] < -EPSILON || pp[i] > 1.0f + EPSILON )
					return p;
			}
		}

		float ip,jp,kp;
		for ( int i = 0; i < 2; i++ )
		{
			ip = i == 0 ? 1.0f - pp.x : pp.x;

			for ( int j = 0; j < 2; j++ )
			{
				jp = ip * (j == 0 ? 1.0f - pp.y : pp.y);

				for ( int k = 0; k < 2; k++ )
				{
					kp = jp * (k == 0 ? 1.0f - pp.z : pp.z);

					int ix = (i * 4) + (j * 2) + k;
					q.x += pt[ix].x * kp;
					q.y += pt[ix].y * kp;
					q.z += pt[ix].z * kp;
				}
			}
		}

		return invtm.MultiplyPoint3x4(q);
	}

	public override int GridIndex(int i, int j, int k)
	{
		return (i * 4) + (j * 2) + k;
	}
}