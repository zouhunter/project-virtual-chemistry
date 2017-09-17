
using UnityEngine;
using System.Collections.Generic;

// TODO: Help as a popup window of text

// This will be a basic box mesh but need to have different uvs for front and back, and maybe different mtlids
// if have thickness then again different mtlid maybe and or uvs
// first version ignore thickness

// each mesh should have an ption to set pivot, or centre it, also rotate

// should derive from SimpleMesh or some class as will be adding a few of these
[AddComponentMenu("MegaShapes/Page Mesh")]
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaMeshPage : MonoBehaviour
{
	public float	Width		= 1.0f;
	public float	Length		= 1.41f;
	public float	Height		= 0.1f;
	public int		WidthSegs	= 10;
	public int		LengthSegs	= 10;
	public int		HeightSegs	= 1;
	public bool		genUVs		= true;
	public float	rotate		= 0.0f;
	public bool		PivotBase	= false;
	public bool		PivotEdge	= true;
	public bool		tangents	= false;
	public bool		optimize	= false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=853");
	}

	void Reset()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mf != null )
		{
			Mesh mesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);

			if ( mesh == null )
			{
				mesh = new Mesh();
				mf.sharedMesh = mesh;
			}

			if ( mesh != null )
			{
				BuildMesh(mesh);
				MegaModifyObject mo = GetComponent<MegaModifyObject>();
				if ( mo != null )
				{
					mo.MeshUpdated();
				}
			}
		}
	}

	static void MakeQuad1(List<int> f, int a, int b, int c, int d)
	{
		f.Add(a);
		f.Add(b);
		f.Add(c);

		f.Add(c);
		f.Add(d);
		f.Add(a);
	}

	// Put in utils
	int MaxComponent(Vector3 v)
	{
		if ( Mathf.Abs(v.x) > Mathf.Abs(v.y) )
		{
			if ( Mathf.Abs(v.x) > Mathf.Abs(v.z) )
				return 0;
			else
				return 2;
		}
		else
		{
			if ( Mathf.Abs(v.y) > Mathf.Abs(v.z) )
				return 1;
			else
				return 2;
		}
	}

	void BuildMesh(Mesh mesh)
	{
		Width = Mathf.Clamp(Width, 0.0f, float.MaxValue);
		Length = Mathf.Clamp(Length, 0.0f, float.MaxValue);
		Height = Mathf.Clamp(Height, 0.0f, float.MaxValue);

		LengthSegs = Mathf.Clamp(LengthSegs, 1, 200);
		HeightSegs = Mathf.Clamp(HeightSegs, 1, 200);
		WidthSegs = Mathf.Clamp(WidthSegs, 1, 200);

		Vector3 vb = new Vector3(Width, Height, Length) / 2.0f;
		Vector3 va = -vb;

		if ( PivotBase )
		{
			va.y = 0.0f;
			vb.y = Height;
		}

		if ( PivotEdge )
		{
			va.x = 0.0f;
			vb.x = Width;
		}

		float dx = Width / (float)WidthSegs;
		float dy = Height / (float)HeightSegs;
		float dz = Length / (float)LengthSegs;

		Vector3 p = va;

		// Lists should be static, clear out to reuse
		List<Vector3>	verts = new List<Vector3>();
		List<Vector2>	uvs = new List<Vector2>();
		List<int>		tris = new List<int>();
		List<int>		tris1 = new List<int>();
		List<int>		tris2 = new List<int>();

		Vector2 uv = Vector2.zero;

		// Do we have top and bottom
		if ( Width > 0.0f && Length > 0.0f )
		{
			Matrix4x4 tm1 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, rotate, 0.0f), Vector3.one);

			Vector3 uv1 = Vector3.zero;

			p.y = vb.y;
			for ( int iz = 0; iz <= LengthSegs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);

					if ( genUVs )
					{
						//uv.x = Mathf.Repeat((p.x + vb.x) / Width, 1.0f);
						uv.x = (p.x - va.x) / Width;
						uv.y = (p.z + vb.z) / Length;

						uv1.x = uv.x - 0.5f;
						uv1.y = 0.0f;
						uv1.z = uv.y - 0.5f;

						uv1 = tm1.MultiplyPoint3x4(uv1);
						uv.x = 0.5f + uv1.x;
						uv.y = 0.5f + uv1.z;

						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iz = 0; iz < LengthSegs; iz++ )
			{
				int kv = iz * (WidthSegs + 1);
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris, kv, kv + WidthSegs + 1, kv + WidthSegs + 2, kv + 1);
					kv++;
				}
			}

			int index = verts.Count;

			p.y = va.y;
			p.z = va.z;

			for ( int iy = 0; iy <= LengthSegs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = 1.0f - ((p.x + vb.x) / Width);
						uv.y = ((p.z + vb.z) / Length);

						uv1.x = uv.x - 0.5f;
						uv1.y = 0.0f;
						uv1.z = uv.y - 0.5f;

						uv1 = tm1.MultiplyPoint3x4(uv1);
						uv.x = 0.5f + uv1.x;
						uv.y = 0.5f + uv1.z;

						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iy = 0; iy < LengthSegs; iy++ )
			{
				int kv = iy * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris1, kv, kv + 1, kv + WidthSegs + 2, kv + WidthSegs + 1);
					kv++;
				}
			}
		}

		// Front back
		if ( Width > 0.0f && Height > 0.0f )
		{
			int index = verts.Count;

			p.z = va.z;
			p.y = va.y;
			for ( int iz = 0; iz <= HeightSegs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.x + vb.x) / Width;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < HeightSegs; iz++ )
			{
				int kv = iz * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + WidthSegs + 1, kv + WidthSegs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.z = vb.z;
			p.y = va.y;
			for ( int iy = 0; iy <= HeightSegs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.x + vb.x) / Width;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < HeightSegs; iy++ )
			{
				int kv = iy * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + WidthSegs + 2, kv + WidthSegs + 1);
					kv++;
				}
			}
		}

		// Left Right
		if ( Length > 0.0f && Height > 0.0f )
		{
			int index = verts.Count;

			p.x = vb.x;
			p.y = va.y;
			for ( int iz = 0; iz <= HeightSegs; iz++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= LengthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.z + vb.z) / Length;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < HeightSegs; iz++ )
			{
				int kv = iz * (LengthSegs + 1) + index;
				for ( int ix = 0; ix < LengthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + LengthSegs + 1, kv + LengthSegs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.x = va.x;
			p.y = va.y;
			for ( int iy = 0; iy <= HeightSegs; iy++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= LengthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.z + vb.z) / Length;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}

					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < HeightSegs; iy++ )
			{
				int kv = iy * (LengthSegs + 1) + index;
				for ( int ix = 0; ix < LengthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + LengthSegs + 2, kv + LengthSegs + 1);
					kv++;
				}
			}
		}

		mesh.Clear();

		mesh.subMeshCount = 3;

		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.SetTriangles(tris.ToArray(), 0);
		mesh.SetTriangles(tris1.ToArray(), 1);
		mesh.SetTriangles(tris2.ToArray(), 2);

		mesh.RecalculateNormals();

		if ( tangents )
			MegaUtils.BuildTangents(mesh);

		if ( optimize )
			mesh.Optimize();

		mesh.RecalculateBounds();
	}


	void BuildMeshOld(Mesh mesh)
	{
		Width = Mathf.Clamp(Width, 0.0f, float.MaxValue);
		Length = Mathf.Clamp(Length, 0.0f, float.MaxValue);
		Height = Mathf.Clamp(Height, 0.0f, float.MaxValue);

		LengthSegs = Mathf.Clamp(LengthSegs, 1, 200);
		HeightSegs = Mathf.Clamp(HeightSegs, 1, 200);
		WidthSegs = Mathf.Clamp(WidthSegs, 1, 200);

		Vector3 vb = new Vector3(Width, Height, Length) / 2.0f;
		Vector3 va = -vb;

		if ( PivotBase )
		{
			va.y = 0.0f;
			vb.y = Height;
		}

		if ( PivotEdge )
		{
			va.x = 0.0f;
			vb.x = Width;
		}

		float dx = Width / (float)WidthSegs;
		float dy = Height / (float)HeightSegs;
		float dz = Length / (float)LengthSegs;

		Vector3 p = va;

		// Lists should be static, clear out to reuse
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int>			tris = new List<int>();
		List<int>			tris1 = new List<int>();
		List<int>			tris2 = new List<int>();

		Vector2 uv = Vector2.zero;

		// Do we have top and bottom
		if ( Width > 0.0f && Length > 0.0f )
		{
			//Matrix4x4 tm1 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, rotate, 0.0f), Vector3.one);

			p.y = vb.y;
			for ( int iz = 0; iz <= LengthSegs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);

					if ( genUVs )
					{
						uv.x = (p.x + vb.x) / Width;
						uv.y = (p.z + vb.z) / Length;
						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iz = 0; iz < LengthSegs; iz++ )
			{
				int kv = iz * (WidthSegs + 1);
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris, kv, kv + WidthSegs + 1, kv + WidthSegs + 2, kv + 1);
					kv++;
				}
			}

			int index = verts.Count;

			p.y = va.y;
			p.z = va.z;

			for ( int iy = 0; iy <= LengthSegs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = 1.0f - ((p.x + vb.x) / Width);
						uv.y = ((p.z + vb.z) / Length);

						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iy = 0; iy < LengthSegs; iy++ )
			{
				int kv = iy * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris1, kv, kv + 1, kv + WidthSegs + 2, kv + WidthSegs + 1);
					kv++;
				}
			}
		}

		// Front back
		if ( Width > 0.0f && Height > 0.0f )
		{
			int index = verts.Count;

			p.z = va.z;
			p.y = va.y;
			for ( int iz = 0; iz <= HeightSegs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.x + vb.x) / Width;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < HeightSegs; iz++ )
			{
				int kv = iz * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + WidthSegs + 1, kv + WidthSegs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.z = vb.z;
			p.y = va.y;
			for ( int iy = 0; iy <= HeightSegs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= WidthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.x + vb.x) / Width;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < HeightSegs; iy++ )
			{
				int kv = iy * (WidthSegs + 1) + index;
				for ( int ix = 0; ix < WidthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + WidthSegs + 2, kv + WidthSegs + 1);
					kv++;
				}
			}
		}

		// Left Right
		if ( Length > 0.0f && Height > 0.0f )
		{
			int index = verts.Count;

			p.x = vb.x;
			p.y = va.y;
			for ( int iz = 0; iz <= HeightSegs; iz++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= LengthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.z + vb.z) / Length;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}
					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < HeightSegs; iz++ )
			{
				int kv = iz * (LengthSegs + 1) + index;
				for ( int ix = 0; ix < LengthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + LengthSegs + 1, kv + LengthSegs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.x = va.x;
			p.y = va.y;
			for ( int iy = 0; iy <= HeightSegs; iy++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= LengthSegs; ix++ )
				{
					verts.Add(p);
					if ( genUVs )
					{
						uv.x = (p.z + vb.z) / Length;
						uv.y = (p.y + vb.y) / Height;
						uvs.Add(uv);
					}

					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < HeightSegs; iy++ )
			{
				int kv = iy * (LengthSegs + 1) + index;
				for ( int ix = 0; ix < LengthSegs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + LengthSegs + 2, kv + LengthSegs + 1);
					kv++;
				}
			}
		}

		mesh.Clear();

		mesh.subMeshCount = 3;

		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.SetTriangles(tris.ToArray(), 0);
		mesh.SetTriangles(tris1.ToArray(), 1);
		mesh.SetTriangles(tris2.ToArray(), 2);

		mesh.RecalculateNormals();

		if ( tangents )
			MegaUtils.BuildTangents(mesh);

		if ( optimize )
			mesh.Optimize();

		mesh.RecalculateBounds();
	}
}
