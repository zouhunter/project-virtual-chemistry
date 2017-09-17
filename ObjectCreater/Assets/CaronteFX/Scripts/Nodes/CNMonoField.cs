using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public abstract class CNMonoField : CommandNode
  {
    [SerializeField]
    protected CNField field_;
    public abstract CNField Field { get; }
    //-----------------------------------------------------------------------------------
    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      return (field_.UpdateNodeReferences( dictNodeToClonedNode ));
    }

  }
}
