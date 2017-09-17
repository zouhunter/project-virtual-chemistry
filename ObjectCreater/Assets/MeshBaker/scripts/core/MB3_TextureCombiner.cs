//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalOpus.MB.Core{	
	[System.Serializable]
	public class MB3_TextureCombiner{
		public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;
		
		public static string[] shaderTexPropertyNames = new string[] { "_MainTex", "_BumpMap", "_Normal", "_BumpSpecMap", "_DecalTex", "_Detail", "_GlossMap", "_Illum", "_LightTextureB0", "_ParallaxMap","_ShadowOffset", "_TranslucencyMap", "_SpecMap", "_TranspMap" };
		
		[SerializeField] protected MB2_TextureBakeResults _textureBakeResults;
		public MB2_TextureBakeResults textureBakeResults{
			get{return _textureBakeResults;}
			set{_textureBakeResults = value;}
		}
		
		[SerializeField] protected int _atlasPadding = 1;
		public int atlasPadding{
			get{return _atlasPadding;}
			set{_atlasPadding = value;}
		}
		
		[SerializeField] protected bool _resizePowerOfTwoTextures = false;
		public bool resizePowerOfTwoTextures{
			get{return _resizePowerOfTwoTextures;}
			set{_resizePowerOfTwoTextures = value;}
		}
		
		[SerializeField] protected bool _fixOutOfBoundsUVs = false;
		public bool fixOutOfBoundsUVs{
			get{return _fixOutOfBoundsUVs;}
			set{_fixOutOfBoundsUVs = value;}
		}
		
		[SerializeField] protected int _maxTilingBakeSize = 1024;
		public int maxTilingBakeSize{
			get{return _maxTilingBakeSize;}
			set{_maxTilingBakeSize = value;}
		}
		
		[SerializeField] protected bool _saveAtlasesAsAssets = false;
		public bool saveAtlasesAsAssets{
			get{return _saveAtlasesAsAssets;}
			set{_saveAtlasesAsAssets = value;}
		}
		
		[SerializeField] protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.UnitysPackTextures;
		public MB2_PackingAlgorithmEnum packingAlgorithm{
			get{return _packingAlgorithm;}
			set{_packingAlgorithm = value;}
		}
		
		[SerializeField] protected List<string> _customShaderPropNames = new List<string>();		
		public List<string> customShaderPropNames{
			get{return _customShaderPropNames;}
			set{_customShaderPropNames = value;}
		}
		
		protected List<Texture2D> _temporaryTextures = new List<Texture2D>();

		//Like a material but also stores its tiling info since the same texture
		//with different tiling needs to be baked to a separate spot in the atlas
		public class MeshBakerMaterialTexture{
			public Vector2 offset = new Vector2(0f,0f);
			public Vector2 scale = new Vector2(1f,1f);
			public Vector2 obUVoffset = new Vector2(0f,0f);
			public Vector2 obUVscale = new Vector2(1f,1f);
			public Texture2D t;
			public MeshBakerMaterialTexture(){}
			public MeshBakerMaterialTexture(Texture2D tx){ t = tx;	}
			public MeshBakerMaterialTexture(Texture2D tx, Vector2 o, Vector2 s, Vector2 oUV, Vector2 sUV){
				t = tx;
				offset = o;
				scale = s;
				obUVoffset = oUV;
				obUVscale = sUV;
			}
		}
		
		//a set of textures one for each "maintex","bump" that one or more materials use.
		class MB_TexSet{
			public MeshBakerMaterialTexture[] ts;
			public List<Material> mats;
			public List<GameObject> gos;
			public int idealWidth; //all textures will be resized to this size
			public int idealHeight;
			
			public MB_TexSet(MeshBakerMaterialTexture[] tss){
				ts = tss;
				mats = new List<Material>();
				gos = new List<GameObject>();
			}
			
			public bool IsEqual(object obj, bool fixOutOfBoundsUVs)
			{
				if (!(obj is MB_TexSet)){
					return false;
				}
				MB_TexSet other = (MB_TexSet) obj;
				if(other.ts.Length != ts.Length){ 
					return false;
				} else {
					for (int i = 0; i < ts.Length; i++){
						if (ts[i].offset != other.ts[i].offset)
							return false;
						if (ts[i].scale != other.ts[i].scale)
							return false;
						if (ts[i].t != other.ts[i].t)
							return false;
						if (fixOutOfBoundsUVs && ts[i].obUVoffset != other.ts[i].obUVoffset)
							return false;
						if (fixOutOfBoundsUVs && ts[i].obUVscale != other.ts[i].obUVscale)
							return false;					
					}
					return true;
				}
			}
		}	
			
		/**<summary>Combines meshes and generates texture atlases.</summary>
	    *  <param name="createTextureAtlases">Whether or not texture atlases should be created. If not uvs will not be adjusted.</param>
	    *  <param name="progressInfo">A delegate function that will be called to report progress.</param>
	    *  <param name="textureEditorMethods">If called from the editor should be an instance of MB2_EditorMethods. If called at runtime should be null.</param>
	    *  <remarks>Combines meshes and generates texture atlases</remarks> */		
		public bool CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods = null){
			return _CombineTexturesIntoAtlases(progressInfo,resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
		}		
		
		bool _CollectPropertyNames(Material resultMaterial, List<string> texPropertyNames){
			//try custom properties remove duplicates
			for (int i = 0; i < texPropertyNames.Count; i++){
				string s = _customShaderPropNames.Find(x => x.Equals(texPropertyNames[i]));
				if (s != null){
					_customShaderPropNames.Remove(s);
				}
			}
			
			Material m = resultMaterial;
			if (m == null){
				Debug.LogError("Please assign a result material. The combined mesh will use this material.");
				return false;			
			}
	
			//Collect the property names for the textures
			string shaderPropStr = "";
			for (int i = 0; i < shaderTexPropertyNames.Length; i++){
				if (m.HasProperty(shaderTexPropertyNames[i])){
					shaderPropStr += ", " + shaderTexPropertyNames[i];
					if (!texPropertyNames.Contains(shaderTexPropertyNames[i])) texPropertyNames.Add(shaderTexPropertyNames[i]);
					if (m.GetTextureOffset(shaderTexPropertyNames[i]) != new Vector2(0f,0f)){
						if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material has non-zero offset. This is may be incorrect.");	
					}
					if (m.GetTextureScale(shaderTexPropertyNames[i]) != new Vector2(1f,1f)){
						if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material should may be have tiling of 1,1");
					}					
				}
			}
	
			for (int i = 0; i < _customShaderPropNames.Count; i++){
				if (m.HasProperty(_customShaderPropNames[i]) ){
					shaderPropStr += ", " + _customShaderPropNames[i];
					texPropertyNames.Add(_customShaderPropNames[i]);
					if (m.GetTextureOffset(_customShaderPropNames[i]) != new Vector2(0f,0f)){
						if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material has non-zero offset. This is probably incorrect.");	
					}
					if (m.GetTextureScale(_customShaderPropNames[i]) != new Vector2(1f,1f)){
						if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material should probably have tiling of 1,1.");
					}					
				} else {
					if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material shader does not use property " + _customShaderPropNames[i] + " in the list of custom shader property names");	
				}
			}			
			
			return true;
		}
		
		bool _CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods){
			bool success = false;
			try{
				_temporaryTextures.Clear();
				
				if (textureEditorMethods != null) textureEditorMethods.Clear();

				if (objsToMesh == null || objsToMesh.Count == 0){
					Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
					return false;
				}
				if (_atlasPadding < 0){
					Debug.LogError("Atlas padding must be zero or greater.");
					return false;
				}
				if (_maxTilingBakeSize < 2 || _maxTilingBakeSize > 4096){
					Debug.LogError("Invalid value for max tiling bake size.");
					return false;			
				}
				
				if (progressInfo != null)
					progressInfo("Collecting textures for " + objsToMesh.Count + " meshes.", .01f);
				
				List<string> texPropertyNames = new List<string>();	
				if (!_CollectPropertyNames(resultMaterial, texPropertyNames)){
					return false;
				}
				success = __CombineTexturesIntoAtlases(progressInfo,resultAtlasesAndRects, resultMaterial, texPropertyNames, objsToMesh,allowedMaterialsFilter, textureEditorMethods);
			} catch (MissingReferenceException mrex){
				Debug.LogError("Creating atlases failed a MissingReferenceException was thrown. This is normally only happens when trying to create very large atlases and Unity is running out of Memory. Try changing the 'Texture Packer' to a different option, it may work with an alternate packer. This error is sometimes intermittant. Try baking again.");
				Debug.LogError(mrex);
			} catch (Exception ex){
				Debug.LogError(ex);
			} finally {
				_destroyTemporaryTextures();
				if (textureEditorMethods != null) textureEditorMethods.SetReadFlags(progressInfo);
			}
			return success;
		}
		
		
		//texPropertyNames is the list of texture properties in the resultMaterial
		//allowedMaterialsFilter is a list of materials. Objects without any of these materials will be ignored.
		//						 this is used by the multiple materials filter
		//textureEditorMethods encapsulates editor only functionality such as saving assets and tracking texture assets whos format was changed. Is null if using at runtime. 
		bool __CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<string> texPropertyNames, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods){
			if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("__CombineTexturesIntoAtlases atlases:" + texPropertyNames.Count + " objsToMesh:" + objsToMesh.Count + " _fixOutOfBoundsUVs:" + _fixOutOfBoundsUVs);
			
			if (progressInfo != null) progressInfo("Collecting textures ", .01f);
			/*
			each atlas (maintex, bump, spec etc...) will have distinctMaterialTextures.Count images in it.
			each distinctMaterialTextures record is a set of textures, one for each atlas. And a list of materials
			that use that distinct set of textures. 
			*/
			List<MB_TexSet> distinctMaterialTextures = new List<MB_TexSet>(); //one per distinct set of textures
			List<GameObject> usedObjsToMesh = new List<GameObject>();
			if (!__Step1_CollectDistinctMatTexturesAndUsedObjects(objsToMesh, allowedMaterialsFilter, texPropertyNames, textureEditorMethods, distinctMaterialTextures, usedObjsToMesh)){
				return false;	
			}
	
			//Textures in each material (_mainTex, Bump, Spec ect...) must be same size
			//Calculate the best sized to use. Takes into account tiling
			//if only one texture in atlas re-uses original sizes			
			int _padding = __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(distinctMaterialTextures);	
						
			__Step3_BuildAndSaveAtlasesAndStoreResults(progressInfo,distinctMaterialTextures,texPropertyNames,_padding,textureEditorMethods,resultAtlasesAndRects,resultMaterial);
			
			return true;
		}

		//Fills distinctMaterialTextures and usedObjsToMesh
		//If allowedMaterialsFilter is empty then all materials on allObjsToMesh will be collected and usedObjsToMesh will be same as allObjsToMesh
		//else only materials in allowedMaterialsFilter will be included and usedObjsToMesh will be objs that use those materials.
		bool __Step1_CollectDistinctMatTexturesAndUsedObjects(List<GameObject> allObjsToMesh, 
															 List<Material> allowedMaterialsFilter, 
															 List<string> texPropertyNames, 
															 MB2_EditorMethodsInterface textureEditorMethods, 
															 List<MB_TexSet> distinctMaterialTextures, //Will be populated
															 List<GameObject> usedObjsToMesh) //Will be populated, is a subset of allObjsToMesh
		{
			// Collect distinct list of textures to combine from the materials on objsToCombine
			bool outOfBoundsUVs = false;
			for (int i = 0; i < allObjsToMesh.Count; i++){
				GameObject obj = allObjsToMesh[i];
				if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Collecting textures for object " + obj);
				
				if (obj == null){
					Debug.LogError("The list of objects to mesh contained nulls.");
					return false;
				}
				
				Mesh sharedMesh = MB_Utility.GetMesh(obj);
				if (sharedMesh == null){
					Debug.LogError("Object " + obj.name + " in the list of objects to mesh has no mesh.");				
					return false;
				}
	
				Material[] sharedMaterials = MB_Utility.GetGOMaterials(obj);
				if (sharedMaterials == null){
					Debug.LogError("Object " + obj.name + " in the list of objects has no materials.");
					return false;				
				}
				
				for(int matIdx = 0; matIdx < sharedMaterials.Length; matIdx++){
					Material mat = sharedMaterials[matIdx];
					
					//check if this material is in the list of source materaials
					if (allowedMaterialsFilter != null && !allowedMaterialsFilter.Contains(mat)){
						continue;
					}
					
					Rect uvBounds = new Rect();
					bool mcOutOfBoundsUVs = MB_Utility.hasOutOfBoundsUVs(sharedMesh,ref uvBounds,matIdx);
					outOfBoundsUVs = outOfBoundsUVs || mcOutOfBoundsUVs;					
					
					if (mat.name.Contains("(Instance)")){
						Debug.LogError("The sharedMaterial on object " + obj.name + " has been 'Instanced'. This was probably caused by a script accessing the meshRender.material property in the editor. " +
							           " The material to UV Rectangle mapping will be incorrect. To fix this recreate the object from its prefab or re-assign its material from the correct asset.");	
						return false;
					}
					
					if (_fixOutOfBoundsUVs){
						if (!MB_Utility.validateOBuvsMultiMaterial(sharedMaterials)){
							if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + obj.name + " uses the same material on multiple submeshes. This may generate strange resultAtlasesAndRects especially when used with fix out of bounds uvs. Try duplicating the material.");		
						}
					}
										
					//collect textures scale and offset for each texture in objects material
					MeshBakerMaterialTexture[] mts = new MeshBakerMaterialTexture[texPropertyNames.Count];
					for (int j = 0; j < texPropertyNames.Count; j++){
						Texture2D tx = null;
						Vector2 scale = Vector2.one;
						Vector2 offset = Vector2.zero;
						Vector2 obUVscale = Vector2.one;
						Vector2 obUVoffset = Vector2.zero; 
						if (mat.HasProperty(texPropertyNames[j])){
							Texture txx = mat.GetTexture(texPropertyNames[j]);
							if (txx != null){
								if (txx is Texture2D){
									tx = (Texture2D) txx;
									TextureFormat f = tx.format;
									bool isNormalMap = false;
									if (!Application.isPlaying && textureEditorMethods != null) isNormalMap = textureEditorMethods.IsNormalMap(tx);
									if ((f == TextureFormat.ARGB32 ||
										f == TextureFormat.RGBA32 ||
										f == TextureFormat.BGRA32 ||
										f == TextureFormat.RGB24  ||
										f == TextureFormat.Alpha8) && !isNormalMap) //DXT5 does not work
									{
										//good
									} else {
										//TRIED to copy texture using tex2.SetPixels(tex1.GetPixels()) but bug in 3.5 means DTX1 and 5 compressed textures come out skewed
										//MB2_Log.Log(MB2_LogLevel.warn,obj.name + " in the list of objects to mesh uses Texture "+tx.name+" uses format " + f + " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These formats cannot be resized. MeshBaker will create duplicates.");
										//tx = createTextureCopy(tx);
										if (!Application.isPlaying){
											//Debug.LogWarning("Object " + obj.name + " in the list of objects to mesh uses Texture "+tx.name+" uses format " + f + " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'");													
											if (textureEditorMethods != null) textureEditorMethods.AddTextureFormat(tx, isNormalMap);
											tx = (Texture2D) mat.GetTexture(texPropertyNames[j]);
										} else {
											Debug.LogError("Object " + obj.name + " in the list of objects to mesh uses Texture "+tx.name+" uses format " + f + " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized at runtime. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'" );																						
											return false;
										}
									}
								} else {
									Debug.LogError("Object " + obj.name + " in the list of objects to mesh uses a Texture that is not a Texture2D. Cannot build atlases.");				
									return false;
								}
							}
							offset = mat.GetTextureOffset(texPropertyNames[j]);
							scale = mat.GetTextureScale(texPropertyNames[j]);
						}
						if (tx == null){
							if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("No texture selected for " + texPropertyNames[j] + " in object " + allObjsToMesh[i].name + ". A 2x2 clear texture will be generated and used in the atlas.");
						}
						if (mcOutOfBoundsUVs){
							obUVscale = new Vector2(uvBounds.width,uvBounds.height);
							obUVoffset = new Vector2(uvBounds.x,uvBounds.y);
						}
						mts[j] = new MeshBakerMaterialTexture(tx,offset,scale,obUVoffset,obUVscale);
					}
				
					//Add to distinct set of textures if not already there
					MB_TexSet setOfTexs = new MB_TexSet(mts);
					MB_TexSet setOfTexs2 = distinctMaterialTextures.Find(x => x.IsEqual(setOfTexs,_fixOutOfBoundsUVs));
					if (setOfTexs2 != null){
						setOfTexs = setOfTexs2;
					} else {
						distinctMaterialTextures.Add(setOfTexs);	
					}
					if (!setOfTexs.mats.Contains(mat)){
						setOfTexs.mats.Add(mat);
					}
					if (!setOfTexs.gos.Contains(obj)){
						setOfTexs.gos.Add(obj);
						if (!usedObjsToMesh.Contains(obj)) usedObjsToMesh.Add(obj);
					}
				}
			}
			
			return true;
		}
		
		int __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(List<MB_TexSet> distinctMaterialTextures){
			int _padding = _atlasPadding;
			if (distinctMaterialTextures.Count == 1){
				if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("All objects use the same textures in this set of atlases. Original textures will be reused instead of creating atlases.");
				_padding = 0;
			} else {
				for(int i = 0; i < distinctMaterialTextures.Count; i++){
					if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Calculating ideal sizes for texSet TexSet " + i + " of " + distinctMaterialTextures.Count);
					MB_TexSet txs = distinctMaterialTextures[i];
					txs.idealWidth = 1;
					txs.idealHeight = 1;
					int tWidth = 1;
					int tHeight = 1;			
					//get the best size all textures in a TexSet must be the same size.
					for (int j = 0; j < txs.ts.Length; j++){
						MeshBakerMaterialTexture matTex = txs.ts[j];
						if (!matTex.scale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1){
							if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Texture " + matTex.t + "is tiled by " + matTex.scale + " tiling will be baked into a texture with maxSize:" + _maxTilingBakeSize);
						}
						if (!matTex.obUVscale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1 && _fixOutOfBoundsUVs){
							if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Texture " + matTex.t + "has out of bounds UVs that effectively tile by " + matTex.obUVscale + " tiling will be baked into a texture with maxSize:" + _maxTilingBakeSize);
						}	
						if (matTex.t != null){
							Vector2 dim = GetAdjustedForScaleAndOffset2Dimensions(matTex);						
							if ((int)(dim.x * dim.y) > tWidth * tHeight){
								if (LOG_LEVEL >= MB2_LogLevel.trace)  Debug.Log("    matTex " + matTex.t + " " + dim + " has a bigger size than " + tWidth + " " + tHeight);
								tWidth = (int) dim.x;
								tHeight = (int) dim.y;
							}
						}
					}
					if (_resizePowerOfTwoTextures){
						if (IsPowerOfTwo(tWidth)){
							tWidth -= _padding * 2; 
						}
						if (IsPowerOfTwo(tHeight)){
							tHeight -= _padding * 2; 
						}
						if (tWidth < 1) tWidth = 1;
						if (tHeight < 1) tHeight = 1;
					}
					if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("    Ideal size is " + tWidth + " " + tHeight);
					txs.idealWidth = tWidth;
					txs.idealHeight = tHeight;
				}
			}
			return _padding;
		}
		
		void __Step3_BuildAndSaveAtlasesAndStoreResults(ProgressUpdateDelegate progressInfo, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, int _padding, MB2_EditorMethodsInterface textureEditorMethods, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial){
			int numAtlases = texPropertyNames.Count;
			//generate report want to do this before
			//todo if atlas is compressed then doesn't report correct compression 
			StringBuilder report = new StringBuilder();
			if (numAtlases > 0){
				report = new StringBuilder();
				report.AppendLine("Report");
				for (int i = 0; i < distinctMaterialTextures.Count; i++){
					MB_TexSet txs = distinctMaterialTextures[i];
					report.AppendLine("----------");
					report.Append("This set of textures will be resized to:" + txs.idealWidth + "x" + txs.idealHeight + "\n");
					for (int j = 0; j < txs.ts.Length; j++){
						if (txs.ts[j].t != null){
							report.Append("   [" + texPropertyNames[j] + " " + txs.ts[j].t.name + " " + txs.ts[j].t.width + "x" + txs.ts[j].t.height + "]");
							if (txs.ts[j].scale != Vector2.one || txs.ts[j].offset != Vector2.zero) report.AppendFormat(" material scale {0} offset{1} ", txs.ts[j].scale.ToString("G4"), txs.ts[j].offset.ToString("G4"));
							if (txs.ts[j].obUVscale != Vector2.one || txs.ts[j].obUVoffset != Vector2.zero) report.AppendFormat(" obUV scale {0} offset{1} ", txs.ts[j].obUVscale.ToString("G4"), txs.ts[j].obUVoffset.ToString("G4"));
							report.AppendLine("");
						} else { 
							report.Append("   [" + texPropertyNames[j] + " null a blank texture will be created]\n");
						}
					}
					report.AppendLine("");
					report.Append("Materials using:");
					for (int j = 0; j < txs.mats.Count; j++){
						report.Append(txs.mats[j].name + ", ");
					}
					report.AppendLine("");
				}
			}		
	
			if (progressInfo != null) progressInfo("Creating txture atlases.", .1f);

			//run the garbage collector to free up as much memory as possible before bake to reduce MissingReferenceException problems
			GC.Collect();
			Texture2D[] atlases = new Texture2D[numAtlases];			
			Rect[] uvRects;
			if (_packingAlgorithm == MB2_PackingAlgorithmEnum.UnitysPackTextures){
				uvRects = __CreateAtlasesUnityTexturePacker(progressInfo, numAtlases, distinctMaterialTextures, texPropertyNames, resultMaterial, atlases, textureEditorMethods, _padding);
			} else {
				uvRects = __CreateAtlasesMBTexturePacker(progressInfo, numAtlases, distinctMaterialTextures, texPropertyNames, resultMaterial, atlases, textureEditorMethods, _padding);
			}
			if (progressInfo != null) progressInfo("Building Report",.7f);
			
			//report on atlases created
			StringBuilder atlasMessage = new StringBuilder();
			atlasMessage.AppendLine("---- Atlases ------");
			for (int i = 0; i < numAtlases; i++){
				if (atlases[i] != null) atlasMessage.AppendLine("Created Atlas For: " + texPropertyNames[i] + " h=" + atlases[i].height + " w=" + atlases[i].width);
			}
			report.Append(atlasMessage.ToString());
			
			
			Dictionary<Material,Rect> mat2rect_map = new Dictionary<Material, Rect>();
			for (int i = 0; i < distinctMaterialTextures.Count; i++){
				List<Material> mats = distinctMaterialTextures[i].mats;
				for (int j = 0; j < mats.Count; j++){
					if (!mat2rect_map.ContainsKey(mats[j])){
						mat2rect_map.Add(mats[j],uvRects[i]);
					}
				}
			}
			
			resultAtlasesAndRects.atlases = atlases;                             // one per texture on source shader
			resultAtlasesAndRects.texPropertyNames = texPropertyNames.ToArray(); // one per texture on source shader
			resultAtlasesAndRects.mat2rect_map = mat2rect_map;
			
			if (progressInfo != null) progressInfo("Restoring Texture Formats & Read Flags",.8f);
			_destroyTemporaryTextures();
			if (textureEditorMethods != null) textureEditorMethods.SetReadFlags(progressInfo);
			if (report != null && LOG_LEVEL >= MB2_LogLevel.info) Debug.Log(report.ToString());		
		}
		
		Rect[] __CreateAtlasesMBTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding){
			Rect[] uvRects;
			if (distinctMaterialTextures.Count == 1){
				if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
				uvRects = new Rect[1];
				uvRects[0] = new Rect(0f,0f,1f,1f);
				for (int i = 0; i < numAtlases; i++){
					MeshBakerMaterialTexture dmt = distinctMaterialTextures[0].ts[i];
					atlases[i] = dmt.t;
					resultMaterial.SetTexture(texPropertyNames[i],atlases[i]);
					resultMaterial.SetTextureScale(texPropertyNames[i],dmt.scale);
					resultMaterial.SetTextureOffset(texPropertyNames[i],dmt.offset);
				}
			} else {
				List<Vector2> imageSizes = new List<Vector2>();
				for (int i = 0; i < distinctMaterialTextures.Count; i++){
					imageSizes.Add(new Vector2(distinctMaterialTextures[i].idealWidth, distinctMaterialTextures[i].idealHeight));	
				}
				MB2_TexturePacker tp = new MB2_TexturePacker();
				int atlasSizeX = 1;
				int atlasSizeY = 1;
				
				//todo add sanity warnings for huge atlasesr
				int atlasMaxDimension = 4096;
				
				if (textureEditorMethods != null) atlasMaxDimension = textureEditorMethods.GetMaximumAtlasDimension();
				
				uvRects = tp.GetRects(imageSizes,atlasMaxDimension,_padding,out atlasSizeX, out atlasSizeY);
				if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("Generated atlas will be " + atlasSizeX + "x" + atlasSizeY + " (Max atlas size for platform: " + atlasMaxDimension + ")");
				for (int i = 0; i < numAtlases; i++){
					GC.Collect();
					if (progressInfo != null) progressInfo("Creating Atlas '" + texPropertyNames[i] + "'", .01f);
					Color[] atlasPixels = new Color[atlasSizeX * atlasSizeY];//atlas.GetPixels();
					bool isNormalMap = false;
					if (texPropertyNames[i].Equals("_Normal") || texPropertyNames[i].Equals("_BumpMap")) isNormalMap = true;
					for (int j = 0; j < atlasPixels.Length; j++) { 
						if (isNormalMap){
							atlasPixels[j] = new Color(.5f,.5f,1f); //neutral bluish for normal maps
						} else {
							atlasPixels[j] = Color.clear;	
						}
					}
					for (int j = 0; j < distinctMaterialTextures.Count; j++){
						if (LOG_LEVEL >= MB2_LogLevel.trace) MB2_Log.Trace("Adding texture {0} to atlas {1}", distinctMaterialTextures[j].ts[i].t == null ? "null" : distinctMaterialTextures[j].ts[i].t.ToString(),texPropertyNames[i]);
						Rect r = uvRects[j];
						Texture2D t = distinctMaterialTextures[j].ts[i].t;
						int x = Mathf.RoundToInt(r.x * atlasSizeX);
						int y = Mathf.RoundToInt(r.y * atlasSizeY);
						int ww = Mathf.RoundToInt(r.width * atlasSizeX);
						int hh = Mathf.RoundToInt(r.height * atlasSizeY);
						if (ww == 0 || hh == 0) Debug.LogError("Image in atlas has no height or width");
						if (textureEditorMethods != null) textureEditorMethods.SetReadWriteFlag(t, true, true);
						if (progressInfo != null) progressInfo("Copying to atlas: '" + distinctMaterialTextures[j].ts[i].t + "'", .02f);
						CopyScaledAndTiledToAtlas(distinctMaterialTextures[j].ts[i],x,y,ww,hh,_fixOutOfBoundsUVs,_maxTilingBakeSize,atlasPixels,atlasSizeX,isNormalMap,progressInfo);
					}
					if (progressInfo != null) progressInfo("Applying changes to atlas: '" + texPropertyNames[i] + "'", .03f);
					Texture2D atlas = new Texture2D(atlasSizeX, atlasSizeY,TextureFormat.ARGB32, true);
					atlas.SetPixels(atlasPixels);
					atlas.Apply();
					if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Saving atlas " + texPropertyNames[i] + " w=" + atlas.width + " h=" + atlas.height);
					atlases[i] = atlas;
					if (progressInfo != null) progressInfo("Saving atlas: '" + texPropertyNames[i] + "'", .04f);
					if (_saveAtlasesAsAssets && textureEditorMethods != null){
						textureEditorMethods.SaveAtlasToAssetDatabase(atlases[i], texPropertyNames[i], i, resultMaterial);
					} else {
						resultMaterial.SetTexture(texPropertyNames[i], atlases[i]);
					}
					resultMaterial.SetTextureOffset(texPropertyNames[i], Vector2.zero);
					resultMaterial.SetTextureScale(texPropertyNames[i],Vector2.one);
					_destroyTemporaryTextures(); // need to save atlases before doing this				
				}
			}
			return uvRects;
		}

		
		
		Rect[] __CreateAtlasesUnityTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding){
			Rect[] uvRects;
			if (distinctMaterialTextures.Count == 1){
				if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
				uvRects = new Rect[1];
				uvRects[0] = new Rect(0f,0f,1f,1f);
				for (int i = 0; i < numAtlases; i++){
					MeshBakerMaterialTexture dmt = distinctMaterialTextures[0].ts[i];
					atlases[i] = dmt.t;
					resultMaterial.SetTexture(texPropertyNames[i],atlases[i]);
					resultMaterial.SetTextureScale(texPropertyNames[i],dmt.scale);
					resultMaterial.SetTextureOffset(texPropertyNames[i],dmt.offset);
				}
			} else {
				long estArea = 0;
				int atlasSizeX = 1;
				int atlasSizeY = 1;
				uvRects = null;
				for (int i = 0; i < numAtlases; i++){ //i is an atlas "MainTex", "BumpMap" etc...
					if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("Beginning loop " + i + " num temporary textures " + _temporaryTextures.Count);
					for(int j = 0; j < distinctMaterialTextures.Count; j++){ //j is a distinct set of textures one for each of "MainTex", "BumpMap" etc...
						MB_TexSet txs = distinctMaterialTextures[j];
						
						int tWidth = txs.idealWidth;
						int tHeight = txs.idealHeight;
		
						Texture2D tx = txs.ts[i].t;
						if (tx == null) tx = txs.ts[i].t = _createTemporaryTexture(tWidth,tHeight,TextureFormat.ARGB32, true);
		
						if (progressInfo != null)
							progressInfo("Adjusting for scale and offset " + tx, .01f);	
						if (textureEditorMethods != null) textureEditorMethods.SetReadWriteFlag(tx, true, true); 
						tx = GetAdjustedForScaleAndOffset2(txs.ts[i]);				
						
						//create a resized copy if necessary
						if (tx.width != tWidth || tx.height != tHeight) {
							if (progressInfo != null) progressInfo("Resizing texture '" + tx + "'", .01f);
							if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("Copying and resizing texture " + texPropertyNames[i] + " from " + tx.width + "x" + tx.height + " to " + tWidth + "x" + tHeight);
							if (textureEditorMethods != null) textureEditorMethods.SetReadWriteFlag((Texture2D) tx, true, true);
							tx = _resizeTexture((Texture2D) tx,tWidth,tHeight);
						}
						txs.ts[i].t = tx;
					}
		
					Texture2D[] texToPack = new Texture2D[distinctMaterialTextures.Count];
					for (int j = 0; j < distinctMaterialTextures.Count;j++){
						Texture2D tx = distinctMaterialTextures[j].ts[i].t;
						estArea += tx.width * tx.height;
						texToPack[j] = tx;
					}
					
					if (textureEditorMethods != null) textureEditorMethods.CheckBuildSettings(estArea);
			
					if (Math.Sqrt(estArea) > 3500f){
						if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("The maximum possible atlas size is 4096. Textures may be shrunk");
					}
					atlases[i] = new Texture2D(1,1,TextureFormat.ARGB32,true);
					if (progressInfo != null)
						progressInfo("Packing texture atlas " + texPropertyNames[i], .25f);	
					if (i == 0){
						if (progressInfo != null)
							progressInfo("Estimated min size of atlases: " + Math.Sqrt(estArea).ToString("F0"), .1f);			
						if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("Estimated atlas minimum size:" + Math.Sqrt(estArea).ToString("F0"));
						
						_addWatermark(texToPack);			
						
						if (distinctMaterialTextures.Count == 1){ //don't want to force power of 2 so tiling will still work
							uvRects = new Rect[1] {new Rect(0f,0f,1f,1f)};
							atlases[i] = _copyTexturesIntoAtlas(texToPack,_padding,uvRects,texToPack[0].width,texToPack[0].height);
						} else {
							int maxAtlasSize = 4096;
							uvRects = atlases[i].PackTextures(texToPack,_padding,maxAtlasSize,false);
						}
						
						if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("After pack textures atlas size " + atlases[i].width + " " + atlases[i].height);
						atlasSizeX = atlases[i].width;
						atlasSizeY = atlases[i].height;	
						atlases[i].Apply();
					} else {
						if (progressInfo != null)
							progressInfo("Copying Textures Into: " + texPropertyNames[i], .1f);					
						atlases[i] = _copyTexturesIntoAtlas(texToPack,_padding,uvRects, atlasSizeX, atlasSizeY);
					}
					
					if (_saveAtlasesAsAssets && textureEditorMethods != null){
						textureEditorMethods.SaveAtlasToAssetDatabase(atlases[i], texPropertyNames[i], i, resultMaterial);
					} else {
						resultMaterial.SetTexture(texPropertyNames[i], atlases[i]);
					}
					resultMaterial.SetTextureOffset(texPropertyNames[i], Vector2.zero);
					resultMaterial.SetTextureScale(texPropertyNames[i],Vector2.one);
					
					_destroyTemporaryTextures(); // need to save atlases before doing this
					GC.Collect();
				}
			}
			return uvRects;
		}	
		
		void _addWatermark(Texture2D[] texToPack){
//			if (MB2_MeshCombiner.EVAL_VERSION){
//				MB2_Watermark.AddWatermarkTo(texToPack);
//			}			
		}

		Texture2D _addWatermark(Texture2D texToPack){
//			if (MB2_MeshCombiner.EVAL_VERSION){
//				MB2_Watermark.AddWatermarkTo(texToPack);
//			}
			return texToPack;
		}		
		
		Texture2D _copyTexturesIntoAtlas(Texture2D[] texToPack,int padding, Rect[] rs, int w, int h){
			Texture2D ta = new Texture2D(w,h,TextureFormat.ARGB32,true);
			MB_Utility.setSolidColor(ta,Color.clear);
			for (int i = 0; i < rs.Length; i++){
				Rect r = rs[i];
				Texture2D t = texToPack[i];
				int x = Mathf.RoundToInt(r.x * w);
				int y = Mathf.RoundToInt(r.y * h);
				int ww = Mathf.RoundToInt(r.width * w);
				int hh = Mathf.RoundToInt(r.height * h);
				if (t.width != ww && t.height != hh){
					t = MB_Utility.resampleTexture(t,ww,hh);
					_temporaryTextures.Add(t);	
				}
				ta.SetPixels(x,y,ww,hh,t.GetPixels());
			}
			ta.Apply();
			return ta;
		}
		
		bool IsPowerOfTwo(int x){
	    	return (x & (x - 1)) == 0;
		}	
		
		Vector2 GetAdjustedForScaleAndOffset2Dimensions(MeshBakerMaterialTexture source){
			if (source.offset.x == 0f && source.offset.y == 0f && source.scale.x == 1f && source.scale.y == 1f){
				if (_fixOutOfBoundsUVs){
					if (source.obUVoffset.x == 0f && source.obUVoffset.y == 0f && source.obUVscale.x == 1f && source.obUVscale.y == 1f){
						return new Vector2(source.t.width,source.t.height); //no adjustment necessary
					}
				} else {
					return new Vector2(source.t.width,source.t.height); //no adjustment necessary
				}
			}
	
			if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("GetAdjustedForScaleAndOffset2Dimensions: " + source.t + " " + source.obUVoffset + " " + source.obUVscale);
			float newWidth = source.t.width * source.scale.x;
			float newHeight = source.t.height * source.scale.y;
			if (_fixOutOfBoundsUVs){
				newWidth *= source.obUVscale.x;	 
				newHeight *= source.obUVscale.y;
			}
			if (newWidth > _maxTilingBakeSize) newWidth = _maxTilingBakeSize;
			if (newHeight > _maxTilingBakeSize) newHeight = _maxTilingBakeSize;
			if (newWidth < 1f) newWidth = 1f;
			if (newHeight < 1f) newHeight = 1f;	
			return new Vector2(newWidth,newHeight);
		}
		
		public Texture2D GetAdjustedForScaleAndOffset2(MeshBakerMaterialTexture source){
			if (source.offset.x == 0f && source.offset.y == 0f && source.scale.x == 1f && source.scale.y == 1f){
				if (_fixOutOfBoundsUVs){
					if (source.obUVoffset.x == 0f && source.obUVoffset.y == 0f && source.obUVscale.x == 1f && source.obUVscale.y == 1f){
						return source.t; //no adjustment necessary
					}
				} else {
					return source.t; //no adjustment necessary
				}
			}
			Vector2 dim = GetAdjustedForScaleAndOffset2Dimensions(source);
			
			if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("GetAdjustedForScaleAndOffset2: " + source.t + " " + source.obUVoffset + " " + source.obUVscale);
			float newWidth = dim.x;
			float newHeight = dim.y;
			float scx = source.scale.x;
			float scy = source.scale.y;
			float ox = source.offset.x;
			float oy = source.offset.y;
			if (_fixOutOfBoundsUVs){
				scx *= source.obUVscale.x;
				scy *= source.obUVscale.y;
				ox += source.obUVoffset.x;
				oy += source.obUVoffset.y;
			}
			Texture2D newTex = _createTemporaryTexture((int)newWidth,(int)newHeight,TextureFormat.ARGB32,true);
			for (int i = 0;i < newTex.width; i++){
				for (int j = 0;j < newTex.height; j++){
					float u = i/newWidth*scx + ox;
					float v = j/newHeight*scy + oy;
					newTex.SetPixel(i,j,source.t.GetPixelBilinear(u,v));
				}			
			}
			newTex.Apply();
			return newTex;
		}	

		public void CopyScaledAndTiledToAtlas(MeshBakerMaterialTexture source, int targX, int targY, int targW, int targH, bool _fixOutOfBoundsUVs, int maxSize, Color[] atlasPixels, int atlasWidth, bool isNormalMap, ProgressUpdateDelegate progressInfo=null){			
			if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("CopyScaledAndTiledToAtlas: " + source.t + " inAtlasX=" + targX + " inAtlasY=" + targY + " inAtlasW=" + targW + " inAtlasH=" + targH);
			float newWidth = targW;
			float newHeight = targH;
			float scx = source.scale.x;
			float scy = source.scale.y;
			float ox = source.offset.x;
			float oy = source.offset.y;
			if (_fixOutOfBoundsUVs){
				scx *= source.obUVscale.x;
				scy *= source.obUVscale.y;
				ox += source.obUVoffset.x;
				oy += source.obUVoffset.y;
			}
			int w = (int) newWidth;
			int h = (int) newHeight;
			Texture2D t = source.t;
			if (t == null){
				t = _createTemporaryTexture(2,2,TextureFormat.ARGB32, true);
				if (isNormalMap){
					MB_Utility.setSolidColor(t,new Color(.5f,.5f,1f));
				} else {
					MB_Utility.setSolidColor(t,Color.clear);
				}
			}
			t = _addWatermark(t);
			for (int i = 0;i < w; i++){
				if (progressInfo != null && w > 0) progressInfo("CopyScaledAndTiledToAtlas " + (((float)i/(float)w)*100f).ToString("F0"),.2f);
				for (int j = 0;j < h; j++){
					float u = i/newWidth*scx + ox;
					float v = j/newHeight*scy + oy;
					atlasPixels[(targY + j) * atlasWidth + targX + i] = t.GetPixelBilinear(u,v);
				}			
			}
		}		
		
		//used to track temporary textures that were created so they can be destroyed
		Texture2D _createTemporaryTexture(int w, int h, TextureFormat texFormat, bool mipMaps){
			Texture2D t = new Texture2D(w,h,texFormat,mipMaps);
			MB_Utility.setSolidColor(t,Color.clear);
			_temporaryTextures.Add(t);
			return t;
		}
		
		Texture2D _createTextureCopy(Texture2D t){
			Texture2D tx = MB_Utility.createTextureCopy(t);
			_temporaryTextures.Add(tx);
			return tx;	
		}
						
		Texture2D _resizeTexture(Texture2D t, int w, int h){
			Texture2D tx = MB_Utility.resampleTexture(t,w,h);
			_temporaryTextures.Add(tx);
			return tx;							
		}
		
		void _destroyTemporaryTextures(){
			if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Destroying " + _temporaryTextures.Count + " temporary textures");
			for (int i = 0; i < _temporaryTextures.Count; i++){
				MB_Utility.Destroy( _temporaryTextures[i] );
			}
			_temporaryTextures.Clear();
		}		

		public void SuggestTreatment(List<GameObject> objsToMesh, Material[] resultMaterials, List<string> _customShaderPropNames){
			this._customShaderPropNames = _customShaderPropNames;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < objsToMesh.Count; i++){
				GameObject obj = objsToMesh[i];
				if (obj == null) continue;
				Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
				if (ms.Length > 1){ // and each material is not mapped to its own layer
					sb.AppendFormat("\nObject {0} uses {1} materials. Possible treatments:\n", objsToMesh[i].name, ms.Length);
					sb.AppendFormat("  1) Collapse the submeshes together into one submesh in the combined mesh. Each of the original submesh materials will map to a different UV rectangle in the atlas(es) used by the combined material.\n");
					sb.AppendFormat("  2) Use the multiple materials feature to map submeshes in the source mesh to submeshes in the combined mesh.\n");
				}
				Mesh m = MB_Utility.GetMesh(obj);
				Rect r = new Rect();
				for (int j = 0; j < ms.Length; j++){
					if (MB_Utility.hasOutOfBoundsUVs(m, ref r,j)){
						sb.AppendFormat("\nObject {0} submesh={1} material={2} uses UVs outside the range 0,0 .. 1,1 to create tiling that tiles the box {3},{4} .. {5},{6}. This is a problem because the UVs outside the 0,0 .. 1,1 " + 
										"rectangle will pick up neighboring textures in the atlas. Possible Treatments:\n",obj,j,ms[j],r.x.ToString("G4"),r.y.ToString("G4"),(r.x+r.width).ToString("G4"),(r.y+r.height).ToString("G4"));
						sb.AppendFormat("    1) Ignore the problem. The tiling may not affect result significantly.\n");
						sb.AppendFormat("    2) Use the 'fix out of bounds UVs' feature to bake the tiling and scale the UVs to fit in the 0,0 .. 1,1 rectangle.\n");
						sb.AppendFormat("    3) Use the Multiple Materials feature to map the material on this submesh to its own submesh in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
						sb.AppendFormat("    4) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
					}
				}
				if (MB_Utility.doSubmeshesShareVertsOrTris(m) != 0){
					//todo be specific about which submeshes overlap
					sb.AppendFormat("\nObject {0} has submeshes that share vertices. This is a problem because each vertex can have only one UV coordinate and may be required to map to different positions in the various atlases that are generated. Possible treatments:\n", objsToMesh[i]); 
					sb.AppendFormat(" 1) Ignore the problem. The vertices may not affect the result.\n");
					sb.AppendFormat(" 2) Use the Multiple Materials feature to map the submeshs that overlap to their own submeshs in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
					sb.AppendFormat(" 3) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
				}
			}
			Dictionary<Material,List<GameObject>> m2gos = new Dictionary<Material, List<GameObject>>();
			for (int i = 0; i < objsToMesh.Count; i++){
				if (objsToMesh[i] != null){
					Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
					for (int j = 0; j < ms.Length; j++){
						if (ms[j] != null){
							List<GameObject> lgo;
							if (!m2gos.TryGetValue(ms[j],out lgo)){
								lgo = new List<GameObject>();
								m2gos.Add(ms[j],lgo);
							}
							if (!lgo.Contains(objsToMesh[i])) lgo.Add(objsToMesh[i]);
						}
					}
				}
			}
			
			List<string> texPropertyNames = new List<string>();
			for (int i = 0; i < resultMaterials.Length; i++){
				_CollectPropertyNames(resultMaterials[i], texPropertyNames);
				foreach(Material m in m2gos.Keys){
					for (int j = 0; j < texPropertyNames.Count; j++){
//						Texture2D tx = null;
//						Vector2 scale = Vector2.one;
//						Vector2 offset = Vector2.zero;
//						Vector2 obUVscale = Vector2.one;
//						Vector2 obUVoffset = Vector2.zero; 
						if (m.HasProperty(texPropertyNames[j])){
							Texture txx = m.GetTexture(texPropertyNames[j]);
							if (txx != null){
								Vector2 o = m.GetTextureOffset(texPropertyNames[j]);
								Vector3 s = m.GetTextureScale(texPropertyNames[j]);
								if (o.x < 0f || o.x + s.x > 1f ||
									o.y < 0f || o.y + s.y > 1f){
									sb.AppendFormat("\nMaterial {0} used by objects {1} uses texture {2} that is tiled (scale={3} offset={4}). If there is more than one texture in the atlas " +
														" then Mesh Baker will bake the tiling into the atlas. If the baked tiling is large then quality can be lost. Possible treatments:\n",m,PrintList(m2gos[m]),txx,s,o);
									sb.AppendFormat("  1) Use the baked tiling.\n");
									sb.AppendFormat("  2) Use the Multiple Materials feature to map the material on this object/submesh to its own submesh in the combined mesh. No other materials should map to this submesh. The original material can be applied to this submesh.\n");
									sb.AppendFormat("  3) Combine only meshes that use the same (or subset of) the set of textures on this mesh. The original material can be applied to the result.\n");
								}
							}
						}
					}
				}
			}
			string outstr = "";
			if (sb.Length == 0){
				outstr = "====== No problems detected. These meshes should combine well ====\n  If there are problems with the combined meshes please report the problem to digitalOpus.ca so we can improve Mesh Baker.";	
			} else {
				outstr = "====== There are possible problems with these meshes that may prevent them from combining well. TREATMENT SUGGESTIONS (copy and paste to text editor if too big) =====\n" + sb.ToString();	
			}
			Debug.Log(outstr);
		}
	
		string PrintList(List<GameObject> gos){
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < gos.Count; i++){
				sb.Append( gos[i] + ",");
			}
			return sb.ToString();
		}
		
	}
}
