
using UnityEngine;

public class MegaTestAnimator : MonoBehaviour
{
	MegaMorphAnimator anim;

	void Update()
	{
		if ( anim == null )
		{
			anim = GetComponent<MegaMorphAnimator>();
		}

		if ( anim )
		{
			if ( Input.GetKeyDown(KeyCode.Alpha1) )
			{
				anim.PlayClip(0);
			}

			if ( Input.GetKeyDown(KeyCode.Alpha2) )
			{
				anim.PlayClip(1);
			}

			if ( Input.GetKeyDown(KeyCode.Alpha3) )
			{
				anim.PlayClip(2);
			}

		}
	}
}