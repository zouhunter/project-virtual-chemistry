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


[CustomEditor(typeof(MB2_MeshBaker))]
public class MB2_MeshBakerEditor : Editor {
	MB2_MeshBakerEditorInternal mbe = new MB2_MeshBakerEditorInternal();
//	[MenuItem("GameObject/Create Other/Mesh Baker/Mesh And Material Baker")]
	public static GameObject CreateNewMeshBaker(){
		MB2_MeshBaker[] mbs = (MB2_MeshBaker[]) Editor.FindObjectsOfType(typeof(MB2_MeshBaker));
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
		GameObject nmb = new GameObject("MeshBaker" + largest);
		nmb.transform.position = Vector3.zero;
		MB2_TextureBaker tb = nmb.AddComponent<MB2_TextureBaker>();
		tb.texturePackingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;
		nmb.AddComponent<MB2_MeshBaker>();
		return nmb.gameObject;
	}
	
	public override void OnInspectorGUI(){
		MB2_MeshBaker tb = (MB2_MeshBaker) target;
		if (GUILayout.Button(" MIGRATE COMPONENTS TO VERSION 3 ")){
			GameObject go = tb.gameObject;
			MB2_TextureBakerEditor.MigrateTestureBakerToVersion3Component(go);
			MB2_MultiMeshBakerEditor.MigrateMultiMeshBakerToVersion3Component(go);
			MigrateMeshBakerToVersion3Component(go);
		}
		mbe.OnInspectorGUI((MB2_MeshBakerCommon) target, typeof(MB_MeshBakerEditorWindow));
	}

	public static void MigrateMeshBakerToVersion3Component(GameObject go){
		MB2_MeshBaker tb2 = go.GetComponent<MB2_MeshBaker>();
		if (tb2 == null) return;
		Debug.Log("Migrating Mesh Baker");
		MB3_MeshBaker tb3 = go.AddComponent<MB3_MeshBaker>();
		
		tb3.textureBakeResults = tb2.textureBakeResults;
		tb3.bakeAssetsInPlaceFolderPath = tb2.bakeAssetsInPlaceFolderPath;
		tb3.objsToMesh = tb2.objsToMesh;
		tb3.resultPrefab = tb2.resultPrefab;
		tb3.useObjsToMeshFromTexBaker = tb2.useObjsToMeshFromTexBaker;
		tb3.meshCombiner.doCol = tb2.doCol;
		tb3.meshCombiner.doNorm = tb2.doNorm;
		tb3.meshCombiner.doTan = tb2.doTan;
		tb3.meshCombiner.doUV = tb2.doUV;
		tb3.meshCombiner.doUV1 = tb2.doUV1;
		tb3.meshCombiner.lightmapOption = tb2.lightmapOption;
		tb3.meshCombiner.outputOption = tb2.outputOption;
		tb3.meshCombiner.resultSceneObject = tb2.resultSceneObject;
		tb3.meshCombiner.renderType = tb2.renderType;
		tb3.meshCombiner.targetRenderer = tb2.meshCombiner.targetRenderer;		

		DestroyImmediate(tb2);
	}	

}
