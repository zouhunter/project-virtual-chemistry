using UnityEngine;
using System.Collections;

public class MB2_TestShowHide : MonoBehaviour {
	public MB3_MeshBaker mb;
	public GameObject[] objs;
	
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount == 100){
			mb.ShowHide(null,objs);
			mb.ApplyShowHide();
			Debug.Log("should have disappeared");
		}

		if (Time.frameCount == 200){
			mb.ShowHide(objs,null);
			mb.ApplyShowHide();
			Debug.Log("should show");
		}
	}
}
