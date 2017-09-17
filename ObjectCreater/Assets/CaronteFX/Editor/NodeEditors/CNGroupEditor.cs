using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaronteFX
{
  public class CNGroupEditor : CNMonoFieldEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get { return icon_; } }

    new protected CNGroup Data { get; set; }

    public CNGroupEditor( CNGroup data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNGroup)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();
      FieldController.SetIsScope(true);
    }
    //----------------------------------------------------------------------------------
    public uint GetFieldId()
    {
      return FieldController.GetFieldId();
    }
    //-----------------------------------------------------------------------------------
    public override void SceneSelection()
    {
      FieldController.SceneSelectionTopMost();
    }
    //-----------------------------------------------------------------------------------
    public void CheckUpdate( out int[] unityIdsAdded, out int[] unityIdsRemoved )
    {
      unityIdsAdded   = null;
      unityIdsRemoved = null;

      Data.NeedsUpdate = false;
    }
    //-----------------------------------------------------------------------------------
    public void RemoveMissingChildAt( int childIdx )
    {
      Data.Children.RemoveAt(childIdx);
    }
    //----------------------------------------------------------------------------------
    public void DettachChild( CommandNode child )
    {
      child.Parent = null;
      Data.Children.Remove( child );
    }
    //----------------------------------------------------------------------------------
    public override void SetActivityState()
    {
      base.SetActivityState();
      cnHierarchy.SetActivityState( Data.Children );
    }
    //-----------------------------------------------------------------------------------
    public override void SetVisibilityState()
    {
      base.SetVisibilityState();
      cnHierarchy.SetVisibilityState(Data.Children);
    }
    //----------------------------------------------------------------------------------
    public override void SetExcludedState()
    {
      base.SetExcludedState();
      cnHierarchy.SetExcludedState(Data.Children);
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI( Rect area, bool isEditable )
    { 
      GUILayout.BeginArea( area );
      
      RenderTitle(isEditable);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects("Scope", FieldController, true, true, CNFieldWindow.Type.normal );
      EditorGUI.EndDisabledGroup();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      EditorGUILayout.Space();
      GUILayout.EndArea();
    }
  }
}
