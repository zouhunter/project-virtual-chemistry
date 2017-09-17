
using UnityEngine;
using UnityEditor;
//using System.Reflection;

[CustomEditor(typeof(MegaModifyObject))]
public class MegaModifyObjectEditor : Editor
{
	Texture image;
	//bool showhelp = false;
	bool showorder = false;
	bool showmulti = false;

	bool showgroups = false;

	public override void OnInspectorGUI()
	{
		MegaModifyObject mod = (MegaModifyObject)target;

#if !UNITY_4_3
		EditorGUIUtility.LookLikeInspector();
#endif
		MegaModifiers.GlobalDisplay = EditorGUILayout.Toggle("GlobalDisplayGizmos", MegaModifiers.GlobalDisplay);
		mod.Enabled			= EditorGUILayout.Toggle("Enabled", mod.Enabled);
		mod.recalcnorms		= EditorGUILayout.Toggle("Recalc Normals", mod.recalcnorms);
		MegaNormalMethod method = mod.NormalMethod;
		mod.NormalMethod	= (MegaNormalMethod)EditorGUILayout.EnumPopup("Normal Method", mod.NormalMethod);
		mod.recalcbounds	= EditorGUILayout.Toggle("Recalc Bounds", mod.recalcbounds);
		mod.recalcCollider	= EditorGUILayout.Toggle("Recalc Collider", mod.recalcCollider);
		mod.recalcTangents	= EditorGUILayout.Toggle("Recalc Tangents", mod.recalcTangents);
		mod.DoLateUpdate	= EditorGUILayout.Toggle("Do Late Update", mod.DoLateUpdate);
		mod.InvisibleUpdate = EditorGUILayout.Toggle("Invisible Update", mod.InvisibleUpdate);
		mod.GrabVerts		= EditorGUILayout.Toggle("Grab Verts", mod.GrabVerts);
		mod.DrawGizmos		= EditorGUILayout.Toggle("Draw Gizmos", mod.DrawGizmos);

		if ( mod.NormalMethod != method && mod.NormalMethod == MegaNormalMethod.Mega )
		{
			mod.BuildNormalMapping(mod.mesh, false);
		}

		//showmulti = EditorGUILayout.Foldout(showmulti, "Multi Core");
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Copy Object") )
		{
			GameObject obj = MegaCopyObject.DoCopyObjects(mod.gameObject);
			if ( obj )
			{
				obj.transform.position = mod.gameObject.transform.position;

				Selection.activeGameObject = obj;
			}
		}

		if ( GUILayout.Button("Copy Hierarchy") )
		{
			GameObject obj = MegaCopyObject.DoCopyObjectsChildren(mod.gameObject);
			Selection.activeGameObject = obj;
		}

		EditorGUILayout.EndHorizontal();
#endif
		if ( GUILayout.Button("Threading Options") )
			showmulti = !showmulti;

		if ( showmulti )
		{
			MegaModifiers.ThreadingOn = EditorGUILayout.Toggle("Threading Enabled", MegaModifiers.ThreadingOn);
			mod.UseThreading = EditorGUILayout.Toggle("Thread This Object", mod.UseThreading);
		}

		EditorGUIUtility.LookLikeControls();

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		showorder = EditorGUILayout.Foldout(showorder, "Modifier Order");

		if ( showorder && mod.mods != null )
		{
			for ( int i = 0; i < mod.mods.Length; i++ )
			{
				EditorGUILayout.LabelField("", i.ToString() + " - " + mod.mods[i].ModName() + " " + mod.mods[i].Order  + " MaxLOD: " + mod.mods[i].MaxLOD);
				if ( mod.mods[i].Label != "" )
					EditorGUILayout.LabelField("\t" + mod.mods[i].Label);
			}
		}

		// Group stuff
		if ( GUILayout.Button("Group Members") )
			showgroups = !showgroups;

		if ( showgroups )
		{
			//if ( GUILayout.Button("Add Object") )
			//{
				//MegaModifierTarget targ = new MegaModifierTarget();
			//	mod.group.Add(targ);
			//}

			for ( int i = 0; i < mod.group.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				mod.group[i] = (GameObject)EditorGUILayout.ObjectField("Obj " + i, mod.group[i], typeof(GameObject), true);
				if ( GUILayout.Button("Del") )
				{
					mod.group.Remove(mod.group[i]);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}

			GameObject newobj = (GameObject)EditorGUILayout.ObjectField("Add", null, typeof(GameObject), true);
			if ( newobj )
			{
				mod.group.Add(newobj);
			}

			if ( GUILayout.Button("Update") )
			{
				// for each group member check if it has a modify object comp, if not add one and copy values over
				// calculate box for all meshes and set, and set the Offset for each one
				// then for each modifier attached find or add and set instance value
				// in theory each gizmo should overlap the others

				// Have a method to update box and offsets if we allow moving in the group

			}
		}

		//if ( GUILayout.Button("Create Copy") )
		//{
		//	CloneObject();
		//}
	}

#if false
	// Could do fields and just not do mesh jobs
	// I am going to need a copy method for each modifier
	// Make a new version of the object in the scene, should be a non editor method for this
	GameObject CloneObject()
	{
		MegaModifyObject modobj = (MegaModifyObject)target;

		GameObject obj = modobj.gameObject;

		MonoBehaviour[] comps = obj.GetComponents<MonoBehaviour>();
		Debug.Log("Comps " + comps.Length);

		GameObject newobj = new GameObject(obj.name + " Copy");

		for ( int i = 0; i < comps.Length; i++ )
		{
			MonoBehaviour newcomp = (MonoBehaviour)newobj.AddComponent(comps[i].GetType());
			//EditorUtility.CopySerialized(comps[i], newcomp);

			foreach ( FieldInfo f in comps[i].GetType().GetFields() )
			{
				Debug.Log("field " + f.Name);
				f.SetValue(newcomp, f.GetValue(comps[i]));
			}
#if false
			foreach ( FieldInfo info in comps[i].GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic) )
			{
				Debug.Log("field " + info.Name);
				if ( info.IsPublic || info.GetCustomAttributes(typeof(SerializeField), true).Length != 0 )
				{
					info.SetValue(newcomp, info.GetValue(comps[i]));
				}
			}
#endif
		}

		return newobj;
	}

	// Build a prefab of the object
	void MakePrefab()
	{
	}
#endif
}
