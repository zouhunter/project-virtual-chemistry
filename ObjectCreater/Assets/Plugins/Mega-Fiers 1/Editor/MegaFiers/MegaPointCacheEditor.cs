
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// Support anim uvs here as well
[CustomEditor(typeof(MegaPointCache))]
public class MegaPointCacheEditor : MegaModifierEditor
{
	static string lastpath = " ";

	public delegate bool ParseBinCallbackType(BinaryReader br, string id);
	public delegate void ParseClassCallbackType(string classname, BinaryReader br);

	MegaModifiers mods;
	List<MegaPCVert>	Verts = new List<MegaPCVert>();

	// Mapping values
	float	tolerance	= 0.0001f;
	float	scl			= 1.0f;
	bool	flipyz		= false;
	bool	negx		= false;
	bool	negz		= false;	// 8 cases now

	string Read(BinaryReader br, int count)
	{
		byte[] buf = br.ReadBytes(count);
		return System.Text.Encoding.ASCII.GetString(buf, 0, buf.Length);
	}

	struct MCCFrame
	{
		public Vector3[] points;
	}

	// Maya format
	void LoadMCC()
	{
		//MegaPointCache am = (MegaPointCache)target;
		//mods = am.gameObject.GetComponent<MegaModifiers>();

		string filename = EditorUtility.OpenFilePanel("Maya Cache File", lastpath, "mc");

		if ( filename == null || filename.Length < 1 )
			return;

		LoadMCC(filename);
	}

#if false
	public void LoadMCC(string filename)
	{
		MegaPointCache am = (MegaPointCache)target;
		mods = am.gameObject.GetComponent<MegaModifiers>();

		if ( mods == null )
		{
			Debug.LogWarning("You need to add a Mega Modify Object component first!");
			return;
		}

		lastpath = filename;

		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		string id = Read(br, 4);
		if ( id != "FOR4" )
		{
			Debug.Log("wrong file");
			return;
		}

		int offset  = MegaParse.ReadMotInt(br);

		br.ReadBytes(offset);

		List<MCCFrame> frames = new List<MCCFrame>();

		while ( true )
		{
			string btag = Read(br, 4);
			if ( btag == "" )
				break;

			if ( btag != "FOR4" )
			{
				Debug.Log("File format error");
				return;
			}

			int blocksize = MegaParse.ReadMotInt(br);

			int bytesread = 0;

			btag = Read(br, 4);
			if ( btag != "MYCH" )
			{
				Debug.Log("File format error");
				return;
			}
			bytesread += 4;

			btag = Read(br, 4);
			if ( btag != "TIME" )
			{
				Debug.Log("File format error");
				return;
			}
			bytesread += 4;

			br.ReadBytes(4);
			bytesread += 4;

			int time = MegaParse.ReadMotInt(br);
			bytesread += 4;

			am.maxtime = (float)time / 6000.0f;

			while ( bytesread < blocksize )
			{
				btag = Read(br, 4);
				if ( btag != "CHNM" )
				{
					Debug.Log("chm error");
					return;
				}
				bytesread += 4;

				int chnmsize = MegaParse.ReadMotInt(br);
				bytesread += 4;

				int mask = 3;
				int chnmsizetoread = (chnmsize + mask) & (~mask);
				//byte[] channelname = br.ReadBytes(chnmsize);
				br.ReadBytes(chnmsize);

				int paddingsize = chnmsizetoread - chnmsize;

				if ( paddingsize > 0 )
					br.ReadBytes(paddingsize);

				bytesread += chnmsizetoread;

				btag = Read(br, 4);

				if ( btag != "SIZE" )
				{
					Debug.Log("Size error");
					return;
				}
				bytesread += 4;

				br.ReadBytes(4);
				bytesread += 4;

				int arraylength = MegaParse.ReadMotInt(br);
				bytesread += 4;

				MCCFrame frame = new MCCFrame();
				frame.points = new Vector3[arraylength];

				string dataformattag = Read(br, 4);
				int bufferlength = MegaParse.ReadMotInt(br);
				bytesread += 8;

				if ( dataformattag == "FVCA" )
				{
					if ( bufferlength != arraylength * 3 * 4 )
					{
						Debug.Log("buffer len error");
						return;
					}

					for ( int i = 0; i < arraylength; i++ )
					{
						frame.points[i].x = MegaParse.ReadMotFloat(br);
						frame.points[i].y = MegaParse.ReadMotFloat(br);
						frame.points[i].z = MegaParse.ReadMotFloat(br);
					}

					bytesread += arraylength * 3 * 4;
				}
				else
				{
					if ( dataformattag == "DVCA" )
					{
						if ( bufferlength != arraylength * 3 * 8 )
						{
							Debug.Log("buffer len error");
							return;
						}

						for ( int i = 0; i < arraylength; i++ )
						{
							frame.points[i].x = (float)MegaParse.ReadMotDouble(br);
							frame.points[i].y = (float)MegaParse.ReadMotDouble(br);
							frame.points[i].z = (float)MegaParse.ReadMotDouble(br);
						}

						bytesread += arraylength * 3 * 8;
					}
					else
					{
						Debug.Log("Format Error");
						return;
					}
				}

				frames.Add(frame);
			}
		}

		// Build table
		am.Verts = new MegaPCVert[frames[0].points.Length];

		for ( int i = 0; i < am.Verts.Length; i++ )
		{
			am.Verts[i] = new MegaPCVert();
			am.Verts[i].points = new Vector3[frames.Count];

			for ( int p = 0; p < am.Verts[i].points.Length; p++ )
				am.Verts[i].points[p] = frames[p].points[i];
		}

		BuildData(mods, am, filename);
		br.Close();
	}
#else
	public void LoadMCC(string filename)
	{
		MegaPointCache am = (MegaPointCache)target;
		oldverts = am.Verts;
		mods = am.gameObject.GetComponent<MegaModifiers>();

		if ( mods == null )
		{
			Debug.LogWarning("You need to add a Mega Modify Object component first!");
			return;
		}

		lastpath = filename;

		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		string id = Read(br, 4);
		if ( id != "FOR4" )
		{
			Debug.Log("wrong file");
			return;
		}

		int offset  = MegaParse.ReadMotInt(br);

		br.ReadBytes(offset);

		List<MCCFrame> frames = new List<MCCFrame>();

		int buflen = 0;
		//int clen = 0;

		while ( true )
		{
			string btag = Read(br, 4);
			if ( btag == "" )
				break;

			if ( btag != "FOR4" )
			{
				Debug.Log("File format error");
				return;
			}

			int blocksize = MegaParse.ReadMotInt(br);

			int bytesread = 0;

			btag = Read(br, 4);
			if ( btag != "MYCH" )
			{
				Debug.Log("File format error");
				return;
			}
			bytesread += 4;

			btag = Read(br, 4);
			if ( btag != "TIME" )
			{
				Debug.Log("File format error");
				return;
			}
			bytesread += 4;

			br.ReadBytes(4);
			bytesread += 4;

			int time = MegaParse.ReadMotInt(br);
			bytesread += 4;

			am.maxtime = (float)time / 6000.0f;

			while ( bytesread < blocksize )
			{
				btag = Read(br, 4);
				if ( btag != "CHNM" )
				{
					Debug.Log("chm error");
					return;
				}
				bytesread += 4;

				int chnmsize = MegaParse.ReadMotInt(br);
				bytesread += 4;

				int mask = 3;
				int chnmsizetoread = (chnmsize + mask) & (~mask);
				//byte[] channelname = br.ReadBytes(chnmsize);
				br.ReadBytes(chnmsize);

				int paddingsize = chnmsizetoread - chnmsize;

				if ( paddingsize > 0 )
					br.ReadBytes(paddingsize);

				bytesread += chnmsizetoread;

				btag = Read(br, 4);

				if ( btag != "SIZE" )
				{
					Debug.Log("Size error");
					return;
				}
				bytesread += 4;

				br.ReadBytes(4);
				bytesread += 4;

				int arraylength = MegaParse.ReadMotInt(br);
				bytesread += 4;

				MCCFrame frame = new MCCFrame();
				frame.points = new Vector3[arraylength];

				string dataformattag = Read(br, 4);
				int bufferlength = MegaParse.ReadMotInt(br);
				//Debug.Log("buflen " + bufferlength);

				if ( buflen == 0 )
				{
					buflen = bufferlength;
				}

				bytesread += 8;

				if ( dataformattag == "FVCA" )
				{
					if ( bufferlength != arraylength * 3 * 4 )
					{
						Debug.Log("buffer len error");
						return;
					}

					for ( int i = 0; i < arraylength; i++ )
					{
						frame.points[i].x = MegaParse.ReadMotFloat(br);
						frame.points[i].y = MegaParse.ReadMotFloat(br);
						frame.points[i].z = MegaParse.ReadMotFloat(br);
					}

					bytesread += arraylength * 3 * 4;
				}
				else
				{
					if ( dataformattag == "DVCA" )
					{
						if ( bufferlength != arraylength * 3 * 8 )
						{
							Debug.Log("buffer len error");
							return;
						}

						for ( int i = 0; i < arraylength; i++ )
						{
							frame.points[i].x = (float)MegaParse.ReadMotDouble(br);
							frame.points[i].y = (float)MegaParse.ReadMotDouble(br);
							frame.points[i].z = (float)MegaParse.ReadMotDouble(br);
						}

						bytesread += arraylength * 3 * 8;
					}
					else
					{
						Debug.Log("Format Error");
						return;
					}
				}

				if ( buflen == bufferlength )
					frames.Add(frame);
			}
		}

		//Debug.Log("frames " + frames.Count);

		// Build table
		am.Verts = new MegaPCVert[frames[0].points.Length];

		//Debug.Log("am.Verts " + am.Verts.Length);

		for ( int i = 0; i < am.Verts.Length; i++ )
		{
			am.Verts[i] = new MegaPCVert();
			am.Verts[i].points = new Vector3[frames.Count];

			for ( int p = 0; p < am.Verts[i].points.Length; p++ )
			{
				//Debug.Log("i " + i + " p " + p);
				am.Verts[i].points[p] = frames[p].points[i];
			}
		}

		BuildData(mods, am, filename);
		br.Close();
	}
#endif

