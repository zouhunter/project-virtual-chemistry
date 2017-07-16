// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

function OnDrawGizmos () {
	Gizmos.DrawIcon (transform.position, "../Resources/Editor/FlowProbe.tga");
}

function OnDrawGizmosSelected () {
	var areaOfEffect = transform.localScale.magnitude;
	var color : Color = Color( 0.14, 0.32, 0.73, 0.25 );
	Gizmos.color = color;
	Gizmos.DrawSphere (transform.position, areaOfEffect);
	Debug.DrawRay( transform.position - transform.right * areaOfEffect, transform.right * areaOfEffect * 2, color );
	Debug.DrawRay( transform.position, transform.forward * areaOfEffect, color );
	Debug.DrawRay( transform.position, -transform.forward * areaOfEffect, Color.red );
	Debug.DrawRay( transform.position - transform.up * areaOfEffect, transform.up * areaOfEffect * 2, color );
}