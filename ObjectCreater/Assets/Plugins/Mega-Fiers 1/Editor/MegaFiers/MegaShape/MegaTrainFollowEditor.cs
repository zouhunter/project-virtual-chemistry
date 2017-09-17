
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MegaTrainFollow))]
public class MegaTrainFollowEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaTrainFollow mod = (MegaTrainFollow)target;

		EditorGUIUtility.LookLikeControls();

		mod.path = (MegaShape)EditorGUILayout.ObjectField("Path", mod.path, typeof(MegaShape), true);

		if ( mod.path && mod.path.splines != null )
		{
			if ( mod.path.splines.Count > 1 )
				mod.curve = EditorGUILayout.IntSlider("Curve", mod.curve, 0, mod.path.splines.Count - 1);

			if ( mod.curve < 0 )	mod.curve = 0;
			if ( mod.curve > mod.path.splines.Count - 1 )
				mod.curve = mod.path.splines.Count - 1;
		}

		mod.distance = EditorGUILayout.FloatField("Distance", mod.distance);
		mod.speed = EditorGUILayout.FloatField("Speed", mod.speed);
		mod.showrays = EditorGUILayout.Toggle("Show Rays", mod.showrays);

		if ( mod.carriages.Count < 1 )
		{
			if ( GUILayout.Button("Add") )
			{
				MegaCarriage car = new MegaCarriage();
				mod.carriages.Add(car);
			}
		}

		for ( int i = 0; i < mod.carriages.Count; i++ )
		{
			MegaCarriage car = mod.carriages[i];

			EditorGUILayout.BeginVertical("Box");

			car.length = EditorGUILayout.FloatField("Length", car.length);
			car.bogeyoff = EditorGUILayout.FloatField("Bogey Off", car.bogeyoff);

			car.carriage = (GameObject)EditorGUILayout.ObjectField("Carriage", car.carriage, typeof(GameObject), true);
			car.carriageOffset = EditorGUILayout.Vector3Field("Carriage Off", car.carriageOffset);
			car.rot = EditorGUILayout.Vector3Field("Carriage Rot", car.rot);
			car.bogey1 = (GameObject)EditorGUILayout.ObjectField("Front Bogey", car.bogey1, typeof(GameObject), true);
			car.bogey1Offset = EditorGUILayout.Vector3Field("Front Bogey Off", car.bogey1Offset);
			car.bogey1Rot = EditorGUILayout.Vector3Field("Front Bogey Rot", car.bogey1Rot);
			car.bogey2 = (GameObject)EditorGUILayout.ObjectField("Rear Bogey", car.bogey2, typeof(GameObject), true);
			car.bogey2Offset = EditorGUILayout.Vector3Field("Rear Bogey Off", car.bogey2Offset);
			car.bogey2Rot = EditorGUILayout.Vector3Field("Rear Bogey Rot", car.bogey2Rot);

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button("Add") )
			{
				MegaCarriage nc = new MegaCarriage();
				mod.carriages.Add(nc);
			}

			if ( GUILayout.Button("Delete") )
				mod.carriages.Remove(car);

			EditorGUILayout.EndHorizontal();
		}

		if ( GUI.changed )	//rebuild )
		{
			EditorUtility.SetDirty(target);
		}
	}

	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
	static void RenderGizmo(MegaTrainFollow mod, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{
			if ( !mod.showrays )
				return;

			for ( int i = 0; i < mod.carriages.Count; i++ )
			{
				MegaCarriage car = mod.carriages[i];

				Handles.color = Color.white;
				Handles.DrawLine(car.b1, car.b2);
				//Gizmos.DrawSphere(car.b1, car.length * 0.025f);
				//Gizmos.DrawSphere(car.b2, car.length * 0.025f);
				Handles.SphereCap(0, car.cp, Quaternion.identity, car.length * 0.025f);
				Handles.SphereCap(0, car.b1, Quaternion.identity, car.length * 0.025f);
				Handles.SphereCap(0, car.b2, Quaternion.identity, car.length * 0.025f);
									//if ( showrays )
				Handles.color = Color.red;
				Handles.DrawLine(car.cp, car.bp1);
				Handles.SphereCap(0, car.bp1, Quaternion.identity, car.length * 0.025f);

				//Gizmos.color = Color.green;
				Handles.color = Color.green;
				//Gizmos.DrawLine(car.cp, car.bp2);
				Handles.DrawLine(car.cp, car.bp2);

				//Gizmos.DrawSphere(car.bp2, car.length * 0.025f);
				Handles.SphereCap(0, car.bp2, Quaternion.identity, car.length * 0.025f);
			}
		}
	}
}