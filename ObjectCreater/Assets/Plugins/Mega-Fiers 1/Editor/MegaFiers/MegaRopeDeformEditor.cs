
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[CustomEditor(typeof(MegaRopeDeform))]
public class MegaRopeDeformEditor : MegaModifierEditor
{
	public override string GetHelpString() { return "Rope Deform Modifier by Chris West"; }
	public override Texture LoadImage() { return (Texture)EditorGUIUtility.LoadRequired("MegaFiers\\bend_help.png"); }

	bool showsoft = false;

	public override bool Inspector()
	{
		MegaRopeDeform mod = (MegaRopeDeform)target;

		//DrawDefaultInspector();
		//EditorGUIUtility.LookLikeControls();
		//mod.angle = EditorGUILayout.FloatField("Angle", mod.angle);
		//mod.dir = EditorGUILayout.FloatField("Dir", mod.dir);
		//mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);
		//mod.doRegion = EditorGUILayout.Toggle("Do Region", mod.doRegion);
		//mod.from = EditorGUILayout.FloatField("From", mod.from);
		//mod.to = EditorGUILayout.FloatField("To", mod.to);

		if ( GUILayout.Button("Rebuild") )
			mod.init = true;

		//mod.timeStep = EditorGUILayout.Slider("Time Step", mod.timeStep, 0.001f, 0.2f);

		mod.floorOff = EditorGUILayout.FloatField("Floor Off", mod.floorOff);
		mod.NumMasses = EditorGUILayout.IntField("Num Masses", mod.NumMasses);
		if ( mod.NumMasses < 2 )
			mod.NumMasses = 2;

		mod.Mass = EditorGUILayout.FloatField("Mass", mod.Mass);
		if ( mod.Mass < 0.01f )
			mod.Mass = 0.01f;

		mod.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", mod.axis);

		mod.stiffnessCrv = EditorGUILayout.CurveField("Stiffness Crv", mod.stiffnessCrv);
		mod.stiffspring = EditorGUILayout.FloatField("Stiff Spring", mod.stiffspring);
		mod.stiffdamp = EditorGUILayout.FloatField("Stiff Damp", mod.stiffdamp);

		mod.spring = EditorGUILayout.FloatField("Spring", mod.spring);
		mod.damp = EditorGUILayout.FloatField("Damp", mod.damp);

		mod.off = EditorGUILayout.FloatField("Off", mod.off);

		mod.SpringCompress = EditorGUILayout.FloatField("Spring Compress", mod.SpringCompress);

		mod.BendSprings = EditorGUILayout.Toggle("Bend Springs", mod.BendSprings);
		mod.Constraints = EditorGUILayout.Toggle("Constraints", mod.Constraints);

		mod.DampingRatio = EditorGUILayout.FloatField("Damping Ratio", mod.DampingRatio);

		mod.left = (Transform)EditorGUILayout.ObjectField("Left", mod.left, typeof(Transform), true);
		mod.right = (Transform)EditorGUILayout.ObjectField("Right", mod.right, typeof(Transform), true);

		mod.weight = EditorGUILayout.FloatField("Weight", mod.weight);
		mod.weightPos = EditorGUILayout.FloatField("Weight Pos", mod.weightPos);

		showsoft = EditorGUILayout.Foldout(showsoft, "Physics");

		if ( showsoft )
		{
			mod.soft.timeStep = EditorGUILayout.Slider("Time Step", mod.soft.timeStep, 0.001f, 0.2f);
			mod.soft.gravity = EditorGUILayout.Vector3Field("Gravity", mod.soft.gravity);
			mod.soft.airdrag = EditorGUILayout.FloatField("Air Drag", mod.soft.airdrag);
			mod.soft.friction = EditorGUILayout.FloatField("Friction", mod.soft.friction);

			mod.soft.iters = EditorGUILayout.IntField("Iterations", mod.soft.iters);
			mod.soft.method = (MegaIntegrator)EditorGUILayout.EnumPopup("Method", mod.soft.method);
			mod.soft.applyConstraints = EditorGUILayout.Toggle("Apply Constraints", mod.soft.applyConstraints);
		}

		mod.DisplayDebug = EditorGUILayout.BeginToggleGroup("Display Debug", mod.DisplayDebug);
		mod.drawsteps = EditorGUILayout.IntField("Draw Steps", mod.drawsteps);
		mod.boxsize = EditorGUILayout.FloatField("Box Size", mod.boxsize);
		EditorGUILayout.EndToggleGroup();

		return false;
	}

	static MegaRopeDeformEditor()
	{
		//EditorApplication.update += Update;
	}

	// Have a per object flag for editor update
	static void Update1()
	{
		GameObject obj = Selection.activeGameObject;

		if ( obj )
		{
			//MegaRopeDeform mr = (MegaRopeDeform)obj.GetComponent<MegaRopeDeform>();

			//if ( mr )
			{
				MegaModifyObject mod = (MegaModifyObject)obj.GetComponent<MegaModifyObject>();
				if ( mod )
				{
					mod.ModifyObject();
				}
			}
		}
	}

	static void Update()
	{
#if UNITY_3_5
		MegaModifyObject[] mods = (MegaModifyObject[])FindSceneObjectsOfType(typeof(MegaModifyObject));
#else
		MegaModifyObject[] mods = (MegaModifyObject[])FindObjectsOfType(typeof(MegaModifyObject));
#endif

		for ( int i = 0; i < mods.Length; i++ )
			mods[i].ModifyObject();
	}
}