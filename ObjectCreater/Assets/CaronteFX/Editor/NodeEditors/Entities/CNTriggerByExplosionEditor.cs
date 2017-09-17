using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNTriggerByExplosionEditor : CNTriggerEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
    
    CNFieldController FieldControllerExplosions { get; set; }
    CNFieldController FieldControllerBodies { get; set; }

    new CNTriggerByExplosion Data { get; set; }
    public CNTriggerByExplosionEditor( CNTriggerByExplosion data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNTriggerByExplosion)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      FieldControllerExplosions = new CNFieldController( Data, Data.FieldExplosions, eManager, goManager );
      FieldControllerExplosions.SetFieldType( CNField.AllowedTypes.ExplosionNode );


      CNField.AllowedTypes allowedTypesBodies =   CNField.AllowedTypes.Geometry
                                                | CNField.AllowedTypes.BodyNode;
                      
      FieldControllerBodies = new CNFieldController( Data, Data.FieldBodies, eManager, goManager );
      FieldControllerBodies.SetFieldType( allowedTypesBodies );
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldController          .RestoreFieldInfo();
      FieldControllerExplosions.RestoreFieldInfo();
      FieldControllerBodies    .RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldController          .StoreFieldInfo();
      FieldControllerExplosions.StoreFieldInfo();
      FieldControllerBodies    .StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldController          .BuildListItems();
      FieldControllerExplosions.BuildListItems();
      FieldControllerBodies    .BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");

      bool removed      = Data.Field          .RemoveNode(node);
      bool removedFromA = Data.FieldExplosions.RemoveNode(node);
      bool removedFromB = Data.FieldBodies    .RemoveNode(node);

      return ( removed || removedFromA || removedFromB );
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldController          .SetScopeId(scopeId);
      FieldControllerExplosions.SetScopeId(scopeId);
      FieldControllerBodies    .SetScopeId(scopeId);
    }
    //----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldController          .DestroyField();
      FieldControllerExplosions.DestroyField();
      FieldControllerBodies    .DestroyField();

      eManager.DestroyEntity( Data );
    }
    //----------------------------------------------------------------------------------
    public override void CreateEntitySpec()
    {
      eManager.CreateTriggerByExplosion( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      CommandNode[] arrExplosionNode = FieldControllerExplosions .GetCommandNodes();
      GameObject[]  arrGameObject    = FieldControllerBodies.GetUnityGameObjects();
      CommandNode[] arrEntityNode    = FieldController.GetCommandNodes();

      eManager.RecreateTriggerByExplosion( Data, arrExplosionNode, arrGameObject, arrEntityNode );
    }
    //----------------------------------------------------------------------------------
    public void AddGameObjectsToBodies( UnityEngine.Object[] objects, bool recalculateFields )
    {
      FieldControllerBodies.AddGameObjects( objects, recalculateFields );
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Explosions", FieldControllerExplosions, true, false, CNFieldWindow.Type.extended );
      RenderFieldObjects( "Bodies",     FieldControllerBodies,     true, false, CNFieldWindow.Type.extended );

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      RenderFieldObjects( "Attentive Nodes", FieldController, true, false, CNFieldWindow.Type.extended );
      EditorGUILayout.Space();
      CRGUIUtils.Splitter();

      EditorGUI.EndDisabledGroup();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }


  }
}

