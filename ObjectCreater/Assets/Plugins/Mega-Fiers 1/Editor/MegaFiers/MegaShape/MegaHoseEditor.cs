
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaHose))]
public class MegaHoseEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaShape/Hose")]
	static void CreatePageMesh()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Hose");

		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.sharedMesh = new Mesh();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();

		Material[] mats = new Material[3];

		mr.sharedMaterials = mats;
		MegaHose pm = go.AddComponent<MegaHose>();

		go.transform.position = pos;
		Selection.activeObject = go;
		pm.rebuildcross = true;
		pm.updatemesh = true;
		//pm.Rebuild();
	}

	public override void OnInspectorGUI()
	{
		MegaHose mod = (MegaHose)target;

		EditorGUIUtility.LookLikeControls();

		mod.dolateupdate = EditorGUILayout.Toggle("Do Late Update", mod.dolateupdate);
		mod.InvisibleUpdate = EditorGUILayout.Toggle("Invisible Update", mod.InvisibleUpdate);

		mod.wiretype = (MegaHoseType)EditorGUILayout.EnumPopup("Wire Type", mod.wiretype);
		mod.segments = EditorGUILayout.IntField("Segments", mod.segments);
		mod.capends = EditorGUILayout.Toggle("Cap Ends", mod.capends);
		mod.calcnormals = EditorGUILayout.Toggle("Calc Normals", mod.calcnormals);
		mod.calctangents = EditorGUILayout.Toggle("Calc Tangents", mod.calctangents);
		mod.optimize = EditorGUILayout.Toggle("Optimize", mod.optimize);
		mod.recalcCollider = EditorGUILayout.Toggle("Calc Collider", mod.recalcCollider);

		switch ( mod.wiretype )
		{
			case MegaHoseType.Round:
				mod.rnddia  = EditorGUILayout.FloatField("Diameter", mod.rnddia);
				mod.rndsides = EditorGUILayout.IntField("Sides", mod.rndsides);
				mod.rndrot = EditorGUILayout.FloatField("Rotate", mod.rndrot);
				break;

			case MegaHoseType.Rectangle:
				mod.rectwidth = EditorGUILayout.FloatField("Width", mod.rectwidth);
				mod.rectdepth = EditorGUILayout.FloatField("Depth", mod.rectdepth);
				mod.rectfillet = EditorGUILayout.FloatField("Fillet", mod.rectfillet);
				mod.rectfilletsides = EditorGUILayout.IntField("Fillet Sides", mod.rectfilletsides);
				mod.rectrotangle = EditorGUILayout.FloatField("Rotate", mod.rectrotangle);
				break;

			case MegaHoseType.DSection:
				mod.dsecwidth = EditorGUILayout.FloatField("Width", mod.dsecwidth);
				mod.dsecdepth = EditorGUILayout.FloatField("Depth", mod.dsecdepth);
				mod.dsecrndsides = EditorGUILayout.IntField("Rnd Sides", mod.dsecrndsides);
				mod.dsecfillet = EditorGUILayout.FloatField("Fillet", mod.dsecfillet);
				mod.dsecfilletsides = EditorGUILayout.IntField("Fillet Sides", mod.dsecfilletsides);
				mod.dsecrotangle = EditorGUILayout.FloatField("Rotate", mod.dsecrotangle);
				break;
		}

		mod.uvscale = EditorGUILayout.Vector2Field("UV Scale", mod.uvscale);

		if ( GUI.changed )
		{
			mod.updatemesh = true;
			mod.rebuildcross = true;
		}

		mod.custnode = (GameObject)EditorGUILayout.ObjectField("Start Object", mod.custnode, typeof(GameObject), true);
		mod.offset = EditorGUILayout.Vector3Field("Offset", mod.offset);
		mod.rotate = EditorGUILayout.Vector3Field("Rotate", mod.rotate);
		mod.scale = EditorGUILayout.Vector3Field("Scale", mod.scale);
		mod.custnode2 = (GameObject)EditorGUILayout.ObjectField("End Object", mod.custnode2, typeof(GameObject), true);
		mod.offset1 = EditorGUILayout.Vector3Field("Offset", mod.offset1);
		mod.rotate1 = EditorGUILayout.Vector3Field("Rotate", mod.rotate1);
		mod.scale1 = EditorGUILayout.Vector3Field("Scale", mod.scale1);

		mod.flexon = EditorGUILayout.BeginToggleGroup("Flex On", mod.flexon);
		mod.flexstart = EditorGUILayout.Slider("Start", mod.flexstart, 0.0f, 1.0f);
		mod.flexstop = EditorGUILayout.Slider("Stop", mod.flexstop, 0.0f, 1.0f);

		if ( mod.flexstart > mod.flexstop )
			mod.flexstart = mod.flexstop;

		if ( mod.flexstop < mod.flexstart )
			mod.flexstop = mod.flexstart;

		mod.flexcycles = EditorGUILayout.IntField("Cycles", mod.flexcycles);
		mod.flexdiameter = EditorGUILayout.FloatField("Diameter", mod.flexdiameter);

		EditorGUILayout.EndToggleGroup();

		mod.usebulgecurve = EditorGUILayout.BeginToggleGroup("Use Bulge Curve", mod.usebulgecurve);
		mod.bulge = EditorGUILayout.CurveField("Bulge", mod.bulge);
		mod.bulgeamount = EditorGUILayout.FloatField("Bulge Amount", mod.bulgeamount);
		mod.bulgeoffset = EditorGUILayout.FloatField("Bulge Offset", mod.bulgeoffset);

		mod.animatebulge = EditorGUILayout.BeginToggleGroup("Animate", mod.animatebulge);
		mod.bulgespeed = EditorGUILayout.FloatField("Speed", mod.bulgespeed);
		mod.minbulge = EditorGUILayout.FloatField("Min", mod.minbulge);
		mod.maxbulge = EditorGUILayout.FloatField("Max", mod.maxbulge);
		EditorGUILayout.EndToggleGroup();

		EditorGUILayout.EndToggleGroup();

		mod.tension1 = EditorGUILayout.FloatField("Tension Start", mod.tension1);
		mod.tension2 = EditorGUILayout.FloatField("Tension End", mod.tension2);

		mod.freecreate = EditorGUILayout.BeginToggleGroup("Free Create", mod.freecreate);
		mod.noreflength = EditorGUILayout.FloatField("Free Length", mod.noreflength);
		EditorGUILayout.EndToggleGroup();

		mod.up = EditorGUILayout.Vector3Field("Up", mod.up);
		mod.displayspline = EditorGUILayout.Toggle("Display Spline", mod.displayspline);

		if ( GUI.changed )	//rebuild )
		{
			mod.updatemesh = true;
			mod.Rebuild();
		}
	}
