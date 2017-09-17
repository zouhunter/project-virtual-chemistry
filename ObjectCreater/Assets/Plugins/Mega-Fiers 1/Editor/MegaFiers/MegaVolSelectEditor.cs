
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaVolSelect))]
public class MegaVolSelectEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Vol Select Modifier by Chris West"; }
	//public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	public override bool DisplayCommon() { return false; }

	public override bool Inspector()
	{
		MegaVolSelect mod = (MegaVolSelect)target;

		EditorGUIUtility.LookLikeControls();
		mod.Label = EditorGUILayout.TextField("Label", mod.Label);
		mod.MaxLOD = EditorGUILayout.IntField("MaxLOD", mod.MaxLOD);
		mod.ModEnabled = EditorGUILayout.Toggle("Enabled", mod.ModEnabled);
		mod.Order = EditorGUILayout.IntField("Order", mod.Order);
		mod.volType = (MegaVolumeType)EditorGUILayout.EnumPopup("Type", mod.volType);

		if ( mod.volType == MegaVolumeType.Sphere )
			mod.radius = EditorGUILayout.FloatField("Radius", mod.radius);
		else
			mod.boxsize = EditorGUILayout.Vector3Field("Size", mod.boxsize);

		mod.weight = EditorGUILayout.Slider("Weight", mod.weight, 0.0f, 1.0f);
		mod.falloff = EditorGUILayout.FloatField("Falloff", mod.falloff);
		mod.origin = EditorGUILayout.Vector3Field("Origin", mod.origin);
		mod.target = (Transform)EditorGUILayout.ObjectField("Target", mod.target, typeof(Transform), true);
		mod.useCurrentVerts = EditorGUILayout.Toggle("Use Stack Verts", mod.useCurrentVerts);

		mod.displayWeights = EditorGUILayout.Toggle("Show Weights", mod.displayWeights);
		mod.gizCol = EditorGUILayout.ColorField("Gizmo Col", mod.gizCol);
		mod.gizSize = EditorGUILayout.FloatField("Gizmo Size", mod.gizSize);
		mod.freezeSelection = EditorGUILayout.Toggle("Freeze Selection", mod.freezeSelection);

		return false;
	}

	// option to use base verts or current stack verts for distance calc
	// flag to display weights
	// size of weights and color of gizmo

	public override void DrawSceneGUI()
	{
		MegaVolSelect mod = (MegaVolSelect)target;

		if ( !mod.ModEnabled )
			return;

		MegaModifiers mc = mod.gameObject.GetComponent<MegaModifiers>();

		float[] sel = mod.GetSel();

		if ( mc != null && sel != null )
		{
			//Color col = Color.black;

			Matrix4x4 tm = mod.gameObject.transform.localToWorldMatrix;
			Handles.matrix = tm;	//Matrix4x4.identity;

			if ( mod.displayWeights )
			{
				for ( int i = 0; i < sel.Length; i++ )
				{
					float w = sel[i];
					if ( w > 0.001f )
					{
						if ( w > 0.5f )
							Handles.color = Color.Lerp(Color.green, Color.red, (w - 0.5f) * 2.0f);
						else
							Handles.color = Color.Lerp(Color.blue, Color.green, w * 2.0f);

						Handles.DotCap(i, mc.sverts[i], Quaternion.identity, mod.gizSize);
					}
				}
			}

			Handles.color = mod.gizCol;	//new Color(0.5f, 0.5f, 0.5f, 0.2f);

			//Handles.DrawWireDisc(0, tm.MultiplyPoint(mod.origin), Quaternion.identity, mod.radius);
			//Handles.DrawWireDisc(tm.MultiplyPoint(mod.origin), Vector3.up, mod.radius);
			//Handles.DrawWireDisc(tm.MultiplyPoint(mod.origin), Vector3.right, mod.radius);
			//Handles.DrawWireDisc(tm.MultiplyPoint(mod.origin), Vector3.forward, mod.radius);
			if ( mod.volType == MegaVolumeType.Sphere )
				Handles.SphereCap(0, tm.MultiplyPoint(mod.origin), Quaternion.identity, mod.radius * 2.0f);

			Handles.matrix = tm;
			
			Vector3 origin = Handles.PositionHandle(mod.origin, Quaternion.identity);

			if ( origin != mod.origin )
			{
				mod.origin = origin;
				EditorUtility.SetDirty(target);
			}
			Handles.matrix = Matrix4x4.identity;
		}
	}
}