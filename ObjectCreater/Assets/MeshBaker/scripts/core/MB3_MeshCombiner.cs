using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.Core{
	
	//TODO bug with triangles if using showHide with AddDelete reproduce by using the AddDeleteParts script and changeing some of it to show hide
	[System.Serializable]
	public abstract class MB3_MeshCombiner{

		public delegate void GenerateUV2Delegate(Mesh m);		
		
		public static bool EVAL_VERSION{
			get {return false;}
		}
		
		[SerializeField] protected MB2_LogLevel _LOG_LEVEL = MB2_LogLevel.info;
		public virtual MB2_LogLevel LOG_LEVEL{
			get{return _LOG_LEVEL;}
			set{_LOG_LEVEL = value;}
		}
		
		[SerializeField] protected MB2_ValidationLevel _validationLevel = MB2_ValidationLevel.robust;
		public virtual MB2_ValidationLevel validationLevel{
			get {return _validationLevel;}
			set {_validationLevel = value;}
		}
		
		[SerializeField] protected string _name;
		public string name { 
			get{return _name;}
			set{_name = value;}
		}
		
		[SerializeField] protected MB2_TextureBakeResults _textureBakeResults;
		public virtual MB2_TextureBakeResults textureBakeResults { 
			get{return _textureBakeResults;} 
			set{_textureBakeResults = value;}
		}

		[SerializeField] protected GameObject _resultSceneObject;
		public virtual GameObject resultSceneObject { 
			get{return _resultSceneObject;}
			set{_resultSceneObject = value;} 
		}		
		
		[SerializeField] protected UnityEngine.Renderer _targetRenderer;
		public virtual Renderer targetRenderer { 
			get{return _targetRenderer;}
			set{
				if (_targetRenderer != null && _targetRenderer != value){
					Debug.LogWarning("Previous targetRenderer was not null. Combined mesh may be being used by more than one Renderer");
				}
				_targetRenderer = value;
			} 
		}
		
		[SerializeField] protected MB_RenderType _renderType;
		public virtual MB_RenderType renderType { 
			get{return _renderType;} 
			set{ _renderType = value;} 
		}
		
		[SerializeField] protected MB2_OutputOptions _outputOption;
		public virtual MB2_OutputOptions outputOption { 
			get{return _outputOption;} 
			set{_outputOption = value;} 
		}
	
		[SerializeField] protected MB2_LightmapOptions _lightmapOption = MB2_LightmapOptions.ignore_UV2;
		public virtual MB2_LightmapOptions lightmapOption { 
			get{return _lightmapOption;} 
			set{_lightmapOption = value;} 
		}
		
		[SerializeField] protected bool _doNorm = true;
		public virtual bool doNorm { 
			get{return _doNorm;} 
			set{_doNorm = value;} 
		}
		
		[SerializeField] protected bool _doTan = true;
		public virtual bool doTan { 
			get{return _doTan;} 
			set{_doTan = value;}
		}
		
		[SerializeField] protected bool _doCol;
		public virtual bool doCol { 
			get{return _doCol;} 
			set{_doCol = value;}
		}
		
		[SerializeField] protected bool _doUV = true;
		public virtual bool doUV { 
			get{return _doUV;} 
			set{_doUV = value;}
		}
		
		[SerializeField] protected bool _doUV1;
		public virtual bool doUV1 { 
			get{return _doUV1;} 
			set{_doUV1 = value;}
		}
		
		
		public virtual bool doUV2(){
			return _lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged || _lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping;
		}
		
		public abstract int GetLightmapIndex();
		public abstract void ClearMesh();
		public abstract void DestroyMesh();
		public abstract void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods);
		public abstract List<GameObject> GetObjectsInCombined();
		public abstract int GetNumObjectsInCombined();
		public abstract int GetNumVerticesFor(GameObject go);
		public abstract int GetNumVerticesFor(int instanceID);
	
		/// <summary>
		/// Copies Mesh Baker internal data to the mesh.
		/// </summary>		
		public virtual void Apply(){
			Apply(null);
		}
		
		/// <summary>
		/// Copies Mesh Baker internal data to the mesh.
		/// </summary>
		/// <param name='uv2GenerationMethod'>
		/// Uv2 generation method. This is normally editor class method Unwrapping.GenerateSecondaryUVSet
		/// </param>
		public abstract void Apply(GenerateUV2Delegate uv2GenerationMethod);
		
		/// <summary>
		/// Apply the specified triangles, vertices, normals, tangents, uvs, colors, uv1, uv2, bones and uv2GenerationMethod.
		/// </summary>
		/// <param name='triangles'>
		/// Triangles.
		/// </param>
		/// <param name='vertices'>
		/// Vertices.
		/// </param>
		/// <param name='normals'>
		/// Normals.
		/// </param>
		/// <param name='tangents'>
		/// Tangents.
		/// </param>
		/// <param name='uvs'>
		/// Uvs.
		/// </param>
		/// <param name='colors'>
		/// Colors.
		/// </param>
		/// <param name='uv1'>
		/// Uv1.
		/// </param>
		/// <param name='uv2'>
		/// Uv2.
		/// </param>
		/// <param name='bones'>
		/// Bones.
		/// </param>
		/// <param name='uv2GenerationMethod'>
		/// Uv2 generation method. This is normally method Unwrapping.GenerateSecondaryUVSet. This should be null when calling Apply at runtime.
		/// </param>		
		public abstract void Apply(bool triangles,
						  bool vertices,
						  bool normals,
						  bool tangents,
						  bool uvs,
						  bool colors,
						  bool uv1,
						  bool uv2,
						  bool bones=false,
						  GenerateUV2Delegate uv2GenerationMethod = null);

		/// <summary>
		/// Updates the data in the combined mesh for meshes that are already in the combined mesh.
		/// This is faster than adding and removing a mesh and has a much lower memory footprint.
		/// This method can only be used if the meshes being updated have the same layout(number of 
		/// vertices, triangles, submeshes).
		/// This is faster than removing and re-adding
		/// For efficiency update as few channels as possible.
		/// Apply must be called to apply the changes to the combined mesh
		/// </summary>		
		public abstract void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true,
										bool updateVertices = true, bool updateNormals = true, bool updateTangents = true,
									    bool updateUV = false, bool updateUV1 = false, bool updateUV2 = false,
										bool updateColors = false, bool updateSkinningInfo = false);		
		
		public abstract bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource=true);

		public abstract bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource);
		public abstract bool CombinedMeshContains(GameObject go);
		public abstract void UpdateSkinnedMeshApproximateBounds();
		public abstract void UpdateSkinnedMeshApproximateBoundsFromBones();

		/// <summary>
		/// Updates the skinned mesh approximate bounds from the bounds of the source objects.
		/// </summary>		
		public abstract void UpdateSkinnedMeshApproximateBoundsFromBounds();
		
		/// <summary>
		/// Updates the skinned mesh bounds by creating a bounding box that contains the bones (skeleton) of the source objects.
		/// </summary>		
		public static void UpdateSkinnedMeshApproximateBoundsFromBonesStatic(Transform[] bs, SkinnedMeshRenderer smr){
			Vector3 max, min;
			max = bs[0].position;
			min = bs[0].position;
			for (int i = 1; i < bs.Length; i++){
				Vector3 v = bs[i].position;
				if (v.x < min.x) min.x = v.x;
				if (v.y < min.y) min.y = v.y;
				if (v.z < min.z) min.z = v.z;
				if (v.x > max.x) max.x = v.x;
				if (v.y > max.y) max.y = v.y;
				if (v.z > max.z) max.z = v.z;			
			}
			Vector3 center = (max + min)/2f;
			Vector3 size = max - min;
			Matrix4x4 w2l = smr.worldToLocalMatrix;
			Bounds b = new Bounds(w2l * center, w2l * size);		
			smr.localBounds = b;
		}

		public static void UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(List<GameObject> objectsInCombined,SkinnedMeshRenderer smr){
			Bounds b = new Bounds();
			Bounds bigB = new Bounds();
			if (MB_Utility.GetBounds(objectsInCombined[0],out b)){
				bigB = b;
			} else {
				Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");	
				return;
			}
			for (int i = 1; i < objectsInCombined.Count; i++){
				if (MB_Utility.GetBounds(objectsInCombined[i],out b)){
					bigB.Encapsulate(b);
				} else {
					Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");	
					return;					
				}
			}	
			smr.localBounds = bigB;			
		}		
	}
}