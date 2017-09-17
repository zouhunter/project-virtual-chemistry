using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace CaronteFX
{

public class CustomDragData
{
  public int originalIndex;
  public CRListBox originalList;
}

public class CRListBox : IListView
{ 

  //----------------------------------------------------------------------------------
  public IListController Controller { get; private set; }

  public bool Focused { get; set; } 
  public int  CurrentMouseDragItem { get; set; }
  public bool EdgeDrag { get; set; }

  //----------------------------------------------------------------------------------
  public float itemHeight_;

  public Color currentGUIColor_;
  public Color currentContentColor_;
  public Color backgroundColor_;
  public Color focusedSelectedItemColor_;
  public Color unfocusedSelectedItemColor_;
  public Color dragToItemColor_;
  public Color dragFromItemColor_;

  public Color textColorSelected_;
  public Color textColorUnselected_;
  //----------------------------------------------------------------------------------
  public string   dragDropIdentifier_;

  private Vector2 scrollPos_;
  private int     draggingItemIdx_;
  private bool    isDragging_;
  private bool    itemEditedClicked_;
  private bool    isInternalDrag_;
  private bool    dragOverAnyItem_;

  private bool    isTreeList_;

  private GUIStyle textFieldStyle_ = new GUIStyle( EditorStyles.textField );
  private GUIStyle foldoutStyle_   = new GUIStyle( EditorStyles.foldout   );   
  private GUIStyle iconStyle_      = new GUIStyle( EditorStyles.label     );

  private float contentWidth_  = 0f;
  private float contentHeight_ = 0f;
  private float toggleWidth_   = 18f;
  private float iconWidth_     = 22f;
  private float pixelSpace_    = 10f;
  private float margin_        = 10f;
  //----------------------------------------------------------------------------------
  public CRListBox( IListController controller, string dragDropIdentifier, bool isTreeList )
  {
    Controller            = controller;
    Focused               = true;
    dragDropIdentifier_   = dragDropIdentifier;
    CurrentMouseDragItem  = -1;
    draggingItemIdx_      = -1;
    isDragging_           = false;
    itemHeight_           = 20.0f;
    itemEditedClicked_    = false;
    dragOverAnyItem_      = false;
    isTreeList_           = isTreeList;

    if (!isTreeList_)
    {
      toggleWidth_ = 0f;
    }

    currentGUIColor_      = GUI.color;
    currentContentColor_  = GUI.contentColor;
    backgroundColor_      = Color.white;
    
    bool isPro = UnityEditorInternal.InternalEditorUtility.HasPro();
    if (isPro)
    {
      focusedSelectedItemColor_   = Color.blue;
      unfocusedSelectedItemColor_ = new Color( 0.5f, 0.5f, 0.5f, 0.3f );
      dragToItemColor_            = new Color( 0.4f, 0.4f, 0f );
      textColorUnselected_        = new Color( 0.8f, 0.8f, 0.8f, 1.0f);
      textColorSelected_          = Color.white;
    }
    else
    {
      focusedSelectedItemColor_   = new Color( 205f/255f, 1.0f, 195f/255f, 1.0f );
      unfocusedSelectedItemColor_ = new Color( 0.70f, 0.70f, 0.70f, 1.0f );
      dragToItemColor_            = new Color( 153f/255f, 204f/255f, 1.0f, 1.0f );
      textColorUnselected_        = Color.black;
      textColorSelected_          = Color.black;
    }

    textFieldStyle_.alignment = TextAnchor.LowerLeft;

    foldoutStyle_.active    = EditorStyles.label.normal;
    foldoutStyle_.focused   = EditorStyles.label.normal;
    foldoutStyle_.onActive  = EditorStyles.label.onNormal;
    foldoutStyle_.onFocused = EditorStyles.label.onNormal;
    foldoutStyle_.alignment = TextAnchor.LowerLeft;

    iconStyle_.alignment = TextAnchor.MiddleCenter;
    iconStyle_.margin = new RectOffset();
  }
  //----------------------------------------------------------------------------------
  private void CalculateContentSize( Rect area )
  {
    GUIStyle labelStyle = EditorStyles.label;

    contentWidth_  = 0f;  
    contentHeight_ = 0f;

    int nElements = Controller.NumVisibleElements;

    for (int i = 0; i < nElements; i++)
    {
      if (Controller.ItemIsNull(i))
      { 

        continue;
      }

      string s1 = Controller.GetItemListName( i );
      GUIContent s1Content = new GUIContent( s1 );

      Vector2 size = labelStyle.CalcSize( s1Content );
      float addedSpace = margin_ + toggleWidth_ + iconWidth_ + ( Controller.ItemIndentLevel( i ) * pixelSpace_ );
      size.x += addedSpace;

      if ( size.x > contentWidth_ )
      {
        contentWidth_ = size.x + 20f;
      }

      contentHeight_ += itemHeight_;    
    }

    if (contentHeight_ > area.height)
    {
      contentHeight_ += 3 * itemHeight_;
    }
  }
  //----------------------------------------------------------------------------------
  private Texture2D MakeTex( int width, int height, Color col )
  {
    Color[] pix = new Color[width * height];
    for( int i = 0; i < pix.Length; ++i )
    {
        pix[ i ] = col;
    }
    Texture2D result = new Texture2D( width, height );
    result.SetPixels( pix );
    result.Apply();
    return result;
  }
  //----------------------------------------------------------------------------------
  public void RenderGUI( Rect area )
  { 
    Event ev = Event.current;
    EventType evType = ev.type;

    CalculateContentSize( area );

    GUI.color  = backgroundColor_;
    Rect frame = new Rect( area.x - 1, area.y - 1, area.width + 2, area.height + 2 );

    GUI.Box(frame, "");
    GUI.color = currentGUIColor_;

    Rect contentArea = new Rect( 0, 0, contentWidth_, contentHeight_ );   

    scrollPos_ = GUI.BeginScrollView( area, scrollPos_, contentArea );
  
    string s1 = string.Empty;
    string s2 = string.Empty;

    float listboxWidth  = Math.Max( area.width , contentArea.width );
    float listboxHeight = Math.Max( area.height, contentArea.height );

    //Draw the Listbox.
    Rect listBox = new Rect( 0, 0, listboxWidth, listboxHeight );
    GUILayout.BeginArea( listBox, "" ); 

    ProcessEventsPredraw( ev, evType, scrollPos_, area, contentArea ); 
    
    int visibleElements = (int) Mathf.Ceil( area.height / itemHeight_ ) + 1;

    int visibleBeginIdx = Math.Max( (int)Mathf.Floor( scrollPos_.y / itemHeight_ ), 0 );
    int visibleEndIdx   = Math.Min( visibleBeginIdx + visibleElements, Controller.NumVisibleElements );

    dragOverAnyItem_ = false;

    for ( int i = visibleBeginIdx; i < visibleEndIdx; i++ )
    {
      s1 = Controller.GetItemListName( i );
      s2 = Controller.GetItemName( i );

      bool isGroup = Controller.ItemIsGroup( i );
      float yPos   = i * itemHeight_;
      
      //Scroller
      float commandBoxWidth  = area.width;
      float commandBoxHeight = itemHeight_;

      float labelStart = Controller.ItemIndentLevel( i ) * pixelSpace_;
  
      Rect commandBox = new Rect( 0 + scrollPos_.x, yPos,     commandBoxWidth, commandBoxHeight );  
      Rect labelBox   = new Rect( 1 + labelStart,   1 + yPos, contentWidth_,   itemHeight_ );
      
      Rect toggleBox  = new Rect( labelBox.xMin + 2, commandBox.yMin + 4, toggleWidth_,  commandBox.height - 4 );
      Rect iconBox    = new Rect( toggleBox.xMax, commandBox.yMin, iconWidth_, commandBox.height );

      float textBoxWidth = labelBox.width - toggleWidth_ - iconWidth_;

      Rect textBox               = new Rect(iconBox.xMax + 4,  iconBox.yMin, textBoxWidth,  labelBox.height -1);
      Rect textBoxWithoutToggle  = new Rect(toggleBox.x,       toggleBox.y,  textBox.xMax - toggleBox.x, textBox.yMax - toggleBox.y);

      DrawItem( i, commandBox, labelBox, toggleBox, iconBox, textBox, textBoxWithoutToggle, s1, s2, isGroup );
      
      ProcessEventsItemList( listBox, i, commandBox, labelBox, toggleBox, ev, isGroup );
      
      // Need to update the endIndex in case we collapsed a group node 
      visibleEndIdx   = Math.Min ( visibleBeginIdx + visibleElements, Controller.NumVisibleElements );
    }

    ProccessOtherEvents( ev, listBox );  

    GUILayout.EndArea();
    GUI.EndScrollView();

    GUI.color        = currentGUIColor_;
    GUI.contentColor = currentContentColor_;
  } 
  //----------------------------------------------------------------------------------
  private void DrawItem( int itemIdx, Rect commandBox, Rect labelBox, Rect toggleBox, Rect iconBox, Rect textBox, Rect textBoxWithoutToggle, string itemName, string realName, bool isGroup )
  {
    GUIStyle labelStyle      = new GUIStyle(EditorStyles.label);

    labelStyle.alignment = TextAnchor.LowerLeft;
    labelStyle.fontStyle = FontStyle.Normal;

    GUIStyle boxStyle = new GUIStyle( GUI.skin.box );

    bool itemIsSelected = Controller.ItemIsSelected(itemIdx);
    if ( itemIsSelected )
    {
      if ( itemIdx != Controller.ItemIdxEditing )
      {
        Texture2D tex = null;
        if (Focused)
        {
          tex = MakeTex( (int)Mathf.Floor(commandBox.width), (int)Mathf.Floor(commandBox.height), focusedSelectedItemColor_);
          boxStyle.normal.background = tex;   
        }
        else
        {
          tex = MakeTex( (int)Mathf.Floor(commandBox.width), (int)Mathf.Floor(commandBox.height), unfocusedSelectedItemColor_);
          boxStyle.normal.background = tex;
        }
        EditorGUI.LabelField( commandBox, "", boxStyle );
        UnityEngine.Object.DestroyImmediate(tex);
      }
    }

    if(draggingItemIdx_ == itemIdx && isDragging_)
    {
      Texture2D tex = MakeTex( (int)Mathf.Floor(commandBox.width), (int)Mathf.Floor(commandBox.height), dragToItemColor_ ); 
      boxStyle.normal.background = tex;
      GUI.Box( commandBox, "", boxStyle );
      UnityEngine.Object.DestroyImmediate(tex);
    }   

    //Draw
    if (CurrentMouseDragItem == itemIdx)
    {
      Texture2D tex = MakeTex( (int)Mathf.Floor(commandBox.width), (int)Mathf.Floor(commandBox.height), dragToItemColor_ ); 
      boxStyle.normal.background = tex;
      if (EdgeDrag)
      {
        Rect edgeBox = new Rect(textBox.xMin, labelBox.yMax - 3, labelBox.width, 6);
        GUI.Box( edgeBox, "", boxStyle);
      }
      else
      {
        GUI.Box(commandBox, "", boxStyle);
      }
      UnityEngine.Object.DestroyImmediate(tex);
    }

    Texture icon = Controller.GetItemIcon(itemIdx);

    bool itemIsDisabled = Controller.ItemIsDisabled(itemIdx);
    bool itemIsExcluded = Controller.ItemIsExcluded(itemIdx);

    if (isGroup)
    {
      bool foldout = Controller.ItemIsExpanded(itemIdx);
      EditorGUI.BeginChangeCheck();
      foldout = EditorGUI.Toggle(toggleBox, foldout, foldoutStyle_);
      if (EditorGUI.EndChangeCheck())
      {
        Controller.ItemSetExpanded(itemIdx, foldout);
      }
    }
   
    if (icon != null)
    {
      EditorGUI.BeginDisabledGroup( itemIsExcluded || itemIsDisabled );
      EditorGUI.LabelField( iconBox, new GUIContent(icon), iconStyle_ );
      EditorGUI.EndDisabledGroup();
    }

    if (Controller.ItemIsBold(itemIdx))
    {
      labelStyle.fontStyle = FontStyle.Bold;
    }

    if ( itemIdx == Controller.ItemIdxEditing )
    {
      GUI.SetNextControlName("ListBoxItemEdition" + itemIdx );
      Controller.ItemIdxEditingName = EditorGUI.TextField(textBox, Controller.ItemIdxEditingName, textFieldStyle_);
      EditorGUI.FocusTextInControl("ListBoxItemEdition" + itemIdx );
    }
    else
    {
      if (itemIsExcluded)
      {
        labelStyle.fontStyle = FontStyle.Italic;
        labelStyle.normal.textColor = Color.red;
      }
      else if (itemIsDisabled)
      {
        labelStyle.normal.textColor = Color.grey;     
      }
      else
      {
        if (itemIsSelected)
        {
          labelStyle.normal.textColor = textColorSelected_;
        }
        else
        {
          labelStyle.normal.textColor = textColorUnselected_;
        }
      } 
      EditorGUI.LabelField(textBox, itemName, labelStyle);
    }
  }
  //----------------------------------------------------------------------------------
  private void ProcessEventsItemList( Rect listBox, int itemIdx, Rect commandBox, Rect labelBox, Rect toggleBox,  Event ev, bool isGroup )
  {
    if ( !listBox.Contains(ev.mousePosition) )
      return;

    if ( Controller.ItemIdxEditing == itemIdx )
      return;

    switch ( ev.type )
    {
     case EventType.Used:
       return;

     case EventType.MouseDown:   
      if ( !Controller.ItemIsSelectable(itemIdx) )
      {
        return;
      }

      if ( ev.button == 0 )
      {  
        if (isGroup && toggleBox.Contains(ev.mousePosition))
        {
          ev.Use();
          return;
        }

        if ( commandBox.Contains( ev.mousePosition ) )
        { 
          if ( itemIdx != Controller.ItemIdxEditing )
          {
            if (Controller.ItemIdxEditing != -1)
            {
              Controller.SetItemName(Controller.ItemIdxEditing, Controller.ItemIdxEditingName);
              Controller.ItemIdxEditing = -1;
            }
            if (Controller.ItemIsDraggable(itemIdx) && !Controller.BlockEdition)
            {
              DragAndDrop.PrepareStartDrag();
              CustomDragData dragData = new CustomDragData();
              
              dragData.originalIndex = itemIdx;
              dragData.originalList  = this;
              draggingItemIdx_       = itemIdx;
              isInternalDrag_        = true;

              DragAndDrop.objectReferences = new UnityEngine.Object[0];
              DragAndDrop.SetGenericData(dragDropIdentifier_, dragData);
            }
            if (ev.button == 0)
            {
              if (ev.clickCount == 1)
              {
                Controller.OnClickItem(itemIdx, ev.control, ev.shift, ev.alt, false);
              }
              else if (ev.clickCount == 2)
              {
                Controller.OnDoubleClickItem(itemIdx);
              }
            } 
          }
          else
          {
            itemEditedClicked_ = true;
          }
          ev.Use();
        }     
      }
      break; 
    
      case EventType.MouseUp:
        if ( isGroup && toggleBox.Contains(ev.mousePosition) )
        {
          ev.Use();
          return;
        }

        if (!Controller.ItemIsSelectable(itemIdx))
        {
          return;
        }
        if ( commandBox.Contains( ev.mousePosition ) && itemIdx != Controller.ItemIdxEditing )
        {
          if (ev.button == 0)
          {
            if (ev.clickCount == 1)
            {
              Controller.OnClickItem(itemIdx, ev.control, ev.shift, ev.alt, true);
            }
            else if (ev.clickCount == 2)
            {
              Controller.OnDoubleClickItem(itemIdx);
            }
            ev.Use();
          } 
        }
        break;


    case EventType.DragUpdated:     
      Rect edgeBox = new Rect(commandBox.x, commandBox.yMax - 6, commandBox.width, 6);
      if ( edgeBox.Contains( ev.mousePosition ) && isInternalDrag_ )
      {
        if ( Controller.ItemIsValidDragTarget( itemIdx, dragDropIdentifier_ ) )
        {
          EdgeDrag               = true;
          CurrentMouseDragItem   = itemIdx;
          DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          dragOverAnyItem_       = true;
          ev.Use();
        }
      }
      else if ( commandBox.Contains( ev.mousePosition ) )
      { 
        EdgeDrag = false;
        if ( Controller.ItemIsValidDragTarget( itemIdx, dragDropIdentifier_ ) ) 
        {
          CurrentMouseDragItem   = itemIdx;
          DragAndDrop.visualMode = DragAndDropVisualMode.Link;  
          dragOverAnyItem_       = true;
        }
        else
        { 
          CurrentMouseDragItem   = -1;
          DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
        ev.Use();
      }
      break;

    case EventType.ContextClick:
      if (!Controller.ItemIsSelectable(itemIdx))
      {
        return;
      }

      if ( commandBox.Contains ( ev.mousePosition ) && Controller.ItemHasContext( itemIdx ) && !Controller.BlockEdition )
      {
        Controller.OnContextClickItem( itemIdx, ev.control, ev.shift, ev.alt );
        ev.Use ();
      }
      break;
    }
  }
  //----------------------------------------------------------------------------------
  private void ProccessOtherEvents( Event ev, Rect listBox )
  {
    switch (ev.type)
    {  
    case EventType.MouseDrag:
      {
        if (listBox.Contains( ev.mousePosition ) )
        {
          CustomDragData receivedDragData = DragAndDrop.GetGenericData(dragDropIdentifier_) as CustomDragData;
          UnityEngine.Object[] objects = DragAndDrop.objectReferences;     
          if ( receivedDragData != null || ( objects != null && objects.Length > 0 ) )     
          {
            isDragging_ = true;
            DragAndDrop.StartDrag("Dragging List Element");
            ev.Use();
          }
        }
       break; 
      }
 
    case EventType.DragUpdated:
      if ( listBox.Contains( ev.mousePosition ) )
      { 
        if ( !dragOverAnyItem_ )
        {
          CurrentMouseDragItem   = -1;
          DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
          if (Controller.ListIsValidDragTarget())
          {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          }
        }
        ev.Use();
      }
      break;
      
    case EventType.DragPerform:
      {
        if ( listBox.Contains( ev.mousePosition ) )
        {
          CustomDragData receivedDragData = DragAndDrop.GetGenericData(dragDropIdentifier_) as CustomDragData;
          if (receivedDragData != null)
          {
            Controller.MoveDraggedItem(receivedDragData.originalIndex, CurrentMouseDragItem, EdgeDrag);
            return;
          }

          UnityEngine.Object[] objects = DragAndDrop.objectReferences;
          if (receivedDragData != null || (objects != null && objects.Length > 0) )
          {
            Controller.AddDraggedObjects(CurrentMouseDragItem, objects);
          }
          
          DragAndDrop.AcceptDrag();
        }

        CleanUp();
        Controller.GUIUpdateRequested();
      }
      break;
      
    case EventType.DragExited:
      CleanUp();
      Controller.GUIUpdateRequested();
      break;
      
    case EventType.MouseUp:
      if ( listBox.Contains( ev.mousePosition) )
      {
        // Clean up, in case MouseDrag never occurred:
        CleanUp();
      }

      break;

    case EventType.MouseDown:
      if ( ( !listBox.Contains( ev.mousePosition ) || ( listBox.Contains( ev.mousePosition ) && !itemEditedClicked_ ) )  && Controller.ItemIdxEditing != -1 )
      {
        Controller.SetItemName( Controller.ItemIdxEditing, Controller.ItemIdxEditingName );
        Controller.ItemIdxEditing = -1;
        //CleanUp();
        Controller.GUIUpdateRequested();
      }
      itemEditedClicked_ = false;
      break;

    case EventType.ContextClick:
      if ( listBox.Contains(ev.mousePosition) )
      {
        Controller.OnContextClickList();
        ev.Use();
      }
      break;
    }

  }
  //----------------------------------------------------------------------------------
  private void ProcessEventsPredraw(Event ev, EventType evType, Vector2 scrollPos, Rect area, Rect contentArea )
  {
    float itemsHeight   = itemHeight_ * (Controller.NumVisibleElements);
    if ( evType == EventType.MouseDown)
    {
      float mousePosX = ev.mousePosition.x;
      float mousePosY = ev.mousePosition.y;

      float mousePosXRelative =  mousePosX - scrollPos.x;
      float mousePosYRelative =  mousePosY - scrollPos.y;

      Vector2 mousePosAreaRelative = ev.mousePosition + area.min - scrollPos;

      float scrollBarSize = 15f;

      if ( area.Contains(mousePosAreaRelative) )
      {
        Focused = true;
      }
      else
      {
        Focused = false;
        CleanUp();
        Controller.GUIUpdateRequested();
      }

      if ( area.Contains(mousePosAreaRelative) )
      {

        if ( contentArea.height > area.height && mousePosXRelative > area.width - scrollBarSize) 
        {
          ev.Use();
        }
        else if ( contentArea.width > area.width && mousePosYRelative > area.height - scrollBarSize )
        {
          ev.Use();
        }
        else if ( (mousePosY > itemsHeight) )
        {
          Controller.UnselectSelected();
          CleanUp();
          ev.Use();
        }
      }
    }

    if (!Focused)
      return;

    if ( evType == EventType.KeyDown)
    {
      if ( ev.keyCode == KeyCode.Delete )
      {
          Controller.RemoveSelected();
          ev.Use();
      }
      else if ( Controller.ItemIdxEditing != -1 &&
                ( ev.keyCode == KeyCode.KeypadEnter || 
                  ev.keyCode == KeyCode.Return ) )
      {
         Controller.SetItemName( Controller.ItemIdxEditing, Controller.ItemIdxEditingName );
        Controller.ItemIdxEditing = -1;
         ev.Use();
      }
      else if ( Controller.ItemIdxEditing != -1 &&
                ev.keyCode == KeyCode.Escape )
      {
        Controller.ItemIdxEditing     = -1;
        Controller.ItemIdxEditingName = string.Empty;
        ev.Use();
      }
    }
  }
  //----------------------------------------------------------------------------------
  private void CleanUp()
  {
    if (Controller.ItemIdxEditing != -1)
    {
      Controller.SetItemName(Controller.ItemIdxEditing, Controller.ItemIdxEditingName);
      Controller.ItemIdxEditing = -1;
    }
    DragAndDrop.PrepareStartDrag();
    EdgeDrag                  = false;
    CurrentMouseDragItem      = -1;
    Controller.ItemIdxEditing = -1;
    draggingItemIdx_          = -1;
    isDragging_               = false;
    isInternalDrag_           = false;
  }

} //class CRListBox

} //namespace Caronte...


