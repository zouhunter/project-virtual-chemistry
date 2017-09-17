
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Warp
{
	public string		name = "None";
	public Vector3[]	points = new Vector3[16];

	public void GetWarp(MegaBezPatch mod)
	{
		mod.p11 = points[0];
		mod.p12 = points[1];
		mod.p13 = points[2];
		mod.p14 = points[3];

		mod.p21 = points[4];
		mod.p22 = points[5];
		mod.p23 = points[6];
		mod.p24 = points[7];

		mod.p31 = points[8];
		mod.p32 = points[9];
		mod.p33 = points[10];
		mod.p34 = points[11];

		mod.p41 = points[12];
		mod.p42 = points[13];
		mod.p43 = points[14];
		mod.p44 = points[15];
	}

	public void SetWarp(MegaBezPatch mod)
	{
		points[0] = mod.p11;
		points[1] = mod.p12;
		points[2] = mod.p13;
		points[3] = mod.p14;

		points[4] = mod.p21;
		points[5] = mod.p22;
		points[6] = mod.p23;
		points[7] = mod.p24;

		points[8] = mod.p31;
		points[9] = mod.p32;
		points[10] = mod.p33;
		points[11] = mod.p34;

		points[12] = mod.p41;
		points[13] = mod.p42;
		points[14] = mod.p43;
		points[15] = mod.p44;
	}

	public void AdjustLattice(float wr, float hr)
	{
		Vector3 r = new Vector3(wr, hr, 1.0f);

		points[0] = Vector3.Scale(points[0], r);
		points[1] = Vector3.Scale(points[1], r);
		points[2] = Vector3.Scale(points[2], r);
		points[3] = Vector3.Scale(points[3], r);

		points[4] = Vector3.Scale(points[4], r);
		points[5] = Vector3.Scale(points[5], r);
		points[6] = Vector3.Scale(points[6], r);
		points[7] = Vector3.Scale(points[7], r);

		points[8] = Vector3.Scale(points[8], r);
		points[9] = Vector3.Scale(points[9], r);
		points[10] = Vector3.Scale(points[10], r);
		points[11] = Vector3.Scale(points[11], r);

		points[12] = Vector3.Scale(points[12], r);
		points[13] = Vector3.Scale(points[13], r);
		points[14] = Vector3.Scale(points[14], r);
		points[15] = Vector3.Scale(points[15], r);
	}
}

[ExecuteInEditMode]
public class MegaBezPatch : MonoBehaviour
{
	public float	Width				= 1.0f;
	public float	Height				= 1.0f;
	public int		WidthSegs			= 20;
	public int		HeightSegs			= 20;
	public bool		GenUVs				= true;
	public bool		recalcBounds		= false;
	//public bool		recalcNormals		= false;
	public bool		recalcTangents		= true;
	public bool		recalcCollider		= false;
	public bool		showgizmos			= true;
	public bool		showlatticepoints	= false;
	public Color	latticecol			= Color.white;
	public float	handlesize			= 0.075f;
	public bool		positionhandles		= false;
	public bool		showlabels			= true;
	public Vector2	snap				= new Vector2(0.25f, 0.25f);

	public List<Warp>	warps = new List<Warp>();

	public int		srcwarp;
	public int		destwarp;

	[HideInInspector]
	public Vector3[]	verts;
	[HideInInspector]
	public Vector2[]	uvs;
	[HideInInspector]
	public int[]		tris;
	[HideInInspector]
	public Vector3[]	norms;

	[HideInInspector]
	public bool rebuild = true;

	public Vector2	UVOffset = Vector2.zero;
	public Vector2	UVScale = Vector2.one;

	public int	currentwarp = 0;

	[HideInInspector]
	public Mesh mesh;

	public float	switchtime = 1.0f;
	public float	time = 1000.0f;

	public Vector3	p11;
	public Vector3	p21;
	public Vector3	p31;
	public Vector3	p41;

	public Vector3	p12;
	public Vector3	p22;
	public Vector3	p32;
	public Vector3	p42;

	public Vector3	p13;
	public Vector3	p23;
	public Vector3	p33;
	public Vector3	p43;

