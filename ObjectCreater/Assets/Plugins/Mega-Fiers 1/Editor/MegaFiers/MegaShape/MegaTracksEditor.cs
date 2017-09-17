
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaTracks))]
public class MegaTracksEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaShape/Tracks")]
	static void CreateTracks()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Tracks Control");
		go.AddComponent<MegaTracks>();
		go.transform.position = pos;
		Selection.activeObject = go;
	}

	public override void OnInspectorGUI()
	{
		MegaTracks mod = (MegaTracks)target;

		EditorGUIUtility.LookLikeControls();
		//DrawDefaultInspector();

		mod.shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);

		if ( mod.shape != null )
		{
			if ( mod.shape.splines.Count > 1 )
				mod.curve = EditorGUILayout.IntSlider("Curve", mod.curve, 0, mod.shape.splines.Count - 1);
		}

		mod.LinkObj = (GameObject)EditorGUILayout.ObjectField("Link Object", mod.LinkObj, typeof(GameObject), true);

		mod.start = EditorGUILayout.FloatField("Start", mod.start);
		mod.linkOff = EditorGUILayout.Vector3Field("Link Off Start", mod.linkOff);
		mod.linkOff1 = EditorGUILayout.Vector3Field("Link Off End", mod.linkOff1);
		mod.linkPivot = EditorGUILayout.Vector3Field("Link Pivot", mod.linkPivot);
		mod.rotate = EditorGUILayout.Vector3Field("Rotate", mod.rotate);
		mod.linkScale = EditorGUILayout.Vector3Field("Scale", mod.linkScale);
		mod.LinkSize = EditorGUILayout.FloatField("Link Size", mod.LinkSize);

		mod.trackup = EditorGUILayout.Vector3Field("Track Up", mod.trackup);
	
		mod.animate = EditorGUILayout.BeginToggleGroup("Animate", mod.animate);

		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		EditorGUILayout.EndToggleGroup();

		mod.dolateupdate = EditorGUILayout.Toggle("Do LateUpdate", mod.dolateupdate);
		mod.InvisibleUpdate = EditorGUILayout.Toggle("Invisible Update", mod.InvisibleUpdate);
		mod.displayspline = EditorGUILayout.Toggle("Display Spline", mod.displayspline);

		if ( GUI.changed )
		{
			mod.rebuild = true;
			EditorUtility.SetDirty(target);
			mod.Rebuild();
		}
	}

	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
	static void RenderGizmo(MegaTracks track, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{
			if ( !track.displayspline )
				return;

			DrawGizmos(track, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			Color col = Color.yellow;
			col.a = 0.5f;	//0.75f;
			Gizmos.color = col;	//Color.yellow;

			Matrix4x4 RingTM = Matrix4x4.identity;
			RingTM = track.transform.localToWorldMatrix * RingTM;

			float gsize = 0.1f;

			gsize *= 0.1f;

			if ( track.shape != null )
			{
				MegaSpline spl = track.shape.splines[track.curve];

				for ( int p = 0; p < spl.knots.Count; p++ )
				{
					Vector3 p1 = RingTM.MultiplyPoint(spl.knots[p].p);

					Gizmos.color = Color.green;
					Gizmos.DrawSphere(p1, gsize);
				}
			}
		}
	}

	static void DrawGizmos(MegaTracks track, Color modcol1)
	{
		Matrix4x4 RingTM = Matrix4x4.identity;
		Matrix4x4 tm = track.transform.localToWorldMatrix;

		if ( track.shape == null )
			return;

		MegaSpline spl = track.shape.splines[track.curve];
		float ldist = 1.0f * 0.1f;
		if ( ldist < 0.01f )
			ldist = 0.01f;

		Color modcol = modcol1;

		if ( spl.length / ldist > 500.0f )
			ldist = spl.length / 500.0f;

		float ds = spl.length / (spl.length / ldist);

		if ( ds > spl.length )
			ds = spl.length;

		int c	= 0;
		int k	= -1;
		int lk	= -1;

		Vector3 first = spl.Interpolate(0.0f, true, ref lk);

		RingTM = tm * RingTM;

		for ( float dist = ds; dist < spl.length; dist += ds )
		{
			float alpha = dist / spl.length;

			Vector3 pos = spl.Interpolate(alpha, true, ref k);

			if ( (c & 1) == 1 )
				Gizmos.color = Color.black * modcol;
			else
				Gizmos.color = Color.yellow * modcol;

			if ( k != lk )
			{
				for ( lk = lk + 1; lk <= k; lk++ )
				{
					Gizmos.DrawLine(RingTM.MultiplyPoint(first), RingTM.MultiplyPoint(spl.knots[lk].p));
					first = spl.knots[lk].p;
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
		if ( spl.closed )
			lastpos = spl.Interpolate(0.0f, true, ref k);
		else
			lastpos = spl.Interpolate(1.0f, true, ref k);

		Gizmos.DrawLine(RingTM.MultiplyPoint(first), RingTM.MultiplyPoint(lastpos));
	}
}