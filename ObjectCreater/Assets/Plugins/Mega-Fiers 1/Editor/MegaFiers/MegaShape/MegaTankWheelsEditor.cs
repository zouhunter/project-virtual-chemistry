
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaTankWheels))]
public class MegaTankWheelsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeControls();
		DrawDefaultInspector();
	}

	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
	static void RenderGizmo(MegaTankWheels track, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{

			Gizmos.matrix = track.transform.localToWorldMatrix;
			//Gizmos.DrawWireCube(Vector3.zero, new Vector3(track.radius * 0.5f, track.radius * 0.5f, track.radius));
			Gizmos.DrawWireSphere(Vector3.zero, track.radius);
		}
	}
}