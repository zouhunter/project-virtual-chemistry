using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DigitalOpus.MB.Core{
	
	public delegate void ProgressUpdateDelegate(string msg, float progress);	
	
	public enum MB_ObjsToCombineTypes{
		prefabOnly,
		sceneObjOnly,
		dontCare
	}
	
	public enum MB_OutputOptions{
		bakeIntoPrefab,
		bakeMeshsInPlace,
		bakeTextureAtlasesOnly,
		bakeIntoSceneObject
	}
	
	public enum MB_RenderType{
		meshRenderer,
		skinnedMeshRenderer
	}
	
	public enum MB2_OutputOptions{
		bakeIntoSceneObject,
		bakeMeshAssetsInPlace,
		bakeIntoPrefab
	}
	
	public enum MB2_LightmapOptions{
		preserve_current_lightmapping,
		ignore_UV2,
		copy_UV2_unchanged,
		generate_new_UV2_layout
	}

	public enum MB2_PackingAlgorithmEnum{
		UnitysPackTextures,
		MeshBakerTexturePacker
	}
	
	public enum MB2_ValidationLevel{
		none,
		quick,
		robust
	}
	
	/// <summary>
	/// M b2_ texture combiner editor methods.
	/// Contains functionality such as changeing texture formats
	/// Which is only available in the editor. These methods have all been put in a
	/// class so that the UnityEditor namespace does not need to be included in any of the
	/// the runtime classes.
	/// </summary>
	public interface MB2_EditorMethodsInterface{
		void Clear();
		void SetReadFlags(ProgressUpdateDelegate progressInfo);
		void SetReadWriteFlag(Texture2D tx, bool isReadable, bool addToList);
		void AddTextureFormat(Texture2D tx, bool isNormalMap);	
		void SaveAtlasToAssetDatabase(Texture2D atlas, string texPropertyName, int atlasNum, Material resMat);
		void SetMaterialTextureProperty(Material target, string texPropName, string texturePath);
		void SetNormalMap(Texture2D tx);
		bool IsNormalMap(Texture2D tx);
		string GetPlatformString();
		void SetTextureSize(Texture2D tx, int size);
		bool IsCompressed(Texture2D tx);
		int GetMaximumAtlasDimension();
		void CheckBuildSettings(long estimatedAtlasSize);
		bool CheckPrefabTypes(MB_ObjsToCombineTypes prefabType, List<GameObject> gos);
		bool ValidateSkinnedMeshes(List<GameObject> mom);
		/*
		 * Needed because io.writeAllBytes does not exist in webplayer.
		 */
		void Destroy(UnityEngine.Object o);
	}	
}
