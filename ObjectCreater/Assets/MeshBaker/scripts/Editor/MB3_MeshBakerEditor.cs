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


	[CustomEditor(typeof(MB3_MeshBaker))]
	public class MB3_MeshBakerEditor : Editor {
		MB3_MeshBakerEditorInternal mbe = new MB3_MeshBakerEditorInternal();
		[MenuItem("GameObject/Create Other/Mesh Baker/Mesh And Material Baker")]
		public static GameObject CreateNewMeshBaker(){
			MB3_MeshBaker[] mbs = (MB3_MeshBaker[]) Editor.FindObjectsOfType(typeof(MB3_MeshBaker));
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
			MB3_TextureBaker tb = nmb.AddComponent<MB3_TextureBaker>();
			tb.packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;
			nmb.AddComponent<MB3_MeshBaker>();
			return nmb.gameObject;
		}
		
	//	void OnEnable () {
	//		mbe.OnEnable((MB3_MeshBakerCommon) target);
	//	}
		
		public override void OnInspectorGUI(){
			mbe.OnInspectorGUI((MB3_MeshBakerCommon) target, typeof(MB3_MeshBakerEditorWindow));
		}
	}