	void LoadMDD()
	{
		//MegaPointCache am = (MegaPointCache)target;
		//mods = am.gameObject.GetComponent<MegaModifiers>();

		string filename = EditorUtility.OpenFilePanel("Motion Designer File", lastpath, "mdd");

		if ( filename == null || filename.Length < 1 )
			return;

		LoadMDD(filename);
	}

	public void LoadMDD(string filename)
	{
		MegaPointCache am = (MegaPointCache)target;
		oldverts = am.Verts;
		mods = am.gameObject.GetComponent<MegaModifiers>();

		if ( mods == null)
		{
			Debug.LogWarning("You need to add a Mega Modify Object component first!");
			return;
		}
		lastpath = filename;

		// Clear what we have
		Verts.Clear();

		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		int numSamples = MegaParse.ReadMotInt(br);
		int numPoints = MegaParse.ReadMotInt(br);

		float t = 0.0f;

		for ( int i = 0; i < numSamples; i++ )
			t = MegaParse.ReadMotFloat(br);

		am.maxtime = t;

		am.Verts = new MegaPCVert[numPoints];

		for ( int i = 0; i < am.Verts.Length; i++ )
		{
			am.Verts[i] = new MegaPCVert();
			am.Verts[i].points = new Vector3[numSamples];
		}

		for ( int i = 0; i < numSamples; i++ )
		{
			for ( int v = 0; v < numPoints; v++ )
			{
				am.Verts[v].points[i].x = MegaParse.ReadMotFloat(br);
				am.Verts[v].points[i].y = MegaParse.ReadMotFloat(br);
				am.Verts[v].points[i].z = MegaParse.ReadMotFloat(br);
			}
		}

		BuildData(mods, am, filename);
		br.Close();
	}

