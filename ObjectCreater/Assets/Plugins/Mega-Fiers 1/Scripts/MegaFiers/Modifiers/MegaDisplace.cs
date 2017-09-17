
using UnityEngine;

public enum MegaChannel
{
	Red = 0,
	Green,
	Blue,
	Alpha,
}

// Could calc normals for say cylinder mapping etc like in Push
[AddComponentMenu("Modifiers/Displace")]
public class MegaDisplace : MegaModifier
{
	public Texture2D	map;
	public float		amount	= 0.0f;
	public Vector2		offset	= Vector2.zero;
	public float		vertical = 0.0f;
	public Vector2		scale	= Vector2.one;
	public MegaChannel	channel = MegaChannel.Red;
	public bool			CentLum	= true;
	public float		CentVal = 0.5f;
	public float		Decay	= 0.0f;

	[HideInInspector]
	public Vector2[]	uvs;
	[HideInInspector]
	public Vector3[]	normals;

	public override string ModName()	{ return "Displace"; }
	public override string GetHelpURL() { return "?page_id=168"; }

	public override MegaModChannel ChannelsReq()		{ return MegaModChannel.Verts | MegaModChannel.UV; }
	public override MegaModChannel ChannelsChanged()	{ return MegaModChannel.Verts; }

	[ContextMenu("Init")]
	public virtual void Init()
	{
		MegaModifyObject mod = (MegaModifyObject)GetComponent<MegaModifyObject>();
		uvs = mod.cachedMesh.uv;
		normals = mod.cachedMesh.normals;
	}

	public override void MeshChanged()
	{
		Init();
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		p = tm.MultiplyPoint3x4(p);

		if ( i >= 0 )
		{
			Vector2 uv = Vector2.Scale(uvs[i] + offset, scale);
			Color col = map.GetPixelBilinear(uv.x, uv.y);	//uvs[i].x, uvs[i].y);

			float str = amount;

			if ( Decay != 0.0f )
				str *= (float)Mathf.Exp(-Decay * p.magnitude);

			if ( CentLum )
				str *= (col[(int)channel] + CentVal);
			else
				str *= (col[(int)channel]);

			//p += normals[i] * col[(int)channel] * str;
			//p += normals[i] * vertical;

			float of = col[(int)channel] * str;
			p.x += (normals[i].x * of) + (normals[i].x * vertical);
			p.y += (normals[i].y * of) + (normals[i].y * vertical);
			p.z += (normals[i].z * of) + (normals[i].z * vertical);
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override void Modify(MegaModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	public override bool ModLateUpdate(MegaModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(MegaModContext mc)
	{
		if ( uvs == null || uvs.Length == 0 )
			uvs = mc.mod.mesh.uv;

		if ( normals == null || normals.Length == 0 )
		{
			MegaModifyObject mobj = (MegaModifyObject)GetComponent<MegaModifyObject>();
			if ( mobj )
				normals = mobj.cachedMesh.normals;
			else
				normals = mc.mod.mesh.normals;
		}

		if ( uvs.Length == 0 )
			return false;

		if ( normals.Length == 0 )
			return false;

		if ( map == null )
			return false;

		return true;
	}
}