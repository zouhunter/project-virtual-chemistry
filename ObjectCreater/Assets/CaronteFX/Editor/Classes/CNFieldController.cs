using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNFieldController : IListController 
  {
    public static Texture icon_nameselector_;
    public static Texture icon_gameobject_;

    CommandNode     ownerNode_;
    CNField         field_;
    CNHierarchy     cnHierarchy_;
    CRGOManager     goManager_;
    CREntityManager eManager_;

    uint id_      = uint.MaxValue;
    uint scopeId_ = uint.MaxValue;

    public uint TotalObjects { get; private set; }

    public System.Action WantsUpdate;

    private List<int> listSelectedIdx_  = new List<int>();
    private int       lastSelectedIdx_  = -1;

    private int    itemIdxEditing_     = -1;
    private string itemIdxEditingName_ = string.Empty;

    List<CRTreeNode>  listTreeNodeAux_    = new List<CRTreeNode>();
    List<CommandNode> listCommandNodeAux_ = new List<CommandNode>();
    List<GameObject>  listGameObjectAux_  = new List<GameObject>();

    List<GameObject> listFilterSelection_ = new List<GameObject>();
    
    bool blockEdition_ = false;

    public CNField Field 
    {
      get
      {
        return field_;
      }
      set
      {
        field_ = value;
      }
    }

    public int  ItemIdxEditing
    {
      get { return itemIdxEditing_; }
      set { itemIdxEditing_ = value; }
    }

    public string ItemIdxEditingName
    {
      get { return itemIdxEditingName_; }
      set { itemIdxEditingName_ = value; }
    }

    [Flags]
    enum SelectionFlags
    {
      none      = 1,
      renamable = 2,
      deletable = 4,
    }

    enum ItemType
    {
      nodeselector,
      nameselector,
      goselector,
      label
    }

    public bool BlockEdition 
    {
      get { return blockEdition_; }
    }

    public bool IsBodyField
    {
      get;
      set;
    }
   
    List< Tuple5< string, string, ItemType, Texture, int > > listFieldItems_ = new List< Tuple5< string, string, ItemType, Texture, int> >();
    HashSet<int> hsGOIndexOutOfScope_ = new HashSet<int>();    

    //-----------------------------------------------------------------------------------
    public CNFieldController( CommandNode node, CNField field, CREntityManager entityManager, CRGOManager goManager )
    {
      ownerNode_ = node;
      field_     = field;
      field_.PurgeInvalidNullNodes();
    
      goManager_  = goManager;
      eManager_   = entityManager;

      cnHierarchy_= CNManager.Instance.Hierarchy;

      id_ = FieldManager.CreateField( field.IsExclusive );
      SetScopeType( field_.ScopeType );
    }
    //-----------------------------------------------------------------------------------
    public void SetFieldType( CNField.AllowedTypes allowedType )
    {
      if ( field_.AllowedType != allowedType )
      {
        field_.AllowedType = allowedType;
      }
    }
    //-----------------------------------------------------------------------------------
    public void DestroyField()
    {
      FieldManager.DestroyField( (uint)id_ );
    }
    //-----------------------------------------------------------------------------------
    public uint GetFieldId()
    {
      return (uint)id_;
    }
    //-----------------------------------------------------------------------------------
    public int NumVisibleElements 
    { 
      get { return (  listFieldItems_.Count ); }
    }
    //-----------------------------------------------------------------------------------
    public void UnselectSelected()
    {
      listSelectedIdx_.Clear();
      CNFieldWindow.Instance.Repaint();
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsNull(int itemIdx)
    {
      return (listFieldItems_[itemIdx] == null);
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsBold(int itemIdx)
    {
      ItemType type = listFieldItems_[itemIdx].Third;
      return (type == ItemType.label);
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsSelectable(int itemIdx)
    {
      ItemType type = listFieldItems_[itemIdx].Third;
      return (type != ItemType.label);
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsSelected(int itemIdx)
    {
      if ( listSelectedIdx_.Contains(itemIdx) )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsGroup(int itemIdx)
    {
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsExpanded(int itemIdx)
    {
      return true;
    }
    //-----------------------------------------------------------------------------------
    public void ItemSetExpanded(int itemIdx, bool open)
    {
    
    }
    //-----------------------------------------------------------------------------------
    public int ItemIndentLevel(int itemIdx)
    {
      ItemType type = listFieldItems_[itemIdx].Third;
      if ( type == ItemType.label )
      {
        return -1;
      }
      else
      {
        return 1;
      }
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsDraggable(int itemIdx)
    {
      return false;
    }  
    //-----------------------------------------------------------------------------------
    public bool ItemIsValidDragTarget(int itemIdx, string dragDropIdentifier)
    { 
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemHasContext(int itemIdx)
    {
      return true;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsEditable(int itemIdx)
    {
      ItemType type = listFieldItems_[itemIdx].Third;
      if ( type == ItemType.nameselector )
      {
        return true;
      }

      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsDisabled(int itemIdx)
    {
      int realIdx       = listFieldItems_[itemIdx].Fifth;
      ItemType itemType = listFieldItems_[itemIdx].Third;

      if (itemType == ItemType.goselector)
      {
        return ( !FieldManager.IsGameObjectInScope( id_, (uint)realIdx) );
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsExcluded(int itemIdx)
    {
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ListIsValidDragTarget()
    {
      UnityEngine.Object[] dragObjects = DragAndDrop.objectReferences;
      return CREditorUtils.CheckIfAnySceneGameObjects( dragObjects );
    }
    //-----------------------------------------------------------------------------------
    public void MoveDraggedItem(int itemfromIdx, int itemToIdx, bool edgeDrag)
    {

    }
    //-----------------------------------------------------------------------------------
    public void AddDraggedObjects( int itemToIdx, UnityEngine.Object[] objects )
    {
      AddGameObjects( objects, true );

      CRManagerEditor.RepaintIfOpen();
      CNFieldWindow.RepaintIfOpen();
    }
    //-----------------------------------------------------------------------------------
    public void RemoveSelected()
    {
      Undo.RecordObject(ownerNode_, "CaronteFX - Remove selection from field");

      if (listSelectedIdx_.Count == 0) return;

      List<uint> listNodeIdx, listWildcardIdx, listGameObjectIdx;

      GetSelectionLists(out listNodeIdx,out listWildcardIdx, out listGameObjectIdx);
    
      field_.RemoveNodes(listNodeIdx);
      FieldManager.RemoveWildcardSelections( (uint)id_, listWildcardIdx.ToArray() );
      FieldManager.RemoveGameObjects( (uint)id_, listGameObjectIdx.ToArray() );

      cnHierarchy_.RecalculateFieldsDueToUserAction();

      Undo.SetCurrentGroupName("CaronteFX - Remove selection from field");
      Undo.FlushUndoRecordObjects();
      Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );

      if (WantsUpdate != null)
      {
        WantsUpdate();
      }
    }
    //-----------------------------------------------------------------------------------
    void GetSelectionLists( out List<uint> listNodeIdx, out List<uint> listNameSelectorIdx, out List<uint> listGameObjectIdx )
    {
      listNodeIdx          = new List<uint>();
      listNameSelectorIdx  = new List<uint>();
      listGameObjectIdx    = new List<uint>();

      foreach (uint selectedIdx in listSelectedIdx_)
      {
        ItemType type = listFieldItems_[(int)selectedIdx].Third;
        int realIdx   = listFieldItems_[(int)selectedIdx].Fifth;
        switch(type)
        {
          case ItemType.nodeselector:
            listNodeIdx.Add((uint)realIdx);
            break;

          case ItemType.nameselector:
            listNameSelectorIdx.Add((uint)realIdx);
            break;

          case ItemType.goselector:
            listGameObjectIdx.Add((uint)realIdx);
            break;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void GUIUpdateRequested()
    {
      CNFieldWindow.RepaintIfOpen();
    }
    //-----------------------------------------------------------------------------------
    private uint GetWSNumberOfAffectedObjects( int itemIdx )
    {
      return FieldManager.GetWildcardSelectionNumberOfObjects( (uint)id_ , (uint)itemIdx );
    }
    //-----------------------------------------------------------------------------------
    private uint GetWSNumberOfTotalObjects( int itemIdx )
    {
      return FieldManager.GetWildcardSelectionTotalNumberOfObjects( (uint)id_, (uint)itemIdx );
    }
    //-----------------------------------------------------------------------------------
    public string GetItemName( int itemIdx )
    {
      return ( listFieldItems_[itemIdx].First );
    }
    //-----------------------------------------------------------------------------------
    public string GetItemListName( int itemIdx )
    { 
      return ( listFieldItems_[itemIdx].Second );
    }
    //-----------------------------------------------------------------------------------
    public void BuildListItems()
    {
      field_.PurgeInvalidNullNodes();
      listFieldItems_.Clear();

      List< Tuple5<string, string, ItemType, Texture, int > > listFieldItemsOutOfScope = new List< Tuple5<string, string, ItemType, Texture, int> >();

      TotalObjects = 0;

      int nNodes = Field.NumberOfNodes;
      if (nNodes > 0)
      {
        listFieldItems_.Add( Tuple5.New("By Node: ", "By Node:", ItemType.label, (Texture)null, -1) );
        for (int i = 0; i < nNodes; i++)
        {
          CommandNode node = field_.GetNode(i);
          string listName;
          if ( IsBodyField && (node is CNBody || node is CNGroup) )
          {
            uint nBodies = GetNodeNumberOfBodies(node);
            listName = field_.GetNodeName(i) + "  [" + nBodies + " bodies]";
            TotalObjects += nBodies;
          }
          else
          {
            listName = field_.GetNodeName(i);
          }

     
          listFieldItems_.Add( Tuple5.New( field_.GetNodeListName(i), listName, ItemType.nodeselector, GetNodeIcon(i), i ) );
        }  
        //separator
        listFieldItems_.Add( Tuple5.New("", "", ItemType.label, (Texture)null, -1) );
      }
      
      List<string> lNameSelectors = Field.NameSelectors;

      int nNameSelectors = lNameSelectors.Count;
      if (nNameSelectors > 0)
      {
        listFieldItems_.Add( Tuple5.New("By Name:", "By Name:", ItemType.label, (Texture)null, -1) );
        for (int i = 0; i < nNameSelectors; i++)
        {
          string name  = lNameSelectors[i];
          string label;

          if (IsBodyField)
          {
            int[] ids    = FieldManager.GetIdsUnityFromWS( id_, (uint) i);
            uint nBodies = eManager_.GetNumberOfBodyObjects( ids );
            label = name  + "  [" + nBodies +  " bodies]";
            TotalObjects += nBodies;
          }
          else
          {
            uint nObjects = GetWSNumberOfAffectedObjects(i);
            label = name  + "  [" + nObjects + " objects]";  
            TotalObjects += nObjects;
          }        

          listFieldItems_.Add(Tuple5.New(name, label, ItemType.nameselector, (Texture)null, i) );
        }
        //separator
        listFieldItems_.Add( Tuple5.New("", "", ItemType.label, (Texture)null, -1) );
      }

      List<GameObject> lGameObject = Field.GameObjects;

      int nGameObjects   = lGameObject.Count;
      if( nGameObjects > 0 )
      {
        listFieldItems_.Add( Tuple5.New("By GameObject:", "By GameObject:", ItemType.label, (Texture)null, -1) );
        for (int i = 0; i < nGameObjects; i++)
        { 
          GameObject go = lGameObject[i];
          if (go == null)
          {
            continue;
          }

          string name  = lGameObject[i].name;
          string label = string.Empty;

          bool inScope = !hsGOIndexOutOfScope_.Contains(i); 
    
          if (!inScope)
          {
            label = name + " [Out Of Scope]"; 
            listFieldItemsOutOfScope.Add( Tuple5.New(name, label, ItemType.goselector, (Texture)null, i) );
          }
          else
          {
            if (IsBodyField)
            {
              int[] ids = FieldManager.GetIdsUnityFromGO(id_, (uint)i );
              uint nBodies = eManager_.GetNumberOfBodyObjects( ids );
              label = name + "  [" + nBodies + " bodies]";
              TotalObjects += nBodies;
            }
            else
            {
              int[] ids = FieldManager.GetIdsUnityFromGO(id_, (uint)i );
              uint nObjects = (uint)ids.Length;
              label = name + "  [" + nObjects + " objects]"; 
              TotalObjects += nObjects;
            }
            listFieldItems_.Add( Tuple5.New(name, label, ItemType.goselector, (Texture)null, i) ); 
          }

        }
      }

      listFieldItems_.AddRange( listFieldItemsOutOfScope );
    }
    //-----------------------------------------------------------------------------------
    public Texture GetItemIcon( int itemIdx )
    {
      ItemType itemType = listFieldItems_[itemIdx].Third;
      
      Texture  icon = null;

      switch(itemType)
      {
        case ItemType.label:
          icon = null;
          break;

        case ItemType.nodeselector:
          icon = listFieldItems_[itemIdx].Fourth;
          break;

        case ItemType.nameselector:
          icon = icon_nameselector_;
          break;

        case ItemType.goselector:
          icon = icon_gameobject_;
          break;

        default:
          break;
      }

      return (icon);
    }
    //-----------------------------------------------------------------------------------
    private Texture GetNodeIcon(int nodeIdx)
    {
      CommandNode node = field_.GetNode( nodeIdx );
      return ( cnHierarchy_.GetNodeIcon( node ) );
    }
    //-----------------------------------------------------------------------------------
    private void ListSelection()
    {
      List<GameObject> listGameObjects = new List<GameObject>();

      int nSelectedItems = listSelectedIdx_.Count;
      for ( int i = 0; i < nSelectedItems; i++ )
      {
        int itemIdx = listSelectedIdx_[i];

        ItemType type = listFieldItems_[itemIdx].Third;
        int realIdx   = listFieldItems_[itemIdx].Fifth;

        switch (type)
        {
          case ItemType.label:
            break;

          case ItemType.nodeselector:
            {
              GameObject[] arrGameObject = GetNodeDerivedObjects().ToArray();
              listGameObjects.AddRange(arrGameObject);
              break;
            }

          case ItemType.nameselector:
            {
              int[] arrIdsUnity = FieldManager.GetIdsUnityFromWS( id_, (uint)(realIdx));
              GameObject[] arrGameObject = GetArrGOFromIds(arrIdsUnity);
              listGameObjects.AddRange(arrGameObject);
              break;
            }

          case ItemType.goselector:
            {
              int[] arrIdsUnity = FieldManager.GetIdsUnityFromGO( id_, (uint)(realIdx) );
              GameObject[] arrGameObject = GetArrGOFromIds(arrIdsUnity);
              listGameObjects.AddRange(arrGameObject);
              break;
            }

          default:
            break;
        }
      }

      /*
      if (IsBodyField)
      {
        GameObject[] arrBodyGameObject = eManager.GetBodyObjects( listGameObjects );
        Selection.objects = arrBodyGameObject;
      }
      */
      //else
      {
        Selection.objects = listGameObjects.ToArray();
        UnityEngine.Object[] objects = Selection.GetFiltered( typeof(GameObject), SelectionMode.TopLevel );
        Selection.objects = objects;
      }
      
    }
    //-----------------------------------------------------------------------------------
    public GameObject[] GetArrGOFromIds( int[] arrIdsUnity )
    {
      return ( GetListGOFromArrId(arrIdsUnity).ToArray() );
    }
    //-----------------------------------------------------------------------------------
    private List<GameObject> GetListGOFromIdField(int idField)
    {
      int[] arrIdsUnity = FieldManager.GetIdsUnityFromField((uint)idField);
      return (GetListGOFromArrId(arrIdsUnity));
    }
    //-----------------------------------------------------------------------------------
    public void SetItemName( int itemIdx, string name )
    {
      int realIdx = listFieldItems_[itemIdx].Fifth;

      FieldManager.SetWildcardSelectionName( id_, (uint)realIdx, name );
      cnHierarchy_.RecalculateFieldsDueToUserAction();
    }
    //-----------------------------------------------------------------------------------
    public void OnClickItem( int itemIdx, bool ctrlPressed, bool shiftPressed, bool altPressed, bool isUpClick )
    {
      int prevIdx  = lastSelectedIdx_;
      lastSelectedIdx_ = itemIdx;

      if (!isUpClick)
      {
        if (shiftPressed)
        {
          if (prevIdx != -1)
          {
            int range = itemIdx - prevIdx;
            int increment = (range > 0) ? 1 : -1;

            while (prevIdx != itemIdx)
            {
              int currentIdx = prevIdx + increment;

              if (listSelectedIdx_.Contains(currentIdx))
              {
                listSelectedIdx_.Remove(currentIdx);
              }
              else if (ItemIsSelectable(currentIdx) )
              {
                listSelectedIdx_.Add(currentIdx);
              }
              prevIdx += increment;
            }
            ListSelection();
          }
        }   
        else if(ctrlPressed)
        {
          if( listSelectedIdx_.Contains(itemIdx) )
          {
            listSelectedIdx_.Remove(itemIdx);
          }
          else
          {
            listSelectedIdx_.Add(itemIdx);
            ListSelection();
          }
        }
        else
        {
          listSelectedIdx_.Clear();
          listSelectedIdx_.Add(itemIdx);
          ListSelection();
        }
      }

      CRManagerEditor instance = CRManagerEditor.Instance;
      instance.RepaintSubscribers();
    }
    //-----------------------------------------------------------------------------------
    public void OnDoubleClickItem( int itemIdx )
    {
 
    }
    //----------------------------------------------------------------------------------
    public void OnContextClickItem(int itemIdx, bool ctrlPressed, bool shiftPressed, bool altPressed )
    {
      List<uint> listNodeIdx, listWildcardIdx, listGameObjectIdx;
      GetSelectionLists(out listNodeIdx, out listWildcardIdx, out listGameObjectIdx);

      SelectionFlags selectionFlags;
      GetSelectionFlags( listNodeIdx, listWildcardIdx, listGameObjectIdx, out selectionFlags );
      CreateContextMenu(selectionFlags);
    }
    //----------------------------------------------------------------------------------
    public void OnContextClickList()
    {
      List<uint> listNodeIdx, listWildcardIdx, listGameObjectIdx;
      GetSelectionLists(out listNodeIdx, out listWildcardIdx, out listGameObjectIdx);

      SelectionFlags selectionFlags;
      GetSelectionFlags(listNodeIdx, listWildcardIdx, listGameObjectIdx, out selectionFlags);
      CreateContextMenu(selectionFlags);
    }
    //----------------------------------------------------------------------------------
    void GetSelectionFlags(List<uint> listNodeIdx, List<uint> listWildcardIdx, List<uint> listGameObjectIdx, out SelectionFlags selectionFlags  )
    {
      selectionFlags  = SelectionFlags.deletable;

      int nNodes       = listNodeIdx.Count;
      int nWildcards   = listWildcardIdx.Count;
      int nGameObjects = listGameObjectIdx.Count;

      int totalIndexes = nNodes + nWildcards + nGameObjects;

      if (totalIndexes == 0 )
      {
        selectionFlags = SelectionFlags.none;
      }
      else if (totalIndexes == 1 && nWildcards == 1)
      {
        selectionFlags |= SelectionFlags.renamable;
      }
    }
    //----------------------------------------------------------------------------------
    void CreateContextMenu(SelectionFlags selectionFlags)
    {
      GenericMenu menu = new GenericMenu();

      bool none       = (selectionFlags & SelectionFlags.none)      == SelectionFlags.none;
      bool deletable  = (selectionFlags & SelectionFlags.deletable) == SelectionFlags.deletable;
      bool renamable  = (selectionFlags & SelectionFlags.renamable) == SelectionFlags.renamable;

      if ( none )
      {
        FillOptionsMenu(menu);
      }
      else
      {
        if (renamable)
        {
          menu.AddItem(new GUIContent("Rename"), false, RenameSelection);
        }
        else
        {
          menu.AddDisabledItem(new GUIContent("Rename"));
        }
        if (deletable)
        {
          menu.AddItem(new GUIContent("Remove"), false, RemoveSelected);
        }  
      }  
  
      menu.ShowAsContext();
    }
    //----------------------------------------------------------------------------------
    void RenameSelection()
    {
      ItemIdxEditing     = lastSelectedIdx_;
      ItemIdxEditingName = GetItemName(lastSelectedIdx_);
    }
    //-----------------------------------------------------------------------------------
    private void FilterObjects(UnityEngine.Object[] objects, List<GameObject> listFilteredObjects)
    {
      if ( (field_.AllowedType & CNField.AllowedTypes.Locator) == CNField.AllowedTypes.Locator )
      {
        CREditorUtils.GetSceneGameObjects(objects, listFilteredObjects);
      }
      if ( (field_.AllowedType & CNField.AllowedTypes.Geometry) == CNField.AllowedTypes.Geometry )
      {
        CREditorUtils.GetGeometryGameObjects(objects, listFilteredObjects);
      }
      if ( (field_.AllowedType & CNField.AllowedTypes.Animator) == CNField.AllowedTypes.Animator )
      {
        CREditorUtils.GetSceneGameObjectsWithComponent<Animator>(objects, listFilteredObjects);
      }
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjects( UnityEngine.Object[] objects, bool recalculateFields )
    {
      Undo.RecordObject(ownerNode_, "Add GameObjects to node");
      AddGameObjects( objects );

      if (recalculateFields)
      {
        cnHierarchy_.RecalculateFieldsDueToUserAction();
      }
      Undo.SetCurrentGroupName("Add GameObjects to node");
      Undo.FlushUndoRecordObjects();
      Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );

      if (WantsUpdate != null)
      {
        WantsUpdate();
      }
    }
    //-----------------------------------------------------------------------------------
    private void AddGameObjects(UnityEngine.Object[] objects)
    {
      listFilterSelection_.Clear();
      FilterObjects(objects, listFilterSelection_);

      List<uint> arrIdCaronte = new List<uint>();

      for (int i = 0; i < listFilterSelection_.Count; ++i)
      {
        GameObject go = listFilterSelection_[i];
        uint id;
        if (go != null && goManager_.GetIdCaronteFromGO(go, out id) )
        {
          arrIdCaronte.Add(id);
        }
      }

      FieldManager.AddGameObjects( id_, arrIdCaronte.ToArray(), (uint)arrIdCaronte.Count);
    }
    //-----------------------------------------------------------------------------------
    public void RemoveGameObjects( UnityEngine.Object[] selectedObjects )
    {
      Undo.RecordObject(ownerNode_, "CaronteFX - Remove GameObjects from field");

      listFilterSelection_.Clear();
      FilterObjects(selectedObjects, listFilterSelection_);

      for (int i = 0; i < listFilterSelection_.Count; ++i)
      {
        GameObject go = selectedObjects[i] as GameObject;
        RemoveGameObject( go );
      }

      Undo.SetCurrentGroupName("CaronteFX - Remove GameObjects from field");
      Undo.FlushUndoRecordObjects();
      Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );

      if (WantsUpdate != null)
      {
        WantsUpdate();
      }
    }
    //-----------------------------------------------------------------------------------
    private void RemoveGameObject(GameObject go)
    {
      uint id;
      if ( goManager_.GetIdCaronteFromGO(go, out id) )
      {
        FieldManager.RemoveGameObjectById( id_, id);

        cnHierarchy_.RecalculateFieldsDueToUserAction();
      }
    }
    //-----------------------------------------------------------------------------------
    public void Set( UnityEngine.Object[] selectedObjects )
    {
      FieldManager.ClearField( id_ );
      AddGameObjects( selectedObjects, true );
    }
    //-----------------------------------------------------------------------------------
    public void AddNameSelector(string nameSelector, bool recalculateFields)
    {
      if (nameSelector != string.Empty)
      {
        FieldManager.AddWildcardSelection( id_, nameSelector );

        if (recalculateFields)
        {
          Undo.RecordObject(ownerNode_, "CaronteFX - Add name selector to field");
          cnHierarchy_.RecalculateFieldsDueToUserAction();
          Undo.SetCurrentGroupName("CaronteFX - Add name selector to field");
          Undo.FlushUndoRecordObjects();
          Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
        }
      }

      if (WantsUpdate != null)
      {
        WantsUpdate();
      }
    }
    //-----------------------------------------------------------------------------------
    #region window commands
    //-----------------------------------------------------------------------------------
    public void AddNameSelectorWindow()
    {
      CNItemPopupWindow itemWindow = null;
      if (CNFieldWindow.IsOpen)
      {
        itemWindow = CNItemPopupWindow.ShowWindow( "Add name selector", CNFieldWindow.Instance.position );
      }
      else if ( CRManagerEditor.IsOpen)
      {
        itemWindow = CNItemPopupWindow.ShowWindow( "Add name selector", CRManagerEditor.Instance.position );
      }
      else
      {
        itemWindow = CNItemPopupWindow.ShowWindow( "Add name selector", new Rect() );
      }
   
      itemWindow.Init( new CNAddView( this ) );
    }
    //-----------------------------------------------------------------------------------
    public void AddNameSelectorContext( Rect position )
    {
      CNItemPopupWindow itemWindow = CNItemPopupWindow.ShowWindow( "Add name selector", position );
      itemWindow.Init( new CNAddView(this) );
    }
    //-----------------------------------------------------------------------------------
    public void SetToSelection()
    {
      FieldManager.ClearField( id_ );
      AddCurrentSelection();
    }
    //-----------------------------------------------------------------------------------
    public void AddCurrentSelection()
    {
      UnityEngine.Object[] selectedObjects = Selection.objects;

      listFilterSelection_.Clear();
      FilterObjects(selectedObjects, listFilterSelection_);

      if (selectedObjects.Length > 0)
      {
        AddGameObjects( listFilterSelection_.ToArray(), true );
      }
    }
    //-----------------------------------------------------------------------------------
    public void RemoveCurrentSelection()
    {
      UnityEngine.Object[] selectedObjects = Selection.objects;

      if (selectedObjects.Length > 0)
      {
        RemoveGameObjects( selectedObjects );
      }              
    }
    #endregion
    //-----------------------------------------------------------------------------------
    public GameObject[] GetUnityGameObjects()
    {   
      // field gameobjects
      List<GameObject> listGameObjects = GetListGOFromIdField(id_);

      // Append body node derived objects
      listGameObjects.AddRange( GetNodeDerivedObjects() );
      
      return ( listGameObjects.ToArray() );
    }
    //-----------------------------------------------------------------------------------
    public bool HasNoReferences()
    {
      List<GameObject>  listGameObject   = field_.GameObjects;
      List<String>      listNameSelector = field_.NameSelectors;
      List<CommandNode> listCommandNode  = field_.CommandNodes;

      if ( listGameObject.Count == 0 && listNameSelector.Count == 0 && listCommandNode.Count == 0 )
      {
        return true;
      }

      return false;
    }
    //-----------------------------------------------------------------------------------
    public GameObject[] GetUnityGameObjectsTopMost()
    {   
      // field gameobjects
      List<GameObject> listGameObjects = GetListGOFromIdField(id_);

      // Append body node derived objects
      listGameObjects.AddRange( GetNodeDerivedObjects() );
      
      UnityEngine.Object[] currentSelection = Selection.objects;
      Selection.objects = listGameObjects.ToArray();
      Selection.GetFiltered( typeof(GameObject), SelectionMode.TopLevel );

      listGameObjects.Clear();
      foreach(UnityEngine.Object obj in Selection.objects)
      {
        GameObject go = obj as GameObject;
        if ( go != null)
        {
          listGameObjects.Add(go);
        }
      }
      Selection.objects = currentSelection;
 
      return ( listGameObjects.ToArray() );
    }

    //-----------------------------------------------------------------------------------
    public CommandNode[] GetCommandNodes()
    {
      List<CommandNode> listCommandNode = field_.CommandNodes;
      listCommandNodeAux_.Clear();
      foreach( CommandNode cNode in listCommandNode )
      {
        listTreeNodeAux_.Clear();
        cNode.GetHierarchyPlainList(listTreeNodeAux_);

        foreach( CRTreeNode treeNode in listTreeNodeAux_)
        {
          CommandNode node = (CommandNode) treeNode;
          listCommandNodeAux_.Add(node);
        }
      }

      return listCommandNodeAux_.ToArray();
    }
    //-----------------------------------------------------------------------------------
    private List<GameObject> GetListGOFromIdField(uint idField)
    {
      int[] arrIdsUnity = FieldManager.GetIdsUnityFromField((uint)idField);
      return (GetListGOFromArrId(arrIdsUnity));
    }
    //-----------------------------------------------------------------------------------
    private List<GameObject> GetListGOFromArrId( int[] arrIdsUnity )
    {  
      List<GameObject> gameObjects = new List<GameObject>();
      int size = arrIdsUnity.Length;
      for ( int i = 0; i < size; i++ )
      {
        UnityEngine.Object objectInstance = EditorUtility.InstanceIDToObject( arrIdsUnity[i] );
        if (objectInstance != null)
        {
          gameObjects.Add( (GameObject)objectInstance );
        }  
      }
      return gameObjects;
    }
    //-----------------------------------------------------------------------------------
    private List<GameObject> GetNodeDerivedObjects()
    {
      listGameObjectAux_.Clear();

      List<CommandNode> listCommandNode = field_.CommandNodes;
      foreach( CommandNode cnode in listCommandNode )
      {
        listTreeNodeAux_.Clear();
        cnode.GetHierarchyPlainList(listTreeNodeAux_);

        foreach(CRTreeNode crTreeNode in listTreeNodeAux_)
        {
          CommandNodeEditor nodeEditor = cnHierarchy_.GetNodeEditor( (CommandNode)crTreeNode );
          CNBodyEditor bodyEditor = nodeEditor as CNBodyEditor;
          if (bodyEditor != null)
          {
            listGameObjectAux_.AddRange( bodyEditor.GetGameObjects() );  
          }      
        }     
      }

      return listGameObjectAux_;
    }
    //-----------------------------------------------------------------------------------
    private uint GetNodeNumberOfBodies(CommandNode node)
    {
      List<CommandNodeEditor> listBodyEditors = cnHierarchy_.GetEditorsFromHierarchy<CNBodyEditor>(node);
     
      uint total = 0;
      foreach(CommandNodeEditor nodeEditor in listBodyEditors)
      {
        CNBodyEditor bodyEditor = (CNBodyEditor)nodeEditor;
        List<GameObject> listGameObject = bodyEditor.GetGameObjects();
        total += eManager_.GetNumberOfBodyObjects( listGameObject.ToArray() );
      }
      return total;
    }
    //-----------------------------------------------------------------------------------   
    public void SceneSelectionAllObjects()
    {
      GameObject[] unityGameObjects = GetUnityGameObjects();
      Selection.objects = unityGameObjects;
    }
    //-----------------------------------------------------------------------------------
    public void SceneSelectionTopMost()
    {
      GameObject[] unityGameObjects = GetUnityGameObjects();
      Selection.objects = unityGameObjects;
      UnityEngine.Object[] topMostObjects = Selection.GetFiltered( typeof(GameObject), SelectionMode.TopLevel );
      Selection.objects = topMostObjects;
    }
    //-----------------------------------------------------------------------------------
    public void SceneSelectionBodiesCreated()
    {
      GameObject[] arrGameObjects = GetUnityGameObjects();

      HashSet<GameObject> setGameObjects = new HashSet<GameObject>();
      
      foreach( GameObject go in arrGameObjects )
      {
        setGameObjects.Add( go );
      }

      List<GameObject> listGameObjectsCreated = new List<GameObject>();
      foreach (GameObject go in arrGameObjects)
      {
        uint idBody = eManager_.GetIdBodyFromGo(go);
        if ( idBody != uint.MaxValue )
        {
          listGameObjectsCreated.Add(go);
        }
      }
      Selection.objects = listGameObjectsCreated.ToArray();
    }
    //-----------------------------------------------------------------------------------
    public void SceneSelectionBodiesCreatedTopMost()
    {
      GameObject[] arrGameObjects = GetUnityGameObjects();

      HashSet<GameObject> setGameObjects = new HashSet<GameObject>();
      
      foreach( GameObject go in arrGameObjects )
      {
        setGameObjects.Add( go );
      }

      List<GameObject> listGameObjectsCreated = new List<GameObject>();
      foreach (GameObject go in arrGameObjects)
      {
        uint idBody = eManager_.GetIdBodyFromGo(go);
        if ( idBody != uint.MaxValue )
        {
          listGameObjectsCreated.Add(go);
        }
      }
      Selection.objects = listGameObjectsCreated.ToArray();
      UnityEngine.Object[] topMostObjects = Selection.GetFiltered( typeof(GameObject), SelectionMode.TopLevel );
      Selection.objects = topMostObjects;
    }
    //-----------------------------------------------------------------------------------
    public void RestoreFieldInfo()
    {      
      bool FieldExists = FieldManager.IsFieldCreated( id_ );
      if ( FieldExists )
      {
        FieldManager.ClearField( id_ );
      }

      field_.PurgeInvalidNullNodes();
      LoadNameSelectors();
      LoadGOSelectors();
    }
    //-----------------------------------------------------------------------------------
    private void LoadNameSelectors()
    {
      List<string> lNameSelector = field_.NameSelectors;
      for (int i = 0; i < lNameSelector.Count; i++)
      {
        string wildcardName = lNameSelector[i];
        FieldManager.AddWildcardSelection( id_, wildcardName );
      }
    }
    //-----------------------------------------------------------------------------------
    private void LoadGOSelectors()
    {
      List<uint> listIdCaronte = new List<uint>();

      List<GameObject> lGameObject = field_.GameObjects;
      int sizeIds = lGameObject.Count;
      for (int i = 0; i < sizeIds; ++i )
      {
        GameObject go = lGameObject[i];
        uint id;
        if ( go != null && goManager_.GetIdCaronteFromGO( go, out id ) )
        {
          listIdCaronte.Add( id );
        }
      }
      FieldManager.AddGameObjects( id_, listIdCaronte.ToArray(), (uint)listIdCaronte.Count );
    }
    //-----------------------------------------------------------------------------------
    public void StoreFieldInfo()
    {
      field_.PurgeInvalidNullNodes();
      CreateSerializedNameSelectorInfo();
      CreateSerializedGameObjectsInfo();
    }
    //-----------------------------------------------------------------------------------
    private void CreateSerializedNameSelectorInfo()
    {
      List<string> lNameSelectors = field_.NameSelectors;

      lNameSelectors       .Clear();
      int nWildcardNames = (int)FieldManager.GetNumberOfWilcardSelections( id_ );
      for (int i = 0; i < nWildcardNames; ++i)
      {
        string wildcardName = FieldManager.GetWildcardSelectionName( id_, (uint) i );
        lNameSelectors.Add( wildcardName );
      }
    }
    //-----------------------------------------------------------------------------------
    private void CreateSerializedGameObjectsInfo()
    {    
      List<GameObject> lGameObjectIn  = field_.GameObjects;
      lGameObjectIn.Clear();

      hsGOIndexOutOfScope_ .Clear();

      int nGameObjects = (int)FieldManager.GetNumberOfGameObjects( id_ );
      for ( int i = 0; i < nGameObjects; ++i )
      {
        int id = FieldManager.GetSingleIdUnityFromGO( id_, (uint)i );
        lGameObjectIn.Add( (GameObject)EditorUtility.InstanceIDToObject( id ) );
        if ( !FieldManager.IsGameObjectInScope( id_, (uint)i ) )
        {
          hsGOIndexOutOfScope_.Add( i );    
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetScopeId(uint scopeId)
    {
      scopeId_ = scopeId;
      FieldManager.SetScopeID(id_, scopeId_ );
    }
    //-----------------------------------------------------------------------------------
    public void SetIsScope(bool isScope)
    {
      FieldManager.SetIsScope(id_, isScope);
    }
    //-----------------------------------------------------------------------------------
    public void SetCalculatesDiff(bool calculatesDiff)
    {
      FieldManager.SetCalculatesDiff(id_, calculatesDiff);
    }
    //-----------------------------------------------------------------------------------
    public CNField.ScopeFlag GetScopeType()
    {
      return field_.ScopeType;
    }
    //-----------------------------------------------------------------------------------
    public void SetScopeType(CNField.ScopeFlag scopeType)
    {
      field_.ScopeType = scopeType;
      switch (scopeType)
      {
        case CNField.ScopeFlag.Global:
          FieldManager.SetScopeRestricted(id_, false);
          break;
        case CNField.ScopeFlag.Inherited:
          FieldManager.SetScopeRestricted(id_, true);
          break;
      }
    }
    //-----------------------------------------------------------------------------------
    public bool IsUpdateNeeded()
    {
      List<CommandNode> lCommandNode = field_.CommandNodes;

      int[] arrIdsAdded, arrIdsDeleted;
      foreach( CommandNode node in lCommandNode )
      {
        if (node.NeedsUpdate)
        {
          return true;
        }
      }

      FieldManager.GetIdsUnityAdded( id_, out arrIdsAdded);
      FieldManager.GetIdsUnityDeleted( id_, out arrIdsDeleted);

      return (arrIdsAdded.Length > 0 || arrIdsDeleted.Length > 0);
    }
    //-----------------------------------------------------------------------------------
    public bool IsUpdateNeeded(out int[] arrIdsAdded, out int[] arrIdsDeleted)
    {
      FieldManager.GetIdsUnityAdded( id_, out arrIdsAdded );
      FieldManager.GetIdsUnityDeleted( id_, out arrIdsDeleted );
      return ( arrIdsAdded.Length > 0 || arrIdsDeleted.Length > 0 );
    }
    //-----------------------------------------------------------------------------------
    public bool AreGameObjectsAllowed()
    {
      return field_.AreGameObjectsAllowed();
    }
    //-----------------------------------------------------------------------------------
    public void FillOptionsMenu( GenericMenu optionsMenu )
    {
      if ( AreGameObjectsAllowed() )
      {
        optionsMenu.AddItem(new GUIContent("Add name selector"), false, AddNameSelectorWindow);
        optionsMenu.AddSeparator("");
        optionsMenu.AddItem(new GUIContent("Set to current GameObject selection"), false, SetToSelection);
        optionsMenu.AddItem(new GUIContent("Add current GameObject selection"),    false, AddCurrentSelection);
        optionsMenu.AddItem(new GUIContent("Remove current GameObject selection"), false, RemoveCurrentSelection);
        optionsMenu.AddSeparator("");
        optionsMenu.AddItem(new GUIContent("Clear field"), false, () => { Clear(true); } );
      }
      else
      {
        optionsMenu.AddDisabledItem(new GUIContent("Add name selector") );
        optionsMenu.AddSeparator("");
        optionsMenu.AddDisabledItem(new GUIContent("Set to current GameObject selection") );
        optionsMenu.AddDisabledItem(new GUIContent("Add current GameObject selection") );
        optionsMenu.AddDisabledItem(new GUIContent("Remove current GameObject selection") );
        optionsMenu.AddSeparator("");
        optionsMenu.AddItem(new GUIContent("Clear field"), false, () => { Clear(true); } );
      }
    }
    //-----------------------------------------------------------------------------------
    public void DrawFieldItems( Rect objectsRect, float tex_icon_size  )
    {
      GUI.Label( objectsRect, "", EditorStyles.objectField );

      Color currentColor = GUI.color;

      float objectsRectWidth = objectsRect.width;

      float currentWidth = 3;
      float newWidth     = 3;

      List<CommandNode> lCommandNode = Field.CommandNodes;
      List<string>     lNameSelector = Field.NameSelectors;
      List<GameObject>   lGameObject = Field.GameObjects;

      int nNodes         = lCommandNode.Count;
      int nNameSelectors = lNameSelector.Count;
      int nGameObjects   = lGameObject.Count;

      GUIContent ellipsisContent = new GUIContent("...");
      GUIContent commaContent    = new GUIContent(",");
      Vector2    ellipsisSize    = EditorStyles.label.CalcSize( ellipsisContent );
      Vector2    commaSize       = EditorStyles.label.CalcSize( commaContent );

      for( int i = 0; i < nNodes; i++ )
      {
        CommandNode node         = lCommandNode[i]; 
        string      nodeListName = node.ListName;
        Vector2     size         = EditorStyles.label.CalcSize( new GUIContent(nodeListName) );

        newWidth += size.x + commaSize.x + tex_icon_size;
   
        Rect  iconRect = new Rect(objectsRect.xMin + currentWidth, objectsRect.yMin, tex_icon_size, objectsRect.height);
        Rect labelRect = new Rect(objectsRect.xMin + currentWidth + tex_icon_size, objectsRect.yMin, size.x, size.y);

        if (newWidth + ellipsisSize.x < objectsRectWidth - ellipsisSize.x)
        {
          Texture texIcon = cnHierarchy_.GetNodeIcon( node );
          if (texIcon != null)
          {
            GUI.color = Color.clear;
            EditorGUI.DrawTextureTransparent( iconRect , texIcon, ScaleMode.ScaleToFit );
            GUI.color = currentColor;
          }

          GUI.Label( labelRect, nodeListName );
          if (i != nNodes - 1 || nNameSelectors > 0 || nGameObjects > 0)
          {
            Rect commaRect = new Rect(labelRect.xMax - 3, labelRect.yMin, commaSize.x, commaSize.y );
            GUI.Label( commaRect, commaContent);
          }
        }
        else
        {
          Rect ellipsisRect = new Rect(iconRect.xMin - 3, iconRect.yMin, ellipsisSize.x, ellipsisSize.y);
          GUI.Label( ellipsisRect, ellipsisContent );
          return;
        }

        currentWidth = newWidth;
      }

      for (int i = 0; i < nNameSelectors; i++)
      {
        string nameSelector = lNameSelector[i];
        GUIContent content = new GUIContent(nameSelector);
        Vector2 size = EditorStyles.label.CalcSize(content);

        newWidth += size.x + commaSize.x + tex_icon_size;

        Rect iconRect = new Rect(objectsRect.xMin + currentWidth, objectsRect.yMin, tex_icon_size, objectsRect.height);
        Rect labelRect = new Rect(objectsRect.xMin + currentWidth + tex_icon_size, objectsRect.yMin, size.x, size.y);

        if (newWidth + ellipsisSize.x < objectsRectWidth - ellipsisSize.x)
        {
          GUI.color = Color.clear;
          EditorGUI.DrawTextureTransparent(iconRect, CNFieldController.icon_nameselector_, ScaleMode.ScaleToFit);
          GUI.color = currentColor;
          GUI.Label(labelRect, content);
          if (i != nNameSelectors - 1 || nGameObjects > 0)
          {
            Rect commaRect = new Rect(labelRect.xMax - 3, labelRect.yMin, commaSize.x, commaSize.y);
            GUI.Label(commaRect, commaContent);
          }
        }
        else
        {
          Rect ellipsisRect = new Rect(iconRect.xMin - 3, iconRect.yMin, ellipsisSize.x, ellipsisSize.y);
          GUI.Label(ellipsisRect, ellipsisContent);
          return;
        }

        currentWidth = newWidth;
      }

      for (int i = 0; i < nGameObjects; i++)
      {
        GameObject go = lGameObject[i];
        if (go != null)
        {
          string nameGameObject  = lGameObject[i].name;
          GUIContent content;

          bool outOfScope = hsGOIndexOutOfScope_.Contains(i);
          if ( outOfScope )
          {
            content = new GUIContent(nameGameObject + " [out]");
          }
          else
          {
            content = new GUIContent(nameGameObject);
          }
               
          Vector2 size = EditorStyles.label.CalcSize(content);

          newWidth += size.x + commaSize.x + tex_icon_size;

          Rect iconRect = new Rect(objectsRect.xMin + currentWidth, objectsRect.yMin, tex_icon_size, objectsRect.height);
          Rect labelRect = new Rect(objectsRect.xMin + currentWidth + tex_icon_size, objectsRect.yMin, size.x, size.y);

          if (newWidth + ellipsisSize.x < objectsRectWidth - ellipsisSize.x)
          {
            GUI.color = Color.clear;
            EditorGUI.DrawTextureTransparent(iconRect, CNFieldController.icon_gameobject_, ScaleMode.ScaleToFit);
            GUI.color = currentColor;
            
            if (outOfScope)
            {
              EditorGUI.BeginDisabledGroup( outOfScope );
              GUI.color = Color.red;
              GUIStyle mystyle = new GUIStyle(EditorStyles.label);
              mystyle.normal.textColor = Color.red;
              GUI.Label( labelRect, content, mystyle );
              GUI.color = currentColor;
              EditorGUI.EndDisabledGroup();
            }
            else
            {
              GUI.Label( labelRect, content );
            }

 
            if (i != nGameObjects - 1)
            {
              Rect commaRect = new Rect(labelRect.xMax - 3, labelRect.yMin, commaSize.x, commaSize.y);
              GUI.Label(commaRect, commaContent);
            }
          }
          else
          {
            Rect ellipsisRect = new Rect(iconRect.xMin - 3, iconRect.yMin, ellipsisSize.x, ellipsisSize.y);
            GUI.Label(ellipsisRect, ellipsisContent);
            return;
          }
          currentWidth = newWidth;
        }

      }

      GUI.color = currentColor;
    }
    //-----------------------------------------------------------------------------------
    public void Clear(bool recalculateFields)
    {
      if (recalculateFields)
      {
        Undo.RecordObject(ownerNode_, "CaronteFX - Clear node field");
      }
      FieldManager.ClearField( id_ );
      field_.Clear();

      if (recalculateFields)
      {
        cnHierarchy_.RecalculateFieldsDueToUserAction();

        Undo.SetCurrentGroupName("CaronteFX - Clear node field");
        Undo.FlushUndoRecordObjects();
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

        if (WantsUpdate != null)
        {
          WantsUpdate();
        }
      }   
    }
  }
}

