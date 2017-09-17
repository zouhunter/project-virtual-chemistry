using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CNSpeedLimiter : CNEntity
  {
    [SerializeField]
    private float speed_limit_ = 20f;
    public float SpeedLimit
    {
      get { return speed_limit_; }
      set { speed_limit_ = value; }
    }

    [SerializeField]
    private float falling_speed_limit_ = 50f;
    public float FallingSpeedLimit
    {
      get { return falling_speed_limit_; }
      set { falling_speed_limit_ = value; }
    }
    //-----------------------------------------------------------------------------------
    public override CommandNode DeepClone( GameObject dataHolder )
    {
      CNSpeedLimiter clone = dataHolder.AddComponent<CNSpeedLimiter>();      
      
      clone.field_ = field_.DeepClone();

      clone.Name                 = Name;
      clone.speed_limit_         = speed_limit_;
      clone.falling_speed_limit_ = falling_speed_limit_;
      
      return clone;
    }
  }
}