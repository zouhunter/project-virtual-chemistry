using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  
  public class CNTriggerByTimeEditor : CNTriggerEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }


    new CNTriggerByTime Data { get; set; }
    public CNTriggerByTimeEditor( CNTriggerByTime data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNTriggerByTime)data;
    }


    public override void CreateEntitySpec()
    {
      eManager.CreateTriggerByTime( Data );
    }

    public override void ApplyEntitySpec()
    {
      CommandNode[] arrCommandNode = FieldController.GetCommandNodes();

      eManager.RecreateTriggerByTime( Data, arrCommandNode );
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUI.BeginDisabledGroup( !isEditable );
      
      RenderFieldObjects( "Attentive Nodes", FieldController, true, false, CNFieldWindow.Type.extended );
      
      EditorGUILayout.Space();
      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      EditorGUILayout.Space();
      DrawTimer();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }


  }

}

