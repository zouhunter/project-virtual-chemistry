using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace WorldActionSystem
{
    [CustomPropertyDrawer(typeof(ActionPrefabItem), true)]
    public class ActionPrefabItemDrawer : PropertyDrawer
    {
        protected SerializedProperty containsCommandProp;
        protected SerializedProperty containsPickupProp;
        protected SerializedProperty rematrixProp;
        protected SerializedProperty matrixProp;

        protected SerializedProperty instanceIDProp;
        protected SerializedProperty prefabProp;
        protected SerializedProperty ignoreProp;
        protected void FindCommonPropertys(SerializedProperty property)
        {
            rematrixProp = property.FindPropertyRelative("rematrix");
            matrixProp = property.FindPropertyRelative("matrix");
            containsPickupProp = property.FindPropertyRelative("containsPickup");
            instanceIDProp = property.FindPropertyRelative("instanceID");
            containsCommandProp = property.FindPropertyRelative("containsCommand");
            ignoreProp = property.FindPropertyRelative("ignore");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindCommonPropertys(property);
            prefabProp = property.FindPropertyRelative("prefab");
            return (property.isExpanded ? 2:1)* EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindCommonPropertys(property);
            prefabProp = property.FindPropertyRelative("prefab");

            if (prefabProp.objectReferenceValue != null){
                label = new GUIContent(prefabProp.objectReferenceValue.name);
            }
            var rect = new Rect(position.x, position.y, position.width * 0.9f, EditorGUIUtility.singleLineHeight);
            var str = prefabProp.objectReferenceValue == null ? "" : prefabProp.objectReferenceValue.name;
            GUI.contentColor = Color.cyan;
            if (GUI.Button(rect, str, EditorStyles.toolbarDropDown))
            {
                property.isExpanded = !property.isExpanded;
                if (property.isExpanded)
                {
                    ActionEditorUtility.LoadPrefab(prefabProp,containsCommandProp,containsPickupProp, instanceIDProp, rematrixProp, matrixProp);
                }
                else
                {
                    ActionEditorUtility.SavePrefab(instanceIDProp, rematrixProp, matrixProp);
                }
            }
            GUI.contentColor = Color.white;

            InformationShow(rect);

             rect = new Rect(position.max.x - position.width * 0.1f, position.y,20, EditorGUIUtility.singleLineHeight);

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    break;
                case EventType.DragPerform:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        Debug.Log(DragAndDrop.objectReferences.Length);
                        if (DragAndDrop.objectReferences.Length > 0)
                        {
                            var obj = DragAndDrop.objectReferences[0];
                            if (obj is GameObject){
                                ActionEditorUtility.InsertItem(prefabProp, obj);
                            }
                            DragAndDrop.AcceptDrag();
                        }
                        Event.current.Use();
                    }
                    break;
                case EventType.DragExited:
                    break;
            }

            if (prefabProp.objectReferenceValue != null)
            {
                if (GUI.Button(rect, "", EditorStyles.objectFieldMiniThumb))
                {
                    EditorGUIUtility.PingObject(prefabProp.objectReferenceValue);
                }
            }
            else
            {
                prefabProp.objectReferenceValue = EditorGUI.ObjectField(rect, null, typeof(GameObject), false);
            }
          
            rect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

            if (property.isExpanded)
            {
                DrawOptions(rect);
            }
        }

        protected void DrawOptions(Rect rect)
        {
            var optionCount = 4;
            var width = rect.width / optionCount;
            var choiseRect = new Rect(rect.x, rect.y, width, rect.height);
            rematrixProp.boolValue = EditorGUI.ToggleLeft(choiseRect, "Matrix", rematrixProp.boolValue);
            choiseRect.x += width;
            ignoreProp.boolValue = EditorGUI.ToggleLeft(choiseRect, "[Ignore]", ignoreProp.boolValue);
            choiseRect.x += width;
        }

        protected void InformationShow(Rect rect)
        {
            if (prefabProp.objectReferenceValue == null)
            {
                EditorGUI.HelpBox(rect, "丢失", MessageType.Error);
            }
            else
            {
                var infoRect = rect;
                infoRect.x = infoRect.width - 80;
                infoRect.width = 25;
                if (containsPickupProp.boolValue)
                {
                    GUI.color = new Color(0.3f, 0.5f, 0.8f);
                    EditorGUI.SelectableLabel(infoRect, "[p]");
                    infoRect.x += infoRect.width;
                }

                if (rematrixProp.boolValue)
                {
                    GUI.color = new Color(0.8f, 0.8f, 0.4f);
                    EditorGUI.SelectableLabel(infoRect, "[m]");
                    infoRect.x += infoRect.width;
                }

                if (containsCommandProp.boolValue)
                {
                    GUI.color = new Color(0.5f, 0.8f, 0.3f);
                    EditorGUI.SelectableLabel(infoRect, "[c]");
                    infoRect.x += infoRect.width;
                }
                if (EditorGUIUtility.isProSkin)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = new Color(1, 0, 0, 0.3f);
                }
                if (ignoreProp.boolValue)
                {
                    GUI.Box(rect,"");
                }
                GUI.color = Color.white;
            }
        }
    }
}