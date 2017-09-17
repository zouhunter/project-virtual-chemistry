using UnityEngine;
using System;
using System.Collections;

namespace CaronteFX
{
  [Serializable]
  public class CRCollisionEvInfo
  {
    public string emitterName_;

    public GameObject GameObjectA;
    public GameObject GameObjectB;

    public Vector3 position_;
    public Vector3 velocityA_;
    public Vector3 velocityB_;

    public float relativeSpeed_N_;
    public float relativeSpeed_T_;

    public float relativeP_N_;
    public float relativeP_T_;
  }

}
