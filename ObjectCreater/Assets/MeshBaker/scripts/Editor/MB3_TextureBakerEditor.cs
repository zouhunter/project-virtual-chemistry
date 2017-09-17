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

[CustomEditor(typeof(MB3_TextureBaker))]
public class MB3_TextureBakerEditor : Editor {
	
	MB3_TextureBakerEditorInternal tbe = new MB3_TextureBakerEditorInternal();
	
	public override void OnInspectorGUI(){
		tbe.DrawGUI((MB3_TextureBaker) target, typeof(MB3_MeshBakerEditorWindow));	
	}
	
}