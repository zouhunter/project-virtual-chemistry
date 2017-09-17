using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNSubstituterEditor : CNEntityEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
    
    CNFieldController FieldControllerA { get; set; }
    CNFieldController FieldControllerB { get; set; }

    new CNSubstituter Data { get; set; }
    public CNSubstituterEditor( CNSubstituter data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNSubstituter)data;
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
      FieldControllerA.RestoreFieldInfo();
      FieldControllerB.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldControllerA.StoreFieldInfo();
      FieldControllerB.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldControllerA.BuildListItems();
      FieldControllerB.BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      bool removedfromA = Data.FieldA.RemoveNode(node);
      bool removedFromB = Data.FieldB.RemoveNode(node);
      return ( removedfromA || removedFromB );
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldControllerA.SetScopeId(scopeId);
      FieldControllerB.SetScopeId(scopeId);
    }
    //----------------------------------------------------------------------------------
    public override void FreeResources()
    {
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
      eManager.CreateSubstituter( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      GameObject[]  arrGameObjectA = FieldControllerA.GetUnityGameObjects();
      GameObject[]  arrGameObjectB = FieldControllerB.GetUnityGameObjects();
      
      eManager.RecreateSubstituter( Data, arrGameObjectA, arrGameObjectB );
    }
    //----------------------------------------------------------------------------------
    private void DrawProbability()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider( "Probability", Data.Probability, 0f, 1f ); 
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change probability - " + Data.Name);
        Data.Probability = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawProbabilitySeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = (uint)EditorGUILayout.IntField( "Probability seed", (int)Data.ProbabilitySeed );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change probability seed - " + Data.Name);
        Data.ProbabilitySeed = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Originals", FieldControllerA , true, false, CNFieldWindow.Type.extended );
      EditorGUILayout.Space();
      RenderFieldObjects( "Substitutes", FieldControllerB, true, false, CNFieldWindow.Type.extended );

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawTimer();
      EditorGUILayout.Space();
      DrawProbabilitySeed();
      DrawProbability();
      
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }
  }

}
