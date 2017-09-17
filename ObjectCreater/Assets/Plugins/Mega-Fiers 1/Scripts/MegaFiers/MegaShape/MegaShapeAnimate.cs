
using UnityEngine;
using System.Collections.Generic;

// Should this be a window? Or add it direct into Shape, do test code here
// Should be a window I think
[ExecuteInEditMode]
public class MegaShapeAnimate : MonoBehaviour
{
	public bool		animate = false;
	public float	time = 0.0f;
	public float	speed = 1.0f;
	public MegaRepeatMode	LoopMode = MegaRepeatMode.Loop;
	public List<MegaKnotAnim>	animations;

	// Button to clear anim
	// BUtton to start anim
	// set a max time, then have a time slider
	// button to click to add state as keyframe

}