
using UnityEngine;
using System.Collections;

public class MegaNearestPointTest
{
	public static Vector3 NearestPointOnMesh1(Vector3 pt, Vector3[] verts, int[] tri, ref int index, ref Vector3 bary)
	{
		float nearestSqDist = float.MaxValue;

		Vector3 nearestPt = Vector3.zero;
		nearestSqDist = float.MaxValue;

		for ( int i = 0; i < tri.Length; i += 3 )
		{
			//int triOff = tri.t[i] * 3;
			Vector3 a = verts[tri[i]];
			Vector3 b = verts[tri[i + 1]];
			Vector3 c = verts[tri[i + 2]];

			//Vector3 possNearestPt = Vector3.zero;
			float dist = DistPoint3Triangle3Dbl(pt, a, b, c);

			float possNearestSqDist = dist;

			if ( possNearestSqDist < nearestSqDist )
			{
				index = i;
				bary = mTriangleBary;
				nearestPt = mClosestPoint1;
				nearestSqDist = possNearestSqDist;
			}
		}

		return nearestPt;
	}

	public static Vector3 NearestPointOnMesh2(Vector3 pt, Vector3[] verts, int[] tri, ref int index, ref Vector3 bary)
	{
		float nearestSqDist = float.MaxValue;

		Vector3 nearestPt = Vector3.zero;
		nearestSqDist = float.MaxValue;

		for ( int i = 0; i < tri.Length; i += 3 )
		{
			//int triOff = tri.t[i] * 3;
			Vector3 a = verts[tri[i]];
			Vector3 b = verts[tri[i + 1]];
			Vector3 c = verts[tri[i + 2]];

			//Vector3 possNearestPt = Vector3.zero;
			float dist = DistPoint3Triangle3Dbl(pt, a, b, c);

			float possNearestSqDist = dist;

			if ( possNearestSqDist < nearestSqDist )
			{
				index = i;
				bary = mTriangleBary;
				nearestPt = mClosestPoint1;
				nearestSqDist = possNearestSqDist;
			}
		}

		return nearestPt;
	}