	public Vector3	p14;
	public Vector3	p24;
	public Vector3	p34;
	public Vector3	p44;

	public void AddWarp()
	{
		Warp warp = new Warp();
		warp.SetWarp(this);
		warps.Add(warp);
	}

	public void UpdateWarp(int i)
	{
		Warp warp = warps[i];
		warp.SetWarp(this);
	}

	public void SetWarp(int i)
	{
		if ( Application.isPlaying )
		{
			time = 0.0f;
			srcwarp = currentwarp;
			destwarp = i;
		}
		else
		{
			time = 100.0f;
			currentwarp = i;
			warps[i].GetWarp(this);
		}
	}

	void Start()
	{
		time = 0.0f;
	}

	public void Reset()
	{
		InitLattice();
		Rebuild();
	}

	public void Rebuild()
	{
		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mf != null )
		{
			Mesh mesh1 = mf.sharedMesh;

			if ( mesh1 == null )
			{
				mesh1 = new Mesh();
				mf.sharedMesh = mesh1;
			}
			mesh = mesh1;
		}
	}

	void Update()
	{
		ChangeWarp(srcwarp, destwarp);

		if ( mesh == null )
			Rebuild();

		if ( rebuild )
			BuildMesh(mesh);
	}

	void MakeQuad1(int f, int a, int b, int c, int d)
	{
		tris[f++] = a;
		tris[f++] = b;
		tris[f++] = c;

		tris[f++] = c;
		tris[f++] = d;
		tris[f++] = a;
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

	void UpdateSurface()
	{
	}

	// Only call this on size or seg change
	void BuildMesh(Mesh mesh)
	{
		if ( WidthSegs < 1 )
			WidthSegs = 1;

		if ( HeightSegs < 1 )
			HeightSegs = 1;

		Vector3 p = Vector3.zero;

		int numverts = (WidthSegs + 1) * (HeightSegs + 1);

		if ( verts == null )
		{
			InitLattice();
		}

		if ( verts == null || verts.Length != numverts )
		{
			verts = new Vector3[numverts];
			uvs = new Vector2[numverts];
			tris = new int[HeightSegs * WidthSegs * 2 * 3];

			norms = new Vector3[numverts];
			for ( int i = 0; i < norms.Length; i++ )
				norms[i] = Vector3.back;
		}

		Vector2 uv = Vector2.zero;

		int index = 0;

		p = Vector3.zero;

		for ( int i = 0; i <= HeightSegs; i++ )
		{
			index = i * (WidthSegs + 1);
			for ( int j = 0; j <= WidthSegs; j++ )
			{
				float xIndex = (float)j / (float)WidthSegs;
				float yIndex = (float)i / (float)HeightSegs;

				float omx = 1.0f - xIndex;
				float omy = 1.0f - yIndex;

				float x1 = omx * omx * omx;
				float x2 = (3.0f * omx) * omx * xIndex;
				float x3 = (3.0f * omx) * xIndex * xIndex;
				float x4 = xIndex * xIndex * xIndex;
				float y1 = omy * omy * omy;
				float y2 = (3.0f * omy) * omy * yIndex;
				float y3 = (3.0f * omy) * yIndex * yIndex;
				float y4 = yIndex * yIndex * yIndex;

				p.x = (x1 * p11.x * y1) + (x2 * p12.x * y1) + (x3 * p13.x * y1) + (x4 * p14.x * y1)
					+ (x1 * p21.x * y2) + (x2 * p22.x * y2) + (x3 * p23.x * y2) + (x4 * p24.x * y2)
					+ (x1 * p31.x * y3) + (x2 * p32.x * y3) + (x3 * p33.x * y3) + (x4 * p34.x * y3)
					+ (x1 * p41.x * y4) + (x2 * p42.x * y4) + (x3 * p43.x * y4) + (x4 * p44.x * y4);
				p.y = (x1 * p11.y * y1) + (x2 * p12.y * y1) + (x3 * p13.y * y1) + (x4 * p14.y * y1)
					+ (x1 * p21.y * y2) + (x2 * p22.y * y2) + (x3 * p23.y * y2) + (x4 * p24.y * y2)
					+ (x1 * p31.y * y3) + (x2 * p32.y * y3) + (x3 * p33.y * y3) + (x4 * p34.y * y3)
					+ (x1 * p41.y * y4) + (x2 * p42.y * y4) + (x3 * p43.y * y4) + (x4 * p44.y * y4);

				verts[index + j] = p;

				if ( GenUVs )
				{
					uv.x = (xIndex + UVOffset.x) * UVScale.x;
					uv.y = (yIndex + UVOffset.y) * UVScale.y;
					uvs[index + j] = uv;
				}
			}
		}

		int f = 0;

		for ( int iz = 0; iz < HeightSegs; iz++ )
		{
			int kv = iz * (WidthSegs + 1);
			for ( int ix = 0; ix < WidthSegs; ix++ )
			{
				tris[f++] = kv;
				tris[f++] = kv + WidthSegs + 1;
				tris[f++] = kv + WidthSegs + 2;

				tris[f++] = kv + WidthSegs + 2;
				tris[f++] = kv + 1;
				tris[f++] = kv;
				kv++;
			}
		}

		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
		mesh.normals = norms;
		mesh.RecalculateBounds();

		if ( recalcTangents )
			BuildTangents(mesh, verts, norms, tris, uvs);
	}

	public void InitLattice()
	{
		float w = Width;
		float h = Height;

		p11 = new Vector3(-1.5f * w, -1.5f * h, 0.0f);
		p12 = new Vector3(-0.5f * w, -1.5f * h, 0.0f);
		p13 = new Vector3(0.5f * w, -1.5f * h, 0.0f);
		p14 = new Vector3(1.5f * w, -1.5f * h, 0.0f);
		p21 = new Vector3(-1.5f * w, -0.5f * h, 0.0f);
		p22 = new Vector3(-0.5f * w, -0.5f * h, 0.0f);
		p23 = new Vector3(0.5f * w, -0.5f * h, 0.0f);
		p24 = new Vector3(1.5f * w, -0.5f * h, 0.0f);
		p31 = new Vector3(-1.5f * w, 0.5f * h, 0.0f);
		p32 = new Vector3(-0.5f * w, 0.5f * h, 0.0f);
		p33 = new Vector3(0.5f * w, 0.5f * h, 0.0f);
		p34 = new Vector3(1.5f * w, 0.5f * h, 0.0f);
		p41 = new Vector3(-1.5f * w, 1.5f * h, 0.0f);
		p42 = new Vector3(-0.5f * w, 1.5f * h, 0.0f);
		p43 = new Vector3(0.5f * w, 1.5f * h, 0.0f);
		p44 = new Vector3(1.5f * w, 1.5f * h, 0.0f);
	}

#if false
	public void CreateLattice(int wp, int hp)
	{
		//float w = Width;
		//float h = Height;
		lpoints = new Vector3[wp * hp];

		Vector3 p = Vector3.zero;

		for ( int y = 0; y < hp; y++ )
		{
			float ya = (float)y / (float)(hp - 1);

			for ( int x = 0; x < wp; x++ )
			{
				float a = (float)x / (float)(wp - 1);

				p.x = Mathf.Lerp(-2.0f, 1.0f, a);
				p.y = Mathf.Lerp(-2.0f, 1.0f, ya);
				lpoints[(y * wp) + x] = p;
			}
		}
	}
#endif

	public Vector3[]	lpoints;

	public void AdjustLattice(float w, float h)
	{
		float wr = w / Width;
		float hr = h / Height;

		Vector3 r = new Vector3(wr, hr, 1.0f);

		p11 = Vector3.Scale(p11, r);
		p12 = Vector3.Scale(p12, r);
		p13 = Vector3.Scale(p13, r);
		p14 = Vector3.Scale(p14, r);

		p21 = Vector3.Scale(p21, r);
		p22 = Vector3.Scale(p22, r);
		p23 = Vector3.Scale(p23, r);
		p24 = Vector3.Scale(p24, r);

		p31 = Vector3.Scale(p31, r);
		p32 = Vector3.Scale(p32, r);
		p33 = Vector3.Scale(p33, r);
		p34 = Vector3.Scale(p34, r);

		p41 = Vector3.Scale(p41, r);
		p42 = Vector3.Scale(p42, r);
		p43 = Vector3.Scale(p43, r);
		p44 = Vector3.Scale(p44, r);

		for ( int i = 0; i < warps.Count; i++ )
		{
			warps[i].AdjustLattice(wr, hr);
		}

		Height = h;
		Width = w;
	}

	static public void BuildTangents(Mesh mesh, Vector3[] verts, Vector3[] norms, int[] tris, Vector2[] uvs)
	{
		int vertexCount = mesh.vertices.Length;

		Vector3[] tan1 = new Vector3[vertexCount];
		Vector3[] tan2 = new Vector3[vertexCount];
		Vector4[] tangents = new Vector4[vertexCount];

		for ( int a = 0; a < tris.Length; a += 3 )
		{
			long i1 = tris[a];
			long i2 = tris[a + 1];
			long i3 = tris[a + 2];

			Vector3 v1 = verts[i1];
			Vector3 v2 = verts[i2];
			Vector3 v3 = verts[i3];

			Vector2 w1 = uvs[i1];
			Vector2 w2 = uvs[i2];
			Vector2 w3 = uvs[i3];

			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;

			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;

			float r = 1.0f / (s1 * t2 - s2 * t1);

			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;

			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}

		for ( int a = 0; a < vertexCount; a++ )
		{
			Vector3 n = norms[a];
			Vector3 t = tan1[a];

			Vector3.OrthoNormalize(ref n, ref t);
			tangents[a].x = t.x;
			tangents[a].y = t.y;
			tangents[a].z = t.z;
			tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}

		mesh.tangents = tangents;
	}

	Vector3 bounce(Vector3 start, Vector3 end, float value)
	{
		value /= 1.0f;
		end -= start;
		if ( value < (1.0f / 2.75f) )
		{
			return end * (7.5625f * value * value) + start;
		}
		else
		{
			if ( value < (2.0f / 2.75f) )
			{
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + 0.75f) + start;
			}
			else
			{
				if ( value < (2.5f / 2.75f) )
				{
					value -= (2.25f / 2.75f);
					return end * (7.5625f * (value) * value + .9375f) + start;
				}
				else
				{
					value -= (2.625f / 2.75f);
					return end * (7.5625f * (value) * value + .984375f) + start;
				}
			}
		}
	}

	Vector3 easeInOutSine(Vector3 start, Vector3 end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	float delay = -1.0f;


	public void ChangeWarp(int f, int t)
	{
		if ( !Application.isPlaying )
			return;

		if ( delay > 0.0f )
		{
			delay -= Time.deltaTime;
			return;
		}

		if ( time <= switchtime )
		{
			time += Time.deltaTime;

			Warp from = warps[f];
			Warp to = warps[t];

			float a = time / switchtime;
			if ( a > 1.0f )
			{
				a = 1.0f;
				currentwarp = t;
				t++;
				destwarp = t;
				if ( destwarp >= warps.Count )
					destwarp = 0;
				srcwarp = currentwarp;
				time = 0.0f;
				delay = 1.0f;
			}

			p11 = easeInOutSine(from.points[0], to.points[0], a);
			p12 = easeInOutSine(from.points[1], to.points[1], a);
			p13 = easeInOutSine(from.points[2], to.points[2], a);
			p14 = easeInOutSine(from.points[3], to.points[3], a);

			p21 = easeInOutSine(from.points[4], to.points[4], a);
			p22 = easeInOutSine(from.points[5], to.points[5], a);
			p23 = easeInOutSine(from.points[6], to.points[6], a);
			p24 = easeInOutSine(from.points[7], to.points[7], a);

			p31 = easeInOutSine(from.points[8], to.points[8], a);
			p32 = easeInOutSine(from.points[9], to.points[9], a);
			p33 = easeInOutSine(from.points[10], to.points[10], a);
			p34 = easeInOutSine(from.points[11], to.points[11], a);

			p41 = easeInOutSine(from.points[12], to.points[12], a);
			p42 = easeInOutSine(from.points[13], to.points[13], a);
			p43 = easeInOutSine(from.points[14], to.points[14], a);
			p44 = easeInOutSine(from.points[15], to.points[15], a);
		}
	}
}
