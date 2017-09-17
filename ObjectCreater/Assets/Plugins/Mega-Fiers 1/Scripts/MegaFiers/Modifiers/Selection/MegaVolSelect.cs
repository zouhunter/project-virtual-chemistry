
using UnityEngine;

[AddComponentMenu("Modifiers/Selection/Volume")]
public class MegaVolSelect : MegaSelectionMod
{
	public override MegaModChannel ChannelsReq() { return MegaModChannel.Col | MegaModChannel.Verts; }

	public override string ModName() { return "Vol Select"; }
	public override string GetHelpURL() { return "?page_id=1307"; }

	//public MegaChannel	channel = MegaChannel.Red;

	float[]	modselection;

	public float[] GetSel() { return modselection; }

	public Vector3	origin = Vector3.zero;
	public float	falloff = 1.0f;
	public float	radius = 1.0f;
	public Color	gizCol = new Color(0.5f, 0.5f, 0.5f, 0.25f);
	public float	gizSize = 0.01f;
	public bool		useCurrentVerts = true;
	public bool		displayWeights = true;
	public bool		freezeSelection = false;
	public MegaVolumeType	volType = MegaVolumeType.Sphere;
	public Vector3	boxsize = Vector3.one;
	public float weight = 1.0f;

	public Transform	target;

	float GetDistBox(Vector3 p)
	{
		// Work in the box's coordinate system.
		Vector3 diff = p - origin;

		float sqrDistance = 0.0f;
		float delta;

		Vector3 closest = diff;

		if ( closest.x < -boxsize.x )
		{
			delta = closest.x + boxsize.x;
			sqrDistance += delta * delta;
			closest.x = -boxsize.x;
		}
		else
		{
			if ( closest.x > boxsize.x )
			{
				delta = closest.x - boxsize.x;
				sqrDistance += delta * delta;
				closest.x = boxsize.x;
			}
		}

		if ( closest.y < -boxsize.y )
		{
			delta = closest.y + boxsize.y;
			sqrDistance += delta * delta;
			closest.y = -boxsize.y;
		}
		else
		{
			if ( closest.y > boxsize.y )
			{
				delta = closest.y - boxsize.y;
				sqrDistance += delta * delta;
				closest.y = boxsize.y;
			}
		}

		if ( closest.z < -boxsize.z )
		{
			delta = closest.z + boxsize.z;
			sqrDistance += delta * delta;
			closest.z = -boxsize.z;
		}
		else
		{
			if ( closest.z > boxsize.z )
			{
				delta = closest.z - boxsize.z;
				sqrDistance += delta * delta;
				closest.z = boxsize.z;
			}
		}

		return Mathf.Sqrt(sqrDistance);	// * 0.5f;
	}

	public override void GetSelection(MegaModifiers mc)
	{
		if ( target )
		{
			origin = transform.worldToLocalMatrix.MultiplyPoint(target.position);
		}

		if ( modselection == null || modselection.Length != mc.verts.Length )
		{
			modselection = new float[mc.verts.Length];
		}

		if ( freezeSelection )
		{
			mc.selection = modselection;
			return;
		}

		// we dont need to update if nothing changes
		if ( volType == MegaVolumeType.Sphere )
		{
			if ( useCurrentVerts )
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					float d = Vector3.Distance(origin, verts[i]) - radius;

					if ( d < 0.0f )
						modselection[i] = weight;
					else
					{
						float w = Mathf.Exp(-falloff * Mathf.Abs(d));
						modselection[i] = w * weight;	//mc.cols[i][c];
					}
				}
			}
			else
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					float d = Vector3.Distance(origin, verts[i]) - radius;

					if ( d < 0.0f )
						modselection[i] = weight;
					else
					{
						float w = Mathf.Exp(-falloff * Mathf.Abs(d));
						modselection[i] = w * weight;	//mc.cols[i][c];
					}
				}
			}
		}
		else
		{
			if ( useCurrentVerts )
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					float d = GetDistBox(verts[i]);

					if ( d < 0.0f )
						modselection[i] = weight;
					else
					{
						float w = Mathf.Exp(-falloff * Mathf.Abs(d));
						modselection[i] = w * weight;	//mc.cols[i][c];
					}
				}
			}
			else
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					float d = GetDistBox(verts[i]);

					if ( d < 0.0f )
						modselection[i] = weight;
					else
					{
						float w = Mathf.Exp(-falloff * Mathf.Abs(d));
						modselection[i] = w * weight;	//mc.cols[i][c];
					}
				}
			}
		}
		//if ( weight == 1.0f )
		//	mc.selection = null;	// Normal system

		// We only need the copy if we are first mod
		if ( (mc.dirtyChannels & MegaModChannel.Verts) == 0 )
		{
			mc.InitVertSource();
			//verts.CopyTo(sverts, 0);
			//mc.UpdateMesh = 1;
		}

		//Debug.Log("sel " + modselection.Length);
		mc.selection = modselection;
	}

	public override void DrawGizmo(MegaModContext context)
	{
		if ( ModEnabled )
		{
			base.DrawGizmo(context);

			Matrix4x4 tm = gameObject.transform.localToWorldMatrix;
			Gizmos.matrix = tm;
			if ( enabled && volType == MegaVolumeType.Box )
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(origin, boxsize * 2.0f);	// * 0.5f);
			}

			if ( enabled && volType == MegaVolumeType.Sphere )
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(origin, radius);	// * 0.5f);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

}
