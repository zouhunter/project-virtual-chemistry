using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNContactEmitterEditor : CNEntityEditor 
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
    
    CNFieldController FieldControllerA { get; set; }
    CNFieldController FieldControllerB { get; set; }

    protected GUIContent[] emitModes_  = new GUIContent[] { new GUIContent("Only First Event"), new GUIContent("All Events") };

    new CNContactEmitter Data { get; set; }
    public CNContactEmitterEditor( CNContactEmitter data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNContactEmitter)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();
   
      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry
                                          | CNField.AllowedTypes.RigidBodyNode;

      FieldControllerA = new CNFieldController( Data, Data.FieldA, eManager, goManager );
      FieldControllerA.SetFieldType( allowedTypes );
      FieldControllerA.IsBodyField = true;

      FieldControllerB = new CNFieldController( Data, Data.FieldB, eManager, goManager );
      FieldControllerB.SetFieldType( allowedTypes );
      FieldControllerB.IsBodyField = true;
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldController .RestoreFieldInfo();
      FieldControllerA.RestoreFieldInfo();
      FieldControllerB.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldController .StoreFieldInfo();
      FieldControllerA.StoreFieldInfo();
      FieldControllerB.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldController .BuildListItems();
      FieldControllerA.BuildListItems();
      FieldControllerB.BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      bool removed = Data.Field.RemoveNode(node);
      bool removedFromA = Data.FieldA.RemoveNode(node);
      bool removedFromB = Data.FieldB.RemoveNode(node);
      return ( removed || removedFromA || removedFromB );
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldController.SetScopeId(scopeId);
      FieldControllerA.SetScopeId(scopeId);
      FieldControllerB.SetScopeId(scopeId);
    }
    //----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldController.DestroyField();
      FieldControllerA.DestroyField();
      FieldControllerB.DestroyField();

      eManager.DestroyEntity( Data );
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToA( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerA.AddGameObjects(draggedObjects, recalculateFields);
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToB( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerB.AddGameObjects(draggedObjects, recalculateFields);
    }
    //----------------------------------------------------------------------------------
    public override void CreateEntitySpec()
    {
      eManager.CreateContactEmitter( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      GameObject[]  arrGameObjectA = FieldControllerA.GetUnityGameObjects();
      GameObject[]  arrGameObjectB = FieldControllerB.GetUnityGameObjects();

      eManager.RecreateContactEmitter( Data, arrGameObjectA, arrGameObjectB );
    }
    //----------------------------------------------------------------------------------
    private void DrawEmitMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Popup( new GUIContent("Emit mode"), (int)Data.EmitMode, emitModes_ );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change emit mode - " + Data.Name);
        Data.EmitMode = (CNContactEmitter.EmitModeOption) value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawMaxNumberOfEventsPerSecond()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField( "Max. events per second", Data.MaxEventsPerSecond );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max. events per second " + Data.Name);
        Data.MaxEventsPerSecond = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawRelativeSpeedMin_N()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative normal speed min.", Data.RelativeSpeedMin_N );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change relative normal speed min. " + Data.Name);
        Data.RelativeSpeedMin_N = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawRelativeSpeedMin_T()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative tangent speed min.", Data.RelativeSpeedMin_T );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change relative tangent speed min. " + Data.Name);
        Data.RelativeSpeedMin_T = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawRelativeMomentumMin_N()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative normal momentum min.", Data.RelativeMomentum_N );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change relative normal momentum min. " + Data.Name);
        Data.RelativeMomentum_N = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawRelativeMomentumMin_T()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative tangent momentum min.", Data.RelativeMomentum_T );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change relative tangent momentum min. " + Data.Name);
        Data.RelativeMomentum_T = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawLifeTimeMin()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Lifetime min.", Data.LifeTimeMin);
      if(EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change lifetime min. " + Data.Name);
        Data.LifeTimeMin = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawCollapseRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Collapse radius", Data.CollapseRadius);
      if(EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change collapse radius" + Data.Name);
        Data.CollapseRadius = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Bodies A", FieldControllerA, true, false, CNFieldWindow.Type.extended );
      RenderFieldObjects( "Bodies B", FieldControllerB, true, false, CNFieldWindow.Type.extended );

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();

      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      
      //EditorGUILayout.Space();
      //DrawEmitMode();
      EditorGUILayout.Space();
      DrawMaxNumberOfEventsPerSecond();
      EditorGUILayout.Space();
      DrawRelativeSpeedMin_N();
      DrawRelativeSpeedMin_T();
      //EditorGUILayout.Space();
      //DrawRelativeMomentumMin_N();
      //DrawRelativeMomentumMin_T();
      //EditorGUILayout.Space();
      //DrawLifeTimeMin();
      //DrawCollapseRadius();

      EditorGUILayout.EndScrollView();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      GUILayout.EndArea();
    }

  }
}


