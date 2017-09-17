using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNFieldView 
  {
    protected CNFieldController Controller { get; set;}   
    protected CRListBox selectionListBox_;

    protected CommandNode ownerNode_;
    //-----------------------------------------------------------------------------------
    public CNFieldView( CNFieldController controller, CommandNodeEditor ownerEditor )
    { 
      Controller        = controller;
      selectionListBox_ = new CRListBox( Controller, "FieldLB", false );
      ownerNode_        = ownerEditor.Data;

      Controller.WantsUpdate += controller.BuildListItems;
    }
    //-----------------------------------------------------------------------------------
    public void Deinit()
    {
      Controller.WantsUpdate = null;
    }
    //-----------------------------------------------------------------------------------
    public virtual void RenderGUI( Rect area )
    {
      if (ownerNode_ == null)
      {
        CNFieldWindowSmall.CloseIfOpen();
        EditorGUIUtility.ExitGUI();
      }

      EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
      DrawToolStrip();
      EditorGUILayout.EndHorizontal();

      EditorGUI.LabelField( new Rect(5f, 30f, 50f, 20f), "Objects: ");
      Rect listArea    = new Rect( 5, 50, area.width - 10 , area.height - 55 );

      selectionListBox_.RenderGUI( listArea ); 
    }
    //-----------------------------------------------------------------------------------
    protected void DrawToolStrip()
    {
      GUILayout.BeginHorizontal();

      if (GUILayout.Button("Edit", EditorStyles.toolbarDropDown))
      {
        GenericMenu optionsMenu = new GenericMenu();   
        Controller.FillOptionsMenu(optionsMenu);    
        optionsMenu.DropDown( new Rect(5, 10, 16, 16) );

        EditorGUIUtility.ExitGUI();
      }
      GUIStyle nObjectsStyle = new GUIStyle( EditorStyles.toolbarButton );
      bool isPro = UnityEditorInternal.InternalEditorUtility.HasPro();
      if (isPro)
      {
        nObjectsStyle.normal.textColor = Color.green;
      }
      else
      {
        nObjectsStyle.normal.textColor = Color.blue;
      }
      GUILayout.Label(ownerNode_.Name, EditorStyles.toolbarButton);
      if (Controller.IsBodyField)
      {
        GUILayout.Label( "Total: " + Controller.TotalObjects + " bodies", nObjectsStyle );
      }
      else
      {
        GUILayout.Label( "Total: " + Controller.TotalObjects + " objects", nObjectsStyle );
      }
      EditorGUILayout.Space();
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();
    }
  }
}
