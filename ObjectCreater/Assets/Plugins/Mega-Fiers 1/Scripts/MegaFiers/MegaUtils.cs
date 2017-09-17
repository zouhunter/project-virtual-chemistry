using UnityEngine;
using System;
//using System.IO;
using System.Collections.Generic;

//public delegate bool ParseBinCallbackType(BinaryReader br, string id);
//public delegate void ParseClassCallbackType(string classname, BinaryReader br);

public class MegaModBut
{
	public MegaModBut() { }
	public MegaModBut(string _but, string tooltip, System.Type _classname, Color _col)
	{
		name = _but;
		color = _col;
		classname = _classname;

		content = new GUIContent(_but, tooltip);
	}
	public string		name;
	public Color		color;
	public System.Type	classname;
	public GUIContent	content;
}

public enum MegaAxis
{
	X = 0,
	Y = 1,
	Z = 2,
};

public enum MegaRepeatMode
{
	Loop,
	Clamp,
	PingPong,
};

public class MegaUtils
{
#if false
	static public void Bez3D(out Vector3 b, ref Vector3[] p, float u)
	{
		Vector3 t01 = p[0] + (p[1] - p[0]) * u;
		Vector3 t12 = p[1] + (p[2] - p[1]) * u;
		Vector3 t02 = t01 + (t12 - t01) * u;

		t01 = p[2] + (p[3] - p[2]) * u;

		Vector3 t13 = t12 + (t01 - t12) * u;

		b = t02 + (t13 - t02) * u;
	}
#else
	static public void Bez3D(out Vector3 b, ref Vector3[] p, float u)
	{
		Vector3 t01 = p[0];

		t01.x += (p[1].x - p[0].x) * u;
		t01.y += (p[1].y - p[0].y) * u;
		t01.z += (p[1].z - p[0].z) * u;

		Vector3 t12 = p[1];

		t12.x += (p[2].x - p[1].x) * u;
		t12.y += (p[2].y - p[1].y) * u;
		t12.z += (p[2].z - p[1].z) * u;

		Vector3 t02 = t01 + (t12 - t01) * u;

		t01.x = p[2].x + (p[3].x - p[2].x) * u;
		t01.y = p[2].y + (p[3].y - p[2].y) * u;
		t01.z = p[2].z + (p[3].z - p[2].z) * u;

		t01.x = t12.x + (t01.x - t12.x) * u;
		t01.y = t12.y + (t01.y - t12.y) * u;
		t01.z = t12.z + (t01.z - t12.z) * u;

		b.x = t02.x + (t01.x - t02.x) * u;
		b.y = t02.y + (t01.y - t02.y) * u;
		b.z = t02.z + (t01.z - t02.z) * u;
	}
#endif

	static public float WaveFunc(float radius, float t, float amp, float waveLen, float phase, float decay)
	{
		if ( waveLen == 0.0f )
			waveLen = 0.0000001f;

		float ang = Mathf.PI * 2.0f * (radius / waveLen + phase);
		return amp * Mathf.Sin(ang) * Mathf.Exp(-decay * Mathf.Abs(radius));
	}

	static public Mesh GetMesh(GameObject go)
	{
		if ( !Application.isPlaying )
			return GetSharedMesh(go);
		//return GetSharedMesh(go);

		MeshFilter meshFilter = (MeshFilter)go.GetComponent(typeof(MeshFilter));

		//	Mesh mesh;
		if ( meshFilter != null )
			return meshFilter.mesh;	//sharedMesh;	//sharedMesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)go.GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
				return smesh.sharedMesh;
		}

		return null;
	}

	static public Mesh GetSharedMesh(GameObject go)
	{
		//if ( Application.isPlaying )
			//return GetMesh(go);

		MeshFilter meshFilter = (MeshFilter)go.GetComponent(typeof(MeshFilter));

		//	Mesh mesh;
		if ( meshFilter != null )
			return meshFilter.sharedMesh;
		else
		{
			SkinnedMeshRenderer smesh = (SkinnedMeshRenderer)go.GetComponent(typeof(SkinnedMeshRenderer));

			if ( smesh != null )
				return smesh.sharedMesh;
		}

		return null;
	}

	// All the parse stuff in here
