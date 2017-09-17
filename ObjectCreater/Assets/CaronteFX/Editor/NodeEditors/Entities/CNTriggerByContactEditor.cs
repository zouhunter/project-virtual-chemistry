using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  
  public class CNTriggerByContactEditor : CNTriggerEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
    
    CNFieldController FieldControllerA { get; set; }
    CNFieldController FieldControllerB { get; set; }

    protected GUIContent[] triggeringModes_  = new GUIContent[] { new GUIContent("Only first contact between As and Bs (Global Unleash)"), new GUIContent("Each first contact of each pair (Partial Unleash)") };

    new CNTriggerByContact Data { get; set; }
    public CNTriggerByContactEditor( CNTriggerByContact data, CommandNodeEditorState state)
      : base( data, state )
    {
      Data = (CNTriggerByContact)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();
 
      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry
                                          | CNField.AllowedTypes.BodyNode;

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
      eManager.CreateTriggerByContact( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      GameObject[]  arrGameObjectA = FieldControllerA.GetUnityGameObjects();
      GameObject[]  arrGameObjectB = FieldControllerB.GetUnityGameObjects();
      CommandNode[] arrCommandNode = FieldController.GetCommandNodes();

      eManager.RecreateTriggerByContact( Data, arrGameObjectA, arrGameObjectB, arrCommandNode );
    }
    //----------------------------------------------------------------------------------
    public void DrawTriggeringMode()
    {
      EditorGUI.BeginChangeCheck();

      int optionIdx = Data.TriggerForInvolvedBodies ? 1 : 0;
      var value = EditorGUILayout.Popup(new GUIContent("Triggering mode"), optionIdx, triggeringModes_ );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( Data, "Change triggering mode - " + Data.Name);
        Data.TriggerForInvolvedBodies = (value == 1);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawSpeedMinN()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative normal speed min.", Data.SpeedMinN );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( Data, "Change speed N min - " + Data.Name);
        Data.SpeedMinN = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawSpeedMinT()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Relative tangent speed min.", Data.SpeedMinT );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( Data, "Change speed T min - " + Data.Name);
        Data.SpeedMinT = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Bodies A", FieldControllerA, true, false, CNFieldWindow.Type.extended );
      RenderFieldObjects( "Bodies B", FieldControllerB, true, false, CNFieldWindow.Type.extended );

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
            
      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      RenderFieldObjects( "Attentive Nodes", FieldController, true, false, CNFieldWindow.Type.extended );
      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawTriggeringMode();
      EditorGUILayout.Space();
      DrawSpeedMinN();
      DrawSpeedMinT();
      EditorGUI.EndDisabledGroup();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }
  }

}
