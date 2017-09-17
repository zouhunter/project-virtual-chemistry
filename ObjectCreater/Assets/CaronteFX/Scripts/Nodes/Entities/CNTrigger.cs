using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public abstract class CNTrigger : CNEntity
  {
    
    public override CNField Field 
    { 
      get
      {
        if (field_ == null)
        {
          CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.EntityNode;                      
          field_ = new CNField( false, allowedTypes, false );
        }
        return field_;
      }
    }

  }
}