#if false
	static public Vector3 ReadP3(BinaryReader br)
	{
		Vector3 v = Vector3.zero;

		v.x = br.ReadSingle();
		v.y = br.ReadSingle();
		v.z = br.ReadSingle();

		return v;
	}

	static public string ReadString(BinaryReader br)
	{
		int len = br.ReadInt32();
		string str = new string(br.ReadChars(len - 1));
		br.ReadChar();
		return str;
	}

	static public string ReadStr(BinaryReader br)
	{
		string str = "";
		while ( true )
		{
			char c = br.ReadChar();
			if ( c == 0 )
				break;

			str += c;
		}
		return str;
	}

	static public Vector3[] ReadP3v(BinaryReader br)
	{
		int count = br.ReadInt32();

		Vector3[] tab = new Vector3[count];

		for ( int i = 0; i < count; i++ )
		{
			tab[i].x = br.ReadSingle();
			tab[i].y = br.ReadSingle();
			tab[i].z = br.ReadSingle();
		}

		return tab;
	}

	static public List<Vector3> ReadP3l(BinaryReader br)
	{
		int count = br.ReadInt32();

		List<Vector3> tab = new List<Vector3>(count);

		Vector3 p = Vector3.zero;

		for ( int i = 0; i < count; i++ )
		{
			p.x = br.ReadSingle();
			p.y = br.ReadSingle();
			p.z = br.ReadSingle();
			tab.Add(p);
		}

		return tab;
	}

	static public float ReadMotFloat(BinaryReader br)
	{
		byte[] floatBytes = br.ReadBytes(4);
		// swap the bytes
		Array.Reverse(floatBytes);
		// get the float from the byte array
		return BitConverter.ToSingle(floatBytes, 0);
	}

	static public double ReadMotDouble(BinaryReader br)
	{
		byte[] floatBytes = br.ReadBytes(8);
		// swap the bytes
		Array.Reverse(floatBytes);
		// get the float from the byte array
		return BitConverter.ToDouble(floatBytes, 0);
	}

	static public int ReadMotInt(BinaryReader br)
	{
		byte[] floatBytes = br.ReadBytes(4);
		// swap the bytes
		Array.Reverse(floatBytes);
		// get the float from the byte array
		return BitConverter.ToInt32(floatBytes, 0);
	}

	//public delegate bool ParseBinCallbackType(BinaryReader br, string id);
	//public delegate void ParseClassCallbackType(string classname, BinaryReader br);

	static public void Parse(BinaryReader br, ParseBinCallbackType cb)
	{
		bool readchunk = true;

		while ( readchunk )
		{
			string id = MegaUtils.ReadString(br);

			if ( id == "eoc" )
				break;

			int skip = br.ReadInt32();

			long fpos = br.BaseStream.Position;

			if ( !cb(br, id) )
			{
				Debug.Log("Error Loading chunk id " + id);
				readchunk = false;	// done
				break;
			}

			br.BaseStream.Position = fpos + skip;
		}
	}
#endif

	static public int LargestComponent(Vector3 p)
	{
		if ( p.x > p.y )
			return (p.x > p.z) ? 0 : 2;
		else
			return (p.y > p.z) ? 1 : 2;
	}

	static public float LargestValue(Vector3 p)
	{
		if ( p.x > p.y )
			return (p.x > p.z) ? p.x : p.z;
		else
			return (p.y > p.z) ? p.y : p.z;
	}

	static public float LargestValue1(Vector3 p)
	{
		if ( Mathf.Abs(p.x) > Mathf.Abs(p.y) )
			return (Mathf.Abs(p.x) > Mathf.Abs(p.z)) ? p.x : p.z;
		else
			return (Mathf.Abs(p.y) > Mathf.Abs(p.z)) ? p.y : p.z;
	}

	// These two are utils so can remove from here and old morpher
	static public Vector3 Extents(Vector3[] verts, out Vector3 min, out Vector3 max)
	{
		Vector3 extent = Vector3.zero;

		min = Vector3.zero;
		max = Vector3.zero;

		if ( verts != null && verts.Length > 0 )
		{
			min = verts[0];
			max = verts[0];

			for ( int i = 1; i < verts.Length; i++ )
			{
				if ( verts[i].x < min.x ) min.x = verts[i].x;
				if ( verts[i].y < min.y ) min.y = verts[i].y;
				if ( verts[i].z < min.z ) min.z = verts[i].z;

				if ( verts[i].x > max.x ) max.x = verts[i].x;
				if ( verts[i].y > max.y ) max.y = verts[i].y;
				if ( verts[i].z > max.z ) max.z = verts[i].z;
			}

			extent = max - min;
		}

		return extent;
	}

	static public Vector3 Extents(List<Vector3> verts, out Vector3 min, out Vector3 max)
	{
		Vector3 extent = Vector3.zero;

		min = Vector3.zero;
		max = Vector3.zero;

		if ( verts != null && verts.Count > 0 )
		{
			min = verts[0];
			max = verts[0];

			for ( int i = 1; i < verts.Count; i++ )
			{
				if ( verts[i].x < min.x ) min.x = verts[i].x;
				if ( verts[i].y < min.y ) min.y = verts[i].y;
				if ( verts[i].z < min.z ) min.z = verts[i].z;

				if ( verts[i].x > max.x ) max.x = verts[i].x;
				if ( verts[i].y > max.y ) max.y = verts[i].y;
				if ( verts[i].z > max.z ) max.z = verts[i].z;
			}

			extent = max - min;
		}

		return extent;
	}

	static public int FindVert(Vector3 vert, List<Vector3> verts, float tolerance, float scl, bool flipyz, bool negx, int vn)
	{
		int find = 0;

		if ( negx )
			vert.x = -vert.x;

		if ( flipyz )
		{
			float z = vert.z;
			vert.z = vert.y;
			vert.y = z;
		}

		vert /= scl;

		float closest = Vector3.SqrMagnitude(verts[0] - vert);

		for ( int i = 0; i < verts.Count; i++ )
		{
			float dif = Vector3.SqrMagnitude(verts[i] - vert);

			if ( dif < closest )
			{
				closest = dif;
				find = i;
			}
		}

		if ( closest > tolerance )	//0.0001f )	// not exact
			return -1;

		return find;	//0;
	}

	static public void BuildTangents(Mesh mesh)
	{
		int triangleCount = mesh.triangles.Length;
		int vertexCount = mesh.vertices.Length;

		Vector3[] tan1 = new Vector3[vertexCount];
		Vector3[] tan2 = new Vector3[vertexCount];
		Vector4[] tangents = new Vector4[vertexCount];

		Vector3[] verts	= mesh.vertices;
		Vector2[] uvs		= mesh.uv;
		Vector3[] norms	= mesh.normals;
		int[]			tris	= mesh.triangles;

		for ( int a = 0; a < triangleCount; a += 3 )
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
}