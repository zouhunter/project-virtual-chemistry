using System.Collections;
using System;

using UnityEngine;

namespace CaronteFX
{
  public class CNRigidbody : CNBody
  {

    [SerializeField]
    protected bool isFiniteMass_ = true;
    public bool IsFiniteMass 
    { 
      get { return isFiniteMass_; }
      set { isFiniteMass_ = value; }
    }
    //-----------------------------------------------------------------------------------
    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNRigidbody clone = CommandNode.CreateInstance<CNRigidbody>(dataHolder);
      
      clone.field_ = field_.DeepClone();

      clone.Name                    = Name;
      clone.isFiniteMass_           = isFiniteMass_;

      clone.mass_                   = mass_;
      clone.density_                = density_;
      clone.restitution_in01_       = restitution_in01_;
      clone.frictionKinetic_in01_   = frictionKinetic_in01_;
      clone.frictionStatic_in01_    = frictionStatic_in01_;
      clone.gravity_                = gravity_;
      clone.dampingPerSecond_WORLD_ = dampingPerSecond_WORLD_;
      
      clone.velocityStart_          = velocityStart_;
      clone.omegaStart_inRadSeg_    = omegaStart_inRadSeg_;

      return clone;
    }
    //-----------------------------------------------------------------------------------
  }
}