using UnityEngine;
using System.Collections.Generic;

namespace CaronteFX
{
  public interface IFieldEditor
  {
    void LoadInfo();
    void StoreInfo();
    void BuildListItems();
    bool RemoveNodeFromFields( CommandNode node );
    void SetScopeId(uint scopeId);
  }
}

