using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace CaronteFX
{
  public abstract class CNMonoFieldEditor : CommandNodeEditor
  {
    protected CNFieldController FieldController { get; set; }  
 
    new CNMonoField Data { get; set; }

    public CNMonoFieldEditor( CNMonoField data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNMonoField)data;
    }
    //----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();
      FieldController = new CNFieldController(Data, Data.Field, eManager, goManager);
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldController.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldController.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldController.BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      return (Data.Field.RemoveNode(node));
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldController.SetScopeId(scopeId);
    }
    //-----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldController.DestroyField();
    }
    //-----------------------------------------------------------------------------------
    public void ClearField()
    {
      FieldController.Clear(false);
    }
    //-----------------------------------------------------------------------------------
    public virtual void AddGameObjects( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldController.AddGameObjects(draggedObjects, recalculateFields);
    }
    //-----------------------------------------------------------------------------------
    public void AddWildcard(string wildcardName, bool recalculateFields)
    {
      FieldController.AddNameSelector( wildcardName, recalculateFields);
    }
    //----------------------------------------------------------------------------------
    public virtual List<GameObject> GetGameObjects()
    {
      return FieldController.GetUnityGameObjects().ToList();
    }
    //----------------------------------------------------------------------------------
    public virtual List<GameObject> GetGameObjectsTopMost()
    {
      return FieldController.GetUnityGameObjectsTopMost().ToList();
    }
    //----------------------------------------------------------------------------------
  }
}

