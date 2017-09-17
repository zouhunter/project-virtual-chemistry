//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DigitalOpus.MB.Core;

using UnityEditor;

namespace DigitalOpus.MB.Core{
	public class MB3_TextureBakerEditorInternal{
		//add option to exclude skinned mesh renderer and mesh renderer in filter
		//example scenes for multi material
		
		private static GUIContent insertContent = new GUIContent("+", "add a material");
		private static GUIContent deleteContent = new GUIContent("-", "delete a material");
		private static GUILayoutOption buttonWidth = GUILayout.MaxWidth(20f);
	
		private SerializedObject textureBaker;
		private SerializedProperty textureBakeResults, maxTilingBakeSize, doMultiMaterial, fixOutOfBoundsUVs, resultMaterial, resultMaterials, atlasPadding, resizePowerOfTwoTextures, customShaderPropNames, objsToMesh, texturePackingAlgorithm;
		
		bool resultMaterialsFoldout = true;
		bool showInstructions = false;
		bool showContainsReport = true;
		
		private static GUIContent
			createPrefabAndMaterialLabelContent = new GUIContent("Create Empty Assets For Combined Material", "Creates a material asset and a 'MB2_TextureBakeResult' asset. You should set the shader on the material. Mesh Baker uses the Texture properties on the material to decide what atlases need to be created. The MB2_TextureBakeResult asset should be used in the 'Material Bake Result' field."),
			openToolsWindowLabelContent = new GUIContent("Open Tools For Adding Objects", "Use these tools to find out what can be combined, discover possible problems with meshes, and quickly add objects."),
			fixOutOfBoundsGUIContent = new GUIContent("Fix Out-Of-Bounds UVs", "If mesh has uvs outside the range 0,1 uvs will be scaled so they are in 0,1 range. Textures will have tiling baked."),
			resizePowerOfTwoGUIContent = new GUIContent("Resize Power-Of-Two Textures", "Shrinks textures so they have a clear border of width 'Atlas Padding' around them. Improves texture packing efficiency."),
			customShaderPropertyNamesGUIContent = new GUIContent("Custom Shader Propert Names", "Mesh Baker has a list of common texture properties that it looks for in shaders to generate atlases. Custom shaders may have texture properties not on this list. Add them here and Meshbaker will generate atlases for them."),
			combinedMaterialsGUIContent = new GUIContent("Combined Materials", "Use the +/- buttons to add multiple combined materials. You will also need to specify which materials on the source objects map to each combined material."),
			maxTilingBakeSizeGUIContent = new GUIContent("Max Tiling Bake Size","This is the maximum size tiling textures will be baked to."),
			objectsToCombineGUIContent = new GUIContent("Objects To Be Combined","These can be prefabs or scene objects. They must be game objects with Renderer components, not the parent objects. Materials on these objects will baked into the combined material(s)"),
			textureBakeResultsGUIContent = new GUIContent("Material Bake Result","This asset contains a mapping of materials to UV rectangles in the atlases. It is needed to create combined meshes or adjust meshes so they can use the combined material(s). Create it using 'Create Empty Assets For Combined Material'. Drag it to the 'Material Bake Result' field to use it."),
			texturePackingAgorithmGUIContent = new GUIContent("Texture Packer", "Unity's PackTextures: Atlases are always a power of two. Can crash when trying to generate large atlases. \n\n Mesh Baker Texture Packer: Atlases will be most efficient size and shape (not limited to a power of two). More robust for large atlases."),
			configMultiMatFromObjsContent = new GUIContent("Build Source To Combined Mapping From \n Objects To Be Combined", "This will group the materials on your source objects by shader and create one source to combined mapping for each shader found. For example if combining trees then all the materials with the same bark shader will be grouped togther and all the materials with the same leaf material will be grouped together. You can adjust the results afterwards. \n\nIf fix out-of-bounds UVs is NOT checked then submeshes with UVs outside 0,0..1,1 will be mapped to their own submesh regardless of shader.");
		
