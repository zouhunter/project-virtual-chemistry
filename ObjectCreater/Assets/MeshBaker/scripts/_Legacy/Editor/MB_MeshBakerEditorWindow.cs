//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

public class MB_MeshBakerEditorWindow : EditorWindow, MB2_MeshBakerEditorWindowInterface
{
	public enum LightMapOption{
		ignore,
		preserveLightmapping
	}	
	
	public MB2_MeshBakerRoot _target = null;
	public MonoBehaviour target{
		get{ return _target; }
		set{ _target = (MB2_MeshBakerRoot) value; }
	}
	GameObject targetGO = null;
	GameObject oldTargetGO = null;
	MB2_TextureBaker textureBaker;
	MB2_MeshBaker meshBaker;
	
	bool autoGenerateMeshBakers = false;
	bool onlyStaticObjects = false;
	bool onlyEnabledObjects = false;
	bool excludeMeshesWithOBuvs = true;
	int lightmapIndex = -1;
	Material shaderMat = null;
	Material mat = null;
	
	bool tbFoldout = false;
	bool mbFoldout = false;
	
	bool generate_IncludeStaticObjects = true;
	LightMapOption generate_LightmapOption = LightMapOption.ignore;
	string generate_AssetsFolder = "";
	
	MB2_MeshBakerEditorInternal mbe = new MB2_MeshBakerEditorInternal();
	MB2_TextureBakerEditorInternal tbe = new MB2_TextureBakerEditorInternal();

	Vector2 scrollPos = Vector2.zero;
	
	GUIContent autoGenerateGUIContent = new GUIContent("Auto Generate Bakers (Experimental)", "Generates MeshBakers in the scene based on the groupings in the report.");
	
	void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

		EditorGUILayout.LabelField("Generate Report",EditorStyles.boldLabel);		
		EditorGUILayout.HelpBox("List shaders in scene prints a report to the console of shaders and which objects use them. This is useful for planning which objects to combine.", UnityEditor.MessageType.None);
		
		if (GUILayout.Button("List Shaders In Scene")){
			listMaterialsInScene(false);
		}
		
		EditorGUILayout.Separator();
		MB_EditorUtil.DrawSeparator();

		EditorGUILayout.HelpBox("This feature is experimental. It should be safe to use as all it does is generate game objects with Mesh and Material bakers "+
								"on them and assets for the combined materials.\n\n" +
								"Creates bakers and combined material assets in your scene based on the groupings in 'Generate Report'."+
								"Some configuration may still be required after bakers are generated. Groups are created for objects that " +
								"use the same material(s), shader(s) and lightmap. These groups should produce good results when baked.\n\n" +
							    "This feature groups objects conservatively so bakes almost always work.   This is not the only way to group objects. Objects with different shaders can also be grouped but results are" +
							    " less preditable. Meshes with submeshes are only" +
								"grouped if all meshes use the same set of shaders.", UnityEditor.MessageType.None);
				
