
using UnityEngine;

public class Bouyancy : MonoBehaviour
{
	public float waterLevel = 0.0f;
	public float floatHeight = 0.0f;
	public Vector3 buoyancyCentreOffset = Vector3.zero;
	public float bounceDamp = 1.0f;

	public GameObject water;

	public MegaDynamicRipple	dynamicwater;

	void Start()
	{
		if ( water )
		{
			dynamicwater = (MegaDynamicRipple)water.GetComponent<MegaDynamicRipple>();
		}
	}

	void FixedUpdate()
	{
		if ( dynamicwater )
		{
			waterLevel = dynamicwater.GetWaterHeight(water.transform.worldToLocalMatrix.MultiplyPoint(transform.position));
		}

		Vector3 actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
		float forceFactor = 1.0f - ((actionPoint.y - waterLevel) / floatHeight);
		
		if ( forceFactor > 0.0f )
		{
			Vector3 uplift = -Physics.gravity * (forceFactor - GetComponent<Rigidbody>().velocity.y * bounceDamp);
			GetComponent<Rigidbody>().AddForceAtPosition(uplift, actionPoint);
		}
	}
}