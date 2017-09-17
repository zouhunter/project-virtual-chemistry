
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaDisplaceLimits))]
public class MegaDisplaceLimitsEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Displace Limits Modifier by Chris West"; }

	public override bool Inspector()
	{
		MegaDisplaceLimits mod = (MegaDisplaceLimits)target;

		EditorGUIUtility.LookLikeControls();
		mod.map = (Texture2D)EditorGUILayout.ObjectField("Map", mod.map, typeof(Texture2D), true);
		mod.amount = EditorGUILayout.FloatField("Amount", mod.amount);
		mod.offset = EditorGUILayout.Vector2Field("Offset", mod.offset);
		mod.vertical = EditorGUILayout.FloatField("Vertical", mod.vertical);
		mod.scale = EditorGUILayout.Vector2Field("Scale", mod.scale);
		mod.channel = (MegaChannel)EditorGUILayout.EnumPopup("Channel", mod.channel);
		mod.CentLum = EditorGUILayout.Toggle("Cent Lum", mod.CentLum);
		mod.CentVal = EditorGUILayout.FloatField("Cent Val", mod.CentVal);
		mod.Decay = EditorGUILayout.FloatField("Decay", mod.Decay);
		mod.origin = EditorGUILayout.Vector3Field("Origin", mod.origin);
		mod.size = EditorGUILayout.Vector3Field("Size", mod.size);
		return false;
	}

	public override void DrawSceneGUI()
	{
		MegaDisplaceLimits mod = (MegaDisplaceLimits)target;

		Vector3 pos = mod.transform.TransformPoint(mod.origin);
		Vector3 newpos = Handles.PositionHandle(pos, Quaternion.identity);

		if ( newpos != pos )
		{
			mod.origin = mod.transform.worldToLocalMatrix.MultiplyPoint(newpos);
			EditorUtility.SetDirty(target);
		}
	}
}