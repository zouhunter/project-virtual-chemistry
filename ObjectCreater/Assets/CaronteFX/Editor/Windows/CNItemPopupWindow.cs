using UnityEditor;
using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNItemPopupWindow : CRWindow<CNItemPopupWindow>
  {
    public IView View { get; private set; }
    //---------------------------------------------------------------------------------- 
    const float height_ = 90;
    const float width_ = 300;
    //-----------------------------------------------------------------------------------
    void OnEnable()
    {
    }
    //-----------------------------------------------------------------------------------
    public static CNItemPopupWindow ShowWindow(string windowTitle, Rect position)
    {  
      Instance = CNItemPopupWindow.CreateInstance<CNItemPopupWindow>(); 

      Instance.titleContent = new GUIContent(windowTitle);
      Instance.position  = position;
      Instance.minSize   = new Vector2(width_, height_);
      Instance.maxSize   = new Vector2(width_, height_);

      Instance.ShowAuxWindow();
     
      return (Instance);
    }
    //-----------------------------------------------------------------------------------
    public void Init(IView view)
    {
      View = view;
    }
    //-----------------------------------------------------------------------------------
    void OnGUI()
    {
      View.RenderGUI(position, true);
    }
    //-----------------------------------------------------------------------------------
  }
}


