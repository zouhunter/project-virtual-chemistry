using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public abstract class CNEntityEditor : CNMonoFieldEditor
  {

    public void CreateEntity()
    {
      if (!Data.IsNodeExcludedInHierarchy)
      {
        CreateEntitySpec();
        LoadState();
      }
    }

    public void ApplyEntity()
    {
      if (!Data.IsNodeExcludedInHierarchy)
      {
        ApplyEntitySpec();
        LoadState();
      }
    }

    public abstract void CreateEntitySpec();

    public abstract void ApplyEntitySpec();

    new CNEntity Data { get; set; }

    public CNEntityEditor( CNEntity data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNEntity)data;
    }

    public override void Init()
    {
      base.Init();
      
      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry 
                                          | CNField.AllowedTypes.BodyNode;

      FieldController.SetFieldType(allowedTypes);
      FieldController.IsBodyField = true;
    }

    public override void FreeResources()
    {
      FieldController.DestroyField();
      eManager.DestroyEntity( Data );
    }

    public override void SetActivityState()
    {
      base.SetActivityState();
      eManager.SetActivity(Data);
    }

    public override void SetVisibilityState()
    {
      base.SetVisibilityState();
      eManager.SetVisibility(Data);
    }

    public override void SetExcludedState()
    {
      base.SetExcludedState();
      if (Data.IsNodeExcludedInHierarchy)
      {
        eManager.DestroyEntity(Data);
      }
      else
      {
        CreateEntity();
      }
    }    

    protected void DrawTimer()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Timer (s)", Data.Timer );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change timer - " + Data.Name);
        Data.Timer = value;
        EditorUtility.SetDirty(Data);
      }
    }
  }

}

