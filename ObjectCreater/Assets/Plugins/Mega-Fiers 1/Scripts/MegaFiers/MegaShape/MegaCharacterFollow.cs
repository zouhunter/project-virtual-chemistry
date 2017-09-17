
using UnityEngine;

[ExecuteInEditMode]
public class MegaCharacterFollow : MonoBehaviour
{
	public MegaShape	path;
	//public float		impulse = 10.0f;
	//public float		drive = 0.0f;

	void Start()
	{
		float alpha = 0.0f;
		Vector3 tangent = Vector3.zero;
		int kn = 0;

		Vector3 p = transform.position;
		Vector3 np = path.FindNearestPointWorld(p, 5, ref kn, ref tangent, ref alpha);
		GetComponent<Rigidbody>().MovePosition(np);
	}

	public bool rot = false;
	public Vector3 rotate = Vector3.zero;

	void LateUpdate()
	{
		if ( path )
		{
			Vector3 p = transform.position;

			float alpha = 0.0f;
			Vector3 tangent = Vector3.zero;
			int kn = 0;

			Vector3 np = path.FindNearestPointWorld(p, 5, ref kn, ref tangent, ref alpha);

			//Vector3 dir = np - p;

			if ( rot )
			{
				Vector3 np1 = path.splines[0].InterpCurve3D(alpha + 0.001f, true, ref kn);

				Quaternion er = Quaternion.Euler(rotate);
				Quaternion r = Quaternion.LookRotation(np1 - np);	//transform.LookAt(target.transform.TransformPoint(target.InterpCurve3D(curve, a + ta, target.normalizedInterp)));
				transform.rotation = path.transform.rotation * r * er;
			}

			np.y = p.y;
			transform.position = np;

			//rigidbody.AddForce(dir * impulse);
			//rigidbody.MovePosition(np);
		}
	}
}
