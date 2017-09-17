
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaBezPatch))]
public class MegaBezPatchEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaShape/Bez Patch")]
	static void CreateBezPatch()
	{
		Vector3 pos = Vector3.zero;
		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Bez Patch");

		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.sharedMesh = new Mesh();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();

		Material[] mats = new Material[1];

		mr.sharedMaterials = mats;
		MegaBezPatch pm = go.AddComponent<MegaBezPatch>();
		pm.mesh = mf.sharedMesh;
		go.transform.position = pos;
		Selection.activeObject = go;
		pm.Rebuild();

		// Add the first warp
	}

	public override void OnInspectorGUI()
	{
		MegaBezPatch mod = (MegaBezPatch)target;

		//bool rebuild = DrawDefaultInspector();
		EditorGUIUtility.LookLikeControls();

		float Width = EditorGUILayout.FloatField("Width", mod.Width);
		if ( Width != mod.Width )
		{
			mod.AdjustLattice(Width, mod.Height);
		}

		float Height = EditorGUILayout.FloatField("Height", mod.Height);
		if ( Height != mod.Height )
		{
			mod.AdjustLattice(mod.Width, Height);
		}
		mod.WidthSegs = EditorGUILayout.IntField("Width Segs", mod.WidthSegs);
		mod.HeightSegs = EditorGUILayout.IntField("Height Segs", mod.HeightSegs);

		mod.recalcBounds = EditorGUILayout.Toggle("Recalc Bounds", mod.recalcBounds);
		//mod.recalcNormals = EditorGUILayout.Toggle("Recalc Normals", mod.recalcNormals);
		mod.recalcTangents = EditorGUILayout.Toggle("Recalc Tangents", mod.recalcTangents);

		mod.GenUVs = EditorGUILayout.BeginToggleGroup("Gen UVs", mod.GenUVs);
		mod.UVOffset = EditorGUILayout.Vector2Field("UV Offset", mod.UVOffset);
		mod.UVScale = EditorGUILayout.Vector2Field("UV Scale", mod.UVScale);
		EditorGUILayout.EndToggleGroup();

		mod.showgizmos = EditorGUILayout.Toggle("Show Gizmos", mod.showgizmos);
		mod.showlabels = EditorGUILayout.Toggle("Show Labels", mod.showlabels);
		mod.latticecol = EditorGUILayout.ColorField("Lattice Color", mod.latticecol);
		mod.positionhandles = EditorGUILayout.Toggle("Position Handles", mod.positionhandles);
		mod.handlesize = EditorGUILayout.FloatField("Handle Size", mod.handlesize);
		mod.snap = EditorGUILayout.Vector2Field("Snap", mod.snap);
		mod.showlatticepoints = EditorGUILayout.Foldout(mod.showlatticepoints, "Lattice Points");

		mod.switchtime = EditorGUILayout.FloatField("Switch Time", mod.switchtime);

		if ( mod.showlatticepoints )
		{
			mod.p11 = EditorGUILayout.Vector2Field("p11", mod.p11);
			mod.p12 = EditorGUILayout.Vector2Field("p12", mod.p12);
			mod.p13 = EditorGUILayout.Vector2Field("p13", mod.p13);
			mod.p14 = EditorGUILayout.Vector2Field("p14", mod.p14);

			mod.p21 = EditorGUILayout.Vector2Field("p21", mod.p21);
			mod.p22 = EditorGUILayout.Vector2Field("p22", mod.p22);
			mod.p23 = EditorGUILayout.Vector2Field("p23", mod.p23);
			mod.p24 = EditorGUILayout.Vector2Field("p24", mod.p24);

			mod.p31 = EditorGUILayout.Vector2Field("p31", mod.p31);
			mod.p32 = EditorGUILayout.Vector2Field("p32", mod.p32);
			mod.p33 = EditorGUILayout.Vector2Field("p33", mod.p33);
			mod.p34 = EditorGUILayout.Vector2Field("p34", mod.p34);

			mod.p41 = EditorGUILayout.Vector2Field("p41", mod.p41);
			mod.p42 = EditorGUILayout.Vector2Field("p42", mod.p42);
			mod.p43 = EditorGUILayout.Vector2Field("p43", mod.p43);
			mod.p44 = EditorGUILayout.Vector2Field("p44", mod.p44);
		}

		int currentwarp = EditorGUILayout.IntSlider("Warp", mod.currentwarp, 0, mod.warps.Count - 1);
		if ( currentwarp != mod.currentwarp )
		{
			mod.SetWarp(currentwarp);
		}

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Add Warp") )
		{
			mod.AddWarp();
		}

		if ( GUILayout.Button("Reset") )
		{
			mod.Reset();
		}

		EditorGUILayout.EndHorizontal();

		for ( int i = 0; i < mod.warps.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal();
			mod.warps[i].name = EditorGUILayout.TextField("", mod.warps[i].name);
			//mod.meshes[i].Enabled = EditorGUILayout.Toggle("", mod.meshes[i].Enabled, GUILayout.MaxWidth(20));

			if ( GUILayout.Button("Set", GUILayout.MaxWidth(50)) )
			{
				mod.SetWarp(i);
				EditorUtility.SetDirty(mod);
			}

			if ( GUILayout.Button("Update", GUILayout.MaxWidth(50)) )
			{
				mod.UpdateWarp(i);
			}

			if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
				mod.warps.RemoveAt(i);

			EditorGUILayout.EndHorizontal();
		}

		if ( GUI.changed )	//rebuild )
		{
			mod.Rebuild();
			EditorUtility.SetDirty(target);
		}
	}

	public void OnSceneGUI()
	{
		MegaBezPatch mod = (MegaBezPatch)target;

		if ( mod.showgizmos )
		{
			Handles.matrix = mod.transform.localToWorldMatrix;

			Handles.color = mod.latticecol;

			Handles.DrawLine(mod.p11, mod.p12);
			Handles.DrawLine(mod.p12, mod.p13);
			Handles.DrawLine(mod.p13, mod.p14);

			Handles.DrawLine(mod.p21, mod.p22);
			Handles.DrawLine(mod.p22, mod.p23);
			Handles.DrawLine(mod.p23, mod.p24);

			Handles.DrawLine(mod.p31, mod.p32);
			Handles.DrawLine(mod.p32, mod.p33);
			Handles.DrawLine(mod.p33, mod.p34);

			Handles.DrawLine(mod.p41, mod.p42);
			Handles.DrawLine(mod.p42, mod.p43);
			Handles.DrawLine(mod.p43, mod.p44);

			Handles.DrawLine(mod.p11, mod.p21);
			Handles.DrawLine(mod.p21, mod.p31);
			Handles.DrawLine(mod.p31, mod.p41);

			Handles.DrawLine(mod.p12, mod.p22);
			Handles.DrawLine(mod.p22, mod.p32);
			Handles.DrawLine(mod.p32, mod.p42);

			Handles.DrawLine(mod.p13, mod.p23);
			Handles.DrawLine(mod.p23, mod.p33);
			Handles.DrawLine(mod.p33, mod.p43);

			Handles.DrawLine(mod.p14, mod.p24);
			Handles.DrawLine(mod.p24, mod.p34);
			Handles.DrawLine(mod.p34, mod.p44);


			Quaternion rot = Quaternion.identity;
			if ( mod.showlabels )
			{
				Handles.Label(mod.p11, "11 " + mod.p11.ToString("0.00"));
				Handles.Label(mod.p12, "12 " + mod.p12.ToString("0.00"));
				Handles.Label(mod.p13, "13 " + mod.p13.ToString("0.00"));
				Handles.Label(mod.p14, "14 " + mod.p14.ToString("0.00"));

				Handles.Label(mod.p21, "21 " + mod.p21.ToString("0.00"));
				Handles.Label(mod.p22, "22 " + mod.p22.ToString("0.00"));
				Handles.Label(mod.p23, "23 " + mod.p23.ToString("0.00"));
				Handles.Label(mod.p24, "24 " + mod.p24.ToString("0.00"));

				Handles.Label(mod.p31, "31 " + mod.p31.ToString("0.00"));
				Handles.Label(mod.p32, "32 " + mod.p32.ToString("0.00"));
				Handles.Label(mod.p33, "33 " + mod.p33.ToString("0.00"));
				Handles.Label(mod.p34, "34 " + mod.p34.ToString("0.00"));

				Handles.Label(mod.p41, "41 " + mod.p41.ToString("0.00"));
				Handles.Label(mod.p42, "42 " + mod.p42.ToString("0.00"));
				Handles.Label(mod.p43, "43 " + mod.p43.ToString("0.00"));
				Handles.Label(mod.p44, "44 " + mod.p44.ToString("0.00"));
			}

			Vector3 p11 = mod.p11;
			Vector3 p12 = mod.p12;
			Vector3 p13 = mod.p13;
			Vector3 p14 = mod.p14;

			Vector3 p21 = mod.p21;
			Vector3 p22 = mod.p22;
			Vector3 p23 = mod.p23;
			Vector3 p24 = mod.p24;

			Vector3 p31 = mod.p31;
			Vector3 p32 = mod.p32;
			Vector3 p33 = mod.p33;
			Vector3 p34 = mod.p34;

			Vector3 p41 = mod.p41;
			Vector3 p42 = mod.p42;
			Vector3 p43 = mod.p43;
			Vector3 p44 = mod.p44;

			if ( mod.positionhandles )
			{
				mod.p11 = Handles.PositionHandle(p11, rot);
				mod.p12 = Handles.PositionHandle(p12, rot);
				mod.p13 = Handles.PositionHandle(p13, rot);
				mod.p14 = Handles.PositionHandle(p14, rot);

				mod.p21 = Handles.PositionHandle(p21, rot);
				mod.p22 = Handles.PositionHandle(p22, rot);
				mod.p23 = Handles.PositionHandle(p23, rot);
				mod.p24 = Handles.PositionHandle(p24, rot);

				mod.p31 = Handles.PositionHandle(p31, rot);
				mod.p32 = Handles.PositionHandle(p32, rot);
				mod.p33 = Handles.PositionHandle(p33, rot);
				mod.p34 = Handles.PositionHandle(p34, rot);

				mod.p41 = Handles.PositionHandle(p41, rot);
				mod.p42 = Handles.PositionHandle(p42, rot);
				mod.p43 = Handles.PositionHandle(p43, rot);
				mod.p44 = Handles.PositionHandle(p44, rot);
			}
			else
			{
				Handles.color = Color.green;

				mod.p11 = Handles.FreeMoveHandle(p11, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p12 = Handles.FreeMoveHandle(p12, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p13 = Handles.FreeMoveHandle(p13, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p14 = Handles.FreeMoveHandle(p14, rot, mod.handlesize, mod.snap, Handles.SphereCap);

				mod.p21 = Handles.FreeMoveHandle(p21, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p22 = Handles.FreeMoveHandle(p22, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p23 = Handles.FreeMoveHandle(p23, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p24 = Handles.FreeMoveHandle(p24, rot, mod.handlesize, mod.snap, Handles.SphereCap);

				mod.p31 = Handles.FreeMoveHandle(p31, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p32 = Handles.FreeMoveHandle(p32, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p33 = Handles.FreeMoveHandle(p33, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p34 = Handles.FreeMoveHandle(p34, rot, mod.handlesize, mod.snap, Handles.SphereCap);

				mod.p41 = Handles.FreeMoveHandle(p41, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p42 = Handles.FreeMoveHandle(p42, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p43 = Handles.FreeMoveHandle(p43, rot, mod.handlesize, mod.snap, Handles.SphereCap);
				mod.p44 = Handles.FreeMoveHandle(p44, rot, mod.handlesize, mod.snap, Handles.SphereCap);
			}

			bool dirty = false;
			if ( p11 != mod.p11 ) dirty = true;
			if ( p12 != mod.p12 ) dirty = true;
			if ( p13 != mod.p13 ) dirty = true;
			if ( p14 != mod.p14 ) dirty = true;

			if ( p21 != mod.p21 ) dirty = true;
			if ( p22 != mod.p22 ) dirty = true;
			if ( p23 != mod.p23 ) dirty = true;
			if ( p24 != mod.p24 ) dirty = true;

			if ( p31 != mod.p31 ) dirty = true;
			if ( p32 != mod.p32 ) dirty = true;
			if ( p33 != mod.p33 ) dirty = true;
			if ( p34 != mod.p34 ) dirty = true;

			if ( p41 != mod.p41 ) dirty = true;
			if ( p42 != mod.p42 ) dirty = true;
			if ( p43 != mod.p43 ) dirty = true;
			if ( p44 != mod.p44 ) dirty = true;

			if ( dirty )
				EditorUtility.SetDirty(target);
#if false
			Handles.Label(mod.p11, "11");
			mod.p11 = Handles.PositionHandle(mod.p11, rot);
			Handles.Label(mod.p12, "12");
			mod.p12 = Handles.PositionHandle(mod.p12, rot);
			Handles.Label(mod.p13, "13");
			mod.p13 = Handles.PositionHandle(mod.p13, rot);
			Handles.Label(mod.p14, "14");
			mod.p14 = Handles.PositionHandle(mod.p14, rot);

			Handles.Label(mod.p21, "21");
			mod.p21 = Handles.PositionHandle(mod.p21, rot);
			Handles.Label(mod.p22, "22");
			mod.p22 = Handles.PositionHandle(mod.p22, rot);
			Handles.Label(mod.p23, "23");
			mod.p23 = Handles.PositionHandle(mod.p23, rot);
			Handles.Label(mod.p24, "24");
			mod.p24 = Handles.PositionHandle(mod.p24, rot);

			Handles.Label(mod.p31, "31");
			mod.p31 = Handles.PositionHandle(mod.p31, rot);
			Handles.Label(mod.p32, "32");
			mod.p32 = Handles.PositionHandle(mod.p32, rot);
			Handles.Label(mod.p33, "33");
			mod.p33 = Handles.PositionHandle(mod.p33, rot);
			Handles.Label(mod.p34, "34");
			mod.p34 = Handles.PositionHandle(mod.p34, rot);

			Handles.Label(mod.p41, "41");
			mod.p41 = Handles.PositionHandle(mod.p41, rot);
			Handles.Label(mod.p42, "42");
			mod.p42 = Handles.PositionHandle(mod.p42, rot);
			Handles.Label(mod.p43, "43");
			mod.p43 = Handles.PositionHandle(mod.p43, rot);
			Handles.Label(mod.p44, "44");
			mod.p44 = Handles.PositionHandle(mod.p44, rot);
#endif
		}

		mod.p11.z = 0.0f;
		mod.p12.z = 0.0f;
		mod.p13.z = 0.0f;
		mod.p14.z = 0.0f;

		mod.p21.z = 0.0f;
		mod.p22.z = 0.0f;
		mod.p23.z = 0.0f;
		mod.p24.z = 0.0f;

		mod.p31.z = 0.0f;
		mod.p32.z = 0.0f;
		mod.p33.z = 0.0f;
		mod.p34.z = 0.0f;

		mod.p41.z = 0.0f;
		mod.p42.z = 0.0f;
		mod.p43.z = 0.0f;
		mod.p44.z = 0.0f;
	}
}