// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

using UnityEngine;
using System.Collections;

public class FlowEmitter : MonoBehaviour {
	public Transform FlowVolumes;
	
	public enum FlowEmitType {
		Continuous = 0,
		OneShot = 1,
	}
	
	public FlowEmitType flowEmitType = FlowEmitType.Continuous;
	public float OneShotFlowAmount= 0;
	public float continuousWaterFlowAmt= 2.0f;
	
	private FlowVolumes flowscript;
	
	void OnDrawGizmos (){
		Gizmos.color = new Color(0, 0, 1, 0.5f);
		
		Vector3 pos= transform.position;
		pos.y += Mathf.Max(0.01f, OneShotFlowAmount * 0.1f) * 0.5f;
		Vector3 size= new Vector3(0.2f, Mathf.Max(0.01f, OneShotFlowAmount * 0.1f), 0.2f);
		
		Gizmos.DrawCube (pos, size);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube (pos, size);
	}
	
	void Start (){
		if (FlowVolumes != null) {
			flowscript = FlowVolumes.GetComponent<FlowVolumes>() as FlowVolumes;
		}
	}
	
	void FixedUpdate (){
		if( FlowVolumes == null ) {
			Debug.LogWarning("FlowVolumes has not been defined in: " + transform.name);
			return;
		}
		
		switch( flowEmitType ) {
			case FlowEmitType.Continuous :
				flowscript.AddWater(transform.position, continuousWaterFlowAmt);
				break;
			case FlowEmitType.OneShot :
				flowscript.AddWater(transform.position, OneShotFlowAmount);
				OneShotFlowAmount = 0;
				break;
		}
	}
}