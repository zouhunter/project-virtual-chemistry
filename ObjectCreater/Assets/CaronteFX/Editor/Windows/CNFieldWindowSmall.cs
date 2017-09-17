using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNFieldWindowSmall : CNFieldWindow
  {
    float minWidth  = 300;
    float minHeight = 450f;

    //-----------------------------------------------------------------------------------
    public override void Init( CNFieldController controller, CommandNodeEditor ownerEditor)
    {
      Instance.minSize = new Vector2(minWidth, minHeight);
      View = new CNFieldView( controller, ownerEditor );  
      Controller = controller;
    }
  }
}