	public bool DoAdjustFirst = false;

	public bool uselastframe = false;
	public int mapframe = 0;

	MegaPCVert[]	oldverts;

	void BuildData(MegaModifiers mods, MegaPointCache am, string filename)
	{
		bool domapping = true;
		if ( am.havemapping )
			domapping = EditorUtility.DisplayDialog("Point Cache Mapping", "Replace Existing Mapping?", "Yes", "No");

		if ( !domapping )
		{
			if ( DoAdjustFirst )
				AdjustVertsSimple(mods, am);
		}
		else
		{
			if ( DoAdjustFirst )
				AdjustVerts(mods, am);
		}
		// Build vector3[] of base verts
		Vector3[] baseverts = new Vector3[am.Verts.Length];

		int findex = 0;
		if ( uselastframe )
		{
			findex = am.Verts[0].points.Length - 1;
		}
		for ( int i = 0; i < am.Verts.Length; i++ )
			baseverts[i] = am.Verts[i].points[findex];

#if false
		for ( int i = 0; i < 32; i++ )
		{
			Debug.Log("vert " + mods.verts[i].ToString("0.000"));
		}

		for ( int i = 0; i < 32; i++ )
		{
			Debug.Log("pc " + baseverts[i].ToString("0.000"));
		}
#endif

		if ( domapping )
		{
			if ( !TryMapping1(baseverts, mods.verts) )
			{
				EditorUtility.DisplayDialog("Mapping Failed!", "Mapping of " + System.IO.Path.GetFileNameWithoutExtension(filename) + " failed!", "OK");
				EditorUtility.ClearProgressBar();
				am.havemapping = false;
				return;
			}

			am.negx = negx;
			am.negz = negz;
			am.flipyz = flipyz;
			am.scl = scl;
		}
		else
		{
			negx = am.negx;
			negz = am.negz;
			flipyz = am.flipyz;
			scl = am.scl;
		}

		am.havemapping = true;

		// Remap vertices
		for ( int i = 0; i < am.Verts.Length; i++ )
		{
			for ( int v = 0; v < am.Verts[i].points.Length; v++ )
			{
				if ( negx )
					am.Verts[i].points[v].x = -am.Verts[i].points[v].x;

				if ( flipyz )
				{
					float z = am.Verts[i].points[v].z;
					am.Verts[i].points[v].z = am.Verts[i].points[v].y;
					am.Verts[i].points[v].y = z;
				}

				if ( negz )
					am.Verts[i].points[v].z = -am.Verts[i].points[v].z;

				am.Verts[i].points[v] = am.Verts[i].points[v] * scl;
			}
		}

		if ( domapping )
		{
			for ( int i = 0; i < am.Verts.Length; i++ )
			{
				am.Verts[i].indices = FindVerts(am.Verts[i].points[findex]);

				if ( am.Verts[i].indices.Length == 0 )
				{
					EditorUtility.DisplayDialog("Final Mapping Failed!", "Mapping of " + System.IO.Path.GetFileNameWithoutExtension(filename) + " failed!", "OK");
					EditorUtility.ClearProgressBar();
					return;
				}
			}
		}
		else
		{
			for ( int i = 0; i < am.Verts.Length; i++ )
			{
				am.Verts[i].indices = oldverts[i].indices;
			}
		}

		oldverts = null;
	}

