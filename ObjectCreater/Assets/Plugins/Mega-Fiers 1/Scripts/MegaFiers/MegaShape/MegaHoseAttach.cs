
using UnityEngine;

[ExecuteInEditMode]
public class MegaHoseAttach : MonoBehaviour
{
	public float	alpha = 0.0f;
	public MegaHose	hose;
	public Vector3	offset = Vector3.zero;
	public Vector3	rotate = Vector3.zero;
	public bool		doLateUpdate = true;
	public bool		rot = true;

	void Update()
	{
		if ( !doLateUpdate )
		{
			PositionObject();
		}
	}

	void LateUpdate()
	{
		if ( doLateUpdate )
		{
			PositionObject();
		}
	}

	void PositionObject()
	{
		if ( hose )
		{
			Vector3 p1 = hose.GetPosition(alpha);
			if ( rot )
			{
				Vector3 p2 = hose.GetPosition(alpha + 0.001f);
				Quaternion look = Quaternion.LookRotation(p2 - p1) * Quaternion.Euler(rotate);
				transform.rotation = look;
			}

			transform.position = p1 + transform.TransformDirection(offset);
		}
	}
}