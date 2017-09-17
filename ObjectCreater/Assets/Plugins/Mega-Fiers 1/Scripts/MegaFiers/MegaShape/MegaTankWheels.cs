
using UnityEngine;

[ExecuteInEditMode]
public class MegaTankWheels : MonoBehaviour
{
	public float radius = 1.0f;

	public MegaTracks	track;

	Vector3 localrot = Vector3.zero;
	public float	offang = 0.0f;
	public MegaAxis	axis = MegaAxis.Z;

	void Start()
	{
		localrot = transform.localRotation.eulerAngles;
	}

	void Update()
	{
		if ( track )
		{
			if ( track.shape )
			{
				MegaSpline spl = track.shape.splines[track.curve];

				float len = spl.length;

				//float off = len * Mathf.Repeat(track.start * 0.01f, 1.0f);
				float off = len * track.start * 0.01f;	//, 1.0f);

				float ang = (off / (Mathf.PI * 2.0f * radius)) * Mathf.PI * 2.0f * Mathf.Rad2Deg;
				ang = Mathf.Repeat(ang, 360.0f);
				//Debug.Log("Ang " + ang);
				Vector3 rot = localrot;
				rot[(int)axis] += ang + offang;

				transform.localRotation = Quaternion.Euler(rot);
			}
		}
	}
}