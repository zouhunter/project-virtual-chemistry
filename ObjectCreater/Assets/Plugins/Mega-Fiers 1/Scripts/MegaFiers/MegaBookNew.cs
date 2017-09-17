
#if false
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaPage
{
	public MegaModifyObject	modobj;
	public MegaBend[]		bendmods;
	public Mesh				mesh;
	public float			alpha;
	public float			lastalpha;
}

// Alpha code version of new page turning effect system
[ExecuteInEditMode]
public class MegaBookNew : MonoBehaviour
{
	public MegaPage[]	pages;

	MegaBend[]	bendmods;

	public float	flexer_crease_area = 54.0f;
	public float	flexer_max_angle = 190.0f;
	public float	flexer_crease_center = 0.0f;

	public float	turner_crease_area = 3.0f;
	public float	turner_max_angle = 185.0f;
	public float	turner_crease_center = 0.0f;

	public float	lander_crease_area = 10.0f;
	public float	lander_max_angle = 5.0f;
	public float	lander_crease_center = 0.0f;

	// TODO: randomize sizes for uneven older books
	public float	page_width = 48.0f;	//1.0f;
	public float	page_length = 50.0f;
	public float	page_height = 50.0f;

	public int		width_segs = 50;
	public int		length_segs = 10;
	public int		height_segs = 1;

	public float	ext_rot = 0.0f;

	public float	alpha = 0.0f;

	public int		numpages = 2;
	public float	pagegap = 0.1f;
	public GameObject	frontcover;
	public GameObject	backcover;


