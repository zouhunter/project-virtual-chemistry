
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaCurveSculptLayered))]
public class MegaCurveSculptLayeredEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Mega Curve Sculpt New Modifier by Chris West"; }

	void CurveGUI(MegaSculptCurve crv)
	{
		crv.enabled = EditorGUILayout.BeginToggleGroup("Enabled", crv.enabled);

		//if ( crv.enabled )
		{
			crv.name = EditorGUILayout.TextField("Name", crv.name);
			crv.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", crv.axis);
			crv.curve = EditorGUILayout.CurveField("Curve", crv.curve);

			crv.weight = EditorGUILayout.Slider("Weight", crv.weight, 0.0f, 1.0f);

			crv.affectOffset = (MegaAffect)EditorGUILayout.EnumPopup("Affect Off", crv.affectOffset);
			if ( crv.affectOffset != MegaAffect.None )
				crv.offamount = EditorGUILayout.Vector3Field("Offset", crv.offamount);

			crv.affectScale = (MegaAffect)EditorGUILayout.EnumPopup("Affect Scl", crv.affectScale);
			if ( crv.affectScale != MegaAffect.None )
				crv.sclamount = EditorGUILayout.Vector3Field("Scale", crv.sclamount);

			crv.uselimits = EditorGUILayout.BeginToggleGroup("Limits", crv.uselimits);
			crv.regcol = EditorGUILayout.ColorField("Col", crv.regcol);
			crv.origin = EditorGUILayout.Vector3Field("Origin", crv.origin);
			crv.boxsize = EditorGUILayout.Vector3Field("Boxsize", crv.boxsize);
			EditorGUILayout.EndToggleGroup();
		}

		EditorGUILayout.EndToggleGroup();
	}

	void SwapCurves(MegaCurveSculptLayered mod, int t1, int t2)
	{
		if ( t1 >= 0 && t1 < mod.curves.Count && t2 >= 0 && t2 < mod.curves.Count && t1 != t2 )
		{
			MegaSculptCurve mt1 = mod.curves[t1];
			mod.curves.RemoveAt(t1);
			mod.curves.Insert(t2, mt1);
			EditorUtility.SetDirty(target);
		}
	}

	public override bool Inspector()
	{
		MegaCurveSculptLayered mod = (MegaCurveSculptLayered)target;

		EditorGUIUtility.LookLikeControls();

		if ( GUILayout.Button("Add Curve") )
		{
			mod.curves.Add(MegaSculptCurve.Create());
		}

		for ( int i = 0; i < mod.curves.Count; i++ )
		{
			CurveGUI(mod.curves[i]);

			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button("Up") )
			{
				if ( i > 0 )
					SwapCurves(mod, i, i - 1);
			}

			if ( GUILayout.Button("Down") )
			{
				if ( i < mod.curves.Count - 1 )
					SwapCurves(mod, i, i + 1);
			}

			if ( GUILayout.Button("Delete") )
			{
				mod.curves.RemoveAt(i);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}

		return false;
	}

	public override void DrawSceneGUI()
	{
		MegaCurveSculptLayered mod = (MegaCurveSculptLayered)target;

		for ( int i = 0; i < mod.curves.Count; i++ )
		{
			if ( mod.curves[i].enabled && mod.curves[i].uselimits )
			{
				Vector3 pos = mod.transform.TransformPoint(mod.curves[i].origin);
				Vector3 newpos = Handles.PositionHandle(pos, Quaternion.identity);

				if ( newpos != pos )
				{
					mod.curves[i].origin = mod.transform.worldToLocalMatrix.MultiplyPoint(newpos);
					EditorUtility.SetDirty(target);
				}
			}
		}
	}

}