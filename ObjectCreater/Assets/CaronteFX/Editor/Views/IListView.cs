using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public interface IListView
  {
    IListController Controller { get; }
    void RenderGUI( Rect Area );
  }
}


