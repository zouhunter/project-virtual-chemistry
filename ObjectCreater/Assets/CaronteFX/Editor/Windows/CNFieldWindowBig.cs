using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNFieldWindowBig : CNFieldWindow
  {
    float minWidth  = 450f;
    float minHeight = 450f;

    //-----------------------------------------------------------------------------------
    public override void Init( CNFieldController controller, CommandNodeEditor ownerEditor )
    {
      Instance.minSize = new Vector2(minWidth, minHeight);
      View = new CNFieldExtendedView( controller, ownerEditor ); 
      Controller = controller;
    }
  }
}

