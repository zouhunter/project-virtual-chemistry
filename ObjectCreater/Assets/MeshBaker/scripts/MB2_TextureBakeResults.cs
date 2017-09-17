using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used internally during the material baking process
/// </summary>
[Serializable]
public class MB_AtlasesAndRects{
	public Texture2D[] atlases;
	public Dictionary<Material,Rect> mat2rect_map;
	public string[] texPropertyNames;
}

[System.Serializable]
public class MB_MultiMaterial{
	public Material combinedMaterial;
	public List<Material> sourceMaterials = new List<Material>();
}

/// <summary>
/// This class stores the results from an MB2_TextureBaker when materials are combined into atlases. It stores
/// a list of materials and the corresponding UV rectangles in the atlases. It also stores the configuration
/// options that were used to generate the combined material.
/// 
/// It can be saved as an asset in the project so that textures can be baked in one scene and used in another.
/// 
/// </summary>

public class MB2_TextureBakeResults : ScriptableObject {
	public MB_AtlasesAndRects[] combinedMaterialInfo;
	public Material[] materials;
	public Rect[] prefabUVRects;
	public Material resultMaterial;
	public MB_MultiMaterial[] resultMaterials;
	public bool doMultiMaterial;
	public bool fixOutOfBoundsUVs;
	
	public Dictionary<Material, Rect> GetMat2RectMap(){
		Dictionary<Material, Rect> mat2rect_map = new Dictionary<Material, Rect>();
		if (materials == null || prefabUVRects == null || materials.Length != prefabUVRects.Length){
			Debug.LogWarning("Bad TextureBakeResults could not build mat2UVRect map");
		} else {
			for (int i = 0; i < materials.Length; i++){
				mat2rect_map.Add(materials[i],prefabUVRects[i]);
			}
		}
		return mat2rect_map;		
	}
	
	public string GetDescription(){
		StringBuilder sb = new StringBuilder();
		sb.Append("Shaders:\n");
		HashSet<Shader> shaders = new HashSet<Shader>();
		if (materials != null){
			for (int i = 0; i < materials.Length; i++){
				shaders.Add(materials[i].shader);	
			}
		}
		
		foreach(Shader m in shaders){
			sb.Append("  ").Append(m.name).AppendLine();
		}
		sb.Append("Materials:\n");
		if (materials != null){
			for (int i = 0; i < materials.Length; i++){
				sb.Append("  ").Append(materials[i].name).AppendLine();
			}
		}
		return sb.ToString();
	}
}