using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNAddView : IView
  {
    //-----------------------------------------------------------------------------------
    public CNFieldController Controller { get; private set; }
    //-----------------------------------------------------------------------------------
    private string nameSelector = "*";
    //-----------------------------------------------------------------------------------
    public CNAddView( CNFieldController controller )
    {
      Controller = controller;
    }
    //-----------------------------------------------------------------------------------
    public void RenderGUI( Rect area, bool isEditable )
    {
      GUILayout.BeginHorizontal(EditorStyles.toolbar);
      GUILayout.Label("Name selector");
      GUILayout.EndHorizontal();
    
      GUILayout.Space (10);

      EditorGUILayout.BeginHorizontal();
      //EditorGUILayout.LabelField( "Name Selector", GUILayout.MinWidth(60) );
    
      GUI.SetNextControlName("nameselectortextfield");
      nameSelector = EditorGUILayout.TextField( nameSelector, GUILayout.MinWidth(100) ); 
      EditorGUI.FocusTextInControl("nameselectortextfield");
      EditorGUILayout.EndHorizontal();
    
      GUILayout.Space (15);
 
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if ( GUILayout.Button ("Cancel", GUILayout.Width(80) ) )
      {
        CNItemPopupWindow.CloseIfOpen();
      }
      GUILayout.Space (50);
      if ( GUILayout.Button ("OK", GUILayout.Width(80) ) )
      {
        Controller.AddNameSelector(nameSelector, true);
        CNItemPopupWindow.CloseIfOpen();
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();

      ProcessEvents();
    }
    //-----------------------------------------------------------------------------------
    public void ProcessEvents()
    {
      Event ev         = Event.current;
      EventType evType = ev.type;

      if ( evType == EventType.KeyUp && 
          ( ev.keyCode == KeyCode.KeypadEnter || ev.keyCode == KeyCode.Return ) )
      {
        Controller.AddNameSelector(nameSelector, true);
        CNItemPopupWindow.CloseIfOpen();
        ev.Use();
      }
    }
  }
}