	public void LoadFile(string filename)
	{
		string ext = System.IO.Path.GetExtension(filename);

		ext = ext.ToLower();

		switch ( ext )
		{
			case ".pc2":
				LoadPC2(filename);
				break;

			case ".mdd":
				LoadMDD(filename);
				break;

			case ".mc":
				LoadMCC(filename);
				break;
		}
	}

	void LoadPC2()
	{
		//MegaPointCache am = (MegaPointCache)target;
		//mods = am.gameObject.GetComponent<MegaModifiers>();

		string filename = EditorUtility.OpenFilePanel("Point Cache File", lastpath, "pc2");

		if ( filename == null || filename.Length < 1 )
			return;

		LoadPC2(filename);
	}

	public void LoadPC2(string filename)
	{
		MegaPointCache am = (MegaPointCache)target;
		oldverts = am.Verts;
		mods = am.gameObject.GetComponent<MegaModifiers>();

		if ( mods == null )
		{
			Debug.LogWarning("You need to add a Mega Modify Object component first!");
			return;
		}

		lastpath = filename;

		// Clear what we have
		Verts.Clear();

		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		string sig = MegaParse.ReadStr(br);
		if ( sig != "POINTCACHE2" )
		{
			EditorUtility.DisplayDialog("PC2 Importer", "The selected file does not appear to be a valid PC2 File", "Ok");
			br.Close();
			return;
		}

		int fileVersion = br.ReadInt32();
		if ( fileVersion != 1 )
		{
			br.Close();
			return;
		}

		int numPoints = br.ReadInt32();
		br.ReadSingle();
		br.ReadSingle();
		int numSamples = br.ReadInt32();

		am.Verts = new MegaPCVert[numPoints];

		for ( int i = 0; i < am.Verts.Length; i++ )
		{
			am.Verts[i] = new MegaPCVert();
			am.Verts[i].points = new Vector3[numSamples];
		}

		for ( int i = 0; i < numSamples; i++ )
		{
			for ( int v = 0; v < numPoints; v++ )
				am.Verts[v].points[i] = MegaParse.ReadP3(br);
		}
		BuildData(mods, am, filename);
		br.Close();
	}

