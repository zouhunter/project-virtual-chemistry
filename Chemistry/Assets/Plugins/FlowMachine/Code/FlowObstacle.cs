// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

using UnityEngine;
using System.Collections;

public class FlowObstacle : MonoBehaviour {

	public Transform FlowVolumes;
	
	private FlowVolumes flowscript;
	private Vector3 pos;
	private Vector3 scale;
	public ArrayList graphNodes;
	
	void  Start (){
		if (FlowVolumes) {
			flowscript = FlowVolumes.GetComponent<FlowVolumes>() as FlowVolumes;
			flowscript.AddObstacle( transform );
		}
	}
	
	void  FixedUpdate (){
		if( FlowVolumes == null ) {
			Debug.LogWarning("FlowVolumes has not been defined in: " + transform.name);
			return;
		}
		// Get graph nodes
		if (pos != transform.position || scale != transform.localScale) {
			pos = transform.position;
			scale = transform.localScale;
			
			// Create obstacle area of effect rect in worldspace
			Vector4 obstacleRect = new Vector4();
			obstacleRect[0] = transform.position.x - transform.localScale.x * 0.5f;
			obstacleRect[1] = transform.position.z - transform.localScale.z * 0.5f;
			obstacleRect[2] = transform.localScale.x;
			obstacleRect[3] = transform.localScale.z;
			
			// Reset old nodes
			if(graphNodes != null && graphNodes.Count > 0) flowscript.SetHeightAtNodes(graphNodes, 0.0f);
			
			// Get new nodes
			graphNodes = flowscript.FindVertivesWitihinRect(obstacleRect);
			
			// Update all obstacles in attached flow graph
			flowscript.UpdateObstacles();
		}
	}
}