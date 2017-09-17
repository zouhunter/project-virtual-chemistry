// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class FlowEngine : UnityEngine.Object {
	
	Transform target;
	FlowNode[] graph;
	
	Mesh mesh;
	Vector3[] vertices;
	int[] triangles;
	
	float fluidViscosity= 0.5f;
	int graph_cycle= 0;
	int graphnodespercycle= -1; // '-1' equals all vertices per cycle
	
	public FlowEngine( Transform t ){
		
		target = t;
		
		// Check target
		if( target == null ){
			Debug.LogError( "Target reference has been lost. Has the target object been destroyed?" );
			return;
		}
		
		// Get mesh
		MeshFilter meshF = target.GetComponent<MeshFilter>() as MeshFilter;
		if( meshF == null ){
			Debug.LogError( "Target has no mesh." );
			return;
		}
		mesh = meshF.mesh;
		vertices = mesh.vertices;
		triangles = mesh.triangles;
		
		if( graphnodespercycle < 0 ) graphnodespercycle = vertices.Length;
	}
	
	///////////////////////
	// CLASS FUNCTIONS
	///////////////////////
	
	public void PreProcess() {
		
		// Generate samples
		graph = new FlowNode[vertices.Length];
		for( int i = 0; i < graph.Length; i++ ){
			graph[i] = new FlowNode();
			graph[i].water = 0.0f;
			graph[i].worldposition = target.TransformPoint( vertices[i] );
			graph[i].additionalheight = Mathf.Max( 0.0f, 0.0f - graph[i].worldposition.y );
		}
		
		// Add vertex links to graph nodes
		for( int i = 0; i < triangles.Length; i+=3 ){
			int t0= triangles[i];
			int t1= triangles[i+1];
			int t2= triangles[i+2];
			
			graph[t0].vertexlinks_list.Add(t1);
			graph[t0].vertexlinks_list.Add(t2);
			
			graph[t1].vertexlinks_list.Add(t0);
			graph[t1].vertexlinks_list.Add(t2);
			
			graph[t2].vertexlinks_list.Add(t0);
			graph[t2].vertexlinks_list.Add(t1);
			
			graph[t0].linkedTriangles++;
			graph[t1].linkedTriangles++;
			graph[t2].linkedTriangles++;
		}
		
		// Remove doubles in nodes
		for( int i = 0; i < vertices.Length; i++ ){
			ArrayList links = new ArrayList();
			ArrayList nodelinks = graph[i].vertexlinks_list;
			for( int j = 0; j < nodelinks.Count; j++ ) {
				if (!HasLink(links, (int)nodelinks[j])) links.Add(nodelinks[j]);
			}
			
			// Transfer node links from list to array
			graph[i].vertexlinks = new int[links.Count];
			for( int j = 0; j < graph[i].vertexlinks.Length; j++ ) {
				graph[i].vertexlinks[j] = (int)links[j];
			}
			
			// Clear node list
			graph[i].vertexlinks_list.Clear();
		}
		
		// Find edges
		for( int i = 0; i < graph.Length; i++ ) {
			if( graph[i].vertexlinks.Length > graph[i].linkedTriangles ) graph[i].isEdge = true;
		}
	}
	
	// Transfer flow vectors and water amount to vertex colors for shader visualization
	public void TransferWaterLevel() {
		
		Color[] colors = mesh.colors;
		
		if (colors.Length != vertices.Length) colors = new Color[vertices.Length];
		
		for( int i = 0; i < vertices.Length; i++ ) {
			Color color= Color.black;
			color.r = graph[i].flowdirection[0] * 0.5f + 0.5f;
			color.g = graph[i].flowdirection[2] * 0.5f + 0.5f;
			color.b = Mathf.Clamp01(graph[i].water * 0.1f);
			colors[i] = color;
		}
		
		mesh.colors = colors;
	}
	
	public void FlowUpdate (){

		FlowNode[] buffer = new FlowNode[graph.Length];
		
		
		for( int i = 0; i < graph.Length; i++ ) {
			buffer[i] = ((FlowNode)graph[i]).Clone();
		}
		
		int k= 0;
		while( k < graphnodespercycle ){
			k++;
			graph_cycle++;
			if( graph_cycle == graph.Length ) graph_cycle = 0;
			
			int i=0;
				i = graph_cycle;
			
			FlowNode node= graph[i] as FlowNode;
			float node_h = node.worldposition.y + node.additionalheight + node.water;
			
			ArrayList nlink_flow_node = new ArrayList();
			ArrayList nlink_flow_water= new ArrayList();
			float total_waterflow= 0.0f;
			float fluid_flow_magnitude= 0.0f;
			
			// run loops if this node contains water
			if( node.water > 0.0f ) {
				// Get nodes that water flows to plus height differences
				for( int j = 0; j < node.vertexlinks.Length; j++ ) {
					
					FlowNode n = buffer[node.vertexlinks[j]] as FlowNode;
					
					float node_link_h = n.worldposition.y + n.additionalheight + n.water;
					if( node_h > node_link_h ) {
						float node_diff = node_h - node_link_h;
						
						nlink_flow_node.Add(n);
						nlink_flow_water.Add(node_diff);
						
						total_waterflow += node_diff;
						
						node_diff = Mathf.Min(node_diff, node.water);
						if( node_diff > fluid_flow_magnitude ) fluid_flow_magnitude = node_diff;
					}
				}
				
				// Subtract water from center node
				buffer[i].water -= fluid_flow_magnitude * fluidViscosity;
				
				// Flow off edge
				if( node.isEdge && node.water > 0 ) buffer[i].water = 0;
				
				// Calculate water distribution between node links
				if( total_waterflow > 0.0f ){
					for( int j = 0; j < nlink_flow_node.Count; j++ ) {
						FlowNode n = nlink_flow_node[j] as FlowNode;
						float nw = (float)nlink_flow_water[j];
						n.water += (nw / total_waterflow) * fluid_flow_magnitude * fluidViscosity;
					}
				}
			}
		}
		
		for (int i = 0; i < graph.Length; i++) {
			FlowNode node = (FlowNode)buffer[i];
			
			Vector3 flowdirection = Vector3.zero;
			
			for (int j = 0; j < node.vertexlinks.Length; j++) {
				FlowNode linkedNode = (FlowNode)buffer[node.vertexlinks[j]];
				
				float heightDifference = ((node.water + node.additionalheight + node.worldposition.y) - (linkedNode.water + linkedNode.additionalheight + linkedNode.worldposition.y));
				
				if (heightDifference > 0) {
					flowdirection += Vector3.Normalize(new Vector3(linkedNode.worldposition.x - node.worldposition.x, 0, linkedNode.worldposition.z - node.worldposition.z)) * heightDifference;
				}
			}
			
			node.flowdirection = flowdirection;
		}
		
		graph = buffer;
	}
	
	// Add water value to one flow graph node at given world coordinates.
	public void AddWater( Vector3 pos, float amount ) {
		if( graph != null && graph.Length > 0 ) {
			int vert_i = FindClosestVertex(pos);
			FlowNode node= graph[vert_i];
			node.water += amount;
		}
	}
	
	// Sets an additional height value to every node in the argument 'graphnodes' as FlowNode[]
	public void SetHeightAtNodes( ArrayList graphNodes, float height ) {
		for( int i = 0; i < graphNodes.Count; i++ ){
			FlowNode g = graph[(int)graphNodes[i]] as FlowNode;
			g.additionalheight = Mathf.Max( 0.0f, height - g.worldposition.y );
		}
	}
	
	///////////////////////
	// CLASS SUB FUNCTIONS
	///////////////////////
	
	// Used by flow-graph preprocessor
	bool HasLink (ArrayList arr, int l) {
		for (int i = 0; i < arr.Count; i++) {
			if ((int)arr[i] == l) return true;
		}
		return false;
	}
	
	int FindClosestVertex (Vector3 pos) {
		
		// Search every eighth vertices
		int closest= -1;
		float closest_distance= Mathf.Infinity;
		for (int i = 0; i < vertices.Length; i+=8) {
			Vector3 vert= vertices[i];
			float D = Vector2.Distance(new Vector2(vert.x, vert.z), new Vector2(pos.x, pos.z));
			if( D < closest_distance ) {
				closest = i;
				closest_distance = D;
			}
		}
		
		// Search vertex links
		bool foundClosest= false;
		while (foundClosest == false) {
			FlowNode node = graph[closest];
			bool changes= false;
			for (int i = 0; i < node.vertexlinks.Length; i++) {
				Vector3 nVert = vertices[node.vertexlinks[i]];
				float D = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(nVert.x, nVert.z));
				if (D < closest_distance) {
					closest = node.vertexlinks[i];
					closest_distance = D;
					changes = true;
				}
			}
			
			if (changes == false) foundClosest = true;
		}
		
		return closest;
	}
	
	// Return all graph nodes within 2D world-coordinates.
	// Input arg is a Rect( x, z, length, width )
	public ArrayList FindVerticesWithinRect (Vector4 rect) {
		ArrayList vertlist = new ArrayList();
		for (int i = 0; i < graph.Length; i++) {
			FlowNode g = graph[i];
			if (g.worldposition.x > rect[0] &&
				g.worldposition.x < rect[0] + rect[2] &&
				g.worldposition.z > rect[1] &&
				g.worldposition.z < rect[1] + rect[3]
			) {
				vertlist.Add(i);
			}
		}
		return vertlist;
	}
	
	Vector3 Lerp (Vector3 vec0, Vector3 vec1, float f){
		Vector3 result = new Vector3();
		result[0] = vec0[0] * f + vec1[0] * (1.0f - f);
		result[1] = vec0[1] * f + vec1[1] * (1.0f - f);
		result[2] = vec0[2] * f + vec1[2] * (1.0f - f);
		return result;
	}
}
