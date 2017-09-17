using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CNTriggerByExplosion : CNTrigger
  {
    [SerializeField]
           CNField fieldExplosions_;
    public CNField FieldExplosions
    {
      get
      {
        if (fieldExplosions_ == null)
        {
          CNField.AllowedTypes allowedTypes = CNField.AllowedTypes.ExplosionNode;
                      
          fieldExplosions_ = new CNField( false, allowedTypes, false );
        }
        return fieldExplosions_;
      }
    }

    [SerializeField]
           CNField fieldBodies_;
    public CNField FieldBodies
    {
      get
      {
        if (fieldBodies_ == null)
        {
          CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry
                                              | CNField.AllowedTypes.BodyNode;
                      
          fieldBodies_ = new CNField( false, allowedTypes, false );
        }
        return fieldBodies_;
      }
    }


    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNTriggerByExplosion clone = CommandNode.CreateInstance<CNTriggerByExplosion>(dataHolder);
      
      clone.field_            = field_          .DeepClone();
      clone.fieldExplosions_  = fieldExplosions_.DeepClone();
      clone.fieldBodies_      = fieldBodies_    .DeepClone();

      clone.Name   = Name;
      clone.timer_ = Timer;
    
      return clone;
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updateEntities  = field_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updateExplosion = fieldExplosions_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updateBodies    = fieldBodies_.UpdateNodeReferences(dictNodeToClonedNode);

      return (updateEntities || updateExplosion || updateBodies);
    }
  }
}//namespace CaronteFX


