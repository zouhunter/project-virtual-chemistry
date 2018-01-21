//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Events;
//using System.Collections.Generic;
//using UnityEditor;
//using Rotorz.ReorderableList;
//using Rotorz.ReorderableList.Internal;

//namespace WorldActionSystem
//{
//    public class DragAdapt : SerializedPropertyAdaptor, IReorderableListDropTarget
//    {
//        private string type;
//        private const float MouseDragThresholdInPixels = 0.6f;
//        private const int widthBt = 20;
//        // Static reference to the list adaptor of the selected item.
//        private static DragAdapt s_SelectedList;
//        // Static reference limits selection to one item in one list.
//        private static SerializedProperty s_SelectedItem;
//        // Position in GUI where mouse button was anchored before dragging occurred.
//        private static Vector2 s_MouseDownPosition;

//        // Holds data representing the item that is being dragged.
//        private class DraggedItem
//        {

//            public static readonly string TypeName = typeof(DraggedItem).FullName;

//            public readonly DragAdapt SourceListAdaptor;
//            public readonly int Index;
//            public readonly SerializedProperty ShoppingItem;

//            public DraggedItem(DragAdapt sourceList, int index, SerializedProperty shoppingItem)
//            {
//                SourceListAdaptor = sourceList;
//                Index = index;
//                ShoppingItem = shoppingItem;
//            }

//        }

//        public DragAdapt(SerializedProperty list,string type) : base(list)
//        {
//            this.type = type;
//        }

//        public override void DrawItemBackground(Rect position, int index)
//        {
//            if (this == s_SelectedList && this[index] == s_SelectedItem)
//            {
//                Color restoreColor = GUI.color;
//                GUI.color = ReorderableListStyles.SelectionBackgroundColor;
//                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
//                GUI.color = restoreColor;
//            }
//        }

//        public override void DrawItem(Rect position, int index)
//        {
//            SerializedProperty shoppingItem = this[index];

//            int controlID = GUIUtility.GetControlID(FocusType.Passive);

//            switch (Event.current.GetTypeForControl(controlID))
//            {
//                case EventType.MouseDown:
//                    Rect totalItemPosition = ReorderableListGUI.CurrentItemTotalPosition;
//                    //var width = totalItemPosition.width - widthBt * 8;
//                    //width /= 1.5f;
//                    //Rect draggableRect = new Rect(width + totalItemPosition.x, totalItemPosition.y, totalItemPosition.width - width - widthBt * 8,EditorGUIUtility.singleLineHeight);
//                    var draggableRect = new Rect(totalItemPosition.x, totalItemPosition.y, totalItemPosition.width * 0.1f, EditorGUIUtility.singleLineHeight);
//                    if (draggableRect.Contains(Event.current.mousePosition))
//                    {
//                        // Select this list item.
//                        s_SelectedList = this;
//                        s_SelectedItem = shoppingItem;
//                    }

//                    // Calculate rectangle of draggable area of the list item.
//                    // This example excludes the grab handle at the left.
//                    //draggableRect.x = position.x;
//                    //draggableRect.width = position.width;

//                    if (Event.current.button == 0 && draggableRect.Contains(Event.current.mousePosition))
//                    {
//                        // Select this list item.
//                        s_SelectedList = this;
//                        s_SelectedItem = shoppingItem;

//                        // Lock onto this control whilst left mouse button is held so
//                        // that we can start a drag-and-drop operation when user drags.
//                        GUIUtility.hotControl = controlID;
//                        s_MouseDownPosition = Event.current.mousePosition;
//                        Event.current.Use();
//                    }
//                    break;

//                case EventType.MouseDrag:
//                    if (GUIUtility.hotControl == controlID)
//                    {
//                        GUIUtility.hotControl = 0;

//                        // Begin drag-and-drop operation when the user drags the mouse
//                        // pointer across the threshold. This threshold helps to avoid
//                        // inadvertently starting a drag-and-drop operation.
//                        if (Vector2.Distance(s_MouseDownPosition, Event.current.mousePosition) >= MouseDragThresholdInPixels)
//                        {
//                            // Prepare data that will represent the item.
//                            var item = new DraggedItem(this, index, shoppingItem);

//                            // Start drag-and-drop operation with the Unity API.
//                            DragAndDrop.PrepareStartDrag();
//                            // Need to reset `objectReferences` and `paths` because `PrepareStartDrag`
//                            // doesn't seem to reset these (at least, in Unity 4.x).
//                            DragAndDrop.objectReferences = new Object[0];
//                            DragAndDrop.paths = new string[0];

//                            DragAndDrop.SetGenericData(DraggedItem.TypeName, item);
//                            var element = shoppingItem.FindPropertyRelative("prefab");
//                            if(element.objectReferenceValue != null)
//                            {
//                                DragAndDrop.StartDrag(element.objectReferenceValue.name);
//                            }
//                        }

//                        // Use this event so that the host window gets repainted with
//                        // each mouse movement.
//                        Event.current.Use();
//                    }
//                    break;

//                case EventType.Repaint:
//                    //EditorStyles.label.Draw(position, shoppingItem.FindPropertyRelative("assetName").stringValue, false, false, false, false);
//                    break;
//            }
//            EditorGUI.PropertyField(position, this[index], true);
//        }

//        public bool CanDropInsert(int insertionIndex)
//        {
//            if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
//                return false;

//            // Drop insertion is possible if the current drag-and-drop operation contains
//            // the supported type of custom data.
//            if (!(DragAndDrop.GetGenericData(DraggedItem.TypeName) is DraggedItem))
//            {
//                return false;
//            }
//            var dragedItem = DragAndDrop.GetGenericData(DraggedItem.TypeName) as DraggedItem;
//            return dragedItem.SourceListAdaptor.type == type;
//        }

//        public void ProcessDropInsertion(int insertionIndex)
//        {
//            if (Event.current.type == EventType.DragPerform)
//            {
//                var draggedItem = DragAndDrop.GetGenericData(DraggedItem.TypeName) as DraggedItem;

//                // Are we just reordering within the same list?
//                if (draggedItem.SourceListAdaptor == this)
//                {
//                    Move(draggedItem.Index, insertionIndex);
//                }
//                else
//                {
//                    // Nope, we are moving the item!
//                    this.Insert(insertionIndex);
//                    SerializedPropertyUtility.CopyPropertyValue(this[insertionIndex], draggedItem.ShoppingItem);
//                    if(!Event.current.control) draggedItem.SourceListAdaptor.Remove(draggedItem.Index);
//                    draggedItem.SourceListAdaptor.arrayProperty.serializedObject.ApplyModifiedProperties();
//                    // Ensure that the item remains selected at its new location!
//                    s_SelectedList = this;
//                }
//            }
//        }

//        public override float GetItemHeight(int index)
//        {
//            return base.GetItemHeight(index);
//        }

//    }
//}
