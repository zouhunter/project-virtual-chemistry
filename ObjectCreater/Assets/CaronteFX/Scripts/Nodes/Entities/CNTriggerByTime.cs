using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNTriggerByTime : CNTrigger
  {
    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNTriggerByTime clone = CommandNode.CreateInstance<CNTriggerByTime>(dataHolder);

      clone.field_        = field_.DeepClone();

      clone.Name   = Name;
      clone.timer_ = Timer;
    
      return clone;
    }

  }
}