	public static float DistPoint3Triangle3Dbl(Vector3 mPoint, Vector3 v0, Vector3 v1, Vector3 v2)
	{
		Vector3 diff = v0 - mPoint;
		Vector3 edge0 = v1 - v0;
		Vector3 edge1 = v2 - v0;
		double a00 = edge0.sqrMagnitude;	//.SquaredLength();
		//double a01 = Vector3.Dot(edge0, edge1);
		double a01 = Vector3.Dot(edge1, edge0);
		double a11 = edge1.sqrMagnitude;
		//double b0 = Vector3.Dot(diff, edge0);
		double b0 = Vector3.Dot(edge0, diff);
		//double b1 = Vector3.Dot(diff, edge1);
		double b1 = Vector3.Dot(edge1, diff);
		double c = diff.sqrMagnitude;
		double det = Mathf.Abs((float)a00 * (float)a11 - (float)a01 * (float)a01);
		double s = a01 * b1 - a11 * b0;
		double t = a01 * b0 - a00 * b1;
		double sqrDistance;

		if ( s + t <= det )
		{
			if ( s < (double)0.0 )
			{
				if ( t < (double)0 )  // region 4
				{
					if ( b0 < (double)0 )
					{
						t = (double)0;
						if ( -b0 >= a00 )
						{
							s = (double)1;
							sqrDistance = a00 + ((double)2) * b0 + c;
						}
						else
						{
							s = -b0 / a00;
							sqrDistance = b0 * s + c;
						}
					}
					else
					{
						s = (double)0;
						if ( b1 >= (double)0 )
						{
							t = (double)0;
							sqrDistance = c;
						}
						else if ( -b1 >= a11 )
						{
							t = (double)1;
							sqrDistance = a11 + ((double)2) * b1 + c;
						}
						else
						{
							t = -b1 / a11;
							sqrDistance = b1 * t + c;
						}
					}
				}
				else  // region 3
				{
					s = (double)0;
					if ( b1 >= (double)0 )
					{
						t = (double)0;
						sqrDistance = c;
					}
					else if ( -b1 >= a11 )
					{
						t = (double)1;
						sqrDistance = a11 + ((double)2) * b1 + c;
					}
					else
					{
						t = -b1 / a11;
						sqrDistance = b1 * t + c;
					}
				}
			}
			else if ( t < (double)0 )  // region 5
			{
				t = (double)0;
				if ( b0 >= (double)0 )
				{
					s = (double)0;
					sqrDistance = c;
				}
				else if ( -b0 >= a00 )
				{
					s = (double)1;
					sqrDistance = a00 + ((double)2) * b0 + c;
				}
				else
				{
					s = -b0 / a00;
					sqrDistance = b0 * s + c;
				}
			}
			else  // region 0
			{
				// minimum at interior point
				double invDet = ((double)1) / det;
				s *= invDet;
				t *= invDet;
				sqrDistance = s * (a00 * s + a01 * t + ((double)2) * b0) +
					t * (a01 * s + a11 * t + ((double)2) * b1) + c;
			}
		}
		else
		{
			double tmp0, tmp1, numer, denom;

			if ( s < (double)0 )  // region 2
			{
				tmp0 = a01 + b0;
				tmp1 = a11 + b1;
				if ( tmp1 > tmp0 )
				{
					numer = tmp1 - tmp0;
					denom = a00 - ((double)2) * a01 + a11;
					if ( numer >= denom )
					{
						s = (double)1;
						t = (double)0;
						sqrDistance = a00 + ((double)2) * b0 + c;
					}
					else
					{
						s = numer / denom;
						t = (double)1 - s;
						sqrDistance = s * (a00 * s + a01 * t + ((double)2) * b0) +
							t * (a01 * s + a11 * t + ((double)2) * b1) + c;
					}
				}
				else
				{
					s = (double)0;
					if ( tmp1 <= (double)0 )
					{
						t = (double)1;
						sqrDistance = a11 + ((double)2) * b1 + c;
					}
					else if ( b1 >= (double)0 )
					{
						t = (double)0;
						sqrDistance = c;
					}
					else
					{
						t = -b1 / a11;
						sqrDistance = b1 * t + c;
					}
				}
			}
			else if ( t < (double)0 )  // region 6
			{
				tmp0 = a01 + b1;
				tmp1 = a00 + b0;
				if ( tmp1 > tmp0 )
				{
					numer = tmp1 - tmp0;
					denom = a00 - ((double)2) * a01 + a11;
					if ( numer >= denom )
					{
						t = (double)1;
						s = (double)0;
						sqrDistance = a11 + ((double)2) * b1 + c;
					}
					else
					{
						t = numer / denom;
						s = (double)1 - t;
						sqrDistance = s * (a00 * s + a01 * t + ((double)2) * b0) +
							t * (a01 * s + a11 * t + ((double)2) * b1) + c;
					}
				}
				else
				{
					t = (double)0;
					if ( tmp1 <= (double)0 )
					{
						s = (double)1;
						sqrDistance = a00 + ((double)2) * b0 + c;
					}
					else if ( b0 >= (double)0 )
					{
						s = (double)0;
						sqrDistance = c;
					}
					else
					{
						s = -b0 / a00;
						sqrDistance = b0 * s + c;
					}
				}
			}
			else  // region 1
			{
				numer = a11 + b1 - a01 - b0;
				if ( numer <= (double)0 )
				{
					s = (double)0;
					t = (double)1;
					sqrDistance = a11 + ((double)2) * b1 + c;
				}
				else
				{
					denom = a00 - ((double)2) * a01 + a11;
					if ( numer >= denom )
					{
						s = (double)1;
						t = (double)0;
						sqrDistance = a00 + ((double)2) * b0 + c;
					}
					else
					{
						s = numer / denom;
						t = (double)1 - s;
						sqrDistance = s * (a00 * s + a01 * t + ((double)2) * b0) +
							t * (a01 * s + a11 * t + ((double)2) * b1) + c;
					}
				}
			}
		}

		// Account for numerical round-off error.
		if ( sqrDistance < (double)0 )
		{
			sqrDistance = (double)0;
		}

		//mClosestPoint0 = mPoint;
		mClosestPoint1.x = v0.x + (float)(s * edge0.x + t * edge1.x);
		mClosestPoint1.y = v0.y + (float)(s * edge0.y + t * edge1.y);
		mClosestPoint1.z = v0.z + (float)(s * edge0.z + t * edge1.z);
		mTriangleBary[1] = (float)s;
		mTriangleBary[2] = (float)t;
		mTriangleBary[0] = (float)((double)1 - s - t);
		return (float)sqrDistance;
	}

	static Vector3 mTriangleBary = Vector3.zero;
	//static Vector3 mClosestPoint0 = Vector3.zero;
	static Vector3 mClosestPoint1 = Vector3.zero;
}
