
using UnityEngine;

[ExecuteInEditMode]
public class MegaGlobeLink : MonoBehaviour
{
	public Transform target;
	MegaGlobe	globe;
	public float	angle = 0.0f;	

	void Update()
	{
		if ( target )
		{
			Vector3 scale = target.localScale;

			if ( globe == null )
			{
				globe = GetComponent<MegaGlobe>();
			}

			if ( globe )
			{
				globe.radius = scale.x / 2.0f;

				Vector3 lpos = Vector3.zero;

				lpos.x = Mathf.Sin(angle) * globe.radius;
				lpos.z = Mathf.Cos(angle) * globe.radius;

				Vector3 pos = target.position + lpos;	//target.TransformPoint(lpos);
				transform.position = pos;
			}
		}
	}
}