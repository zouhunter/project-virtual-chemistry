using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CommandNodeEditorState
  {
    public bool isNodeEnabled_;
    public bool isNodeVisible_;
    public bool isNodeExcluded_;
  }

  public abstract class CommandNodeEditor : IView, IFieldEditor
  {
    public abstract Texture TexIcon { get; }

    protected const float label_width       = 180f;
    protected const float short_label_width = 60f;
    protected const float long_label_width  = 100f;
    protected const float width_indent      = 2f;

    protected const float simple_space  = 10f;
    protected const float double_space  = 20f;
    protected const float tex_icon_size = 12f;

    private UnityEngine.Object[] emptySelection_ = new UnityEngine.Object[0];

    private float objects_rect_width = 1f;

    GUIStyle styleTitle = new GUIStyle(EditorStyles.label);

    protected Vector2 scroller_;

    protected string currentFocusedControlName_ = string.Empty;
    protected string lastFocusedControlName_    = string.Empty;

    protected bool Enabled
    {
      set
      {
        if (Data.IsNodeEnabled != value)
        {
          Data.IsNodeEnabled = value;
          EditorUtility.SetDirty(Data);
          SetActivityState();
        }
      }
    }

    protected bool Visible
    {
      set
      {
        if (Data.IsNodeVisible != value)
        {
          Data.IsNodeVisible = value;
          EditorUtility.SetDirty(Data);
          SetVisibilityState();
        }
      }
    }

    #region IFieldEditor methods
    public virtual void LoadInfo()
    {

    }

    public virtual void StoreInfo()
    {

    }

    public virtual void BuildListItems()
    {

    }

    public virtual bool RemoveNodeFromFields(CommandNode node)
    {
      return false;
    }

    public virtual void SetScopeId(uint scopeId)
    {

    }
    #endregion

    #region IView methods
    public abstract void RenderGUI( Rect area, bool isEditable );
    #endregion

    public virtual void FreeResources()
    {

    }

    public void ResetState()
    {
      state_.isNodeEnabled_  = IsEnabled;
      state_.isNodeVisible_  = IsVisible;
      state_.isNodeExcluded_ = IsExcluded;
    }

    protected virtual void LoadState()
    {
      state_.isNodeEnabled_  = IsEnabled;
      state_.isNodeVisible_  = IsVisible;
      state_.isNodeExcluded_ = IsExcluded;
    }

    public virtual void ValidateState()
    {
      if ( IsEnabled != state_.isNodeEnabled_ )
      {
        SetActivityState();
        state_.isNodeEnabled_ = IsEnabled;
      }

      if ( IsVisible != state_.isNodeVisible_ )
      {
        SetVisibilityState();
        state_.isNodeVisible_ = IsVisible;
      }

      if (IsExcluded != state_.isNodeExcluded_ )
      {
        SetExcludedState();
        state_.isNodeExcluded_ = IsExcluded;
      }
    }

    public bool IsEnabled
    {
      get
      {
        return Data.IsNodeEnabledInHierarchy;
      }
    }

    public bool IsVisible
    {
      get
      {
        return Data.IsNodeVisibleInHierarchy;
      }
    }

    public bool IsExcluded
    {
      get
      {
        return Data.IsNodeExcludedInHierarchy;
      }
    }

    protected CNManager       cnManager  { get; private set; }
    protected CNHierarchy    cnHierarchy { get; private set; }
    protected CREntityManager eManager   { get; private set; }
    protected CRGOManager     goManager  { get; private set; }

    protected CommandNodeEditorState state_;
    public CommandNode Data { get; set; }

    public CommandNodeEditor( CommandNode data, CommandNodeEditorState state )
    {
      cnManager   = CNManager.Instance;
      cnHierarchy = cnManager.Hierarchy;
      eManager    = cnManager.EntityManager;
      goManager   = cnManager.GOManager;

      Data   = data;
      state_ = state;

      styleTitle.fontSize = 20;
      styleTitle.fontStyle = FontStyle.Bold;
      styleTitle.alignment = TextAnchor.LowerLeft;
    }
    //-----------------------------------------------------------------------------------
    public virtual void Init()
    {
      LoadState();
    }
    //-----------------------------------------------------------------------------------
    public virtual void SceneSelection()
    {
      Selection.objects = emptySelection_;
    }
    //-----------------------------------------------------------------------------------
    public virtual void SetActivityState()
    {
      state_.isNodeEnabled_ = IsEnabled;
    }
    //-----------------------------------------------------------------------------------
    public virtual void SetVisibilityState()
    {
      state_.isNodeVisible_ = IsVisible;
    }
    //-----------------------------------------------------------------------------------
    public virtual void SetExcludedState()
    {
      state_.isNodeExcluded_ = IsExcluded;
    }
    //-----------------------------------------------------------------------------------
    protected void RenderTitle(bool isEditable, bool drawEnabledToggle = true, bool drawVisibleToggle = true, bool isDebugRender = false)
    {
      currentFocusedControlName_ = GUI.GetNameOfFocusedControl();

      EditorGUILayout.Space();
      GUILayout.BeginHorizontal();

      string title;
      if ( Data.NeedsUpdate )
        title = Data.Name + "(*)";
      else
        title = Data.Name;

      GUILayout.Space(5f);
      Rect iconRect = GUILayoutUtility.GetRect(30f, 30f);
      GUI.Label(iconRect, new GUIContent( TexIcon ) );
      EditorGUI.BeginDisabledGroup(!Data.IsNodeEnabledInHierarchy);
      GUILayout.Label( new GUIContent( title ), styleTitle );
      EditorGUI.EndDisabledGroup();
      
      GUILayout.FlexibleSpace();

      EditorGUILayout.BeginVertical();
      EditorGUILayout.Space();

      EditorGUI.BeginChangeCheck();

      EditorGUI.BeginDisabledGroup( !(isEditable) );
      float width =  ( (Data.IsNodeEnabled == Data.IsNodeEnabledInHierarchy) && (Data.IsNodeVisible == Data.IsNodeVisibleInHierarchy) ) ? 100f : 170f;
      if (drawEnabledToggle)
      {
       string nameEnabled = (Data.IsNodeEnabled == Data.IsNodeEnabledInHierarchy) ? "Enabled" : "Enabled (Overidden to off)";   
       Enabled = EditorGUILayout.ToggleLeft( nameEnabled, Data.IsNodeEnabled, GUILayout.Width(width) );
      }
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup( !(isEditable || isDebugRender) );
      if (drawVisibleToggle)
      {
        string nameVisible = (Data.IsNodeVisible == Data.IsNodeVisibleInHierarchy) ? "Visible" : "Visible (Overidden to off)";
        Visible = EditorGUILayout.ToggleLeft( nameVisible, Data.IsNodeVisible, GUILayout.Width(width) );
      }
      EditorGUI.EndDisabledGroup();

      if (EditorGUI.EndChangeCheck() )
      {
        SceneView.RepaintAll();
      }

      EditorGUILayout.EndVertical();
      GUILayout.EndHorizontal();

      CRGUIUtils.Splitter();

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }
    //-----------------------------------------------------------------------------------
    public void RenderFieldObjects( string labelText, CNFieldController fieldController, bool enabled, bool showScope, CNFieldWindow.Type windowType )
    {
      if (objects_rect_width == 1)
      {
        CRManagerEditor.RepaintIfOpen();
      }

      EditorGUI.BeginDisabledGroup( !enabled );
      Event ev         = Event.current;
      EventType evType = ev.type;
      
      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.LabelField( labelText, GUILayout.MaxWidth(showScope ? short_label_width : long_label_width) );
 
      GUILayout.Label( "", EditorStyles.objectField );
      Rect objectsRect = GUILayoutUtility.GetLastRect();

      if (evType == EventType.Repaint)
      {
        objects_rect_width = objectsRect.width;
      }

      fieldController.DrawFieldItems( objectsRect, tex_icon_size );

      if ( evType == EventType.MouseDown && ev.button == 0 &&
           objectsRect.Contains(ev.mousePosition) )
      {
        if (windowType == CNFieldWindow.Type.normal)
        {
          CNFieldWindowSmall.ShowWindow<CNFieldWindowSmall>( labelText, fieldController, this );
        }
        else if ( windowType == CNFieldWindow.Type.extended)
        {
          CNFieldWindowBig.ShowWindow<CNFieldWindowBig>( labelText, fieldController, this );
        }
      }
      if (showScope)
      {
        GUILayout.Space( 10f );
        EditorGUILayout.LabelField("Scope", GUILayout.Width( 40f ) );
        CNField.ScopeFlag auxScope = fieldController.GetScopeType();
        EditorGUI.BeginChangeCheck();
        auxScope = (CNField.ScopeFlag) EditorGUILayout.EnumPopup( auxScope, GUILayout.Width(70f) );
        if (EditorGUI.EndChangeCheck())
        {
          fieldController.SetScopeType( auxScope );
          cnHierarchy.RecalculateFieldsDueToUserAction();
          EditorUtility.SetDirty(Data);
        }
      }  
      ProccesEvents( ev, evType, fieldController, objectsRect );
      EditorGUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------
    private void ProccesEvents( Event ev, EventType evType, CNFieldController fieldController, Rect FieldBox )
    {
      switch (evType)
      {
        case EventType.ContextClick:
          if ( FieldBox.Contains( ev.mousePosition ) )
          {
            Rect position = CRManagerEditor.Instance.position;
            ShowContextMenu( fieldController, position );
          }
          break;

        case EventType.DragUpdated:
        if ( FieldBox.Contains( ev.mousePosition ) )
        { 
          if ( FieldIsValidDragTarget() )
          {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          }
          ev.Use();
        }
        break;
      
        case EventType.DragPerform:
        {
          if ( FieldBox.Contains(ev.mousePosition) )
          {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;
            if (objects != null && objects.Length > 0)
            {
              fieldController.AddDraggedObjects(-1, objects);
              ev.Use();
            }
          }
          DragAndDrop.AcceptDrag();   
        }
        break;
      
        case EventType.DragExited:
        DragAndDrop.PrepareStartDrag();
        break;
      }
    }
    //-----------------------------------------------------------------------------------
    private bool FieldIsValidDragTarget()
    {
      UnityEngine.Object[] objects = DragAndDrop.objectReferences;
 
      if ( (objects != null && CREditorUtils.CheckIfAnySceneGameObjects( objects ) ) )
      {
        return true;
      }

      return false;
    }
    //-----------------------------------------------------------------------------------
    private void ShowContextMenu(CNFieldController fieldController, Rect FieldBox )
    {
      GenericMenu optionsMenu = new GenericMenu();
      fieldController.FillOptionsMenu( optionsMenu );
      optionsMenu.ShowAsContext();
    }
  }
}
