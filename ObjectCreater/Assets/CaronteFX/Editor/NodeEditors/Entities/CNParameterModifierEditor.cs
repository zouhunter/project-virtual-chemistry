using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{

  public class CNParameterModifierEditor : CNEntityEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    protected string[] properties = new string[] { "Velocity Linear", "Velocity Angular", "Enabled", "Plasticity", "Freeze", "Visible", "Force Multiplier"  };
    protected string[] onOffFlip  = new string[] { "Off", "On", "Flip" };


    new CNParameterModifier Data { get; set; }

    public CNParameterModifierEditor( CNParameterModifier data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNParameterModifier)data;
    }

    public override void Init()
    {
      base.Init();
  
      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Bodies 
                                          | CNField.AllowedTypes.JointServosNode
                                          | CNField.AllowedTypes.DaemonNode
                                          | CNField.AllowedTypes.TriggerNode;

      FieldController.SetFieldType(allowedTypes);

      List<ParameterModifierCommand> listPmCommand = Data.ListPmCommand;
      if (listPmCommand.Count == 0)
      {
        listPmCommand.Add( new ParameterModifierCommand() );
      }
    }

    public override void CreateEntitySpec()
    {
      eManager.CreateParameterModifier( Data );
    }

    public override void ApplyEntitySpec()
    {
      GameObject[] arrGameObject   = FieldController.GetUnityGameObjects();
      CommandNode[] arrCommandNode = FieldController.GetCommandNodes();

      eManager.RecreateParameterModifier( Data, arrGameObject, arrCommandNode );
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.extended );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();
      
      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup( !isEditable );
      List<ParameterModifierCommand> listPmCommand = Data.ListPmCommand;

      EditorGUILayout.Space();

      DrawTimer();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Object parameters to modify: ");

      CRGUIUtils.Splitter();
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      GUILayout.Space(10f);

      ParameterModifierCommand pmCommandToRemove = null;
      ParameterModifierCommand pmCommandToAdd = null;
      int addPosition = 0;


      int nPmCommand = listPmCommand.Count;

      for( int i = 0; i < nPmCommand; i++ )
      {
        ParameterModifierCommand pmCommand = listPmCommand[i];
        DrawPmCommand(i, pmCommand, ref pmCommandToRemove, ref pmCommandToAdd, ref addPosition );
      }

      if (pmCommandToRemove != null && listPmCommand.Count > 1)
      {
        Undo.RecordObject(Data, "Remove parameter - " + Data.Name);
        listPmCommand.Remove( pmCommandToRemove );
        pmCommandToRemove = null;
        EditorUtility.SetDirty(Data);
      }
      if (pmCommandToAdd != null )
      {
        Undo.RecordObject(Data, "Add parameter - " + Data.Name);
        listPmCommand.Insert( addPosition, pmCommandToAdd );
        pmCommandToAdd = null;
        EditorUtility.SetDirty(Data);
      }


      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }

    private void DrawPmCommand(int commandNumber, ParameterModifierCommand pmCommand, ref ParameterModifierCommand pmCommandToRemove, ref ParameterModifierCommand pmCommandToAdd, ref int addPosition)
    {
      EditorGUILayout.BeginHorizontal();

      EditorGUI.BeginChangeCheck();
      var valueProperty = (PARAMETER_MODIFIER_PROPERTY)EditorGUILayout.Popup( (int)pmCommand.target_, properties, GUILayout.Width( 150f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change parameter type - " + Data.Name);
        pmCommand.target_ = valueProperty;
        EditorUtility.SetDirty(Data);
      }

      GUILayout.Space(30f);

      switch (pmCommand.target_)
      {
        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL:
        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Vector3Field( "", pmCommand.valueVector3_, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.valueVector3_  = value;
              EditorUtility.SetDirty(Data);
            }
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.ACTIVITY:
        case PARAMETER_MODIFIER_PROPERTY.VISIBILITY:
        case PARAMETER_MODIFIER_PROPERTY.FREEZE:
        case PARAMETER_MODIFIER_PROPERTY.PLASTICITY:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Popup( pmCommand.valueInt_, onOffFlip, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.valueInt_ = (int)value;
              EditorUtility.SetDirty(Data);
            }
          
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.FloatField( pmCommand.valueVector3_.x, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.valueVector3_.x = value;
              EditorUtility.SetDirty(Data);
            }
            break;
          }


        default:
          throw new NotImplementedException();
      }

      GUILayout.Space(30f);

      if ( GUILayout.Button( new GUIContent("-", "delete"), EditorStyles.miniButtonLeft, GUILayout.Width(25f) ) )
      {
        pmCommandToRemove = pmCommand;
      }
      if ( GUILayout.Button( new GUIContent("+", "Add"), EditorStyles.miniButtonRight, GUILayout.Width(25f) ) )
      {
        pmCommandToAdd = new ParameterModifierCommand();
        addPosition = commandNumber + 1;
      }

      EditorGUILayout.EndHorizontal();

      if (pmCommand.target_ != PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL && pmCommand.target_ != PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR)
      {
        GUILayout.Space(16f);
      }
    }
  }

 }
