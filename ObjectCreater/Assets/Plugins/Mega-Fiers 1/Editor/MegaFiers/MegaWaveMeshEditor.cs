
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaWaveMesh))]
public class MegaWaveMeshEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaShape/Wave Mesh")]
	static void CreateWaveMesh()
	{
		Vector3 pos = Vector3.zero;
		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Wave Mesh");

		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.sharedMesh = new Mesh();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();

		Material[] mats = new Material[1];

		mr.sharedMaterials = mats;
		MegaWaveMesh pm = go.AddComponent<MegaWaveMesh>();
		pm.mesh = mf.sharedMesh;
		go.transform.position = pos;
		Selection.activeObject = go;
		pm.Rebuild();
	}

	static bool showwave1 = true;
	static bool showwave2 = true;
	static bool showwave3 = true;

	public override void OnInspectorGUI()
	{
		MegaWaveMesh mod = (MegaWaveMesh)target;

		//bool rebuild = DrawDefaultInspector();
		EditorGUIUtility.LookLikeControls();

#if false
		DrawDefaultInspector();
			public float Width = 1.0f;
	public float Height = 1.0f;
	public float Length = 0.0f;

	public int	 WidthSegs = 1;

	public bool	GenUVs = true;
	public bool	recalcBounds = false;
	public bool recalcNormals = false;

	public float mspeed = 1.0f;

	public float flex = 1.0f;
	public float amp = 0.0f;
	public float wave = 1.0f;
	public float phase = 0.0f;
	public float mtime = 0.0f;
	public float speed = 1.0f;
	float dist = 0.0f;
	float time = 0.0f;

	public float flex1 = 1.0f;
	public float amp1 = 0.0f;
	public float wave1 = 1.0f;
	public float phase1 = 0.0f;
	public float mtime1 = 0.0f;
	public float speed1 = 1.0f;
	float dist1 = 0.0f;
	float time1 = 0.0f;

	public float flex2 = 1.0f;
	public float amp2 = 0.0f;
	public float wave2 = 1.0f;
	public float phase2 = 0.0f;
	public float mtime2 = 0.0f;
	public float speed2 = 1.0f;
	float dist2 = 0.0f;
	float time2 = 0.0f;

	public float amount = 1.0f;
#endif


		mod.Width = EditorGUILayout.FloatField("Width", mod.Width);
		mod.Length = EditorGUILayout.FloatField("Length", mod.Length);
		mod.Height = EditorGUILayout.FloatField("Height", mod.Height);
		mod.WidthSegs = EditorGUILayout.IntField("Width Segs", mod.WidthSegs);

		mod.linkOffset = EditorGUILayout.Toggle("Link Scroll", mod.linkOffset);
		mod.offset = EditorGUILayout.FloatField("Offset", mod.offset);

		mod.recalcBounds = EditorGUILayout.Toggle("Recalc Bounds", mod.recalcBounds);
		mod.recalcNormals = EditorGUILayout.Toggle("Recalc Normals", mod.recalcNormals);
		mod.recalcCollider = EditorGUILayout.BeginToggleGroup("Recalc Collider", mod.recalcCollider);
		mod.colwidth = EditorGUILayout.FloatField("Collider Width", mod.colwidth);
		mod.smooth = EditorGUILayout.Toggle("Smooth", mod.smooth);
		EditorGUILayout.EndToggleGroup();

		mod.amount = EditorGUILayout.FloatField("Overall Amount", mod.amount);
		mod.mspeed = EditorGUILayout.FloatField("Overall Speed", mod.mspeed);

		mod.GenUVs = EditorGUILayout.BeginToggleGroup("Gen UVs", mod.GenUVs);
		mod.UVOffset = EditorGUILayout.Vector2Field("UV Offset", mod.UVOffset);
		mod.UVScale = EditorGUILayout.Vector2Field("UV Scale", mod.UVScale);
		EditorGUILayout.EndToggleGroup();


		EditorGUILayout.BeginVertical();
		showwave1 = EditorGUILayout.Foldout(showwave1, "Wave 1");

		//EditorGUILayout.LabelField("Wave 1");
		if ( showwave1 )
		{
			mod.flex = EditorGUILayout.FloatField("Flex", mod.flex);
			mod.amp = EditorGUILayout.FloatField("Amplitude", mod.amp);
			mod.wave = EditorGUILayout.FloatField("Wave Len", mod.wave);
			mod.phase = EditorGUILayout.FloatField("Phase", mod.phase);
			mod.mtime = EditorGUILayout.FloatField("Time", mod.mtime);
			mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Separator();

		EditorGUILayout.BeginVertical();
		showwave2 = EditorGUILayout.Foldout(showwave2, "Wave 2");

		if ( showwave2 )
		{
			mod.flex1 = EditorGUILayout.FloatField("Flex", mod.flex1);
			mod.amp1 = EditorGUILayout.FloatField("Amplitude", mod.amp1);
			mod.wave1 = EditorGUILayout.FloatField("Wave Len", mod.wave1);
			mod.phase1 = EditorGUILayout.FloatField("Phase", mod.phase1);
			mod.mtime1 = EditorGUILayout.FloatField("Time", mod.mtime1);
			mod.speed1 = EditorGUILayout.FloatField("Speed", mod.speed1);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Separator();

		EditorGUILayout.BeginVertical();

		showwave3 = EditorGUILayout.Foldout(showwave3, "Wave 3");

		if ( showwave3 )
		{
			mod.flex2 = EditorGUILayout.FloatField("Flex", mod.flex2);
			mod.amp2 = EditorGUILayout.FloatField("Amplitude", mod.amp2);
			mod.wave2 = EditorGUILayout.FloatField("Wave Len", mod.wave2);
			mod.phase2 = EditorGUILayout.FloatField("Phase", mod.phase2);
			mod.mtime2 = EditorGUILayout.FloatField("Time", mod.mtime2);
			mod.speed2 = EditorGUILayout.FloatField("Speed", mod.speed2);
		}
		EditorGUILayout.EndVertical();

#if false
		mod.Width = EditorGUILayout.FloatField("Width", mod.Width);
		mod.Length = EditorGUILayout.FloatField("Length", mod.Length);
		mod.Height = EditorGUILayout.FloatField("Height", mod.Height);
		mod.WidthSegs = EditorGUILayout.IntField("Width Segs", mod.WidthSegs);
		mod.LengthSegs = EditorGUILayout.IntField("Length Segs", mod.LengthSegs);
		mod.HeightSegs = EditorGUILayout.IntField("Height Segs", mod.HeightSegs);
		mod.genUVs = EditorGUILayout.Toggle("Gen UVs", mod.genUVs);
		mod.rotate = EditorGUILayout.FloatField("Rotate", mod.rotate);
		mod.PivotBase = EditorGUILayout.Toggle("Pivot Base", mod.PivotBase);
		mod.PivotEdge = EditorGUILayout.Toggle("Pivot Edge", mod.PivotEdge);
		mod.tangents = EditorGUILayout.Toggle("Tangents", mod.tangents);
		mod.optimize = EditorGUILayout.Toggle("Optimize", mod.optimize);
#endif
		if ( GUI.changed )	//rebuild )
			mod.Rebuild();
	}
}