
using UnityEngine;
using System.Collections.Generic;

public enum MegaLinkSrc
{
	Position,
	LocalPosition,
	Rotation,
	LocalRotation,
	Scale,
	DotRotation,
	Angle,
	RotationXY,
	RotationXZ,
	RotationYZ,
}

[System.Serializable]
public class MegaMorphLinkDesc
{
	public string			name = "";
	public Transform		target;
	public int				channel = 0;
	public MegaAxis			axis = MegaAxis.X;
	public MegaLinkSrc		src = MegaLinkSrc.LocalRotation;
	public float			min = 0.0f;
	public float			max = 90.0f;
	public bool				useCurve = false;
	public AnimationCurve	curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
	public bool				late;
	public bool				active;
	public Quaternion		rot;
	public float			low = 0.0f;
	public float			high = 1.0f;

	float Ang(Quaternion rotationA, Quaternion rotationB, MegaLinkSrc t)
	{
		Vector3 forwardA = rotationA * Vector3.forward;
		Vector3 forwardB = rotationB * Vector3.forward;

		float angleA = 0.0f;
		float angleB = 0.0f;

		switch ( t )
		{
			case MegaLinkSrc.RotationXY:
				angleA = Mathf.Atan2(forwardA.x, forwardA.y) * Mathf.Rad2Deg;
				angleB = Mathf.Atan2(forwardB.x, forwardB.y) * Mathf.Rad2Deg;
				break;

			case MegaLinkSrc.RotationXZ:
				angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
				angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
				break;

			case MegaLinkSrc.RotationYZ:
				angleA = Mathf.Atan2(forwardA.y, forwardA.z) * Mathf.Rad2Deg;
				angleB = Mathf.Atan2(forwardB.y, forwardB.z) * Mathf.Rad2Deg;
				break;
		}

		return Mathf.DeltaAngle(angleA, angleB);
	}

	public float GetVal()
	{
		float val = 0.0f;

		if ( target )
		{
			switch ( src )
			{
				case MegaLinkSrc.Position:		val = target.position[(int)axis]; break;
				case MegaLinkSrc.Rotation:		val = target.rotation.eulerAngles[(int)axis]; break;
				case MegaLinkSrc.LocalPosition: val = target.localPosition[(int)axis]; break;
				case MegaLinkSrc.LocalRotation: val = target.localRotation.eulerAngles[(int)axis]; break;
				case MegaLinkSrc.Scale:			val = target.localScale[(int)axis]; break;
				case MegaLinkSrc.DotRotation:	val = Quaternion.Dot(target.localRotation, rot); break;
				case MegaLinkSrc.Angle:			val = Quaternion.Angle(target.localRotation, rot); break;
				case MegaLinkSrc.RotationXY:	val = Ang(target.localRotation, rot, src); break;
				case MegaLinkSrc.RotationXZ:	val = Ang(target.localRotation, rot, src); break;
				case MegaLinkSrc.RotationYZ:	val = Ang(target.localRotation, rot, src); break;
			}
		}
		return val;
	}

	public void Update(MegaMorph morph, bool islate)
	{
		if ( active && late == islate )	//&& target )
		{
			float alpha = Mathf.Clamp01((GetVal() - min) / (max - min));

			if ( useCurve )
				alpha = curve.Evaluate(alpha);

			//Debug.Log("val " + GetVal() + " percent " + Mathf.Lerp(low, high, alpha) + " alpha " + alpha);
			morph.SetPercentLim(channel, Mathf.Lerp(low, high, alpha));	// * 100.0f);
		}
	}
}

[ExecuteInEditMode]
public class MegaMorphLink : MonoBehaviour
{
	public MegaMorph				morph;
	public List<MegaMorphLinkDesc>	links = new List<MegaMorphLinkDesc>();

	void Start()
	{
		if ( !morph )
			morph = GetComponent<MegaMorph>();
	}

	void UpdateLinks(bool islate)
	{
		if ( morph )
		{
			for ( int i = 0; i < links.Count; i++ )
			{
				links[i].Update(morph, islate);
			}
		}
	}

	void LateUpdate()
	{
		UpdateLinks(true);
	}

	void Update()
	{
		UpdateLinks(false);
	}
}