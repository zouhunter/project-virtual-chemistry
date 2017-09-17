using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNAimedForceEditor : CNEntityEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
    
    CNFieldController FieldControllerAimGameObjects { get; set; }

    new CNAimedForce Data { get; set; }

    public CNAimedForceEditor( CNAimedForce data, CommandNodeEditorState state )
      : base( data, state)
    {
      Data = (CNAimedForce)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      FieldControllerAimGameObjects = new CNFieldController( Data, Data.FieldAimGameObjects, eManager, goManager );
      FieldControllerAimGameObjects.SetFieldType( CNField.AllowedTypes.GameObject );
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldController              .RestoreFieldInfo();
      FieldControllerAimGameObjects.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldController              .StoreFieldInfo();
      FieldControllerAimGameObjects.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      bool removedFromBodies         = Data.Field              .RemoveNode(node);
      bool removedFromAimGameObjects = Data.FieldAimGameObjects.RemoveNode(node);
 
      return ( removedFromBodies || removedFromAimGameObjects );
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldController              .SetScopeId(scopeId);
      FieldControllerAimGameObjects.SetScopeId(scopeId);
    }
    //----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldController              .DestroyField();
      FieldControllerAimGameObjects.DestroyField();

      eManager.DestroyEntity( Data );
    }
    //----------------------------------------------------------------------------------
    public override void CreateEntitySpec()
    {
      eManager.CreateAimedForce( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      GameObject[] arrGameObjectBody = FieldController.GetUnityGameObjects();
      GameObject[] arrGameObjectAim  = FieldControllerAimGameObjects.GetUnityGameObjects();

      eManager.RecreateAimedForce( Data, arrGameObjectBody, arrGameObjectAim );
    }
    //----------------------------------------------------------------------------------
    public void AddGameObjectsToBodies( UnityEngine.Object[] objects, bool recalculateFields )
    {
      FieldController.AddGameObjects( objects, recalculateFields );
    }
    //----------------------------------------------------------------------------------
    public void AddGameObjectsToAim( UnityEngine.Object[] objects, bool recalculateFields )
    {
      FieldControllerAimGameObjects.AddGameObjects( objects, recalculateFields );
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Bodies", FieldController,  true, true, CNFieldWindow.Type.extended );
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      Data.Timer        = EditorGUILayout.FloatField("Timer (s)", Data.Timer );
      Data.TimeDuration = EditorGUILayout.FloatField("Duration (s)", Data.TimeDuration );
      
      EditorGUILayout.Space();

      Data.Multiplier_r = EditorGUILayout.FloatField("Position multiplier", Data.Multiplier_r );
      Data.Multiplier_q = EditorGUILayout.FloatField("Rotation multiplier", Data.Multiplier_q );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      RenderFieldObjects( "Target objects", FieldControllerAimGameObjects, true, false, CNFieldWindow.Type.normal );

      EditorGUI.EndDisabledGroup();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }
  }
}

