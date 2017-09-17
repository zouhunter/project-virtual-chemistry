
using UnityEngine;

#if !UNITY_FLASH
// Could calc normals for say cylinder mapping etc like in Push
[AddComponentMenu("Modifiers/Displace WebCam")]
public class MegaDisplaceWebCam : MegaModifier
{
	public WebCamTexture	map;
	public float			amount	= 0.0f;
	public Vector2			offset	= Vector2.zero;
	public Vector2			scale	= Vector2.one;
	public MegaChannel		channel = MegaChannel.Red;
	public bool				CentLum	= true;
	public float			CentVal = 0.5f;
	public float			Decay	= 0.0f;

	float width = 0.0f;
	float height = 0.0f;

	public int WebWidth = 160;
	public int WebHeight = 120;
	public int WebRate = 15;

	[HideInInspector]
	public Vector2[]	uvs;
	[HideInInspector]
	public Vector3[]	normals;

	public override string ModName() { return "Displace WebCam"; }
	public override string GetHelpURL() { return "?page_id=168"; }

	public override MegaModChannel ChannelsReq() { return MegaModChannel.Verts | MegaModChannel.UV; }
	public override MegaModChannel ChannelsChanged() { return MegaModChannel.Verts; }

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
			Color col = map.GetPixel((int)(uv.x * width), (int)(uv.y * height));	//uvs[i].x, uvs[i].y);

			float str = amount;

			if ( Decay != 0.0f )
			{
				str *= (float)Mathf.Exp(-Decay * p.magnitude);
			}

			if ( CentLum )
				str *= (col[(int)channel] + CentVal);
			else
				str *= (col[(int)channel]);

			p += normals[i] * col[(int)channel] * str;
		}

		return invtm.MultiplyPoint3x4(p);
	}

	public override void Modify(MegaModifiers mc)
	{
		//if ( uvs.Length == 0 )
		//uvs = mc.uvs;

		//if ( normals.Length == 0 )
		//normals = mc.mesh.normals;
		//Vector3[]	verts = mc.GetSourceVerts();
		//Vector3[]	sverts = mc.GetDestVerts();

		for ( int i = 0; i < verts.Length; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	//public override void ModStart(MegaModifiers mc)
	//{
	//	uvs = mc.mesh.uv;
	//	normals = mc.mesh.normals;
	//}

	//public override bool ModLateUpdate(Modifiers mc)
	public override bool ModLateUpdate(MegaModContext mc)
	{
		if ( Application.isPlaying )
		{
			if ( map == null )
			{
				map = new WebCamTexture(WebWidth, WebHeight, WebRate);
				if ( map )
				{
					map.Play();
				}
				else
					Debug.Log("Couldnt create WebCamTexture");
			}
		}
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

		width = map.width;
		height = map.height;

		return true;
	}
}
#endif