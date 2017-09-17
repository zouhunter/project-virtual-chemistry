//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DigitalOpus.MB.Core;



/// <summary>
/// Component that handles baking materials into a combined material.
/// 
/// The result of the material baking process is a MB2_TextureBakeResults object, which 
/// becomes the input for the mesh baking.
/// 
/// This class uses the MB_TextureCombiner to do the combining.
/// 
/// This class is a Component (MonoBehavior) so it is serialized and found using GetComponent. If
/// you want to access the texture baking functionality without creating a Component then use MB_TextureCombiner
/// directly.
/// </summary>
public class MB2_TextureBaker : MB2_MeshBakerRoot {	
	static bool VERBOSE = false;
	
	[HideInInspector] public int maxTilingBakeSize = 1024;
	[HideInInspector] public bool doMultiMaterial;
	[HideInInspector] public bool fixOutOfBoundsUVs = false;
	[HideInInspector] public Material resultMaterial;
	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];
	[HideInInspector] public int atlasPadding = 1;
	[HideInInspector] public bool resizePowerOfTwoTextures = true;
	[HideInInspector] public MB2_PackingAlgorithmEnum texturePackingAlgorithm;
	public List<string> customShaderPropNames = new List<string>();
	public List<GameObject> objsToMesh;
	
	public override List<GameObject> GetObjectsToCombine(){
		if (objsToMesh == null) objsToMesh = new List<GameObject>();
		return objsToMesh;
	}
	
	[Obsolete("CreateAndSaveAtlases is depricated please use CreateAtlases(progressInfo, true, editorFunctions) instead.")]
	public void CreateAndSaveAtlases(ProgressUpdateDelegate progressInfo, MB2_EditorMethodsInterface textureFormatTracker){
		CreateAtlases(progressInfo,true,textureFormatTracker);
	}	
	
	public MB_AtlasesAndRects[] CreateAtlases(){
		return CreateAtlases(null, false, null);
	}
	
	/// <summary>
	/// Creates the atlases.
	/// </summary>
	/// <returns>
	/// The atlases.
	/// </returns>
	/// <param name='progressInfo'>
	/// Progress info is a delegate function that displays a progress dialog. Can be null
	/// </param>
	/// <param name='saveAtlasesAsAssets'>
	/// if true atlases are saved as assets in the project folder. Othersise they are instances in memory
	/// </param>
	/// <param name='textureFormatTracker'>
	/// Texture format tracker. Contains editor functionality such as save assets. Can be null.
	/// </param>
	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface textureFormatTracker=null){
		MB_AtlasesAndRects[] mAndAs = null;
		try{
			mAndAs = _CreateAtlases(progressInfo, saveAtlasesAsAssets, textureFormatTracker);
		} catch(Exception e){
			Debug.LogError(e);	
		} finally {
			if (saveAtlasesAsAssets){ //Atlases were saved to project so we don't need these ones
				if (mAndAs != null){
					for(int j = 0; j < mAndAs.Length; j++){
						MB_AtlasesAndRects mAndA = mAndAs[j];
						if (mAndA != null && mAndA.atlases != null){
							for (int i = 0; i < mAndA.atlases.Length; i++){
								if (mAndA.atlases[i] != null){
									if (textureFormatTracker != null) textureFormatTracker.Destroy(mAndA.atlases[i]);
									else MB_Utility.Destroy(mAndA.atlases[i]);
								}
							}
						}
					}
				}
			}
		}
		return mAndAs;
	}
	
	MB_AtlasesAndRects[] _CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface textureFormatTracker=null){
		//validation
		if (saveAtlasesAsAssets && textureFormatTracker == null){
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true then textureFormatTracker cannot be null.");
			return null;
		}
		if (saveAtlasesAsAssets && !Application.isEditor){
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true it must be called from the Unity Editor.");
			return null;			
		}
		if (!doCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, textureFormatTracker)){
			return null;
		}
		if (doMultiMaterial && !_ValidateResultMaterials()){
			return null;
		} else if (!doMultiMaterial){
			if (resultMaterial == null){
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return null;
			}
			Shader targShader = resultMaterial.shader;
			for (int i = 0; i < objsToMesh.Count; i++){
				Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
				for (int j = 0; j < ms.Length; j++){
					Material m = ms[j];
					if (m != null && m.shader != targShader){
						Debug.LogWarning("Game object " + objsToMesh[i] + " does not use shader " + targShader + " it may not have the required textures. If not 2x2 clear textures will be generated.");
					}

				}
			}
		}

		for (int i = 0; i < objsToMesh.Count; i++){
			Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
			for (int j = 0; j < ms.Length; j++){
				Material m = ms[j];
				if (m == null){
					Debug.LogError("Game object " + objsToMesh[i] + " has a null material. Can't build atlases");
					return null;
				}

			}
		}		
		
		MB_TextureCombiner combiner = new MB_TextureCombiner();	
		// if editor analyse meshes and suggest treatment
		if (!Application.isPlaying){
			Material[] rms;
			if (doMultiMaterial){
				rms = new Material[resultMaterials.Length];
				for (int i = 0; i < rms.Length; i++) rms[i] = resultMaterials[i].combinedMaterial;
			} else {
				rms = new Material[1];
				rms[0] = resultMaterial;
			}
			combiner.SuggestTreatment(objsToMesh, rms, customShaderPropNames);
		}		
		
		//initialize structure to store results
		int numResults = 1;
		if (doMultiMaterial) numResults = resultMaterials.Length;
		MB_AtlasesAndRects[] resultAtlasesAndRects = new MB_AtlasesAndRects[numResults];
		for (int i = 0; i < resultAtlasesAndRects.Length; i++){
			resultAtlasesAndRects[i] = new MB_AtlasesAndRects();
		}
		
		//Do the material combining.
		for (int i = 0; i < resultAtlasesAndRects.Length; i++){
			Material resMatToPass = null;
			List<Material> sourceMats = null;			
			if (doMultiMaterial) {
				sourceMats = resultMaterials[i].sourceMaterials;
				resMatToPass = resultMaterials[i].combinedMaterial;
			} else {
				resMatToPass = resultMaterial;	
			}
			Debug.Log("Creating atlases for result material " + resMatToPass);
			if(!combiner.combineTexturesIntoAtlases(progressInfo, resultAtlasesAndRects[i], resMatToPass, objsToMesh, sourceMats, atlasPadding, customShaderPropNames, resizePowerOfTwoTextures, fixOutOfBoundsUVs, maxTilingBakeSize, saveAtlasesAsAssets, texturePackingAlgorithm, textureFormatTracker)){
				return null;
			}
		}
		
		//Save the results
		textureBakeResults.combinedMaterialInfo = resultAtlasesAndRects;
		textureBakeResults.doMultiMaterial = doMultiMaterial;
		textureBakeResults.resultMaterial = resultMaterial;
		textureBakeResults.resultMaterials = resultMaterials;
		textureBakeResults.fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		unpackMat2RectMap(textureBakeResults);
		
		//originally did all the assign of atlases to the result materials here
		//don't touch result material assets until we are sure atlas creation worked.
		//unfortunatly Unity has a bug where it Destroys Texture2Ds without warning when memory gets low and generates MissingReferenceException
		//so if generating assets in editor then save and assign atlases as soon as each atlas is created
