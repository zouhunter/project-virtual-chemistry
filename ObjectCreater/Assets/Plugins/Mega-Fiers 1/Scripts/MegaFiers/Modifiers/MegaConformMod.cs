
using UnityEngine;

[AddComponentMenu("Modifiers/Conform")]
public class MegaConformMod : MegaModifier
{
	// Will have multiple in the end or layer
	public GameObject	target;
	public float[]		offsets;
	public Collider		conformCollider;
	public Bounds		bounds;
	public float[]		last;
	public Vector3[]	conformedVerts;
	public float		conformAmount = 1.0f;
	public float		raystartoff = 0.0f;
	public float		offset = 0.0f;
	public float		raydist = 100.0f;
	public MegaAxis		axis = MegaAxis.Y;

	Matrix4x4	loctoworld;
	Matrix4x4	ctm;
	Matrix4x4	cinvtm;
	Ray			ray = new Ray();
	RaycastHit	hit;

	public override string ModName()	{ return "Conform"; }
	public override string GetHelpURL() { return "?page_id=4547"; }

	public void SetTarget(GameObject targ)
	{
		target = targ;

		if ( target )
		{
			conformCollider = target.GetComponent<Collider>();
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		return p;
	}

	public override void Modify(MegaModifiers mc)
	{
		if ( conformCollider )
		{
			int ax = (int)axis;

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 origin = ctm.MultiplyPoint(verts[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				sverts[i] = verts[i];

				if ( conformCollider.Raycast(ray, out hit, raydist) )
				{
					Vector3 lochit = cinvtm.MultiplyPoint(hit.point);

					sverts[i][ax] = Mathf.Lerp(verts[i][ax], lochit[ax] + offsets[i] + offset, conformAmount);
					last[i] = sverts[i][ax];
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					sverts[i][ax] = last[i];
				}
			}
		}
		else
		{
			verts.CopyTo(sverts, 0);
		}
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( target )
		{
			if ( conformCollider != target.GetComponent<Collider>() )
				conformCollider = target.GetComponent<Collider>();

			if ( conformCollider == null )
				return false;

			if ( conformedVerts == null || conformedVerts.Length != mc.mod.verts.Length )
			{
				conformedVerts = new Vector3[mc.mod.verts.Length];
				// Need to run through all the source meshes and find the vertical offset from the base

				offsets = new float[mc.mod.verts.Length];
				last = new float[mc.mod.verts.Length];

				for ( int i = 0; i < mc.mod.verts.Length; i++ )
					offsets[i] = mc.mod.verts[i][(int)axis] - mc.bbox.min[(int)axis];
			}

			loctoworld = transform.localToWorldMatrix;

			ctm = loctoworld;
			cinvtm = transform.worldToLocalMatrix;	//ctm.inverse;

			return true;
		}
		else
			conformCollider = null;

		return true;	//false;
	}
}
