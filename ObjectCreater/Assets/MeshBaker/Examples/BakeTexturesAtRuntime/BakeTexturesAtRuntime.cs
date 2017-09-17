using UnityEngine;
using System.Collections;
using DigitalOpus.MB.Core;

/*
 * For building atlases at runtime it is very important that:
 * 		- textures be in trucolor/RBGA32 format
 * 		- textures have read flag set
 * 
 * 
 * It is also Highly recommended to avoid resizing
 *      - build padding into textures in editor
 *      - don't use padding when creating atlases
 *      - don't use tiled materials
 * 
 * If you are having problems look at the Debug Log on the device
 */
public class BakeTexturesAtRuntime : MonoBehaviour {
	public GameObject target;
	float elapsedTime = 0;
	
	void OnGUI(){
		GUILayout.Label("Time to bake textures: " + elapsedTime);
		if (GUILayout.Button("Combine textures & build combined mesh")){
			MB3_MeshBaker meshbaker = target.GetComponent<MB3_MeshBaker>();
			MB3_TextureBaker textureBaker = target.GetComponent<MB3_TextureBaker>();
			
			//These can be assets configured at runtime or you can create them
			// on the fly like this
			textureBaker.textureBakeResults = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
			textureBaker.resultMaterial = new Material( Shader.Find("Diffuse") ); 
			
			float t1 = Time.realtimeSinceStartup;
			textureBaker.CreateAtlases();
			elapsedTime = Time.realtimeSinceStartup - t1;	
			
			meshbaker.ClearMesh(); //only necessary if your not sure whats in the combined mesh
			meshbaker.textureBakeResults = textureBaker.textureBakeResults;
			//Add the objects to the combined mesh
			meshbaker.AddDeleteGameObjects(textureBaker.GetObjectsToCombine().ToArray(), null);
			
			meshbaker.Apply();
		}
	}
}
