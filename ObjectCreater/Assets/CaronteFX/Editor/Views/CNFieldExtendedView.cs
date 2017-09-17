using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNFieldExtendedView : CNFieldView 
  {
    private float botMargin     = 55f;
    private float buttonsHeight = 75f;

    private CNNodesController nodesController_;
    private CRListBox         nodesListBox_;
    private CNField           field_;
    //-----------------------------------------------------------------------------------
    public CNFieldExtendedView( CNFieldController controller, CommandNodeEditor ownerEditor ) :
      base( controller, ownerEditor )
    { 
       nodesController_ = new CNNodesController( controller, ownerEditor );
       nodesListBox_    = new CRListBox( nodesController_, "FieldnodesLB", true );
       field_           = controller.Field;

       controller.WantsUpdate += nodesController_.SetSelectableNodes;
    }
    //-----------------------------------------------------------------------------------
    public override void RenderGUI( Rect area )
    {
      if (ownerNode_ == null)
      {
        CNFieldWindowBig.CloseIfOpen();
        EditorGUIUtility.ExitGUI();
      }

      EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

      DrawToolStrip();

      EditorGUILayout.EndHorizontal();

      Event ev = Event.current;

      EditorGUI.LabelField( new Rect(5f, 30f, area.width, 20f), "Selected Objects:");

      Rect listAreaObjects = new Rect( 5f, 50f, Mathf.Ceil( ( area.width - 10f) / 2f - 30f ), Mathf.Ceil( area.height - botMargin ) );
      Rect buttonRect      = new Rect( listAreaObjects.xMax + 10f, (listAreaObjects.yMax - listAreaObjects.yMin - buttonsHeight ) / 2f + 30f, 40f, 35f );

      Rect listAreaNodes   = new Rect( buttonRect.xMax + 10f, listAreaObjects.yMin, ( area.width - 10f ) / 2f - 30f , area.height - botMargin - buttonsHeight - 10f );
      Rect buttonsArea     = new Rect( buttonRect.xMax + 10f, listAreaNodes.yMax + 2f, listAreaNodes.width, buttonsHeight );
      
      Rect label1Area      = new Rect( buttonRect.xMin + 10f, buttonsArea.yMin + 27f, 20f, 17f);
      Rect label2Area      = new Rect( buttonRect.xMin + 10f, label1Area.yMax + 13f, 20f, 17f);
      
      EditorGUI.BeginDisabledGroup( !Controller.AreGameObjectsAllowed() );

      EditorGUI.LabelField(label1Area,"<<");
      EditorGUI.LabelField(label2Area,"<<");

      EditorGUI.EndDisabledGroup();
      
      ProcessEvents(ev, listAreaObjects, listAreaNodes);
      
      EditorGUI.BeginDisabledGroup( !nodesController_.AnyNodeSelected );
      if (GUI.Button(buttonRect, "<<"))
      {
        nodesController_.AddSelectedNodes();
      }
      EditorGUI.EndDisabledGroup();
      EditorGUI.LabelField(new Rect(buttonRect.xMax + 10f, 30f, area.width, 20f), "Selectable Nodes:");
     
      selectionListBox_.RenderGUI(listAreaObjects);
      nodesListBox_.RenderGUI(listAreaNodes);

      GUILayout.BeginArea(buttonsArea);

      EditorGUI.BeginChangeCheck();

      bool isShowOwnerGroupOnly = field_.ShowOwnerGroupOnly;

      GUIStyle styleToggle = new GUIStyle( EditorStyles.label );

      if ( isShowOwnerGroupOnly )
      {
        styleToggle.fontStyle = FontStyle.Bold;
      }
      
      field_.ShowOwnerGroupOnly = EditorGUILayout.ToggleLeft("Show only owner group", isShowOwnerGroupOnly, styleToggle);
      if (EditorGUI.EndChangeCheck())
      {
        nodesController_.SetSelectableNodes();
        EditorUtility.SetDirty(ownerNode_);
      }

      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup( !Controller.AreGameObjectsAllowed() );
      if (GUILayout.Button("Name Selector"))
      {
        Controller.AddNameSelectorWindow();
      }

      EditorGUILayout.Space();

      if (GUILayout.Button("Game Object Selection"))
      {
        Controller.AddCurrentSelection();
      }
      EditorGUI.EndDisabledGroup();

      GUILayout.EndArea();

    }
    //-----------------------------------------------------------------------------------
    private void ProcessEvents(Event ev, Rect listAreaObjects, Rect listAreaNodes)
    {
      if (ev.type == EventType.mouseDown)
      {
        if ( listAreaObjects.Contains(ev.mousePosition) )
        {
          nodesController_.UnselectSelected();
        }
        else if (listAreaNodes.Contains(ev.mousePosition) )
        {
          Controller.UnselectSelected();
        }
      }
    }
    //-----------------------------------------------------------------------------------
  }
}