		[MenuItem("GameObject/Create Other/Mesh Baker/Material Only Baker")]
		public static void CreateNewTextureBaker(){
			MB3_TextureBaker[] mbs = (MB3_TextureBaker[]) Editor.FindObjectsOfType(typeof(MB3_TextureBaker));
	    	Regex regex = new Regex(@"(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
			int largest = 0;
			try{
				for (int i = 0; i < mbs.Length; i++){
					Match match = regex.Match(mbs[i].name);
					if (match.Success){
						int val = Convert.ToInt32(match.Groups[1].Value);
						if (val >= largest)
							largest = val + 1;
					}
				}
			} catch(Exception e){
				if (e == null) e = null; //Do nothing supress compiler warning
			}
			GameObject nmb = new GameObject("MaterialBaker" + largest);
			nmb.transform.position = Vector3.zero;
			MB3_TextureBaker tb = nmb.AddComponent<MB3_TextureBaker>();
			tb.packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;
		}
	
		void _init(MB3_TextureBaker target) {
			textureBaker = new SerializedObject(target);
			doMultiMaterial = textureBaker.FindProperty("_doMultiMaterial");
			fixOutOfBoundsUVs = textureBaker.FindProperty("_fixOutOfBoundsUVs");
			resultMaterial = textureBaker.FindProperty("_resultMaterial");
			resultMaterials = textureBaker.FindProperty("resultMaterials");
			atlasPadding = textureBaker.FindProperty("_atlasPadding");
			resizePowerOfTwoTextures = textureBaker.FindProperty("_resizePowerOfTwoTextures");
			customShaderPropNames = textureBaker.FindProperty("_customShaderPropNames");
			objsToMesh = textureBaker.FindProperty("objsToMesh");
			maxTilingBakeSize = textureBaker.FindProperty("_maxTilingBakeSize");
			textureBakeResults = textureBaker.FindProperty("_textureBakeResults");
			texturePackingAlgorithm = textureBaker.FindProperty("_packingAlgorithm");
		}	
		
		public void DrawGUI(MB3_TextureBaker mom, System.Type editorWindow){
			if (textureBaker == null){
				_init(mom);
			}
			
			textureBaker.Update();
	
			showInstructions = EditorGUILayout.Foldout(showInstructions,"Instructions:");
			if (showInstructions){
				EditorGUILayout.HelpBox("1. Add scene objects or prefabs to combine. For best results these should use the same shader as result material.\n\n" +
										"2. Create Empty Assets For Combined Material(s)\n\n" +
										"3. Check that shader on result material(s) are correct.\n\n" +
										"4. Bake materials into combined material(s).\n\n" +
										"5. Look at warnings/errors in console. Decide if action needs to be taken.\n\n" +
										"6. You are now ready to build combined meshs or adjust meshes to use the combined material(s).", UnityEditor.MessageType.None);
				
			}
			mom.LOG_LEVEL = (MB2_LogLevel) EditorGUILayout.EnumPopup("Log Level", mom.LOG_LEVEL);

	
			EditorGUILayout.Separator();		
			EditorGUILayout.LabelField("Objects To Be Combined",EditorStyles.boldLabel);	
			if (GUILayout.Button(openToolsWindowLabelContent)){
				MB3_MeshBakerEditorWindowInterface  mmWin = (MB3_MeshBakerEditorWindowInterface) EditorWindow.GetWindow(editorWindow);
				mmWin.target = (MB3_MeshBakerRoot) mom;
			}	
			EditorGUILayout.PropertyField(objsToMesh,objectsToCombineGUIContent, true);		
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Output",EditorStyles.boldLabel);
			if (GUILayout.Button(createPrefabAndMaterialLabelContent)){
				string newPrefabPath = EditorUtility.SaveFilePanelInProject("Asset name", "", "asset", "Enter a name for the baked texture results");
				if (newPrefabPath != null){
					CreateCombinedMaterialAssets(mom, newPrefabPath);
				}
			}	
			EditorGUILayout.PropertyField(textureBakeResults, textureBakeResultsGUIContent);
			if (textureBakeResults.objectReferenceValue != null){
				showContainsReport = EditorGUILayout.Foldout(showContainsReport, "Shaders & Materials Contained");
				if (showContainsReport){
					EditorGUILayout.HelpBox(((MB2_TextureBakeResults)textureBakeResults.objectReferenceValue).GetDescription(), MessageType.Info);	
				}
			}
			EditorGUILayout.PropertyField(doMultiMaterial,new GUIContent("Multiple Combined Materials"));		
			
			if (mom.doMultiMaterial){
				EditorGUILayout.LabelField("Source Material To Combined Mapping",EditorStyles.boldLabel);
				if (GUILayout.Button(configMultiMatFromObjsContent)){
					ConfigureMutiMaterialsFromObjsToCombine(mom);	
				}
				EditorGUILayout.BeginHorizontal();
				resultMaterialsFoldout = EditorGUILayout.Foldout(resultMaterialsFoldout, combinedMaterialsGUIContent);
				
				if(GUILayout.Button(insertContent, EditorStyles.miniButtonLeft, buttonWidth)){
					if (resultMaterials.arraySize == 0){
						mom.resultMaterials = new MB_MultiMaterial[1];	
					} else {
						resultMaterials.InsertArrayElementAtIndex(resultMaterials.arraySize-1);
					}
				}
				if(GUILayout.Button(deleteContent, EditorStyles.miniButtonRight, buttonWidth)){
					resultMaterials.DeleteArrayElementAtIndex(resultMaterials.arraySize-1);
				}			
				EditorGUILayout.EndHorizontal();
				if (resultMaterialsFoldout){
					for(int i = 0; i < resultMaterials.arraySize; i++){
						EditorGUILayout.Separator();
						string s = "";
						if (i < mom.resultMaterials.Length && mom.resultMaterials[i] != null && mom.resultMaterials[i].combinedMaterial != null) s = mom.resultMaterials[i].combinedMaterial.shader.ToString();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("---------- submesh:" + i + " " + s,EditorStyles.boldLabel);
						if(GUILayout.Button(deleteContent, EditorStyles.miniButtonRight, buttonWidth)){
							resultMaterials.DeleteArrayElementAtIndex(i);
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Separator();
						SerializedProperty resMat = resultMaterials.GetArrayElementAtIndex(i);
						EditorGUILayout.PropertyField(resMat.FindPropertyRelative("combinedMaterial"));
						SerializedProperty sourceMats = resMat.FindPropertyRelative("sourceMaterials");
						EditorGUILayout.PropertyField(sourceMats,true);						
					}
				}
				
			} else {			
				EditorGUILayout.PropertyField(resultMaterial,new GUIContent("Combined Mesh Material"));
			}				
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Material Bake Options",EditorStyles.boldLabel);		
			EditorGUILayout.PropertyField(atlasPadding,new GUIContent("Atlas Padding"));
			EditorGUILayout.PropertyField(resizePowerOfTwoTextures, resizePowerOfTwoGUIContent);
			EditorGUILayout.PropertyField(customShaderPropNames,customShaderPropertyNamesGUIContent,true);
			EditorGUILayout.PropertyField(maxTilingBakeSize, maxTilingBakeSizeGUIContent);
			EditorGUILayout.PropertyField(fixOutOfBoundsUVs,fixOutOfBoundsGUIContent);
			if (texturePackingAlgorithm.intValue == (int) MB2_PackingAlgorithmEnum.UnitysPackTextures){
				EditorGUILayout.HelpBox("Unity's texture packer has memory problems and frequently crashes the editor.",MessageType.Warning);
			}
			EditorGUILayout.PropertyField(texturePackingAlgorithm, texturePackingAgorithmGUIContent);
			EditorGUILayout.Separator();				
			if (GUILayout.Button("Bake Materials Into Combined Material")){
				mom.CreateAtlases(updateProgressBar, true, new MB3_EditorMethods());
				EditorUtility.ClearProgressBar();
				if (mom.textureBakeResults != null) EditorUtility.SetDirty(mom.textureBakeResults);
			}
			textureBaker.ApplyModifiedProperties();		
			textureBaker.SetIsDifferentCacheDirty();
		}
			
		public void updateProgressBar(string msg, float progress){
			EditorUtility.DisplayProgressBar("Combining Meshes", msg, progress);
		}
		
		public void ConfigureMutiMaterialsFromObjsToCombine(MB3_TextureBaker mom){
			if (mom.objsToMesh.Count == 0){
				Debug.LogError("You need to add some objects to combine before building the multi material list.");
				return;
			}
			if (resultMaterials.arraySize > 0){
				Debug.LogError("You already have some source to combined material mappings configured. You must remove these before doing this operation.");
				return;			
			}
			if (mom.textureBakeResults == null){
				Debug.LogError("Material Bake Result asset must be set before using this operation.");
				return;
			}
			Dictionary<Shader,List<Material>> shader2Material_map = new Dictionary<Shader, List<Material>>();
			Dictionary<Material,Mesh> obUVobject2material_map = new Dictionary<Material,Mesh>();
			
			//validate that the objects to be combined are valid
			for (int i = 0; i < mom.objsToMesh.Count; i++){
				GameObject go = mom.objsToMesh[i];
				if (go == null) {
					Debug.LogError("Null object in list of objects to combine at position " + i);
					return;
				}
				Renderer r = go.GetComponent<Renderer>();
				if (r == null || (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer))){
					Debug.LogError("GameObject at position " + i + " in list of objects to combine did not have a renderer");
					return;
				}
				if (r.sharedMaterial == null){
					Debug.LogError("GameObject at position " + i + " in list of objects to combine has a null material");
					return;						
				}			
			}
			
			//first pass put any meshes with obUVs on their own submesh if not fixing OB uvs
			if (!mom.fixOutOfBoundsUVs){
				for (int i = 0; i < mom.objsToMesh.Count; i++){
					GameObject go = mom.objsToMesh[i];
					Mesh m = MB_Utility.GetMesh(go);
					Rect dummy = new Rect();
					Renderer r = go.GetComponent<Renderer>();	
					for (int j = 0; j < r.sharedMaterials.Length; j++){
						if (MB_Utility.hasOutOfBoundsUVs(m, ref dummy, j)){
							if (!obUVobject2material_map.ContainsKey(r.sharedMaterials[j])){
								Debug.LogWarning("Object " + go + " submesh " + j + " uses UVs outside the range 0,0..1,1 to generate tiling. This object has been mapped to its own submesh in the combined mesh. It can share a submesh with other objects that use different materials if you use the fix out of bounds UVs feature which will bake the tiling");
								obUVobject2material_map.Add(r.sharedMaterials[j],m);
							}
						}
					}
				}
			}
			
			//second pass  put other materials without OB uvs in a shader to material map
			for (int i = 0; i < mom.objsToMesh.Count; i++){
				Renderer r = mom.objsToMesh[i].GetComponent<Renderer>();
				for (int j = 0; j < r.sharedMaterials.Length; j++){
					if (!obUVobject2material_map.ContainsKey(r.sharedMaterials[j])) {
						if (r.sharedMaterials[j] == null) continue;
						List<Material> matsThatUseShader = null;
						if (!shader2Material_map.TryGetValue(r.sharedMaterials[j].shader, out matsThatUseShader)){
							matsThatUseShader = new List<Material>();
							shader2Material_map.Add(r.sharedMaterials[j].shader, matsThatUseShader);
						}
						if (!matsThatUseShader.Contains(r.sharedMaterials[j])) matsThatUseShader.Add(r.sharedMaterials[j]);
					}
				}			
			}
			
			if (shader2Material_map.Count == 0 && obUVobject2material_map.Count == 0) Debug.LogError("Found no materials in list of objects to combine");
			mom.resultMaterials = new MB_MultiMaterial[shader2Material_map.Count + obUVobject2material_map.Count];
			string pth = AssetDatabase.GetAssetPath(mom.textureBakeResults);
			string baseName = Path.GetFileNameWithoutExtension(pth);
			string folderPath = pth.Substring(0,pth.Length - baseName.Length - 6);		
			int k = 0;
			foreach(Shader sh in shader2Material_map.Keys){ 
				List<Material> matsThatUse = shader2Material_map[sh];
				MB_MultiMaterial mm = mom.resultMaterials[k] = new MB_MultiMaterial();
				mm.sourceMaterials = matsThatUse;
				string matName = folderPath +  baseName + "-mat" + k + ".mat";
				Material newMat = new Material(Shader.Find("Diffuse"));
				if (matsThatUse.Count > 0 && matsThatUse[0] != null){
					MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat, matsThatUse[0]);
				}
				AssetDatabase.CreateAsset(newMat, matName);
				mm.combinedMaterial = (Material) AssetDatabase.LoadAssetAtPath(matName,typeof(Material));
				k++;
			}
			foreach(Material m in obUVobject2material_map.Keys){
				MB_MultiMaterial mm = mom.resultMaterials[k] = new MB_MultiMaterial();
				mm.sourceMaterials = new List<Material>();
				mm.sourceMaterials.Add(m);
				string matName = folderPath +  baseName + "-mat" + k + ".mat";
				Material newMat = new Material(Shader.Find("Diffuse"));
				MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat,m);
				AssetDatabase.CreateAsset(newMat, matName);
				mm.combinedMaterial = (Material) AssetDatabase.LoadAssetAtPath(matName,typeof(Material));
				k++;
			}
			textureBaker.UpdateIfDirtyOrScript();
		}
	