#if false
	public void OnSceneGUI()
	{
		MegaHose hose = (MegaHose)target;

		if ( hose.calcnormals )
		{
			Handles.matrix = hose.transform.localToWorldMatrix;
			Handles.color = Color.red;
			for ( int i = 0; i < hose.verts.Length; i++ )
			{
				//Gizmos.DrawRay(hose.verts[i], hose.normals[i] * 2.0f);
				Handles.DrawLine(hose.verts[i], hose.verts[i] + (hose.normals[i] * 0.5f));
			}
		}
	}
#endif


	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
	static void RenderGizmo(MegaHose hose, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{
			if ( !hose.displayspline )
				return;

			if ( hose.custnode == null || hose.custnode2 == null )
				return;

			DrawGizmos(hose, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			Color col = Color.yellow;
			col.a = 0.5f;	//0.75f;
			Gizmos.color = col;	//Color.yellow;

			Matrix4x4 RingTM = Matrix4x4.identity;
			hose.CalcMatrix(ref RingTM, 0.0f);
			RingTM = hose.transform.localToWorldMatrix * RingTM;

			float gsize = 0.0f;
			switch ( hose.wiretype )
			{
				case MegaHoseType.Round:		gsize = hose.rnddia;	break;
				case MegaHoseType.Rectangle:	gsize = (hose.rectdepth + hose.rectwidth) * 0.5f; break;
				case MegaHoseType.DSection:		gsize = (hose.dsecdepth + hose.dsecwidth) * 0.5f; break;
			}

			gsize *= 0.1f;

			for ( int p = 0; p < hose.hosespline.knots.Count; p++ )
			{
				Vector3 p1 = RingTM.MultiplyPoint(hose.hosespline.knots[p].p);
				Vector3 p2 = RingTM.MultiplyPoint(hose.hosespline.knots[p].invec);
				Vector3 p3 = RingTM.MultiplyPoint(hose.hosespline.knots[p].outvec);

				Gizmos.color = Color.black;
				Gizmos.DrawLine(p2, p1);
				Gizmos.DrawLine(p3, p1);

				Gizmos.color = Color.green;
				Gizmos.DrawSphere(p1, gsize);

				Gizmos.color = Color.red;
				Gizmos.DrawSphere(p2, gsize);
				Gizmos.DrawSphere(p3, gsize);
			}
		}
	}

	static void DrawGizmos(MegaHose hose, Color modcol1)
	{
		Matrix4x4 RingTM = Matrix4x4.identity;
		Matrix4x4 tm = hose.transform.localToWorldMatrix;

		float ldist = 1.0f * 0.1f;
		if ( ldist < 0.01f )
			ldist = 0.01f;

		Color modcol = modcol1;

		if ( hose.hosespline.length / ldist > 500.0f )
			ldist = hose.hosespline.length / 500.0f;

		float ds = hose.hosespline.length / (hose.hosespline.length / ldist);

		if ( ds > hose.hosespline.length )
			ds = hose.hosespline.length;

		int c	= 0;
		int k	= -1;
		int lk	= -1;

		Vector3 first = hose.hosespline.Interpolate(0.0f, true, ref lk);

		hose.CalcMatrix(ref RingTM, 0.0f);
		RingTM = tm * RingTM;

		for ( float dist = ds; dist < hose.hosespline.length; dist += ds )
		{
			float alpha = dist / hose.hosespline.length;

			Vector3 pos = hose.hosespline.Interpolate(alpha, true, ref k);

			if ( (c & 1) == 1 )
				Gizmos.color = Color.black * modcol;
			else
				Gizmos.color = Color.yellow * modcol;

			if ( k != lk )
			{
				for ( lk = lk + 1; lk <= k; lk++ )
				{
					Gizmos.DrawLine(RingTM.MultiplyPoint(first), RingTM.MultiplyPoint(hose.hosespline.knots[lk].p));
					first = hose.hosespline.knots[lk].p;
				}
			}

			lk = k;

			Gizmos.DrawLine(RingTM.MultiplyPoint(first), RingTM.MultiplyPoint(pos));

			c++;

			first = pos;
		}

		if ( (c & 1) == 1 )
			Gizmos.color = Color.blue * modcol;
		else
			Gizmos.color = Color.yellow * modcol;

		Vector3 lastpos;
		if ( hose.hosespline.closed )
			lastpos = hose.hosespline.Interpolate(0.0f, true, ref k);
		else
			lastpos = hose.hosespline.Interpolate(1.0f, true, ref k);

		Gizmos.DrawLine(RingTM.MultiplyPoint(first), RingTM.MultiplyPoint(lastpos));
	}
}