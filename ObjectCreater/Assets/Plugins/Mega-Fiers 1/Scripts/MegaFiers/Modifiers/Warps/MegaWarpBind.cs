
using UnityEngine;

[AddComponentMenu("Modifiers/Warps/Bind")]
public class MegaWarpBind : MegaModifier
{
	public GameObject	SourceWarpObj;	// TODO: or point at mod on the warp
	GameObject			current;
	public float		decay = 0.0f;
	MegaWarp			warp;

	public override string ModName()	{ return "WarpBind"; }
	public override string GetHelpURL() { return "?page_id=576"; }

	[ContextMenu("Add To Siblings")]
	public void AddSiblings()
	{
		Transform parent = transform.parent;

		MegaModifyObject thismod = GetComponent<MegaModifyObject>();

		for ( int i = 0; i < parent.GetChildCount(); i++ )
		{
			Transform child = parent.GetChild(i);

			MegaWarpBind bind = child.gameObject.GetComponent<MegaWarpBind>();

			if ( !bind )
			{
				bind = child.gameObject.AddComponent<MegaWarpBind>();

				MegaModifyObject mod = child.gameObject.GetComponent<MegaModifyObject>();

				mod.NormalMethod = thismod.NormalMethod;

				bind.SetTarget(SourceWarpObj);
			}
		}
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		// Get point to World Space as its a WSM
		p = tm.MultiplyPoint3x4(p);

		// So this mod should transform world point into local space of mod (gizmo offset if OSM, node tm if warp) 
		p = warp.Map(i, p);

		return invtm.MultiplyPoint3x4(p);
	}

	//public override bool ModLateUpdate(Modifiers mc)
	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( SourceWarpObj != current )
		{
			//Debug.Log("Changing target ");
			current = SourceWarpObj;
			warp = null;
		}

		if ( SourceWarpObj != null )
		{
			if ( warp == null )
			{
				//Debug.Log("new warp " + SourceWarpObj.name);
				warp = SourceWarpObj.GetComponent<MegaWarp>();
			}

			if ( warp != null && warp.Enabled )
			{
				tm = transform.localToWorldMatrix;
				invtm = tm.inverse;

				return warp.Prepare(decay);
			}
		}

		return false;
	}

	public void SetTarget(GameObject go)
	{
		SourceWarpObj = go;
		if ( go )
		{
			//warp = SourceWarpObj.GetComponent<MegaWarp>();
		}
		//else
			//warp = null;
	}

	public override void Modify(MegaModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
		{
			Vector3 p = tm.MultiplyPoint3x4(verts[i]);

			// So this mod should transform world point into local space of mod (gizmo offset if OSM, node tm if warp) 
			p = warp.Map(i, p);

			sverts[i] = invtm.MultiplyPoint3x4(p);
		}
	}

	//void Reset()
	//{
		//if ( SourceWarpObj != null )
			//warp = SourceWarpObj.GetComponent<Warp>();
	//}

	public override void ModStart(MegaModifiers mc)
	{
		if ( SourceWarpObj != null && SourceWarpObj != current )
		{
			current = SourceWarpObj;
			warp = SourceWarpObj.GetComponent<MegaWarp>();
			//Debug.Log("new warp1 " + SourceWarpObj.name);
		}
	}
}
