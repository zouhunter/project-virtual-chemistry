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

using UnityEditor;
using DigitalOpus.MB.Core;

[CustomEditor(typeof(MB2_TextureBaker))]
public class MB2_TextureBakerEditor : Editor {
	
	MB2_TextureBakerEditorInternal tbe = new MB2_TextureBakerEditorInternal();
	
	public override void OnInspectorGUI(){
		MB2_TextureBaker tb = (MB2_TextureBaker) target;
		if (GUILayout.Button(" MIGRATE COMPONENTS TO VERSION 3 ")){
			GameObject go = tb.gameObject;
			MigrateTestureBakerToVersion3Component(go);
			MB2_MultiMeshBakerEditor.MigrateMultiMeshBakerToVersion3Component(go);
			MB2_MeshBakerEditor.MigrateMeshBakerToVersion3Component(go);
		}
		
		tbe.DrawGUI(tb, typeof(MB_MeshBakerEditorWindow));	
	}
	
	public static void MigrateTestureBakerToVersion3Component(GameObject go){
		MB2_TextureBaker tb2 = go.GetComponent<MB2_TextureBaker>();
		if (tb2 == null) return;
		Debug.Log("Migrating Texture Baker");
		MB3_TextureBaker tb3 = go.AddComponent<MB3_TextureBaker>();
		tb3.atlasPadding = tb2.atlasPadding;
		tb3.doMultiMaterial = tb2.doMultiMaterial;
		tb3.fixOutOfBoundsUVs = tb2.fixOutOfBoundsUVs;
		tb3.maxTilingBakeSize = tb2.maxTilingBakeSize;
		
		tb3.objsToMesh = tb2.objsToMesh;
		tb3.packingAlgorithm = tb2.texturePackingAlgorithm;
		tb3.resizePowerOfTwoTextures = tb2.resizePowerOfTwoTextures;
		tb3.resultMaterial = tb2.resultMaterial;
		tb3.resultMaterials = tb2.resultMaterials;
		tb3.textureBakeResults = tb2.textureBakeResults;
		DestroyImmediate(tb2);
	}
}