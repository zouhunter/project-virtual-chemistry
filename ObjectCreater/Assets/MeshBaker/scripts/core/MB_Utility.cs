//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace DigitalOpus.MB.Core{
public class MB_Utility{
	public static Texture2D createTextureCopy(Texture2D source){
		Texture2D newTex = new Texture2D(source.width,source.height,TextureFormat.ARGB32,true);
		newTex.SetPixels(source.GetPixels());
		return newTex;
	}
	
	public static Material[] GetGOMaterials(GameObject go){
		if (go == null) return null;
		Material[] sharedMaterials = null;
		Mesh mesh = null;
		MeshRenderer mr = go.GetComponent<MeshRenderer>();
		if (mr != null){
			sharedMaterials = mr.sharedMaterials;
			MeshFilter mf = go.GetComponent<MeshFilter>();
			if (mf == null){
				throw new Exception("Object " + go + " has a MeshRenderer but no MeshFilter.");
			}
			mesh = mf.sharedMesh;
		}
		
		SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
		if (smr != null){
			sharedMaterials = smr.sharedMaterials;
			mesh = smr.sharedMesh;
		}
		
		if (sharedMaterials == null){
			Debug.LogError("Object " + go.name + " does not have a MeshRenderer or a SkinnedMeshRenderer component");
			return null;	
		} else if (mesh == null){
			Debug.LogError("Object " + go.name + " has a MeshRenderer or SkinnedMeshRenderer but no mesh.");
			return null;				
		} else {
			if (mesh.subMeshCount < sharedMaterials.Length){
				Debug.LogWarning("Object " + go + " has only " + mesh.subMeshCount + " submeshes and has " + sharedMaterials.Length + " materials. Extra materials do nothing.");	
				Material[] newSharedMaterials = new Material[mesh.subMeshCount];
				Array.Copy(sharedMaterials,newSharedMaterials,newSharedMaterials.Length);
				sharedMaterials = newSharedMaterials;
			}
			return sharedMaterials;
		}
	}
	
	public static Mesh GetMesh(GameObject go){
		if (go == null) return null;
		MeshFilter mf = go.GetComponent<MeshFilter>();
		if (mf != null){
			return mf.sharedMesh;
		}
		
		SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
		if (smr != null){
			return smr.sharedMesh;
		}
		
		Debug.LogError("Object " + go.name + " does not have a MeshFilter or a SkinnedMeshRenderer component");
		return null;
	}
	
	public static Renderer GetRenderer(GameObject go){
		if (go == null) return null;
		MeshRenderer mr = go.GetComponent<MeshRenderer>();
		if (mr != null) return mr; 
		
		
		SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
		if (smr != null) return smr;
		return null;		
	}
	
	public static void DisableRendererInSource(GameObject go){
		if (go == null) return;
		MeshRenderer mf = go.GetComponent<MeshRenderer>();
		if (mf != null){
			mf.enabled = false;
			return;
		}
		
		SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
		if (smr != null){
			smr.enabled = false;
			return;
		}			
	}
	
	public static bool hasOutOfBoundsUVs(Mesh m, ref Rect uvBounds, int submeshIndex = -1){
		Vector2[] uvs = m.uv;
		if (uvs.Length == 0) return false;
		float minx,miny,maxx,maxy;
		if (submeshIndex >= m.subMeshCount) return false;
		if (submeshIndex >= 0){
			int[] tris = m.GetTriangles(submeshIndex);
			if (tris.Length == 0) return false;
			minx = maxx = uvs[tris[0]].x;
			miny = maxy = uvs[tris[0]].y;
			for (int idx = 0; idx < tris.Length; idx++){
				int i = tris[idx];
				if (uvs[i].x < minx) minx = uvs[i].x;
				if (uvs[i].x > maxx) maxx = uvs[i].x;
				if (uvs[i].y < miny) miny = uvs[i].y;
				if (uvs[i].y > maxy) maxy = uvs[i].y;
			}			
		} else {
			minx = maxx = uvs[0].x;
			miny = maxy = uvs[0].y;
			for (int i = 0; i < uvs.Length; i++){
					if (uvs[i].x < minx) minx = uvs[i].x;
					if (uvs[i].x > maxx) maxx = uvs[i].x;
					if (uvs[i].y < miny) miny = uvs[i].y;
					if (uvs[i].y > maxy) maxy = uvs[i].y;
			}
		} 
		uvBounds.x = minx;
		uvBounds.y = miny;
		uvBounds.width = maxx - minx;
		uvBounds.height = maxy - miny;
		if (maxx > 1f || minx < 0f || maxy > 1f || miny < 0f){
			return true;
		}
		//all well behaved objs use the same rect so TexSets compare properly
		uvBounds.x = uvBounds.y = 0f;
		uvBounds.width = uvBounds.height = 1f;
		return false;
	}
	
