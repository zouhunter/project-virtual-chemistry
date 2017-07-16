// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

using UnityEngine;
using System.Collections;

public class FlowVolumes : MonoBehaviour {
	public Transform WaterMesh;
	
	private FlowEngine flow;
	private ArrayList Obstacles = new ArrayList();
	
	void Start(){
		if (WaterMesh != null) {
			flow = new FlowEngine(WaterMesh);
			flow.PreProcess();
		}
	}
	
	void Update(){
		if( WaterMesh == null ) {
			Debug.LogWarning("Water Mesh has not been defined in: " + transform.name);
			return;
		}
		flow.FlowUpdate();
		flow.TransferWaterLevel();
	}
	
	public void AddWater (Vector3 pos, float amount){
		flow.AddWater(pos, amount);
	}
	
	public void AddObstacle( Transform t ) {
		Obstacles.Add( t );
	}
	
	public void UpdateObstacles() {
		foreach( Transform t in Obstacles ) {
			FlowObstacle f = (FlowObstacle)t.GetComponent(typeof(FlowObstacle));
			if( f.graphNodes != null ) {
				SetHeightAtNodes( f.graphNodes, t.position.y + t.localScale.y * 0.5f );
			}
		}
	}
	
	public void SetHeightAtNodes (ArrayList graphNodes, float height){
		flow.SetHeightAtNodes(graphNodes, height);
	}
	
	public ArrayList FindVertivesWitihinRect (Vector4 rect){
		return flow.FindVerticesWithinRect(rect);
	}
}