	public static void CreateCombinedMaterialAssets(MB3_TextureBaker target, string pth){
		MB3_TextureBaker mom = (MB3_TextureBaker) target;
		string baseName = Path.GetFileNameWithoutExtension(pth);
		if (baseName == null || baseName.Length == 0) return;
		string folderPath = pth.Substring(0,pth.Length - baseName.Length - 6);
		
		List<string> matNames = new List<string>();
		if (mom.doMultiMaterial){
			for (int i = 0; i < mom.resultMaterials.Length; i++){
				matNames.Add( folderPath +  baseName + "-mat" + i + ".mat" );
				AssetDatabase.CreateAsset(new Material(Shader.Find("Diffuse")), matNames[i]);
				mom.resultMaterials[i].combinedMaterial = (Material) AssetDatabase.LoadAssetAtPath(matNames[i],typeof(Material));
			}
		}else{
			matNames.Add( folderPath +  baseName + "-mat.mat" );
			Material newMat = new Material(Shader.Find("Diffuse"));
			if (mom.objsToMesh.Count > 0 && mom.objsToMesh[0] != null){
				Renderer r = mom.objsToMesh[0].GetComponent<Renderer>();
				if (r == null){
					Debug.LogWarning("Object " + mom.objsToMesh[0] + " does not have a Renderer on it.");
				} else {
					if (r.sharedMaterial != null){
						newMat.shader = r.sharedMaterial.shader;					
						MB3_TextureBaker.ConfigureNewMaterialToMatchOld(newMat,r.sharedMaterial);
					}
				}
			} else {
				Debug.Log("If you add objects to be combined before creating the Combined Material Assets. Then Mesh Baker will create a result material that is a duplicate of the material on the first object to be combined. This saves time configuring the shader.");	
			}
			AssetDatabase.CreateAsset(newMat, matNames[0]);
			mom.resultMaterial = (Material) AssetDatabase.LoadAssetAtPath(matNames[0],typeof(Material));
		}
		//create the MB2_TextureBakeResults
		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MB2_TextureBakeResults>(),pth);
		mom.textureBakeResults = (MB2_TextureBakeResults) AssetDatabase.LoadAssetAtPath(pth, typeof(MB2_TextureBakeResults));
		AssetDatabase.Refresh();
	}	
	
	}
	

}