	public AnimationCurve flexangcrv = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.5f, 0.0f), new Keyframe(0.857f, 0.0f), new Keyframe(1, 0.0f));
	public AnimationCurve turnerangcrv = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.142f, 0.0f), new Keyframe(0.857f, 0.0f), new Keyframe(1, 0.0f));

	public AnimationCurve turnertocrv = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
	public AnimationCurve landerangcrv = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.714f, 0.0f), new Keyframe(1.0f, 0.0f));

	public AnimationCurve rotcrv = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));

	void Update()
	{
		if ( bendmods == null )
		{
			bendmods = GetComponents<MegaBend>();

			bendmods[0].axis = MegaAxis.X;
			bendmods[0].doRegion = true;
			bendmods[0].to = flexer_crease_area;
			bendmods[0].from = 0.0f;
			bendmods[0].Offset = new Vector3(flexer_crease_area, 0.0f, 0.0f);

			bendmods[1].axis = MegaAxis.X;
			bendmods[1].doRegion = true;
			bendmods[1].to = turner_crease_area;
			bendmods[1].from = 0.0f;
			bendmods[1].Offset = new Vector3(-(page_width / 2.0f + turner_crease_center), 0.0f, 0.0f);
			bendmods[1].gizmoRot = new Vector3(0.0f, ext_rot, 0.0f);
			bendmods[1].gizmoPos = new Vector3((-((page_width / 2.0f) - (Mathf.Cos((ext_rot)) * (page_width / 2.0f)))), 0.0f, (((Mathf.Abs(Mathf.Sin((ext_rot)))) * (page_width / 2.0f)) + ((Mathf.Abs(Mathf.Sin(Mathf.Rad2Deg * 0.0f))) * (page_width / 2.0f))));

			bendmods[2].axis = MegaAxis.X;
			bendmods[2].doRegion = true;
			bendmods[2].to = 0.0f;
			bendmods[2].from = -lander_crease_area;
			bendmods[2].Offset = new Vector3(-page_width + lander_crease_area, 0.0f, 0.0f);
		}

		if ( bendmods != null && bendmods.Length == 3 )
		{
			UpdateCurves();

			float a = alpha * 0.01f;

			bendmods[1].to = turnertocrv.Evaluate(a);
			bendmods[1].angle = turnerangcrv.Evaluate(a);
			bendmods[0].angle = flexangcrv.Evaluate(a);
			bendmods[2].angle = landerangcrv.Evaluate(a);
		}
	}

	void UpdateCurves()
	{
		float a = alpha * 0.01f;
		ext_rot = rotcrv.Evaluate(a);

		// Init modifiers
		bendmods[0].axis = MegaAxis.X;
		bendmods[0].doRegion = true;
		bendmods[0].to = flexer_crease_area;
		bendmods[0].from = 0.0f;
		bendmods[0].Offset = new Vector3(flexer_crease_center, 0.0f, 0.0f);

		bendmods[1].axis = MegaAxis.X;
		bendmods[1].doRegion = true;
		bendmods[1].to = turner_crease_area;
		bendmods[1].from = 0.0f;
		bendmods[1].Offset = new Vector3(-(page_width / 2.0f + turner_crease_center), 0.0f, 0.0f);
		bendmods[1].gizmoRot = new Vector3(0.0f, ext_rot, 0.0f);
		bendmods[1].gizmoPos = new Vector3((-((page_width / 2.0f) - (Mathf.Cos((Mathf.Deg2Rad * ext_rot)) * (page_width / 2.0f)))), 0.0f, (((Mathf.Abs(Mathf.Sin((Mathf.Deg2Rad * ext_rot)))) * (page_width / 2.0f)) + ((Mathf.Abs(Mathf.Sin(Mathf.Rad2Deg * 0.0f))) * (page_width / 2.0f))));

		bendmods[2].axis = MegaAxis.X;
		bendmods[2].doRegion = true;
		bendmods[2].to = 0.0f;
		bendmods[2].from = -lander_crease_area;
		bendmods[2].Offset = new Vector3(-page_width + lander_crease_center, 0.0f, 0.0f);

		Keyframe kf = turnertocrv.keys[0];
		kf.value = turner_crease_area + 50.0f;
		turnertocrv.MoveKey(0, kf);

		kf = turnertocrv.keys[1];
		kf.value = turner_crease_area;
		turnertocrv.MoveKey(1, kf);

		kf = flexangcrv.keys[1];
		kf.value = -(flexer_max_angle);
		flexangcrv.MoveKey(1, kf);

		kf = flexangcrv.keys[2];
		kf.value = (flexer_max_angle) - (((flexer_max_angle) / 100.0f) * 25.0f);
		flexangcrv.MoveKey(2, kf);

		kf = turnerangcrv.keys[2];
		kf.value = -(turner_max_angle);
		turnerangcrv.MoveKey(2, kf);
		kf = turnerangcrv.keys[3];
		kf.value = -(turner_max_angle);
		turnerangcrv.MoveKey(3, kf);

		kf = landerangcrv.keys[2];
		kf.value = -(lander_max_angle);
		landerangcrv.MoveKey(2, kf);
	}

	// Curves for the angle stuff, show as advanced params
	static void MakeQuad1(List<int> f, int a, int b, int c, int d)
	{
		f.Add(a);
		f.Add(b);
		f.Add(c);

		f.Add(c);
		f.Add(d);
		f.Add(a);
	}

	void BuildMesh(Mesh mesh)
	{
		page_width = Mathf.Clamp(page_width, 0.0f, float.MaxValue);
		page_length = Mathf.Clamp(page_length, 0.0f, float.MaxValue);
		page_height = Mathf.Clamp(page_height, 0.0f, float.MaxValue);

		length_segs = Mathf.Clamp(length_segs, 1, 200);
		height_segs = Mathf.Clamp(height_segs, 1, 200);
		width_segs = Mathf.Clamp(width_segs, 1, 200);

		Vector3 vb = new Vector3(page_width, page_height, page_length) / 2.0f;
		Vector3 va = -vb;

		va.y = 0.0f;
		vb.y = page_height;

		float dx = page_width / (float)width_segs;
		float dy = page_height / (float)height_segs;
		float dz = page_length / (float)length_segs;

		Vector3 p = va;

		// Lists should be static, clear out to reuse
		List<Vector3>	verts = new List<Vector3>();
		List<Vector2>	uvs = new List<Vector2>();
		List<int>		tris = new List<int>();
		List<int>		tris1 = new List<int>();
		List<int>		tris2 = new List<int>();

		Vector2 uv = Vector2.zero;

		// Do we have top and bottom
		if ( page_width > 0.0f && page_length > 0.0f )
		{
			Matrix4x4 tm1 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, rotate, 0.0f), Vector3.one);

			Vector3 uv1 = Vector3.zero;

			p.y = vb.y;
			for ( int iz = 0; iz <= length_segs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= width_segs; ix++ )
				{
					verts.Add(p);

					uv.x = (p.x - va.x) / page_width;
					uv.y = (p.z + vb.z) / page_length;

					uv1.x = uv.x - 0.5f;
					uv1.y = 0.0f;
					uv1.z = uv.y - 0.5f;

					uv1 = tm1.MultiplyPoint3x4(uv1);
					uv.x = 0.5f + uv1.x;
					uv.y = 0.5f + uv1.z;

					uvs.Add(uv);
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iz = 0; iz < length_segs; iz++ )
			{
				int kv = iz * (width_segs + 1);
				for ( int ix = 0; ix < width_segs; ix++ )
				{
					MakeQuad1(tris, kv, kv + width_segs + 1, kv + width_segs + 2, kv + 1);
					kv++;
				}
			}

			int index = verts.Count;

			p.y = va.y;
			p.z = va.z;

			for ( int iy = 0; iy <= length_segs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= width_segs; ix++ )
				{
					verts.Add(p);
					uv.x = 1.0f - ((p.x + vb.x) / page_width);
					uv.y = ((p.z + vb.z) / page_length);

					uv1.x = uv.x - 0.5f;
					uv1.y = 0.0f;
					uv1.z = uv.y - 0.5f;

					uv1 = tm1.MultiplyPoint3x4(uv1);
					uv.x = 0.5f + uv1.x;
					uv.y = 0.5f + uv1.z;

					uvs.Add(uv);
					p.x += dx;
				}
				p.z += dz;
			}

			for ( int iy = 0; iy < length_segs; iy++ )
			{
				int kv = iy * (width_segs + 1) + index;
				for ( int ix = 0; ix < width_segs; ix++ )
				{
					MakeQuad1(tris1, kv, kv + 1, kv + width_segs + 2, kv + width_segs + 1);
					kv++;
				}
			}
		}

		// Front back
		if ( page_width > 0.0f && page_height > 0.0f )
		{
			int index = verts.Count;

			p.z = va.z;
			p.y = va.y;
			for ( int iz = 0; iz <= height_segs; iz++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= width_segs; ix++ )
				{
					verts.Add(p);
					uv.x = (p.x + vb.x) / page_width;
					uv.y = (p.y + vb.y) / page_height;
					uvs.Add(uv);
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < height_segs; iz++ )
			{
				int kv = iz * (width_segs + 1) + index;
				for ( int ix = 0; ix < width_segs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + width_segs + 1, kv + width_segs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.z = vb.z;
			p.y = va.y;
			for ( int iy = 0; iy <= height_segs; iy++ )
			{
				p.x = va.x;
				for ( int ix = 0; ix <= width_segs; ix++ )
				{
					verts.Add(p);
					uv.x = (p.x + vb.x) / page_width;
					uv.y = (p.y + vb.y) / page_height;
					uvs.Add(uv);
					p.x += dx;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < height_segs; iy++ )
			{
				int kv = iy * (width_segs + 1) + index;
				for ( int ix = 0; ix < width_segs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + width_segs + 2, kv + width_segs + 1);
					kv++;
				}
			}
		}

		// Left Right
		if ( page_length > 0.0f && page_height > 0.0f )
		{
			int index = verts.Count;

			p.x = vb.x;
			p.y = va.y;
			for ( int iz = 0; iz <= height_segs; iz++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= length_segs; ix++ )
				{
					verts.Add(p);
					uv.x = (p.z + vb.z) / page_length;
					uv.y = (p.y + vb.y) / page_height;
					uvs.Add(uv);
					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iz = 0; iz < height_segs; iz++ )
			{
				int kv = iz * (length_segs + 1) + index;
				for ( int ix = 0; ix < length_segs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + length_segs + 1, kv + length_segs + 2, kv + 1);
					kv++;
				}
			}

			index = verts.Count;

			p.x = va.x;
			p.y = va.y;
			for ( int iy = 0; iy <= height_segs; iy++ )
			{
				p.z = va.z;
				for ( int ix = 0; ix <= length_segs; ix++ )
				{
					verts.Add(p);
					uv.x = (p.z + vb.z) / page_length;
					uv.y = (p.y + vb.y) / page_height;
					uvs.Add(uv);

					p.z += dz;
				}
				p.y += dy;
			}

			for ( int iy = 0; iy < height_segs; iy++ )
			{
				int kv = iy * (length_segs + 1) + index;
				for ( int ix = 0; ix < length_segs; ix++ )
				{
					MakeQuad1(tris2, kv, kv + 1, kv + length_segs + 2, kv + length_segs + 1);
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

	// just delete all objects
	// only call this num pages change
	void BuildPageObjects()
	{
		//for ( int i = numpages; i < transform.childCount; i++ )
		//{
		//	GameObject go = transform.GetChild(i).gameObject;

		//	if ( Application.isEditor && !Application.isPlaying )
		//		GameObject.DestroyImmediate(go);
		//	else
		//		GameObject.Destroy(go);
		//}

		for ( int i = 0; i < transform.childCount; i++ )
		{
			GameObject go = transform.GetChild(i).gameObject;

			if ( Application.isEditor && !Application.isPlaying )
				GameObject.DestroyImmediate(go);
			else
				GameObject.Destroy(go);
		}

		Vector3 pos = Vector3.zero;

		//for ( int i = 0; i < transform.childCount; i++ )
		//{
		//	GameObject go = transform.GetChild(i).gameObject;
		//	go.name = "Page " + i;

		//	go.transform.localPosition = pos;
		//	pos.y += pagegap;	//((float)i / (float)numpages)

		//	MeshRenderer mr1 = (MeshRenderer)go.GetComponent<MeshRenderer>();
		//	MeshFilter mf1 = (MeshFilter)go.GetComponent<MeshFilter>();

		//	Mesh mesh = mf1.sharedMesh;
		//	BuildMesh(mesh);
		//	mf1.sharedMesh = mesh;
		//}

		//meshes = new Mesh[numpages];
		pages = new MegaPage[numpages];

		for ( int i = 0; i < numpages; i++ )
		{
			GameObject go = new GameObject();	//(GameObject)Instantiate(LinkObj);	//linkMesh);	//, Vectp, Quaternion.identity);
			go.name = "Page " + i;

			go.transform.localPosition = pos;
			pos.y += pagegap;	//((float)i / (float)numpages)

			//MeshRenderer mr1 = (MeshRenderer)go.AddComponent<MeshRenderer>();
			MeshFilter mf1 = (MeshFilter)go.AddComponent<MeshFilter>();

			Mesh mesh = new Mesh();

			BuildMesh(mesh);
			mf1.sharedMesh = mesh;

			//meshes[i] = mesh;
			go.transform.parent = transform;

			MegaModifyObject modobj = go.AddComponent<MegaModifyObject>();
			modobj.NormalMethod = MegaNormalMethod.Unity;

			go.AddComponent<MegaBend>();
			go.AddComponent<MegaBend>();
			go.AddComponent<MegaBend>();

			modobj.MeshUpdated();

			MegaPage page = new MegaPage();
			page.bendmods = GetComponents<MegaBend>();
			page.mesh = mesh;
			page.modobj = modobj;

			pages[i] = page;
			//UpdateModifier(go);
		}

		// So now need to add the modifiers etc to each game object
		// add modify object, set norm mode to unity
		// add 3 bend modifiers
	}

	//Mesh[]	meshes;

	void UpdateMeshes()
	{
		for ( int i = 0; i < pages.Length; i++ )
		{
			BuildMesh(pages[i].mesh);
		}
	}

	void UpdateModifier(GameObject go)
	{
		for ( int i = 0; i < pages.Length; i++ )
		{
			MegaPage page = pages[i];

			page.bendmods[0].axis = MegaAxis.X;
			page.bendmods[0].doRegion = true;
			page.bendmods[0].to = flexer_crease_area;
			page.bendmods[0].from = 0.0f;
			page.bendmods[0].Offset = new Vector3(flexer_crease_center, 0.0f, 0.0f);

			page.bendmods[1].axis = MegaAxis.X;
			page.bendmods[1].doRegion = true;
			page.bendmods[1].to = turner_crease_area;
			page.bendmods[1].from = 0.0f;
			page.bendmods[1].Offset = new Vector3(-(page_width / 2.0f + turner_crease_center), 0.0f, 0.0f);
			page.bendmods[1].gizmoRot = new Vector3(0.0f, ext_rot, 0.0f);
			page.bendmods[1].gizmoPos = new Vector3((-((page_width / 2.0f) - (Mathf.Cos((Mathf.Deg2Rad * ext_rot)) * (page_width / 2.0f)))), 0.0f, (((Mathf.Abs(Mathf.Sin((Mathf.Deg2Rad * ext_rot)))) * (page_width / 2.0f)) + ((Mathf.Abs(Mathf.Sin(Mathf.Rad2Deg * 0.0f))) * (page_width / 2.0f))));

			page.bendmods[2].axis = MegaAxis.X;
			page.bendmods[2].doRegion = true;
			page.bendmods[2].to = 0.0f;
			page.bendmods[2].from = -lander_crease_area;
			page.bendmods[2].Offset = new Vector3(-page_width + lander_crease_center, 0.0f, 0.0f);
		}
	}

	void UpdatePages()
	{
		for ( int i = 0; i < pages.Length; i++ )
		{
			MegaPage page = pages[i];

			// if page alpha not changed then we can disable the modobj
			if ( page.alpha != page.lastalpha )
			{
				page.modobj.Enabled = true;
				page.lastalpha = page.alpha;
			}
			else
			{
				page.modobj.Enabled = false;
			}
		}
	}

	// Need to figure out each page turn alpha
	// book position is page number and fraction is page turn
	// somehow need to turn a lot of pages if jumping ahead, so needs a speed value, delta in position governs how many pages, set time to turn to page
	// if speed due to time is > 1 then will be turning more than one page, ie speed whole value is number of pages

	// can we do spine support?
	// we need to disable modifiers if page is not turning
	// page texture list

	public float	rotate = 0.0f;
	public bool optimize = false;
	public bool tangents = false;
}
#endif