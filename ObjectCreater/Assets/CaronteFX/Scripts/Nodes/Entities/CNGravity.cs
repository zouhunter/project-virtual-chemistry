using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CNGravity : CNEntity
  {
    [SerializeField]
    private Vector3 gravity_ = new Vector3(0.0f, -9.81f, 0.0f);
    public Vector3 Gravity
    {
      get { return gravity_; }
      set { gravity_ = value; }
    }
    //-----------------------------------------------------------------------------------
    public override CommandNode DeepClone( GameObject dataHolder )
    {
      CNGravity clone = dataHolder.AddComponent<CNGravity>();      
      
      clone.field_ = field_.DeepClone();

      clone.Name       = Name;
      clone.gravity_   = gravity_;
      
      return clone;
    }
  }
}