	// Utils methods
	int[] FindVerts(Vector3 p)
	{
		List<int>	indices = new List<int>();
		for ( int i = 0; i < mods.verts.Length; i++ )
		{
			//float dist = Vector3.Distance(p, mods.verts[i]);
			float dist = Vector3.SqrMagnitude(p - mods.verts[i]);
			if ( dist < tolerance )	//0.0001f )	//mods.verts[i].Equals(p)  )
				indices.Add(i);
		}
		return indices.ToArray();
	}

	Vector3 Extents(Vector3[] verts, out Vector3 min, out Vector3 max)
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

	Vector3 Extents(MegaPCVert[] verts, out Vector3 min, out Vector3 max)
	{
		Vector3 extent = Vector3.zero;

		min = Vector3.zero;
		max = Vector3.zero;

		if ( verts != null && verts.Length > 0 )
		{
			min = verts[0].points[0];
			max = verts[0].points[0];

			for ( int i = 1; i < verts.Length; i++ )
			{
				Vector3 p = verts[i].points[0];

				if ( p.x < min.x ) min.x = p.x;
				if ( p.y < min.y ) min.y = p.y;
				if ( p.z < min.z ) min.z = p.z;

				if ( p.x > max.x ) max.x = p.x;
				if ( p.y > max.y ) max.y = p.y;
				if ( p.z > max.z ) max.z = p.z;
			}

			extent = max - min;
		}

		return extent;
	}

	bool showadvanced = false;

	public override void OnInspectorGUI()
	{
		MegaPointCache am = (MegaPointCache)target;

		DoAdjustFirst = EditorGUILayout.Toggle("Mapping Adjust", DoAdjustFirst);
		uselastframe = EditorGUILayout.Toggle("Use Last Frame", uselastframe);

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Import PC2") )
		{
			LoadPC2();
			EditorUtility.SetDirty(target);
		}

		if ( GUILayout.Button("Import MDD") )
		{
			LoadMDD();
			EditorUtility.SetDirty(target);
		}

		if ( GUILayout.Button("Import MC") )
		{
			LoadMCC();
			EditorUtility.SetDirty(target);
		}

		EditorGUILayout.EndHorizontal();

		// Basic mod stuff
		showmodparams = EditorGUILayout.Foldout(showmodparams, "Modifier Common Params");

		if ( showmodparams )
			CommonModParamsBasic(am);

		// Advanced
		showadvanced = EditorGUILayout.Foldout(showadvanced, "Advanced Params");

		if ( showadvanced )
		{
			tolerance = EditorGUILayout.FloatField("Map Tolerance", tolerance * 100.0f) / 100.0f;
			if ( am.Verts != null && am.Verts.Length > 0 )	//[0] != null )
			{
				am.showmapping = EditorGUILayout.BeginToggleGroup("Show Mapping", am.showmapping);
				am.mapStart = EditorGUILayout.IntSlider("StartVert", am.mapStart, 0, am.Verts.Length);
				am.mapEnd = EditorGUILayout.IntSlider("endVert", am.mapEnd, 0, am.Verts.Length);
				am.mappingSize = EditorGUILayout.Slider("Size", am.mappingSize, 0.0005f, 0.01f);
				EditorGUILayout.EndToggleGroup();
			}

			//morph.tolerance = EditorGUILayout.Slider("Tolerance", morph.tolerance, 0.0f, 0.01f);
		}

		am.time = EditorGUILayout.FloatField("Time", am.time);
		am.maxtime = EditorGUILayout.FloatField("Loop Time", am.maxtime);
		am.animated = EditorGUILayout.Toggle("Animated", am.animated);
		am.speed = EditorGUILayout.FloatField("Speed", am.speed);
		am.LoopMode = (MegaRepeatMode)EditorGUILayout.EnumPopup("Loop Mode", am.LoopMode);
		am.interpMethod = (MegaInterpMethod)EditorGUILayout.EnumPopup("Interp Method", am.interpMethod);

		am.blendMode = (MegaBlendAnimMode)EditorGUILayout.EnumPopup("Blend Mode", am.blendMode);
		if ( am.blendMode == MegaBlendAnimMode.Additive )
			am.weight = EditorGUILayout.FloatField("Weight", am.weight);

		if ( am.Verts != null && am.Verts.Length > 0 )
		{
			int mem = am.Verts.Length * am.Verts[0].points.Length * 12;
			EditorGUILayout.LabelField("Memory: ", (mem / 1024) + "KB");
		}

		if ( GUI.changed )
			EditorUtility.SetDirty(target);
	}

