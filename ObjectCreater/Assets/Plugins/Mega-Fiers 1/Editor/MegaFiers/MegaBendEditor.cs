
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaBend))]
public class MegaBendEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Bend Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

#if true
	public override bool Inspector()
	{
		MegaBend mod = (MegaBend)target;

		EditorGUIUtility.LookLikeControls();

		mod.angle = EditorGUILayout.FloatField("Angle", mod.angle);
		mod.dir = EditorGUILayout.FloatField("Dir", mod.dir);
		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		mod.from = EditorGUILayout.FloatField("From", mod.from);
		mod.to = EditorGUILayout.FloatField("To", mod.to);
		return false;
	}
#else
	SerializedProperty	angle;
	SerializedProperty	dir;
	SerializedProperty	axis;
	SerializedProperty	doRegion;
	SerializedProperty	from;
	SerializedProperty	to;

	GUIContent anglelab;
	GUIContent dirlab;
	GUIContent axislab;
	GUIContent doregionlab;
	GUIContent fromlab;
	GUIContent tolab;

	void OnEnable()
	{
		angle = serializedObject.FindProperty("angle");
		dir = serializedObject.FindProperty("dir");
		axis = serializedObject.FindProperty("axis");
		doRegion = serializedObject.FindProperty("doRegion");
		from = serializedObject.FindProperty("from");
		to = serializedObject.FindProperty("to");

		anglelab = new GUIContent("Angle");
		dirlab = new GUIContent("Dir");
		axislab = new GUIContent("Axis");
		doregionlab = new GUIContent("Do Region");
		fromlab = new GUIContent("From");
		tolab = new GUIContent("To");
	}

	public override bool Inspector()
	{
		serializedObject.Update();
		//MegaBend mod = (MegaBend)target;

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(angle, anglelab);
		EditorGUILayout.PropertyField(dir, dirlab);
		EditorGUILayout.PropertyField(axis, axislab);
		EditorGUILayout.PropertyField(doRegion, doregionlab);
		EditorGUILayout.PropertyField(from, fromlab);
		EditorGUILayout.PropertyField(to, tolab);

		serializedObject.ApplyModifiedProperties();
		return false;
	}
#endif
}