//			if (Application.isPlaying){
//				if (doMultiMaterial){
//					for (int j = 0; j < resultMaterials.Length; j++){
//						Material resMat = resultMaterials[j].combinedMaterial; //resultMaterials[j].combinedMaterial;
//						Texture2D[] atlases = resultAtlasesAndRects[j].atlases;
//						for(int i = 0; i < atlases.Length;i++){
//							resMat.SetTexture(resultAtlasesAndRects[j].texPropertyNames[i], atlases[i]);
//						}
//					}					
//				} else {
//					Material resMat = resultMaterial; //resultMaterials[j].combinedMaterial;
//					Texture2D[] atlases = resultAtlasesAndRects[0].atlases;
//					for(int i = 0; i < atlases.Length;i++){
//						resMat.SetTexture(resultAtlasesAndRects[0].texPropertyNames[i], atlases[i]);
//					}
//				}
//			}
		
		//set the texture bake resultAtlasesAndRects on the Mesh Baker component if it exists
		MB2_MeshBakerCommon mb = GetComponent<MB2_MeshBakerCommon>();
		if (mb != null){
			mb.textureBakeResults = textureBakeResults;	
		}			
		
		if (VERBOSE) Debug.Log("Created Atlases");		
		return resultAtlasesAndRects;
	}		

	void unpackMat2RectMap(MB2_TextureBakeResults resultAtlasesAndRects){
		List<Material> ms = new List<Material>();
		List<Rect> rs = new List<Rect>();
		for (int i = 0; i < resultAtlasesAndRects.combinedMaterialInfo.Length; i++){
			MB_AtlasesAndRects newMesh = resultAtlasesAndRects.combinedMaterialInfo[i];
			Dictionary<Material,Rect> map = newMesh.mat2rect_map;
			foreach(Material m in map.Keys){
				ms.Add(m);
				rs.Add(map[m]);
			}
		}
		resultAtlasesAndRects.materials = ms.ToArray();
		resultAtlasesAndRects.prefabUVRects = rs.ToArray();
	}

	public static void ConfigureNewMaterialToMatchOld(Material newMat, Material original){
		if (original == null){
			Debug.LogWarning("Original material is null, could not copy properties to " + newMat + ". Setting shader to " + newMat.shader);
			return;
		}
		newMat.shader = original.shader;					
		newMat.CopyPropertiesFromMaterial(original);
		string[] texPropertyNames = MB_TextureCombiner.shaderTexPropertyNames;
		for (int j = 0; j < texPropertyNames.Length; j++){
			Vector2 scale = Vector2.one;
			Vector2 offset = Vector2.zero;
			if (newMat.HasProperty(texPropertyNames[j])){
				newMat.SetTextureOffset(texPropertyNames[j],offset);
				newMat.SetTextureScale(texPropertyNames[j],scale);
			}
		}
	}
	
	string PrintSet(HashSet<Material> s){
		StringBuilder sb = new StringBuilder();
		foreach(Material m in s){
			sb.Append( m + ",");
		}
		return sb.ToString();
	}	

	public bool _ValidateResultMaterials(){ 			
		HashSet<Material> allMatsOnObjs = new HashSet<Material>();
		for (int i = 0; i < objsToMesh.Count; i++){
			if (objsToMesh[i] != null){
				Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
				for (int j = 0; j < ms.Length; j++){
					if (ms[j] != null) allMatsOnObjs.Add(ms[j]);	
				}
			}
		}
		HashSet<Material> allMatsInMapping = new HashSet<Material>();
		for (int i = 0; i < resultMaterials.Length; i++){
			MB_MultiMaterial mm = resultMaterials[i];
			if (mm.combinedMaterial == null){
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return false;
			}
			Shader targShader = mm.combinedMaterial.shader;
			for (int j = 0; j < mm.sourceMaterials.Count; j++){
				if (mm.sourceMaterials[j] == null){
					Debug.LogError("There are null entries in the list of Source Materials");
					return false;
				}
				if (targShader != mm.sourceMaterials[j].shader){
					Debug.LogWarning("Source material " + mm.sourceMaterials[j] + " does not use shader " + targShader + " it may not have the required textures. If not empty textures will be generated.");	
				}
				if (allMatsInMapping.Contains(mm.sourceMaterials[j])){
					Debug.LogError("A Material " + mm.sourceMaterials[j] + " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique.");	
					return false;
				}
				allMatsInMapping.Add(mm.sourceMaterials[j]);
			}
		}
				
		if (allMatsOnObjs.IsProperSubsetOf(allMatsInMapping)){
			allMatsInMapping.ExceptWith(allMatsOnObjs);
			Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + PrintSet(allMatsInMapping));	
		}
		if (allMatsInMapping.IsProperSubsetOf(allMatsOnObjs)){
			allMatsOnObjs.ExceptWith(allMatsInMapping);
			Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + PrintSet(allMatsOnObjs));	
			return false;
		}		
		return true;
	}
}

