using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public struct CRJointPivot 
  {
    public Color pivotStateColor_;
    public Vector3 posA_;
    public Vector3 posB_;

    public CRJointPivot(Color pivotStateColor, Vector3 posA, Vector3 posB)
    {
       pivotStateColor_ = pivotStateColor;
       posA_ = posA;
       posB_ = posB;
    }

  }
}