		EditorGUILayout.LabelField(autoGenerateGUIContent, EditorStyles.boldLabel);		
		autoGenerateMeshBakers = EditorGUILayout.Foldout(autoGenerateMeshBakers,"Show Tools");
		if ( autoGenerateMeshBakers ){

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select Folder For Combined Material Assets") ){
				generate_AssetsFolder = EditorUtility.SaveFolderPanel("Create Combined Material Assets In Folder", "", "");	
				generate_AssetsFolder = "Assets" + generate_AssetsFolder.Replace(Application.dataPath, "") + "/";
			}
			EditorGUILayout.LabelField("Folder: " + generate_AssetsFolder);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Included Objects Must Be Static", GUILayout.Width(200));
			generate_IncludeStaticObjects = EditorGUILayout.Toggle(GUIContent.none, generate_IncludeStaticObjects);
			EditorGUILayout.EndHorizontal();
			generate_LightmapOption = (LightMapOption) EditorGUILayout.EnumPopup("Lightmapping", generate_LightmapOption);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Generate Mesh Bakers")){
				listMaterialsInScene(true);
			}
			if (GUILayout.Button("Bake Every MeshBaker In Scene")){
				try{
					MB2_TextureBaker[] texBakers = (MB2_TextureBaker[]) FindObjectsOfType(typeof(MB2_TextureBaker));
					for (int i = 0; i < texBakers.Length; i++){
						texBakers[i].CreateAtlases(updateProgressBar, true, new MB2_EditorMethods());	
					}
					MB2_MeshBaker[] mBakers = (MB2_MeshBaker[]) FindObjectsOfType(typeof(MB2_MeshBaker));
					for (int i = 0; i < mBakers.Length; i++){
						if (mBakers[i].textureBakeResults != null){
					    	MB2_MeshBakerEditorFunctions._bakeIntoCombined(mBakers[i], MB_OutputOptions.bakeIntoSceneObject);	
						}
					}					
				} catch (Exception e) {
					Debug.LogError(e);
				}finally{
					EditorUtility.ClearProgressBar();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		MB_EditorUtil.DrawSeparator();
		EditorGUILayout.Separator();		
		
		EditorGUILayout.LabelField("Add Selected Meshes To Bakers",EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Select one or more objects in the hierarchy view. Child Game Objects with MeshRender will be added. Use the fields below to filter what is added.", UnityEditor.MessageType.None);
		target = (MB2_MeshBakerRoot) EditorGUILayout.ObjectField("Target to add objects to",target,typeof(MB2_MeshBakerRoot),true);
		
		if (target != null){
			targetGO = target.gameObject;
		} else {
			targetGO = null;	
		}
			
		if (targetGO != oldTargetGO){
			textureBaker = targetGO.GetComponent<MB2_TextureBaker>();
			meshBaker = targetGO.GetComponent<MB2_MeshBaker>();
			tbe = new MB2_TextureBakerEditorInternal();
			mbe = new MB2_MeshBakerEditorInternal();
			oldTargetGO = targetGO;
		}		
		
		onlyStaticObjects = EditorGUILayout.Toggle("Only Static Objects", onlyStaticObjects);
		
		onlyEnabledObjects = EditorGUILayout.Toggle("Only Enabled Objects", onlyEnabledObjects);
		
		excludeMeshesWithOBuvs = EditorGUILayout.Toggle("Exclude meshes with out-of-bounds UVs", excludeMeshesWithOBuvs);
			
		mat = (Material) EditorGUILayout.ObjectField("Using Material",mat,typeof(Material),true);
		shaderMat = (Material) EditorGUILayout.ObjectField("Using Shader",shaderMat,typeof(Material),true);
		
		string[] lightmapDisplayValues = new string[257];
		int[] lightmapValues = new int[257];
		lightmapValues[0] = -2;
		lightmapValues[1] = -1;
		lightmapDisplayValues[0] = "don't filter on lightmapping";
		lightmapDisplayValues[1] = "not lightmapped";
		for (int i = 2; i < lightmapDisplayValues.Length; i++){
			lightmapDisplayValues[i] = "" + i;
			lightmapValues[i] = i;
		}
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Using Lightmap Index ");
		lightmapIndex = EditorGUILayout.IntPopup(lightmapIndex,
												 lightmapDisplayValues,
												 lightmapValues);
		EditorGUILayout.EndHorizontal();
		
		if (GUILayout.Button("Add Selected Meshes")){
			addSelectedObjects();
		}
		
		/*
		if (GUILayout.Button("Add LOD To Selected")){
			addLODToSelected();
		}
		
		if (GUILayout.Button("Remove LOD From All")){
			LODInternal[] lods = (LODInternal[]) FindObjectsOfType(typeof(LODInternal));
			for (int i = 0; i < lods.Length; i++){
				DestroyImmediate(lods[i]);
			}
		}
		*/		
		
		if (textureBaker != null){
			MB_EditorUtil.DrawSeparator();
			tbFoldout = EditorGUILayout.Foldout(tbFoldout,"Texture Baker");
			if (tbFoldout){
				tbe.DrawGUI((MB2_TextureBaker) textureBaker, typeof(MB_MeshBakerEditorWindow));
			}
			
		}
		if (meshBaker != null){
			MB_EditorUtil.DrawSeparator();
			mbFoldout = EditorGUILayout.Foldout(mbFoldout,"Mesh Baker");
			if (mbFoldout){
				mbe.DrawGUI((MB2_MeshBaker) meshBaker, typeof(MB_MeshBakerEditorWindow));
			}
		}
		EditorGUILayout.EndScrollView();
	}
	
	List<GameObject> GetFilteredList(){
		List<GameObject> newMomObjs = new List<GameObject>();
		MB2_MeshBakerRoot mom = (MB2_MeshBakerRoot) target;
		if (mom == null){
			Debug.LogError("Must select a target MeshBaker to add objects to");
			return newMomObjs;
		}		
		GameObject dontAddMe = null;
		Renderer r = MB_Utility.GetRenderer(mom.gameObject);
		if (r != null){ //make sure that this MeshBaker object is not in list
			dontAddMe = r.gameObject;	
		}
		
		int numInSelection = 0;
		int numStaticExcluded = 0;
		int numEnabledExcluded = 0;
		int numLightmapExcluded = 0;
		int numOBuvExcluded = 0;

		GameObject[] gos = Selection.gameObjects;
		if (gos.Length == 0){
			Debug.LogWarning("No objects selected in hierarchy view. Nothing added.");	
		}
		
		for (int i = 0; i < gos.Length; i++){
			GameObject go = gos[i];
			Renderer[] mrs = go.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < mrs.Length; j++){
				if (mrs[j] is MeshRenderer || mrs[j] is SkinnedMeshRenderer){
					if (mrs[j].GetComponent<TextMesh>() != null){
						continue; //don't add TextMeshes
					}
					numInSelection++;
					if (!newMomObjs.Contains(mrs[j].gameObject)){
						bool addMe = true;
						if (!mrs[j].gameObject.isStatic && onlyStaticObjects){
							numStaticExcluded++;
							addMe = false;
						}
						
						if (!mrs[j].enabled && onlyEnabledObjects){
							numEnabledExcluded++;
							addMe = false;
						}
	
						if (lightmapIndex != -2){
							if (mrs[j].lightmapIndex != lightmapIndex){
								numLightmapExcluded++;
								addMe = false;	
							}
						}
						
						Mesh mm = MB_Utility.GetMesh(mrs[j].gameObject);
						if (mm != null){
							Rect dummy = new Rect();
							if (MB_Utility.hasOutOfBoundsUVs(mm, ref dummy) && excludeMeshesWithOBuvs){
								if (shaderMat != null){
									numOBuvExcluded++;
									addMe = false;
								}
							} else {
								Debug.LogWarning("Object " + mrs[j].gameObject.name + " uses uvs that are outside the range (0,1)" +
									"this object can only be combined with other objects that use the exact same set of source textures (one image in each atlas)" +
									" unless fix out of bounds UVs is used");
							}
						}					
						
						if (shaderMat != null){
							Material[] nMats = mrs[j].sharedMaterials;
							bool usesShader = false;
							foreach(Material nMat in nMats){
								if (nMat != null && nMat.shader == shaderMat.shader){
									usesShader = true;	
								}
							}
							if (!usesShader){
								addMe = false;	
							}
						}
						
						if (mat != null){
							Material[] nMats = mrs[j].sharedMaterials;
							bool usesMat = false;
							foreach(Material nMat in nMats){
								if (nMat == mat){
									usesMat = true;
								}
							}
							if (!usesMat){
								addMe = false;
							}
						}		
									
						if (addMe && mrs[j].gameObject != dontAddMe){
							if (!newMomObjs.Contains(mrs[j].gameObject)){
								newMomObjs.Add(mrs[j].gameObject);
							}
						}	
					}
				}
			}
		}
		Debug.Log( "Total objects in selection " + numInSelection);
		if (numStaticExcluded > 0) Debug.Log(numStaticExcluded + " objects were excluded because they were not static");
		if (numEnabledExcluded > 0) Debug.Log(numEnabledExcluded + " objects were excluded because they were disabled");
		if (numOBuvExcluded > 0) Debug.Log(numOBuvExcluded + " objects were excluded because they had out of bounds uvs");
		if (numLightmapExcluded > 0) Debug.Log(numLightmapExcluded + " objects did not match lightmap filter.");
		return newMomObjs;
	}

	void addLODToSelected(){
		/*
		MB2_MeshBakerRoot mom = (MB2_MeshBakerRoot) target;
		if (mom == null){
			Debug.LogError("Must select a target MeshBaker to add objects to");
			return;
		}

		List<GameObject> newMomObjs = GetFilteredList();
		
		//Undo.RegisterUndo(mom, "Add Objects");
		int numAdded = 0;
		for (int i = 0; i < newMomObjs.Count;i++){
			if (newMomObjs[i].GetComponent<LODInternal>() == null){
				newMomObjs[i].AddComponent<LODInternal>();
				numAdded++;
			}
		}
		
		if (numAdded == 0){
			Debug.LogWarning("Added LOD to 0 objects. Make sure some or all objects are selected in the hierarchy view. Also check ths 'Only Static Objects', 'Using Material' and 'Using Shader' settings");
		} else {
			Debug.Log("Added " + numAdded + " LOD components " + mom.name);
		}
		*/
	}
	
	void addSelectedObjects(){
		MB2_MeshBakerRoot mom = (MB2_MeshBakerRoot) target;
		if (mom == null){
			Debug.LogError("Must select a target MeshBaker to add objects to");
			return;
		}
		List<GameObject> newMomObjs = GetFilteredList();
		
		MB_EditorUtil.RegisterUndo(mom, "Add Objects");
		List<GameObject> momObjs = mom.GetObjectsToCombine();
		int numAdded = 0;
		for (int i = 0; i < newMomObjs.Count;i++){
			if (!momObjs.Contains(newMomObjs[i])){
				momObjs.Add(newMomObjs[i]);
				numAdded++;
			}
		}
		SerializedObject so = new SerializedObject(mom);
		so.SetIsDifferentCacheDirty();
		
		if (numAdded == 0){
			Debug.LogWarning("Added 0 objects. Make sure some or all objects are selected in the hierarchy view. Also check ths 'Only Static Objects', 'Using Material' and 'Using Shader' settings");
		} else {
			Debug.Log("Added " + numAdded + " objects to " + mom.name);
		}
	}
	
	public class _GameObjectAndWarning : IComparable{
		public GameObject go;
		public Shader shader = null;
		public Shader[] shaders = null;
		public Material material = null;
		public Material[] materials = null;
		public bool outOfBoundsUVs = false;
		public bool submeshesOverlap = false;
		public int numMaterials = 1;
		public int lightmapIndex = -1;
		public int numVerts = 0;
		public bool isStatic = false;
		public LightMapOption lightmapSetting;
		public string warning;
		
		public int CompareTo(System.Object obj){
			if (obj is _GameObjectAndWarning){
				_GameObjectAndWarning gobj = (_GameObjectAndWarning) obj;
				
				//compare lightmap settings
				int lightmapCompare = gobj.lightmapIndex - lightmapIndex;
				if (lightmapCompare != 0){
					return lightmapCompare;
				}

                //compare shaders
                int shaderCompare = (shader == null ? "null" : shader.ToString()).CompareTo(gobj.shader == null ? "null" : gobj.shader.ToString());
                if (shaderCompare != 0){
                    return shaderCompare;   
                }

                //compare materials
                int materialCompare = (material == null ? "null" : material.ToString()).CompareTo(gobj.material.ToString());
                if (materialCompare != 0){
                    return materialCompare; 
                }				
				
//				//compare shaders
//				int shaderCompare = shader.ToString().CompareTo(gobj.shader.ToString());
//				if (shaderCompare != 0){
//					return shaderCompare;	
//				}
//				
//				//compare materials
//				int materialCompare = material.ToString().CompareTo(gobj.material.ToString());
//				if (materialCompare != 0){
//					return materialCompare;	
//				}
				
				//obUV compaer
				int obUVCompare = Convert.ToInt32(gobj.outOfBoundsUVs) - Convert.ToInt32(outOfBoundsUVs);
				return obUVCompare;	
			}
			return 0;
		}
		
		public _GameObjectAndWarning(GameObject g, string w, LightMapOption ls){
			go = g;
			lightmapSetting = ls;
			Renderer r = MB_Utility.GetRenderer(g);
			material = r.sharedMaterial;
			if (material != null) shader = material.shader;
			materials = r.sharedMaterials;
			shaders = new Shader[materials.Length];
			for (int i = 0; i < shaders.Length; i++){
				if (materials[i] != null) shaders[i] = materials[i].shader;
			}
			lightmapIndex = r.lightmapIndex;
			Mesh mesh = MB_Utility.GetMesh(g);
			numVerts = 0;
			if (mesh != null) numVerts = mesh.vertexCount;
			isStatic = go.isStatic;
			numMaterials = materials.Length;
			warning = w;
			outOfBoundsUVs = false;
			submeshesOverlap = false;			
		}
	}
	
	void listMaterialsInScene(bool generateMeshBakers){
		Dictionary<Shader,List<_GameObjectAndWarning>> shader2GameObjects = new Dictionary<Shader, List<_GameObjectAndWarning>>();
		Renderer[] rs = (Renderer[]) FindObjectsOfType(typeof(Renderer));
//		Profile.StartProfile("listMaterialsInScene1");
		for (int i = 0; i < rs.Length; i++){
			Renderer r = rs[i];
			if (r is MeshRenderer || r is SkinnedMeshRenderer){
				if (r.GetComponent<TextMesh>() != null){
					continue; //don't add TextMeshes
				}
				Material[] mms = r.sharedMaterials;
				List<_GameObjectAndWarning> gos;
			
				foreach (Material mm in mms){
					if (mm != null){
						string warn = "";
						if (shader2GameObjects.ContainsKey(mm.shader)){
							gos = shader2GameObjects[mm.shader];
						} else {
							gos = new List<_GameObjectAndWarning>();
							shader2GameObjects.Add(mm.shader,gos);
						}
						//todo add warning for texture scaling
						if (r.sharedMaterials.Length > 1){
							warn += " [Uses multiple materials] ";
						}
					
						if (gos.Find(x => x.go == r.gameObject) == null){
							gos.Add(new _GameObjectAndWarning(r.gameObject,warn, generate_LightmapOption));
						}
					}
				}
			}
		}

		foreach(Shader m in shader2GameObjects.Keys){
			int totalVerts = 0;
			List<_GameObjectAndWarning> gos = shader2GameObjects[m];
			for (int i = 0; i < gos.Count; i++){
				Mesh mm = MB_Utility.GetMesh(gos[i].go);
				int nVerts = 0;
				if (mm != null){
					nVerts += mm.vertexCount;
					Rect dummy = new Rect();
					if (MB_Utility.hasOutOfBoundsUVs(mm,ref dummy)){
						int w = (int) dummy.width;
						int h = (int) dummy.height;
						gos[i].outOfBoundsUVs = true;
						gos[i].warning += " [WARNING: has uvs outside the range (0,1) tex is tiled " + w + "x" + h + " times]";
					}
					if (MB_Utility.doSubmeshesShareVertsOrTris(mm) != 0){
						gos[i].submeshesOverlap = true;
						gos[i].warning += " [WARNING: Submeshes share verts or triangles. 'Multiple Combined Materials' feature may not work.]";
					}
				}
				totalVerts += nVerts;
				Renderer mr = gos[i].go.GetComponent<Renderer>();
				if (!MB_Utility.validateOBuvsMultiMaterial(mr.sharedMaterials)){
					gos[i].warning += " [WARNING: Object uses same material on multiple submeshes. This may produce poor results when used with multiple materials or fix out of bounds uvs.]";
				}
			}
		}
		Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>> gs2bakeGroupMap = new Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>>();
		List<_GameObjectAndWarning> objsNotAddedToBaker = new List<_GameObjectAndWarning>();		
		
		sortIntoBakeGroups2(generateMeshBakers, shader2GameObjects, gs2bakeGroupMap, objsNotAddedToBaker);
		
		if (generateMeshBakers){
			createBakers(gs2bakeGroupMap, objsNotAddedToBaker);
			//Debug.Log( generateSceneAnalysisReport(gs2bakeGroupMap, objsNotAddedToBaker) );						
		} else {
			Debug.Log( generateSceneAnalysisReport(gs2bakeGroupMap, objsNotAddedToBaker) );			
		}		
	}
	
	string generateSceneAnalysisReport(Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>> gs2bakeGroupMap, List<_GameObjectAndWarning> objsNotAddedToBaker){
		string outStr = "(Click me, if I am too big copy and paste me into a spreadsheet or text editor)\n";// Materials in scene " + shader2GameObjects.Keys.Count + " and the objects that use them:\n";
		outStr += "\t\tOBJECT NAME\tLIGHTMAP INDEX\tSTATIC\tOVERLAPPING SUBMESHES\tOUT-OF-BOUNDS UVs\tNUM MATS\tMATERIAL\tWARNINGS\n";
		int totalVerts = 0;
		string outStr2 = "";
		foreach(List<_GameObjectAndWarning> gos in gs2bakeGroupMap.Values){
			outStr2 = "";
			totalVerts = 0;
			gos.Sort();
			for (int i = 0; i < gos.Count; i++){
				totalVerts += gos[i].numVerts;
				string matStr = "";
				Renderer mr = gos[i].go.GetComponent<Renderer>();
				foreach(Material mmm in mr.sharedMaterials){
					matStr += "[" + mmm + "] ";
				}				
				outStr2 += "\t\t" + gos[i].go.name + " (" + gos[i].numVerts + " verts)\t" + gos[i].lightmapIndex + "\t" + gos[i].isStatic + "\t" + gos[i].submeshesOverlap + "\t" + gos[i].outOfBoundsUVs + "\t" + gos[i].numMaterials + "\t" + matStr + "\t" + gos[i].warning + "\n";	
			}
			outStr2 = "\t" + gos[0].shader + " (" + totalVerts + " verts): \n" + outStr2;
			outStr += outStr2;
		}
		if (objsNotAddedToBaker.Count > 0){
			outStr += "Other objects\n";
			string shaderName = "";
			totalVerts = 0;
			List<_GameObjectAndWarning> gos1 = objsNotAddedToBaker;
			gos1.Sort();
			outStr2 = "";
			for (int i = 0; i < gos1.Count; i++){
				if (!shaderName.Equals( objsNotAddedToBaker[i].shader.ToString() )){
					outStr2 += "\t" + gos1[0].shader + "\n";
					shaderName = objsNotAddedToBaker[i].shader.ToString();	
				}
				totalVerts += gos1[i].numVerts;
				string matStr = "";
				Renderer mr = gos1[i].go.GetComponent<Renderer>();
				foreach(Material mmm in mr.sharedMaterials){
					matStr += "[" + mmm + "] ";
				}				
				outStr2 += "\t\t" + gos1[i].go.name + " (" + gos1[i].numVerts + " verts)\t" + gos1[i].lightmapIndex + "\t" + gos1[i].isStatic + "\t" + gos1[i].submeshesOverlap + "\t" + gos1[i].outOfBoundsUVs + "\t" + gos1[i].numMaterials + "\t" + matStr + "\t" + gos1[i].warning + "\n";	
			}
			outStr += outStr2;	
		}		
		return outStr;
	}
	
	bool MaterialsAreTheSame(_GameObjectAndWarning a, _GameObjectAndWarning b){
		HashSet<Material> aMats = new HashSet<Material>();
		for(int i = 0; i < a.materials.Length; i++) aMats.Add(a.materials[i]);
		HashSet<Material> bMats = new HashSet<Material>();
		for(int i = 0; i < b.materials.Length; i++) bMats.Add(b.materials[i]);
		return aMats.SetEquals(bMats);
	}

	bool ShadersAreTheSame(_GameObjectAndWarning a, _GameObjectAndWarning b){
		HashSet<Shader> aMats = new HashSet<Shader>();
		for(int i = 0; i < a.shaders.Length; i++) aMats.Add(a.shaders[i]);
		HashSet<Shader> bMats = new HashSet<Shader>();
		for(int i = 0; i < b.shaders.Length; i++) bMats.Add(b.shaders[i]);
		return aMats.SetEquals(bMats);
	}

	void sortIntoBakeGroups2(bool useFilters,
							Dictionary<Shader,List<_GameObjectAndWarning>> shader2GameObjects,
							Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>> gs2bakeGroupMap,
							List<_GameObjectAndWarning> objsNotAddedToBaker){
		foreach(Shader m in shader2GameObjects.Keys){
			List<_GameObjectAndWarning> gos = shader2GameObjects[m];
			gos.Sort();
			List<_GameObjectAndWarning> l = null;
			_GameObjectAndWarning key = gos[0];
			for (int i = 0; i < gos.Count; i++){
				//exclude objects that should not be included
				bool includeThisGameObject = true;
				if (useFilters && generate_IncludeStaticObjects && !gos[i].isStatic) includeThisGameObject = false;
				if (useFilters && gos[i].submeshesOverlap) includeThisGameObject = false;
				if (useFilters && gos[i].numMaterials > 1) includeThisGameObject = false;
				
				if (!includeThisGameObject){
					objsNotAddedToBaker.Add(gos[i]);
					continue;
				}
				
				//compare with key and decide if we need a new list
				if (key.lightmapIndex != gos[i].lightmapIndex) l = null;
				if (gos[i].outOfBoundsUVs && !MaterialsAreTheSame(key, gos[i])) l = null;
				if (key.outOfBoundsUVs && !MaterialsAreTheSame(key, gos[i])) l = null;
			
				/*
				_GameObjectAndWarning key = null;
				foreach(_GameObjectAndWarning consider in gs2bakeGroupMap.Keys){
					if (generate_LightmapOption == LightMapOption.preserveLightmapping && 
						consider.lightmapIndex != gos[i].lightmapIndex) continue;
					if (gos[i].submeshesOverlap) continue;
					
					if (gos[i].outOfBoundsUVs == true){
						if (consider.outOfBoundsUVs == true && MaterialsAreTheSame(consider, gos[i])){
							//materials needs to be the same
							key = consider;
							break;
						}
					} else {
						//shader needs to be the same
						if (ShadersAreTheSame(consider, gos[i])){
							key = consider;
							break;
						}						
					}
				}
				*/					
				
				if (l == null){
					l = new List<_GameObjectAndWarning>();
					gs2bakeGroupMap.Add(gos[i],l);
					key = gos[i];
				}
				l.Add(gos[i]);				
			}
		}
	}	
	  
	void sortIntoBakeGroups(bool useFilters,
							Dictionary<Shader,List<_GameObjectAndWarning>> shader2GameObjects,
							Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>> gs2bakeGroupMap,
							List<_GameObjectAndWarning> objsNotAddedToBaker){
		foreach(Shader m in shader2GameObjects.Keys){
			List<_GameObjectAndWarning> gos = shader2GameObjects[m];
			gos.Sort();
			for (int i = 0; i < gos.Count; i++){
				//exclude objects that should not be included
				bool includeThisGameObject = true;
				if (useFilters && generate_IncludeStaticObjects && !gos[i].isStatic) includeThisGameObject = false;
				if (useFilters && gos[i].submeshesOverlap) includeThisGameObject = false;
				if (useFilters && gos[i].numMaterials > 1) includeThisGameObject = false;
				
				if (!includeThisGameObject){
					objsNotAddedToBaker.Add(gos[i]);
					continue;
				}
				
				//try to find a group that this game object belongs to
				_GameObjectAndWarning key = null;
				foreach(_GameObjectAndWarning consider in gs2bakeGroupMap.Keys){
					if (generate_LightmapOption == LightMapOption.preserveLightmapping && 
						consider.lightmapIndex != gos[i].lightmapIndex) continue;
					if (gos[i].submeshesOverlap) continue;
					
					if (gos[i].outOfBoundsUVs == true){
						if (consider.outOfBoundsUVs == true && MaterialsAreTheSame(consider, gos[i])){
							//materials needs to be the same
							key = consider;
							break;
						}
					} else {
						//shader needs to be the same
						if (ShadersAreTheSame(consider, gos[i])){
							key = consider;
							break;
						}						
					}
				}					
		
				List<_GameObjectAndWarning> l = null;
				if (key == null){
					l = new List<_GameObjectAndWarning>();
					gs2bakeGroupMap.Add(gos[i],l);
				} else {
					l = gs2bakeGroupMap[key];
				}
				l.Add(gos[i]);				
			}
		}
	}
	
	void createBakers(Dictionary<_GameObjectAndWarning,List<_GameObjectAndWarning>> gs2bakeGroupMap, List<_GameObjectAndWarning> objsNotAddedToBaker){
		string s = "";
		int numBakers = 0;
		int numObjsAdded = 0;
		//create set of objects already added to mesh bakers
//		HashSet<GameObject> alreadyInMeshBaker = new HashSet<GameObject>();
//		MB2_MeshBakerCommon[]  mbsInScene = (MB2_MeshBakerCommon[]) FindObjectsOfType(typeof(MB2_MeshBakerCommon));
//		for (int i = 0; i < mbsInScene.Length; i++){
//			List<GameObject> gos = mbsInScene[i].GetObjectsToCombine();
//			for (int j = 0; j < gos.Count; j++){
//				if (!alreadyInMeshBaker.Contains(gos[j])) alreadyInMeshBaker.Add(gos[j]);
//			}
//		}
		
		if (generate_AssetsFolder == null || generate_AssetsFolder == ""){
			Debug.LogError("Need to choose a folder for saving the combined material assets.");
			return;
		}
		
		List<_GameObjectAndWarning> singletonObjsNotAddedToBaker = new List<_GameObjectAndWarning>();
		foreach(List<_GameObjectAndWarning> gaw in gs2bakeGroupMap.Values){
			if (gaw.Count > 1){
				numBakers ++;
				numObjsAdded += gaw.Count;
				createAndSetupBaker(gaw, generate_AssetsFolder);
				s += "  Created meshbaker for shader=" + gaw[0].shader + " lightmap=" + gaw[0].lightmapIndex + " OBuvs=" + gaw[0].outOfBoundsUVs + "\n";
			} else {
				singletonObjsNotAddedToBaker.Add(gaw[0]);
			}
		}		
		s = "Created " + numBakers + " bakers. Added " + numObjsAdded + " objects\n" + s;
		Debug.Log(s);
		s = "Objects not added=" + objsNotAddedToBaker.Count + " objects that have unique material=" + singletonObjsNotAddedToBaker.Count + "\n";
		for (int i = 0; i < objsNotAddedToBaker.Count; i++){
			s += "    " + objsNotAddedToBaker[i].go.name + 
						" isStatic=" + objsNotAddedToBaker[i].isStatic + 
					    " submeshesOverlap" + objsNotAddedToBaker[i].submeshesOverlap + 
					    " numMats=" + objsNotAddedToBaker[i].numMaterials + "\n";
		}
		for (int i = 0; i < singletonObjsNotAddedToBaker.Count; i++){
			s += "    " + singletonObjsNotAddedToBaker[i].go.name + " single\n";
		}		
		Debug.Log(s);		
	}
	
	void createAndSetupBaker(List<_GameObjectAndWarning> gaws, string pthRoot){
		if (gaws.Count < 1){
			return;
		}
		
		int numVerts = 0;
		for (int i = 0; i < gaws.Count; i++){
			numVerts = gaws[i].numVerts;
		}
		
		GameObject newMeshBaker = null;
		if (numVerts >= 65535){
			newMeshBaker = MB2_MultiMeshBakerEditor.CreateNewMeshBaker();
		} else {
			newMeshBaker = MB2_MeshBakerEditor.CreateNewMeshBaker();
		}
		
		newMeshBaker.name = ("MeshBaker-" + gaws[0].shader.name + "-LM" + gaws[0].lightmapIndex).ToString().Replace("/","-");
			
		MB2_TextureBaker tb = newMeshBaker.GetComponent<MB2_TextureBaker>();
		//string pth = AssetDatabase.GenerateUniqueAssetPath(pthRoot);
		
		//create result material
		string pthMat = AssetDatabase.GenerateUniqueAssetPath( pthRoot + newMeshBaker.name + ".mat" );
		AssetDatabase.CreateAsset(new Material(Shader.Find("Diffuse")), pthMat);
		tb.resultMaterial = (Material) AssetDatabase.LoadAssetAtPath(pthMat, typeof(Material));
		
		//create the MB2_TextureBakeResults
		string pthAsset = AssetDatabase.GenerateUniqueAssetPath( pthRoot + newMeshBaker.name + ".asset" );
		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MB2_TextureBakeResults>(),pthAsset);
		tb.textureBakeResults = (MB2_TextureBakeResults) AssetDatabase.LoadAssetAtPath(pthAsset, typeof(MB2_TextureBakeResults));
		AssetDatabase.Refresh();		
		
		tb.resultMaterial.shader = gaws[0].shader;
		tb.objsToMesh = new List<GameObject>();
		for (int i = 0; i < gaws.Count; i++){
			tb.objsToMesh.Add(gaws[i].go);
		}
		MB2_MeshBakerCommon mb = newMeshBaker.GetComponent<MB2_MeshBakerCommon>();
		if (generate_LightmapOption == LightMapOption.ignore){
			mb.lightmapOption = MB2_LightmapOptions.ignore_UV2;
		} else {
			if (gaws[0].lightmapIndex == -1 || gaws[0].lightmapIndex == -2){
				mb.lightmapOption = MB2_LightmapOptions.ignore_UV2;
			} else {
				mb.lightmapOption = MB2_LightmapOptions.preserve_current_lightmapping;
			}
		}
	}
	
	void bakeAllBakersInScene(){
		MB2_MeshBakerRoot[] bakers =(MB2_MeshBakerRoot[]) FindObjectsOfType(typeof(MB2_MeshBakerRoot));	
		for (int i = 0; i < bakers.Length; i++){
			if (bakers[i] is MB2_TextureBaker){
				MB2_TextureBaker tb = (MB2_TextureBaker) bakers[i];
				tb.CreateAtlases(updateProgressBar, true, new MB2_EditorMethods());
			}
		}
		EditorUtility.ClearProgressBar();
//		for (int i = 0; i < bakers.Length; i++){
//			if (bakers[i] is MB2_MeshBaker){
//				MB2_MeshBaker mb = (MB2_MeshBaker) bakers[i];
//			}
//			if (bakers[i] is MB2_MultiMeshBaker){
//				MB2_MultiMeshBaker mb = (MB2_MultiMeshBaker) bakers[i];
//			}
//		}
	}
	
	public void updateProgressBar(string msg, float progress){
		EditorUtility.DisplayProgressBar("Combining Meshes", msg, progress);
	}	
}