// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

using UnityEngine;
using System.Collections;

class FlowNode : UnityEngine.Object {
	public int[] vertexlinks;
	public float water;
	public Vector3 flowdirection;
	public Vector3 worldposition;
	public float additionalheight= 0.0f;
	public bool isEdge= false;
	public ArrayList vertexlinks_list= new ArrayList();
	public float linkedTriangles;
	
	public FlowNode Clone(){
		FlowNode clone = new FlowNode();
		clone.vertexlinks = this.vertexlinks;
		clone.water = this.water;
		clone.flowdirection = this.flowdirection;
		clone.worldposition = this.worldposition;
		clone.additionalheight = this.additionalheight;
		clone.isEdge = this.isEdge;
		clone.vertexlinks_list = this.vertexlinks_list;
		clone.linkedTriangles = this.linkedTriangles;
		
		return clone;
	}
}