	// From here down could move to util class
	int FindVert(Vector3 vert, Vector3[] tverts, float tolerance, float scl, bool flipyz, bool negx, bool negz, int vn)
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

		if ( negz )
			vert.z = -vert.z;

		vert /= scl;

		float closest = Vector3.SqrMagnitude(tverts[0] - vert);

		for ( int i = 0; i < tverts.Length; i++ )
		{
			float dif = Vector3.SqrMagnitude(tverts[i] - vert);

			if ( dif < closest )
			{
				closest = dif;
				find = i;
			}
		}

		if ( closest > tolerance )	//0.0001f )	// not exact
		{
			return -1;
		}

		return find;	//0;
	}

	bool DoMapping(Vector3[] verts, Vector3[] tverts, float scale, bool flipyz, bool negx, bool negz)
	{
		int count = 400;

		for ( int i = 0; i < verts.Length; i++ )
		{
			float a = (float)i / (float)verts.Length;

			count--;
			if ( count < 0 )
			{
				//Debug.Log("map " + i + " vert " + verts[i].ToString("0.00000"));
				EditorUtility.DisplayProgressBar("Mapping", "Mapping vertex " + i, a);
				count = 400;
			}
			int mapping = FindVert(verts[i], tverts, tolerance, scale, flipyz, negx, negz, i);

			if ( mapping == -1 )
			{
				// Failed
				EditorUtility.ClearProgressBar();
				return false;
			}
		}

		EditorUtility.ClearProgressBar();
		return true;
	}

	// Out of this we need scl, negx, negz and flipyz
	bool TryMapping1(Vector3[] tverts, Vector3[] verts)
	{
		//for ( int i = 0; i < 8; i++ )
		//	Debug.Log("cache vert " + tverts[i].ToString("0.00000"));

		//for ( int i = 0; i < 8; i++ )
		//	Debug.Log("mesh vert " + verts[i].ToString("0.00000"));

		// Get extents for mod verts and for imported meshes, if not the same then scale
		Vector3 min1,max1;
		Vector3 min2,max2;

		Vector3 ex1 = Extents(verts, out min1, out max1);
		Vector3 ex2 = Extents(tverts, out min2, out max2);

		//Debug.Log("mesh extents " + ex1.ToString("0.00000"));
		//Debug.Log("cache extents " + ex2.ToString("0.00000"));

		//Debug.Log("mesh min " + min1.ToString("0.00000"));
		//Debug.Log("cache min " + min2.ToString("0.00000"));

		//Debug.Log("mesh max " + max1.ToString("0.00000"));
		//Debug.Log("cache max " + max2.ToString("0.00000"));

		int largest1 = MegaUtils.LargestComponent(ex1);
		int largest2 = MegaUtils.LargestComponent(ex2);

		//Debug.Log(largest1 + " " + largest2);
		scl = ex1[largest1] / ex2[largest2];
		//Debug.Log("scl " + scl.ToString("0.0000"));

		//Vector3 offset = verts[0] - (tverts[0] * scl);
		//Debug.Log("Offset " + offset.ToString("0.00000"));
		// need min max on all axis so we can produce an offset to add

		int map = 0;

		for ( map = 0; map < 8; map++ )
		{
			flipyz = ((map & 4) != 0);
			negx = ((map & 2) != 0);
			negz = ((map & 1) != 0);

			bool mapped = DoMapping(verts, tverts, scl, flipyz, negx, negz);
			if ( mapped )
				break;
		}

		if ( map == 8 )	// We couldnt find any mapping
			return false;

		//Debug.Log("scl " + scl + " negx " + negx + " negz " + negz + " flipyz " + flipyz);
		return true;
	}

	void AdjustVertsSimple(MegaModifiers mods, MegaPointCache am)
	{
		if ( am.Verts != null )
		{
			Vector3[] baseverts = new Vector3[am.Verts.Length];

			for ( int i = 0; i < am.Verts.Length; i++ )
				baseverts[i] = am.Verts[i].points[0];

			for ( int i = 0; i < am.Verts.Length; i++ )
			{
				for ( int j = 0; j < am.Verts[i].points.Length; j++ )
				{
					Vector3 p = am.Verts[i].points[j] * am.adjustscl;

					am.Verts[i].points[j] = p - am.adjustoff;
				}
			}
		}
	}

	void AdjustVerts(MegaModifiers mods, MegaPointCache am)
	{
		if ( am.Verts != null )
		{
			Vector3[] baseverts = new Vector3[am.Verts.Length];

			for ( int i = 0; i < am.Verts.Length; i++ )
				baseverts[i] = am.Verts[i].points[0];

			Vector3 min1,max1;
			Vector3 min2,max2;

			Vector3 ex1 = Extents(mods.verts, out min1, out max1);
			Vector3 ex2 = Extents(baseverts, out min2, out max2);

			//Debug.Log("mesh extents " + ex1.ToString("0.00000"));
			//Debug.Log("cache extents " + ex2.ToString("0.00000"));

			//Debug.Log("mesh min " + min1.ToString("0.00000"));
			//Debug.Log("cache min " + min2.ToString("0.00000"));

			//Debug.Log("mesh max " + max1.ToString("0.00000"));
			//Debug.Log("cache max " + max2.ToString("0.00000"));

			int largest1 = MegaUtils.LargestComponent(ex1);
			int largest2 = MegaUtils.LargestComponent(ex2);

			//Debug.Log(largest1 + " " + largest2);
			am.adjustscl = ex1[largest1] / ex2[largest2];
			//Debug.Log("scl " + scl1.ToString("0.0000"));

			//off = verts[0] - (tverts[0] * scl);
			am.adjustoff = (min2 * am.adjustscl) - min1;

			for ( int i = 0; i < am.Verts.Length; i++ )
			{
				for ( int j = 0; j < am.Verts[i].points.Length; j++ )
				{
					Vector3 p = am.Verts[i].points[j] * am.adjustscl;

					am.Verts[i].points[j] = p - am.adjustoff;
				}
			}
		}
	}

	MegaModifyObject mo = null;

	//public void OnSceneGUI()
	public override void DrawSceneGUI()
	{
		MegaPointCache mod = (MegaPointCache)target;
		if ( mod.showmapping )
		{
			if ( mod.Verts != null && mod.Verts[0] != null )
			{
				float vsize = mod.mappingSize;
				float vsize1 = vsize * 0.75f;
				Matrix4x4 tm = mod.gameObject.transform.localToWorldMatrix;
				Handles.matrix = tm;	//Matrix4x4.identity;
				Handles.color = Color.green;

				if ( mo == null )
					mo = mod.gameObject.GetComponent<MegaModifyObject>();

				if ( mo )
				{
					for ( int i = 0; i < mo.verts.Length; i++ )
					{
						Vector3 p = mo.verts[i];
						Handles.DotCap(i, p, Quaternion.identity, vsize);
					}
				}

				if ( mod.mapEnd >= mod.Verts.Length )
					mod.mapEnd = mod.Verts.Length - 1;

				if ( mod.mapStart > mod.mapEnd )
					mod.mapStart = mod.mapEnd;

				Handles.color = Color.white;	//red;

				int findex = 0;
				if ( uselastframe )
				{
					findex = mod.Verts[0].points.Length - 1;
				}
				for ( int i = mod.mapStart; i < mod.mapEnd; i++ )
				{
					Vector3 p = mod.Verts[i].points[findex];
					Handles.DotCap(i, p, Quaternion.identity, vsize1);
				}

				Handles.matrix = Matrix4x4.identity;
			}
		}
	}
}