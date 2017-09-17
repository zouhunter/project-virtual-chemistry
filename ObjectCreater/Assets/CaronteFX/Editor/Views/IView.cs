using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public interface IView
  {
    void RenderGUI(Rect area, bool isEditable);
  }
}
