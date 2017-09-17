
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("MegaShapes/Track")]
public class MegaTracks : MonoBehaviour
{
	public MegaShape	shape;
	public int			curve = 0;
	public float		start = 0.0f;
	public Vector3		rotate = Vector3.zero;
	public bool			displayspline = true;
	public Vector3		linkOff = Vector3.zero;
	public Vector3		linkScale = Vector3.one;
	public Vector3		linkOff1 = new Vector3(0.0f, 1.0f, 0.0f);
	public Vector3		linkPivot = Vector3.zero;
	public GameObject	LinkObj;
	public bool			RandomOrder = false;
	public float		LinkSize = 1.0f;
	public bool			dolateupdate = false;
	public bool			animate = false;
	public float		speed = 0.0f;
	public Vector3		trackup = Vector3.up;
	public bool			InvisibleUpdate = false;

	public bool			rebuild = true;
	bool				visible = true;
	float				lastpos = -1.0f;
	Matrix4x4			tm;
	Matrix4x4			wtm;
	int					linkcount = 0;
	int					remain;
	Transform[]			linkobjs;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3538");
	}

	void Awake()
	{
		lastpos = -1.0f;
		rebuild = true;
		Rebuild();
	}

	void Reset()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		BuildTrack();
	}

	void Update()
	{
		if ( animate )
			start += speed * Time.deltaTime;

		if ( visible || InvisibleUpdate )
		{
			if ( !dolateupdate )
				BuildTrack();
		}
	}

	void LateUpdate()
	{
		if ( visible || InvisibleUpdate )
		{
			if ( dolateupdate )
				BuildTrack();
		}
	}

	void BuildTrack()
	{
		if ( shape != null && LinkObj != null )
		{
			if ( rebuild || lastpos != start )
			{
				rebuild = false;
				lastpos = start;
				BuildObjectLinks(shape);
			}
		}
	}

	void OnBecameVisible()
	{
		visible = true;
	}

	void OnBecameInvisible()
	{
		visible = false;
	}

	// Taken from chain mesher
	void InitLinkObjects(MegaShape path)
	{
		if ( LinkObj == null )
			return;

		float len = path.splines[curve].length;

		// Assume z axis for now
		float linklen = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;
		linkcount = (int)(len / linklen);

		for ( int i = linkcount; i < gameObject.transform.childCount; i++ )
		{
			GameObject go = gameObject.transform.GetChild(i).gameObject;
			if ( Application.isEditor )
				DestroyImmediate(go);
			else
				Destroy(go);
		}

		linkobjs = new Transform[linkcount];

		if ( linkcount > gameObject.transform.childCount )
		{
			for ( int i = 0; i < gameObject.transform.childCount; i++ )
			{
				GameObject go = gameObject.transform.GetChild(i).gameObject;
#if UNITY_3_5 
				go.SetActiveRecursively(true);
#else
				go.SetActive(true);
#endif
				linkobjs[i] = go.transform;
			}

			int index = gameObject.transform.childCount;

			for ( int i = index; i < linkcount; i++ )
			{
				GameObject go = new GameObject();
				go.name = "Link";

				GameObject obj = LinkObj;

				if ( obj )
				{
					MeshRenderer mr = (MeshRenderer)obj.GetComponent<MeshRenderer>();
					Mesh ms = MegaUtils.GetSharedMesh(obj);

					MeshRenderer mr1 = (MeshRenderer)go.AddComponent<MeshRenderer>();
					MeshFilter mf1 = (MeshFilter)go.AddComponent<MeshFilter>();

					mf1.sharedMesh = ms;

					mr1.sharedMaterial = mr.sharedMaterial;

					go.transform.parent = gameObject.transform;
					linkobjs[i] = go.transform;
				}
			}
		}
		else
		{
			for ( int i = 0; i < linkcount; i++ )
			{
				GameObject go = gameObject.transform.GetChild(i).gameObject;
#if UNITY_3_5
				go.SetActiveRecursively(true);
#else
				go.SetActive(true);
#endif
				linkobjs[i] = go.transform;
			}
		}

		Random.seed = 0;
		for ( int i = 0; i < linkcount; i++ )
		{
			GameObject obj = LinkObj;	//1[oi];
			GameObject go = gameObject.transform.GetChild(i).gameObject;

			MeshRenderer mr = (MeshRenderer)obj.GetComponent<MeshRenderer>();
			Mesh ms = MegaUtils.GetSharedMesh(obj);

			MeshRenderer mr1 = (MeshRenderer)go.GetComponent<MeshRenderer>();
			MeshFilter mf1 = (MeshFilter)go.GetComponent<MeshFilter>();

			mf1.sharedMesh = ms;
			mr1.sharedMaterials = mr.sharedMaterials;
		}
	}

	void BuildObjectLinks(MegaShape path)
	{
		float len = path.splines[curve].length;

		if ( LinkSize < 0.1f )
			LinkSize = 0.1f;

		// Assume z axis for now
		float linklen = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;

		int lc = (int)(len / linklen);

		if ( lc != linkcount )
			InitLinkObjects(path);

		Quaternion linkrot1 = Quaternion.identity;

		linkrot1 = Quaternion.Euler(rotate);

		float spos = start * 0.01f;
		Vector3 poff = linkPivot * linkScale.x * LinkSize;
		float lastalpha = spos;
		Vector3 pos = Vector3.zero;

		Matrix4x4 pmat = Matrix4x4.TRS(poff, linkrot1, Vector3.one);

		Quaternion frot = Quaternion.identity;
		for ( int i = 0; i < linkcount; i++ )
		{
			float alpha = ((float)(i + 1) / (float)linkcount) + spos;
			Quaternion lq = GetLinkQuat(alpha, lastalpha, out pos, path);
			lastalpha = alpha;

			frot = lq * linkrot1;

			if ( linkobjs[i] )
			{
				Matrix4x4 lmat = Matrix4x4.TRS(pos, lq, Vector3.one) * pmat;

				linkobjs[i].localPosition = lmat.GetColumn(3);
				linkobjs[i].localRotation = frot;
				linkobjs[i].localScale = linkScale * LinkSize;
			}
		}
	}

	Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps, MegaShape path)
	{
		int k = 0;
		ps = path.splines[curve].InterpCurve3D(last, shape.normalizedInterp, ref k);
		Vector3 ps1	= path.splines[curve].InterpCurve3D(alpha, shape.normalizedInterp, ref k);

		Vector3 relativePos = ps1 - ps;

		Quaternion rotation = Quaternion.LookRotation(relativePos, trackup);

		return rotation;
	}
}
