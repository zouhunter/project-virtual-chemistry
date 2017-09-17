using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNServosEditor : CommandNodeEditor
  {
    public static Texture icon_texture_cb_;

    public static Texture icon_motor_linear_;
    public static Texture icon_motor_angular_;
    public static Texture icon_servo_linear_;
    public static Texture icon_servo_angular_;

    public override Texture TexIcon
    {
      get
      { 
        if (Data.IsLinearOrAngular)
        {
          if (Data.IsPositionOrVelocity)
          {
            return icon_servo_linear_;
          }
          else
          {
            return icon_motor_linear_;
          }
        }
        else
        {
          if (Data.IsPositionOrVelocity)
          {
            return icon_servo_angular_;
          }
          else
          {
            return icon_motor_angular_;
          }
        }
      } 
    }


    bool unlimitedAction_;
    bool UnlimitedAction
    {
      set
      {
        if (value != unlimitedAction_)
        {
          unlimitedAction_ = value;
          Data.IsBlockedX = value;
          Data.IsBlockedY = value;
          Data.IsBlockedZ = value;

          Data.NeedsUpdate = true;
          EditorUtility.SetDirty( Data );
        }
      }
    }



    protected CNFieldController FieldControllerA { get; set; }
    protected CNFieldController FieldControllerB { get; set; }

    protected new CNServos Data { get; set; }

    //-----------------------------------------------------------------------------------
    public CNServosEditor( CNServos data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNServos)data;
    }
    //-----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry
                                          | CNField.AllowedTypes.RigidBodyNode;

      FieldControllerA = new CNFieldController( Data, Data.ObjectsA, eManager, goManager );
      FieldControllerA.SetFieldType( allowedTypes );
      FieldControllerA.SetCalculatesDiff(true);
      FieldControllerA.IsBodyField = true;

      FieldControllerB = new CNFieldController( Data, Data.ObjectsB, eManager, goManager );
      FieldControllerB.SetFieldType( allowedTypes );
      FieldControllerB.SetCalculatesDiff(true);
      FieldControllerB.IsBodyField = true;
    }
    //-----------------------------------------------------------------------------------
    public void CreateEntities()
    {
      if ( !Data.IsNodeExcludedInHierarchy ) 
      {
        GameObject[] arrGameObjectA;
        GameObject[] arrGameObjectB;

        GetFieldGameObjects( FieldControllerA, out arrGameObjectA );
        GetFieldGameObjects( FieldControllerB, out arrGameObjectB );

        eManager.CreateServos( Data, arrGameObjectA, arrGameObjectB );

        LoadState();
      }
    }
    //-----------------------------------------------------------------------------------
    public virtual void DestroyEntities()
    {
      GameObject[] arrGameObjectA;
      GameObject[] arrGameObjectB;

      GetFieldGameObjects( FieldControllerA, out arrGameObjectA );
      GetFieldGameObjects( FieldControllerB, out arrGameObjectB );

      eManager.DestroyServos( Data, arrGameObjectA, arrGameObjectB );
    }
    //-----------------------------------------------------------------------------------
    public virtual void RecreateEntities()
    {
      DestroyEntities();
      CreateEntities();
    }
    //-----------------------------------------------------------------------------------
    public void EditEntites()
    {
      eManager.EditServos( Data );
    }
    //-----------------------------------------------------------------------------------
    private void GetFieldGameObjects( CNFieldController fieldController, out GameObject[] arrGameObject )
    {
      arrGameObject = fieldController.GetUnityGameObjects();
    }
    //-----------------------------------------------------------------------------------
    public override void SetActivityState()
    {
      base.SetActivityState();
      eManager.SetActivity( Data );
    }
    //-----------------------------------------------------------------------------------
    public override void SetExcludedState()
    {
      base.SetExcludedState();
      if (IsExcluded)
      {
        DestroyEntities();
        EditorUtility.ClearProgressBar();
      }
      else
      {
        CreateEntities();
        EditorUtility.ClearProgressBar();
      }
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToA( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerA.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToB( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerB.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldControllerA.DestroyField();
      FieldControllerB.DestroyField();
      DestroyEntities();
    }
    //-----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldControllerA.RestoreFieldInfo();
      FieldControllerB.RestoreFieldInfo();
    }
    //-----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldControllerA.StoreFieldInfo();
      FieldControllerB.StoreFieldInfo();
    }
    //-----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldControllerA.BuildListItems();
      FieldControllerB.BuildListItems();
    }
    //-----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldControllerA.SetScopeId( scopeId );
      FieldControllerB.SetScopeId( scopeId );
    }
    //-----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields(CommandNode node)
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");

      bool removedNodeA = Data.ObjectsA.RemoveNode(node);
      bool removedNodeB = Data.ObjectsB.RemoveNode(node);

      bool removed = removedNodeA || removedNodeB;
      if (removed)
      {
        Data.NeedsUpdate = true;
      }

      return removed;
    }
    //-----------------------------------------------------------------------------------
    public void CheckUpdate()
    {

      bool updateNeeded = (   FieldControllerA.IsUpdateNeeded() ||
                              FieldControllerB.IsUpdateNeeded()   );

      if (updateNeeded)
      {
        Data.NeedsUpdate = true;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    public override void RenderGUI( Rect area, bool isEditable )
    {
      GUILayout.BeginArea( area );
      
      RenderTitle(isEditable);
        
      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        RenderFieldObjects( "ObjectsA", FieldControllerA, true, true, CNFieldWindow.Type.extended );
        RenderFieldObjects( "ObjectsB", FieldControllerB, true, true, CNFieldWindow.Type.extended );

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button(Data.NeedsUpdate ? "Create/Recreate(*)" : "Create/Recreate", GUILayout.Height(30f)))
        {
          RecreateEntities();
        }
      }
      EditorGUI.EndDisabledGroup();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;
      bool currentMode = EditorGUIUtility.wideMode;
      EditorGUIUtility.wideMode = true;

      EditorGUI.BeginDisabledGroup(!isEditable);
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUI.BeginChangeCheck();
      RenderCreationParams();
      if( EditorGUI.EndChangeCheck() && eManager.IsServosCreated(Data) )
      {
        DestroyEntities();
      }

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      RenderEditParams();

      EditorGUIUtility.labelWidth = originalLabelwidth;
      EditorGUIUtility.wideMode = currentMode;

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();
      GUILayout.EndArea();
    } // RenderGUI

    //-----------------------------------------------------------------------------------
    private void DrawIsBlocked()
    {
      EditorGUILayout.LabelField( "Blocked: ", GUILayout.Width( 60f ) );
      EditorGUILayout.LabelField("X", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck();
      var IsBlockedX = EditorGUILayout.Toggle( "", Data.IsBlockedX, GUILayout.Width( 15f ) );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change blocked x - " + Data.Name);
        Data.IsBlockedX = IsBlockedX;
        if (IsBlockedX)
        {
          Data.IsFreeX = !IsBlockedX;
        }
        EditorUtility.SetDirty(Data);
      }
 
      EditorGUILayout.LabelField("Y", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck();
      var IsBlockedY = EditorGUILayout.Toggle( "", Data.IsBlockedY, GUILayout.Width( 15f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change blocked y - " + Data.Name);
        Data.IsBlockedY = IsBlockedY;
        if (IsBlockedY)
        {
          Data.IsFreeY = !IsBlockedY;
        }
        EditorUtility.SetDirty(Data);
      }

      EditorGUILayout.LabelField("Z", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck();
      var IsBlockedZ = EditorGUILayout.Toggle( "", Data.IsBlockedZ, GUILayout.Width( 15f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change blocked z - " + Data.Name);
        Data.IsBlockedZ = IsBlockedZ;
        if (IsBlockedZ)
        {
          Data.IsFreeZ = !IsBlockedZ;
        }

        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawIsFree()
    {
      EditorGUILayout.LabelField( "Free: ", GUILayout.Width( 60f ) );
      EditorGUILayout.LabelField("X", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck(); 
      var IsFreeX = EditorGUILayout.ToggleLeft( "", Data.IsFreeX, GUILayout.Width( 15f ) );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change free x - " + Data.Name);
        Data.IsFreeX = IsFreeX;
        if (IsFreeX)
        {
          Data.IsBlockedX = !IsFreeX;
        }
        EditorUtility.SetDirty(Data);
      }

      EditorGUILayout.LabelField("Y", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck();
      var IsFreeY = EditorGUILayout.ToggleLeft( "", Data.IsFreeY, GUILayout.Width( 15f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change free y - " + Data.Name);
        Data.IsFreeY = IsFreeY;
        if (IsFreeY)
        {
          Data.IsBlockedY = !IsFreeY;
        }
        EditorUtility.SetDirty(Data);
      }

      EditorGUILayout.LabelField("Z", GUILayout.Width( 15f ));
      EditorGUI.BeginChangeCheck();
      var IsFreeZ = EditorGUILayout.ToggleLeft( "", Data.IsFreeZ, GUILayout.Width( 15f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change free z - " + Data.Name);
        Data.IsFreeZ = IsFreeZ;
        if (IsFreeZ)
        {
          Data.IsBlockedZ = !IsFreeZ;
        }
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawUnlimitedAction()
    {
      if (Data.IsBlockedX && Data.IsBlockedY && Data.IsBlockedZ)
      {
        unlimitedAction_ = true;
      }
      else
      {
        unlimitedAction_ = false;
      }

      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( "Unlimited Action (Blocked XYZ)", unlimitedAction_ );
      if(EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change unlimited action (Blocked XYZ) -  " + Data.Name);
        UnlimitedAction = value;
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDisableCollisionByPairs()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( "Disable collisions by pairs", Data.DisableCollisionByPairs);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change disable collision by pairs - " + Data.Name);
        Data.DisableCollisionByPairs = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RenderCreationParams()
    {
      if (Data.IsLinearOrAngular)
      {
        EditorGUILayout.BeginHorizontal();
        DrawIsBlocked();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawIsFree();
        EditorGUILayout.EndHorizontal();
      }
      else
      {
        DrawUnlimitedAction(); 
      }
      EditorGUILayout.Space(); 
      DrawDisableCollisionByPairs();
    }
    //-----------------------------------------------------------------------------------
    private string GetTargetFieldName()
    {
      
      string targetFieldName = string.Empty;

      if (Data.IsLinearOrAngular)
      {
        if (Data.IsPositionOrVelocity)
        {
          targetFieldName = "Target position";
        }
        else
        {
          targetFieldName = "Target velocity";
        }
      }
      else
      {
        if (Data.IsPositionOrVelocity)
        {
          targetFieldName = "Target angular position";
        }
        else
        {
          targetFieldName = "Target angular velocity";
        }
      }
      
      return targetFieldName;
    }
    //-----------------------------------------------------------------------------------
    private void DrawTargetExternalLocal()
    {
      string targetFieldName = GetTargetFieldName();

      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field( targetFieldName, Data.TargetExternal_LOCAL );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + targetFieldName + Data.Name);
        Data.TargetExternal_LOCAL = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void GetForceFieldNames( out string fieldNameForce, out string fieldNameBrakeForce )
    {
      if (Data.IsLinearOrAngular)
      {
        fieldNameForce      = "Max. force";
        fieldNameBrakeForce = "Max. brake force";
      }
      else
      {
        fieldNameForce      = "Max. torque";
        fieldNameBrakeForce = "Max. brake torque";
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawSpeedMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Max. speed", Data.SpeedMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max speed - " + Data.Name);
        Data.SpeedMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawMaximumSpeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "Maximum", Data.MaximumSpeed, GUILayout.Width(80f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximum - " + Data.Name);
        Data.MaximumSpeed = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawPowerMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Max. power", Data.PowerMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max speed - " + Data.Name);
        Data.PowerMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawMaximumPower()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "Maximum", Data.MaximumPower, GUILayout.Width(80f));
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximum - " + Data.Name);
        Data.MaximumPower = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawForceMax(string fieldNameForce)
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( fieldNameForce, Data.ForceMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + fieldNameForce + " - " + Data.Name);
        Data.ForceMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawMaximumForce()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "Maximum", Data.MaximumForce, GUILayout.Width(80f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximun - " + Data.Name);
        Data.MaximumForce = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void BrakePowerMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Max. brake power", Data.BrakePowerMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max brake power - " + Data.Name);
        Data.BrakePowerMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawMaximumBrakePowerMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "Maximum", Data.MaximumBrakePowerMax, GUILayout.Width(80f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximum - " + Data.Name);
        Data.MaximumBrakePowerMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBrakeForceMax(string fieldNameBrakeForce)
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( fieldNameBrakeForce, Data.BrakeForceMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + fieldNameBrakeForce + " - " + Data.Name);
        Data.BrakeForceMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawMaximumBrakeForce()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "Maximum", Data.MaximumBrakeForceMax, GUILayout.Width(80f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximum - " + Data.Name);
        Data.MaximumBrakeForceMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDampingForce()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Damping force", Data.DampingForce );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change damping force - " + Data.Name);
        Data.DampingForce = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDistanceRange()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Distance range", Data.DistStepToDefineMultiplierDependingOnDist);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change distance range - " + Data.Name);
        Data.DistStepToDefineMultiplierDependingOnDist = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDistanceFunction()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.CurveField( "Distance function", Data.FunctionMultiplierDependingOnDist, Color.blue, new Rect( 0f, 0f, 1f, 1f) ); 
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change distance function - " + Data.Name);
        Data.FunctionMultiplierDependingOnDist = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBreakIfDistExceeded()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( "Break if distance exceeded", Data.IsBreakIfDist );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break if distance exceeded - " + Data.Name);
        Data.IsBreakIfDist = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBreakDistance()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Break distance", Data.BreakDistance );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break distance - " + Data.Name);
        Data.BreakDistance = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBreakIfAngle()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( "Break if angle exceeded", Data.IsBreakIfAng );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break if angle exceeded - " + Data.Name);
        Data.IsBreakIfAng = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBreakAngle()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Break angle", Data.BreakDistance );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( Data, "Change break angle - " + Data.Name);
        Data.BreakAngleInDegrees = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RenderEditParams()
    {      
      DrawTargetExternalLocal();
      EditorGUILayout.Space();

      //Data.ReactionTime         = EditorGUILayout.FloatField( "Reaction time", Data.ReactionTime );
      //Data.OverreactionDelta    = EditorGUILayout.FloatField( "Overreaction delta", Data.OverreactionDelta );

      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup( unlimitedAction_ );

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup( Data.MaximumSpeed );
      DrawSpeedMax();
      EditorGUI.EndDisabledGroup();
      DrawMaximumSpeed();   
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup( Data.MaximumPower );
      DrawPowerMax();
      EditorGUI.EndDisabledGroup();
      DrawMaximumPower();
      EditorGUILayout.EndHorizontal();

      string fieldNameForce;
      string fieldNameBrakeForce;
      GetForceFieldNames( out fieldNameForce, out fieldNameBrakeForce );

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup( Data.MaximumForce );
      DrawForceMax(fieldNameForce);
      EditorGUI.EndDisabledGroup();
      DrawMaximumForce();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup( Data.MaximumBrakePowerMax );
      BrakePowerMax();
      EditorGUI.EndDisabledGroup();
      DrawMaximumBrakePowerMax();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup( Data.MaximumBrakeForceMax );
      DrawBrakeForceMax(fieldNameBrakeForce);
      EditorGUI.EndDisabledGroup();
      DrawMaximumBrakeForce();
      EditorGUILayout.EndHorizontal();
      

      EditorGUILayout.Space();
      DrawDampingForce();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
     
      Data.MultiplierFoldOut = EditorGUILayout.Foldout(Data.MultiplierFoldOut, "Multiplier depending on distance");
      
      if (Data.MultiplierFoldOut)
      {
        GUILayout.Space( 10f );
        DrawDistanceRange();
        DrawDistanceFunction();     
      }

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      Data.BreakFoldout = EditorGUILayout.Foldout( Data.BreakFoldout, "Break");

      if (Data.BreakFoldout)
      {       
        GUILayout.Space( 10f );

        DrawBreakIfDistExceeded();
        EditorGUI.BeginDisabledGroup( !Data.IsBreakIfDist );
        DrawBreakDistance();
        EditorGUI.EndDisabledGroup();

        DrawBreakIfAngle();
        EditorGUI.BeginDisabledGroup( !Data.IsBreakIfAng );
        DrawBreakAngle();
        EditorGUI.EndDisabledGroup();      
      }

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

    }
  } 
}// CaronteFX

