using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DigitalOpus.MB.Core{

	public class MB3_EditorMethods : MB2_EditorMethodsInterface{

		private List<Texture2D> _texturesWithReadWriteFlagSet = new List<Texture2D>();
		private Dictionary<Texture2D,TextureFormatInfo> _textureFormatMap = new Dictionary<Texture2D, TextureFormatInfo>();	
		
		class TextureFormatInfo{	
			public TextureImporterFormat format;
			public bool isNormalMap;
			public String platform;
			public TextureImporterFormat platformOverrideFormat;
			
			public TextureFormatInfo(TextureImporterFormat f, string p, TextureImporterFormat pf, bool isNormMap){
				format = f;
				platform = p;
				platformOverrideFormat = pf;
				isNormalMap = isNormMap;
			}
		}
		
		public void Clear(){
			_texturesWithReadWriteFlagSet.Clear();
			_textureFormatMap.Clear();
		}
	
		public void SetReadFlags(ProgressUpdateDelegate progressInfo){
			for (int i = 0; i < _texturesWithReadWriteFlagSet.Count; i++){
				if (progressInfo != null) progressInfo("Restoring read flag for " + _texturesWithReadWriteFlagSet[i],.9f);
				SetReadWriteFlag(_texturesWithReadWriteFlagSet[i], false,false);
			}
			_texturesWithReadWriteFlagSet.Clear();		
			foreach (Texture2D tex in _textureFormatMap.Keys){
				if (progressInfo != null) progressInfo("Restoring format for " + tex,.9f);
				SetTextureFormat(tex, _textureFormatMap[tex],false,_textureFormatMap[tex].isNormalMap);
			}
			_textureFormatMap.Clear();		
		}
		
	
		public void SetReadWriteFlag(Texture2D tx, bool isReadable, bool addToList){
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is TextureImporter){
				TextureImporter textureImporter = (TextureImporter) ai;
				if (textureImporter.isReadable != isReadable){
					if (addToList) _texturesWithReadWriteFlagSet.Add(tx);
					textureImporter.isReadable = isReadable;	
	//				Debug.LogWarning("Setting read flag for Texture asset " + AssetDatabase.GetAssetPath(tx) + " to " + isReadable);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetOrScenePath(tx));
				}
			}
		}

		public bool IsNormalMap(Texture2D tx){
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is TextureImporter){
				if (((TextureImporter) ai).normalmap) return true;
			}
			return false;
		}
			
		public void AddTextureFormat(Texture2D tx,bool isNormalMap){
			//pixel values don't copy correctly from one texture to another when isNormal is set so unset it.
			SetTextureFormat(tx, 
							new TextureFormatInfo(TextureImporterFormat.ARGB32, GetPlatformString(), TextureImporterFormat.AutomaticTruecolor,isNormalMap), 
							true, false);		
		}
		
		void SetTextureFormat(Texture2D tx, TextureFormatInfo tfi, bool addToList, bool setNormalMap){	
			
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is UnityEditor.TextureImporter){
				string s;
				if (addToList){
					s = "Setting texture format for ";
				} else {
					s = "Restoring texture format for ";
				}
				s += tx + " to " + tfi.format;
				if (tfi.platform != null){
					s += " setting platform override format for " + tfi.platform + " to " + tfi.platformOverrideFormat;
				}
				Debug.Log(s);
				TextureImporter textureImporter = (TextureImporter) ai;
				TextureFormatInfo restoreTfi = new TextureFormatInfo(textureImporter.textureFormat, 
																	tfi.platform, 
																	TextureImporterFormat.AutomaticTruecolor, 
																	textureImporter.normalmap);
				string platform = tfi.platform;
				bool doImport = false;
				if (platform != null){
					int maxSize;
					TextureImporterFormat f;						
					textureImporter.GetPlatformTextureSettings(platform, out maxSize, out f);
					restoreTfi.platformOverrideFormat = f;
					if (f != 0){ //f == 0 means no override or platform doesn't exist
						textureImporter.SetPlatformTextureSettings(platform, maxSize, tfi.platformOverrideFormat);	
						doImport = true;
					}
				}
						
				if (textureImporter.textureFormat != tfi.format){
					textureImporter.textureFormat = tfi.format;
					doImport = true;
				}
				if (textureImporter.normalmap && !setNormalMap){
					textureImporter.normalmap = false;
					doImport = true;
				}
				if (!textureImporter.normalmap && setNormalMap){
					textureImporter.normalmap = true;
					doImport = true;					
				}
				if (addToList && !_textureFormatMap.ContainsKey(tx)) _textureFormatMap.Add(tx, restoreTfi);			
				if (doImport) AssetDatabase.ImportAsset(AssetDatabase.GetAssetOrScenePath(tx), ImportAssetOptions.ForceUpdate);
			}
		}
		
		/**
		 pass in System.IO.File.WriteAllBytes for parameter fileSaveFunction. This is necessary because on Web Player file saving
		 functions only exist for Editor classes
		 */	
		public void SaveAtlasToAssetDatabase(Texture2D atlas, string texPropertyName, int atlasNum, Material resMat){	
			string prefabPth = AssetDatabase.GetAssetPath(resMat);
			if (prefabPth == null || prefabPth.Length == 0){
				Debug.LogError("Could save atlas. Could not find result material in AssetDatabase.");
				return;
			}
			string baseName = Path.GetFileNameWithoutExtension(prefabPth);
			string folderPath = prefabPth.Substring(0,prefabPth.Length - baseName.Length - 4);		
			string fullFolderPath = Application.dataPath + folderPath.Substring("Assets".Length,folderPath.Length - "Assets".Length);
			
			string pth = fullFolderPath + baseName + "-" + texPropertyName + "-atlas" + atlasNum + ".png";
			Debug.Log("Created atlas for: " + texPropertyName + " at " + pth);
			//need to create a copy because sometimes the packed atlases are not in ARGB32 format
			Texture2D newTex = MB_Utility.createTextureCopy(atlas);
			int size = Mathf.Max(newTex.height,newTex.width);
			byte[] bytes = newTex.EncodeToPNG();
			Editor.DestroyImmediate(newTex);
			
			
			System.IO.File.WriteAllBytes(pth, bytes);
	
			AssetDatabase.Refresh();
			
			string relativePath = folderPath + baseName +"-" + texPropertyName + "-atlas" + atlasNum + ".png";                      				
			SetTextureSize((Texture2D) (AssetDatabase.LoadAssetAtPath(relativePath, typeof(Texture2D))),size);
			SetMaterialTextureProperty(resMat, texPropertyName, relativePath);
		
		}
		
		public void SetMaterialTextureProperty(Material target, string texPropName, string texturePath){
//			if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.Log(MB2_LogLevel.debug,"Assigning atlas " + texturePath + " to result material " + target + " for property " + texPropName,LOG_LEVEL);
			if (texPropName.Equals("_BumpMap")){
				SetNormalMap( (Texture2D) (AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D))));
			}
			if (target.HasProperty(texPropName)){
				target.SetTexture(texPropName, (Texture2D) (AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D))));
			}		
		}
	
		public void SetNormalMap(Texture2D tx){		
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is TextureImporter){
				TextureImporter textureImporter = (TextureImporter) ai;
				if (!textureImporter.normalmap){
					textureImporter.normalmap = true;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetOrScenePath(tx));
				}
			}	
		}
			
		public string GetPlatformString(){
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS){
				return "iPhone";	
			}
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android){
				return "Android";
			}
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ||
				EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
				EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 ||
				EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel){
				return "Standalone";	
			}
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer ||
				EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed){
				return "Web";
			}

			return null;
		}
		
		public void SetTextureSize(Texture2D tx, int size){		
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is UnityEditor.TextureImporter){
				TextureImporter textureImporter = (TextureImporter) ai;
				int maxSize = 32;
				if (size > 32)maxSize = 64;
				if (size > 64)maxSize = 128;
				if (size > 128)maxSize = 256;
				if (size > 256)maxSize = 512;
				if (size > 512)maxSize = 1024;
				if (size > 1024)maxSize = 2048;
				if (size > 2048)maxSize = 4096;
				textureImporter.maxTextureSize = maxSize;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetOrScenePath(tx), ImportAssetOptions.ForceUpdate);
			}
		}		

		public bool IsCompressed(Texture2D tx){		
			AssetImporter ai = AssetImporter.GetAtPath( AssetDatabase.GetAssetOrScenePath(tx) );
			if (ai != null && ai is TextureImporter){
				TextureImporter textureImporter = (TextureImporter) ai;
				TextureImporterFormat tf = textureImporter.textureFormat;
				if (tf !=  TextureImporterFormat.ARGB32){
					return true;	
				}
			}
			return false;
		}
		
		public int GetMaximumAtlasDimension(){
			int atlasMaxDimension = 4096;
			if (!Application.isPlaying){		
				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) atlasMaxDimension = 2048;
				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) atlasMaxDimension = 4096;
				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer) atlasMaxDimension = 2048;
				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed) atlasMaxDimension = 2048;
			} else {			
				if (Application.platform == RuntimePlatform.Android) atlasMaxDimension = 2048;
				if (Application.platform == RuntimePlatform.IPhonePlayer) atlasMaxDimension = 4096;
				if (Application.platform == RuntimePlatform.WindowsWebPlayer) atlasMaxDimension = 2048;
				if (Application.platform == RuntimePlatform.OSXWebPlayer) atlasMaxDimension = 2048;
			}
			return atlasMaxDimension;
		}

		public void CheckBuildSettings(long estimatedArea){
			if (Math.Sqrt(estimatedArea) > 1000f){
				if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone){
					Debug.LogWarning("If the current selected build target is not standalone then the generated atlases may be capped at size 1024. If build target is Standalone then atlases of 4096 can be built");	
				}
			}
		}
		
		public bool CheckPrefabTypes(MB_ObjsToCombineTypes objToCombineType, List<GameObject> objsToMesh){
			for (int i = 0; i < objsToMesh.Count; i++){
				UnityEditor.PrefabType pt = UnityEditor.PrefabUtility.GetPrefabType(objsToMesh[i]);
				if (pt == UnityEditor.PrefabType.None ||
					pt == UnityEditor.PrefabType.PrefabInstance || 
					pt == UnityEditor.PrefabType.ModelPrefabInstance || 
					pt == UnityEditor.PrefabType.DisconnectedPrefabInstance ||
					pt == UnityEditor.PrefabType.DisconnectedModelPrefabInstance){
					// these are scene objects
					if (objToCombineType == MB_ObjsToCombineTypes.prefabOnly){
						Debug.LogWarning("The list of objects to combine contains scene objects. You probably want prefabs. If using scene objects ensure position is zero, rotation is zero and scale is one. Translation, Rotation and Scale will be baked into the generated mesh." + objsToMesh[i] + " is a scene object");	
						return false;
					}
				} else if (objToCombineType == MB_ObjsToCombineTypes.sceneObjOnly){
					//these are prefabs
					Debug.LogWarning("The list of objects to combine contains prefab assets. You probably want scene objects." + objsToMesh[i] + " is a prefab object");
					return false;
				}
			}
			return true;
		}

		public bool ValidateSkinnedMeshes(List<GameObject> objs){
			for(int i = 0; i < objs.Count; i++){
				Renderer r = MB_Utility.GetRenderer(objs[i]);
				if (r is SkinnedMeshRenderer){
					Transform[] bones = ((SkinnedMeshRenderer)r).bones;
					if (bones.Length == 0){
						Debug.LogWarning("SkinnedMesh " + i + " (" + objs[i] + ") in the list of objects to combine has no bones. Check that 'optimize game object' is not checked in the 'Rig' tab of the asset importer. Mesh Baker cannot combine optimized skinned meshes because the bones are not available.");
					}
//					UnityEngine.Object parentObject = EditorUtility.GetPrefabParent(r.gameObject);
//					string path = AssetDatabase.GetAssetPath(parentObject);
//					Debug.Log (path);
//					AssetImporter ai = AssetImporter.GetAtPath( path );
//					Debug.Log ("bbb " + ai);
//					if (ai != null && ai is ModelImporter){
//						Debug.Log ("valing 2");
//						ModelImporter modelImporter = (ModelImporter) ai;
//						if(modelImporter.optimizeMesh){
//							Debug.LogError("SkinnedMesh " + i + " (" + objs[i] + ") in the list of objects to combine is optimized. Mesh Baker cannot combine optimized skinned meshes because the bones are not available.");
//						}
//					}
				}
			}
			return true;
		}
		
		public void Destroy(UnityEngine.Object o){
			if (Application.isPlaying){
				MonoBehaviour.Destroy(o);
			} else {
				string p = AssetDatabase.GetAssetPath(o);
				if (p != null && p.Equals("")) // don't try to destroy assets
					MonoBehaviour.DestroyImmediate(o,false);
			}
		}
	}
}