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
public class MB3_TextureBaker : MB3_MeshBakerRoot {	
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;
		
	[SerializeField] protected MB2_TextureBakeResults _textureBakeResults;
	public override MB2_TextureBakeResults textureBakeResults{
		get{return _textureBakeResults;}
		set{_textureBakeResults = value;}
	}

	[SerializeField] protected int _atlasPadding = 1;	
	public virtual int atlasPadding{
		get{return _atlasPadding;}
		set{_atlasPadding = value;}
	}
	
	[SerializeField] protected bool _resizePowerOfTwoTextures = false;
	public virtual bool resizePowerOfTwoTextures{
		get{return _resizePowerOfTwoTextures;}
		set{_resizePowerOfTwoTextures = value;}
	}
	
	[SerializeField] protected bool _fixOutOfBoundsUVs = false;
	public virtual bool fixOutOfBoundsUVs{
		get{return _fixOutOfBoundsUVs;}
		set{_fixOutOfBoundsUVs = value;}
	}
	
	[SerializeField] protected int _maxTilingBakeSize = 1024;
	public virtual int maxTilingBakeSize{
		get{return _maxTilingBakeSize;}
		set{_maxTilingBakeSize = value;}
	}
	
	[SerializeField] protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;
	public virtual MB2_PackingAlgorithmEnum packingAlgorithm{
		get{return _packingAlgorithm;}
		set{_packingAlgorithm = value;}
	}
	
	[SerializeField] protected List<string> _customShaderPropNames = new List<string>();		
	public virtual List<string> customShaderPropNames{
		get{return _customShaderPropNames;}
		set{_customShaderPropNames = value;}
	}
	
	[SerializeField] protected  bool _doMultiMaterial;
	public virtual bool doMultiMaterial{
		get{return _doMultiMaterial;}
		set{_doMultiMaterial = value;}
	}
	
	[SerializeField] protected  Material _resultMaterial;
	public virtual Material resultMaterial{
		get{return _resultMaterial;}
		set{_resultMaterial = value;}
	}	
	
	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];
	
	public List<GameObject> objsToMesh; //todo make this Renderer
	
	public override List<GameObject> GetObjectsToCombine(){
		if (objsToMesh == null) objsToMesh = new List<GameObject>();
		return objsToMesh;
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
	/// <param name='editorMethods'>
	/// Texture format tracker. Contains editor functionality such as save assets. Can be null.
	/// </param>
	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods=null){
		MB_AtlasesAndRects[] mAndAs = null;
		try{
			mAndAs = _CreateAtlases(progressInfo, saveAtlasesAsAssets, editorMethods);
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
									if (editorMethods != null) editorMethods.Destroy(mAndA.atlases[i]);
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
	
	MB_AtlasesAndRects[] _CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods=null){
		//validation
		if (saveAtlasesAsAssets && editorMethods == null){
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true then editorMethods cannot be null.");
			return null;
		}
		if (saveAtlasesAsAssets && !Application.isEditor){
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true it must be called from the Unity Editor.");
			return null;			
		}
		if (!DoCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, editorMethods)){
			return null;
		}
		if (_doMultiMaterial && !_ValidateResultMaterials()){
			return null;
		} else if (!_doMultiMaterial){
			if (_resultMaterial == null){
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return null;
			}
			Shader targShader = _resultMaterial.shader;
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
		
		MB3_TextureCombiner combiner = new MB3_TextureCombiner();
		combiner.LOG_LEVEL = LOG_LEVEL;
		combiner.atlasPadding = _atlasPadding;
		combiner.customShaderPropNames = _customShaderPropNames;
		combiner.fixOutOfBoundsUVs = _fixOutOfBoundsUVs;
		combiner.maxTilingBakeSize = _maxTilingBakeSize;
		combiner.packingAlgorithm = _packingAlgorithm;
		combiner.resizePowerOfTwoTextures = _resizePowerOfTwoTextures;
		combiner.saveAtlasesAsAssets = saveAtlasesAsAssets;

		// if editor analyse meshes and suggest treatment
		if (!Application.isPlaying){
			Material[] rms;
			if (_doMultiMaterial){
				rms = new Material[resultMaterials.Length];
				for (int i = 0; i < rms.Length; i++) rms[i] = resultMaterials[i].combinedMaterial;
			} else {
				rms = new Material[1];
				rms[0] = _resultMaterial;
			}
			combiner.SuggestTreatment(objsToMesh, rms, combiner.customShaderPropNames);
		}		
		
		//initialize structure to store results
		int numResults = 1;
		if (_doMultiMaterial) numResults = resultMaterials.Length;
		MB_AtlasesAndRects[] resultAtlasesAndRects = new MB_AtlasesAndRects[numResults];
		for (int i = 0; i < resultAtlasesAndRects.Length; i++){
			resultAtlasesAndRects[i] = new MB_AtlasesAndRects();
		}
		
		//Do the material combining.
		for (int i = 0; i < resultAtlasesAndRects.Length; i++){
			Material resMatToPass = null;
			List<Material> sourceMats = null;			
			if (_doMultiMaterial) {
				sourceMats = resultMaterials[i].sourceMaterials;
				resMatToPass = resultMaterials[i].combinedMaterial;
			} else {
				resMatToPass = _resultMaterial;	
			}
			Debug.Log("Creating atlases for result material " + resMatToPass);
			if(!combiner.CombineTexturesIntoAtlases(progressInfo, resultAtlasesAndRects[i], resMatToPass, objsToMesh, sourceMats, editorMethods)){
				return null;
			}
		}
		
		//Save the results
		textureBakeResults.combinedMaterialInfo = resultAtlasesAndRects;
		textureBakeResults.doMultiMaterial = _doMultiMaterial;
		textureBakeResults.resultMaterial = _resultMaterial;
		textureBakeResults.resultMaterials = resultMaterials;
		textureBakeResults.fixOutOfBoundsUVs = combiner.fixOutOfBoundsUVs;
		unpackMat2RectMap(textureBakeResults);
		
		//set the texture bake resultAtlasesAndRects on the Mesh Baker component if it exists
		MB3_MeshBakerCommon mb = GetComponent<MB3_MeshBakerCommon>();
		if (mb != null){
			mb.textureBakeResults = textureBakeResults;	
		}			
		
		if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("Created Atlases");		
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
		string[] texPropertyNames = MB3_TextureCombiner.shaderTexPropertyNames;
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

	bool _ValidateResultMaterials(){ 			
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

