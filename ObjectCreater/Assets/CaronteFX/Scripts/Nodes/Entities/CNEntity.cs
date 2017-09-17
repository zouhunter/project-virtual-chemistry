using UnityEngine;
using System.Collections;


namespace CaronteFX
{
  public abstract class CNEntity : CNMonoField
  {
    public override CNField Field 
    { 
      get
      {
        if (field_ == null)
        {
          CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry 
                                              | CNField.AllowedTypes.BodyNode;

          field_ = new CNField( false, allowedTypes, true );
        }
        return field_;
      }
    }

    [SerializeField]
    protected float timer_;
    public float Timer
    {
      get { return timer_; }
      set { timer_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

  }


}

