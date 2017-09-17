using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{

  public interface IDeepClonable<T>
  {
    T DeepClone();
  }

  public interface IMonoDeepClonable<T>
  {
    T DeepClone( GameObject fxData );
    bool UpdateNodeReferences( Dictionary<CommandNode, CommandNode> dictNodeToClonedNode );
  }

}