	public static void setSolidColor(Texture2D t, Color c){
		Color[] cs = t.GetPixels();
		for (int i = 0; i < cs.Length; i++){
			cs[i] = c;	
		}
		t.SetPixels(cs);
		t.Apply();
	}
	
	public static Texture2D resampleTexture(Texture2D source, int newWidth, int newHeight){
		TextureFormat f = source.format;
		if (f == TextureFormat.ARGB32 ||
			f == TextureFormat.RGBA32 ||
			f == TextureFormat.BGRA32 ||
			f == TextureFormat.RGB24  ||
			f == TextureFormat.Alpha8 ||
			f == TextureFormat.DXT1)
		{
			Texture2D newTex = new Texture2D(newWidth,newHeight,TextureFormat.ARGB32,true);
			float w = newWidth;
			float h = newHeight;
			for (int i = 0; i < newWidth; i++){
				for (int j = 0; j < newHeight; j++){
					float u = i/w;
					float v = j/h;
					newTex.SetPixel(i,j,source.GetPixelBilinear(u,v));
				}
			}
			newTex.Apply(); 		
			return newTex;
		} else {
			Debug.LogError("Can only resize textures in formats ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT");	
			return null;
		}
	}

	class MB_Triangle{
		int submeshIdx;
		int[] vs = new int[3];
		
		public bool isSame(object obj){
			MB_Triangle tobj = (MB_Triangle) obj;
			if (vs[0] == tobj.vs[0] &&
				vs[1] == tobj.vs[1] &&
				vs[2] == tobj.vs[2] &&
				submeshIdx != tobj.submeshIdx){
				return true;	
			}
			return false;
		}
		
		public bool sharesVerts(MB_Triangle obj){
			if (vs[0] == obj.vs[0] ||
				vs[0] == obj.vs[1] ||
				vs[0] == obj.vs[2]){
				if (submeshIdx != obj.submeshIdx) return true;	
			}
			if (vs[1] == obj.vs[0] ||
				vs[1] == obj.vs[1] ||
				vs[1] == obj.vs[2]){
				if (submeshIdx != obj.submeshIdx) return true;
			}	
			if (vs[2] == obj.vs[0] ||
				vs[2] == obj.vs[1] ||
				vs[2] == obj.vs[2]){
				if (submeshIdx != obj.submeshIdx) return true;	
			}
			return false;			
		}
		
		public MB_Triangle(int[] ts, int idx, int sIdx){
			vs[0] = ts[idx];
			vs[1] = ts[idx + 1];
			vs[2] = ts[idx + 2];
			submeshIdx = sIdx;
			Array.Sort(vs);
		}
	}
	
	public static bool validateOBuvsMultiMaterial(Material[] sharedMaterials){
		for (int i = 0; i < sharedMaterials.Length; i++){
			for (int j = i + 1; j < sharedMaterials.Length; j++){
				if (sharedMaterials[i] == sharedMaterials[j]){
					return false;
				}
			}
		}
		return true;
	}
	
	public static int doSubmeshesShareVertsOrTris(Mesh m){
		List<MB_Triangle> tlist = new List<MB_Triangle>();
		bool sharesVerts = false;
		bool sharesTris = false;
		for (int i = 0; i < m.subMeshCount; i++){
			int[] sm = m.GetTriangles(i);
			for (int j = 0; j < sm.Length; j+=3){
				MB_Triangle consider = new MB_Triangle(sm,j,i);
				for (int k = 0; k < tlist.Count; k++){
					if (consider.isSame(tlist[k])) sharesTris = true;
					if (consider.sharesVerts(tlist[k])){
						sharesVerts = true;
					}
				}
				tlist.Add(consider);
			}
		}
		if (sharesTris) return 2;
		if (sharesVerts) return 1;
		return 0;
	}	
	
	public static bool GetBounds(GameObject go, out Bounds b){
		if (go == null){
			Debug.LogError("go paramater was null");
			b = new Bounds(Vector3.zero,Vector3.zero);
			return false;				
		}
		Renderer r = GetRenderer(go);
		if (r == null){
			Debug.LogError("GetBounds must be called on an object with a Renderer");
			b = new Bounds(Vector3.zero,Vector3.zero);
			return false;
		}
		if (r is MeshRenderer){
			b = r.bounds;
			return true;
		} else if (r is SkinnedMeshRenderer){
			b = r.bounds;
			return true;
		}
		Debug.LogError("GetBounds must be called on an object with a MeshRender or a SkinnedMeshRenderer.");
		b = new Bounds(Vector3.zero,Vector3.zero);
		return false;		
	}
			
	public static void Destroy(UnityEngine.Object o){
		if (Application.isPlaying){
			MonoBehaviour.Destroy(o);
		} else {
//			string p = AssetDatabase.GetAssetPath(o);
//			if (p != null && p.Equals("")) // don't try to destroy assets
				MonoBehaviour.DestroyImmediate(o,false);
		}
	}		
}
}
