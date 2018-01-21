using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace WorldActionSystem
{

    public abstract class ActionGroupDrawerBase : Editor
    {
        public enum SortType
        {
            ByName
        }
        protected bool swink;
        protected string query;
        protected SerializedProperty script;
        protected SerializedProperty groupKeyProp;
        protected SerializedProperty totalCommandProp;
        protected SerializedProperty prefabListProp;
        //protected DragAdapt prefabListPropAdapt;

        protected SerializedProperty prefabListWorp;
        //protected DragAdapt prefabListWorpAdapt;
        protected static int selected = 0;
        protected static SortType currSortType;
        protected GUIContent[] selectables;
        protected GUIContent[] Selectables
        {
            get
            {
                if (selectables == null)
                {
                    selectables = new GUIContent[] {
                        new GUIContent(EditorGUIUtility.IconContent("winbtn_mac_max").image,"all"),
                        new GUIContent(EditorGUIUtility.IconContent("winbtn_mac_close").image,"contains command"),
                        new GUIContent(EditorGUIUtility.IconContent("winbtn_mac_min").image,"contains pickup"),
                        new GUIContent(EditorGUIUtility.IconContent("winbtn_mac_inact").image,"normal"),
                    };
                }
                return selectables;
            }
        }

        protected ReorderableList prefabList_r;
        protected ReorderableList prefabListWorp_r;
        private void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");
            totalCommandProp = serializedObject.FindProperty("totalCommand");
            groupKeyProp = serializedObject.FindProperty("groupKey");
            prefabListProp = serializedObject.FindProperty("prefabList");
            //prefabListPropAdapt = new DragAdapt(prefabListProp, "prefabList");
            //prefabListWorpAdapt = new DragAdapt(prefabListWorp, "prefabList");
            MarchList();
            CalcuteCommandCount();
            InitReorderList();
        }

        private void OnDisable()
        {
            ApplyWorpPrefabList();
            CalcuteCommandCount();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawScript();
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                DrawToolButtons();
            }
            DrawControllers();
            DrawSelection();
            DrawRuntimeItems();
            DrawAcceptRegion();
            serializedObject.ApplyModifiedProperties();
        }
        private void InitReorderList()
        {
            prefabList_r = new ReorderableList(serializedObject, prefabListProp);
            prefabList_r.elementHeightCallback += GetElementHeight;
            prefabList_r.drawHeaderCallback += DrawPrefabListHeader;
            prefabList_r.drawElementCallback += DrawListElement;
            var sobj = new SerializedObject(ScriptableObject.CreateInstance<ActionGroupObj>());
            prefabListWorp = sobj.FindProperty("prefabList");
            prefabListWorp_r = new ReorderableList(sobj, prefabListWorp);
            prefabListWorp_r.drawHeaderCallback += DrawWorpListHeader;
            prefabListWorp_r.drawElementCallback += DrawWorpListElement;
            prefabListWorp_r.elementHeightCallback += GetWorpElementHeight;
        }

        private float GetWorpElementHeight(int index)
        {
            var property = prefabListWorp.GetArrayElementAtIndex(index);
            return (property.isExpanded ? 2 : 1) * EditorGUIUtility.singleLineHeight;
        }

        private float GetElementHeight(int index)
        {
            var property = prefabListProp.GetArrayElementAtIndex(index);
            return (property.isExpanded ? 2 : 1) * EditorGUIUtility.singleLineHeight;
        }

        private void DrawWorpListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = prefabListWorp.GetArrayElementAtIndex(index);
            if (prop != null) EditorGUI.PropertyField(rect, prop, true);
        }
        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = prefabListProp.GetArrayElementAtIndex(index);
            if (prop != null) EditorGUI.PropertyField(rect, prop, true);
        }

        private void DrawWorpListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "[March]", EditorStyles.boldLabel);
        }

        private void DrawPrefabListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "元素列表", EditorStyles.boldLabel);
        }

        private void DrawSelection()
        {
            EditorGUI.BeginChangeCheck();
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                query = EditorGUILayout.TextField(query);
                selected = GUILayout.SelectionGrid(selected, Selectables, 4);
            }
            if (EditorGUI.EndChangeCheck())
            {
                MarchList();
            }
        }

        private void DrawControllers()
        {
            EditorGUILayout.PropertyField(groupKeyProp);

            if (string.IsNullOrEmpty(groupKeyProp.stringValue))
            {
                groupKeyProp.stringValue = target.name;
            }
        }

        private void DrawScript()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void DrawRuntimeItems()
        {
            if (string.IsNullOrEmpty(query) && selected == 0)
            {
                prefabList_r.DoLayoutList();
            }
            else
            {
                prefabListWorp_r.DoLayoutList();
            }

        }

        private void MarchList()
        {
            if (string.IsNullOrEmpty(query) && selected == 0) return;

            prefabListWorp.ClearArray();

            for (int i = 0; i < prefabListProp.arraySize; i++)
            {
                var prop = prefabListProp.GetArrayElementAtIndex(i);
                var prefabProp = prop.FindPropertyRelative("prefab");

                if (prefabProp.objectReferenceValue == null || !prefabProp.objectReferenceValue.name.ToLower().Contains(query.ToLower()))
                {
                    continue;
                }

                var containCommandProp = prop.FindPropertyRelative("containsCommand");
                var containsPickupProp = prop.FindPropertyRelative("containsPickup");

                if (selected == 1)
                {
                    if (!containCommandProp.boolValue)
                    {
                        continue;
                    }
                }
                else if (selected == 2)
                {
                    if (!containsPickupProp.boolValue)
                    {
                        continue;
                    }
                }
                else if (selected == 3)
                {
                    if (containCommandProp.boolValue || containsPickupProp.boolValue)
                    {
                        continue;
                    }
                }

                prefabListWorp.InsertArrayElementAtIndex(0);
                ActionEditorUtility.CopyPropertyValue(prefabListWorp.GetArrayElementAtIndex(0), prefabListProp.GetArrayElementAtIndex(i));
            }
        }

        private void DrawToolButtons()
        {
            var btnStyle = EditorStyles.toolbarButton;
            if (GUILayout.Button(new GUIContent("%", "移除重复"), btnStyle))
            {
                RemoveDouble();
            }
            if (GUILayout.Button(new GUIContent("！", "排序"), btnStyle))
            {
                SortPrefabs(selected == 0 ? prefabListProp : prefabListWorp);
            }
            if (GUILayout.Button(new GUIContent("o", "批量加载"), btnStyle))
            {
                GroupLoadPrefabs(selected == 0 ? prefabListProp : prefabListWorp);
            }
            if (GUILayout.Button(new GUIContent("c", "批量关闭"), btnStyle))
            {
                CloseAllCreated(selected == 0 ? prefabListProp : prefabListWorp);
            }
            if (GUILayout.Button(new GUIContent("i", "反向忽略"), btnStyle))
            {
                IgnoreNotIgnored(prefabListProp);
            }
        }

        protected abstract void RemoveDouble();

        protected void SortPrefabs(SerializedProperty property)
        {
            if (currSortType == SortType.ByName)
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    for (int j = i; j < property.arraySize - i - 1; j++)
                    {
                        var itemj = property.GetArrayElementAtIndex(j).FindPropertyRelative("prefab");
                        var itemj1 = property.GetArrayElementAtIndex(j + 1).FindPropertyRelative("prefab");

                        if (itemj.objectReferenceValue == null || itemj1.objectReferenceValue == null) continue;

                        if (string.Compare(itemj.objectReferenceValue.name, itemj1.objectReferenceValue.name) > 0)
                        {
                            property.MoveArrayElement(j, j + 1);
                        }
                    }
                }
            }
        }

        private void IgnoreNotIgnored(SerializedProperty prefabListProp)
        {
            for (int i = 0; i < prefabListProp.arraySize; i++)
            {
                var ignorePorp = prefabListProp.GetArrayElementAtIndex(i).FindPropertyRelative("ignore");
                ignorePorp.boolValue = !ignorePorp.boolValue;
            }
        }

        /// <summary>
        /// 绘制作快速导入的区域
        /// </summary>
        private void DrawAcceptRegion()
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent("哈哈"), EditorStyles.toolbarButton);
            rect.y -= EditorGUIUtility.singleLineHeight;
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    break;
                case EventType.DragPerform:
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        var objs = DragAndDrop.objectReferences;
                        for (int i = 0; i < objs.Length; i++)
                        {
                            var obj = objs[i];
                            var prefab = PrefabUtility.GetPrefabParent(obj);
                            UnityEngine.Object goodObj = null;
                            if (prefab != null)
                            {
                                goodObj = PrefabUtility.FindPrefabRoot(prefab as GameObject);
                            }
                            else
                            {
                                var path = AssetDatabase.GetAssetPath(obj);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    goodObj = obj;
                                }
                            }
                            prefabListProp.InsertArrayElementAtIndex(prefabListProp.arraySize);
                            var itemprefab = prefabListProp.GetArrayElementAtIndex(prefabListProp.arraySize - 1);
                            itemprefab.FindPropertyRelative("prefab").objectReferenceValue = goodObj;
                        }
                    }
                    break;
            }
        }

        private void GroupLoadPrefabs(SerializedProperty proprety)
        {
            for (int i = 0; i < proprety.arraySize; i++)
            {
                var itemProp = proprety.GetArrayElementAtIndex(i);
                GameObject prefab = null;
                var prefabProp = itemProp.FindPropertyRelative("prefab");
                var instanceIDProp = itemProp.FindPropertyRelative("instanceID");
                if (instanceIDProp.intValue != 0)
                {
                    var gitem = EditorUtility.InstanceIDToObject(instanceIDProp.intValue);
                    if (gitem != null)
                    {
                        continue;
                    }
                }
                prefab = prefabProp.objectReferenceValue as GameObject;

                if (prefab == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog("空对象", "找不到预制体", "确认");
                }
                else
                {
                    var matrixProp = itemProp.FindPropertyRelative("matrix");
                    var rematrixProp = itemProp.FindPropertyRelative("rematrix");
                    var containsCommandProp = itemProp.FindPropertyRelative("containsCommand");
                    var containsPickupProp = itemProp.FindPropertyRelative("containsPickup");
                    ActionEditorUtility.LoadPrefab(prefabProp, containsCommandProp, containsPickupProp, instanceIDProp, rematrixProp, matrixProp);
                    itemProp.isExpanded = true;
                }

            }
        }



        private void CloseAllCreated(SerializedProperty arrayProp)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var itemProp = arrayProp.GetArrayElementAtIndex(i);
                var instanceIDPorp = itemProp.FindPropertyRelative("instanceID");
                var obj = EditorUtility.InstanceIDToObject(instanceIDPorp.intValue);
                if (obj != null)
                {
                    var matrixProp = itemProp.FindPropertyRelative("matrix");
                    var rematrixProp = itemProp.FindPropertyRelative("rematrix");
                    ActionEditorUtility.SavePrefab(instanceIDPorp, rematrixProp, matrixProp);
                    DestroyImmediate(obj);
                }
                itemProp.isExpanded = false;
            }
        }

        private void TrySaveAllPrefabs(SerializedProperty arrayProp)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var item = arrayProp.GetArrayElementAtIndex(i);
                var instanceIDPorp = item.FindPropertyRelative("instanceID");
                var obj = EditorUtility.InstanceIDToObject(instanceIDPorp.intValue);
                if (obj == null) continue;
                var prefab = PrefabUtility.GetPrefabParent(obj);
                if (prefab != null)
                {
                    var root = PrefabUtility.FindPrefabRoot((GameObject)prefab);
                    if (root != null)
                    {
                        PrefabUtility.ReplacePrefab(obj as GameObject, root, ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
            }
        }

        private void CalcuteCommandCount()
        {
            if (target == null) return;

            var commandList = new List<ActionCommand>();

            if (target is ActionGroup)
            {
                var transform = (target as ActionGroup).transform;
                Utility.RetriveCommand(transform, (x) => { if (!commandList.Contains(x)) commandList.Add(x); });
            }


            for (int i = 0; i < prefabListProp.arraySize; i++)
            {
                var prop = prefabListProp.GetArrayElementAtIndex(i);
                var ignore = prop.FindPropertyRelative("ignore");

                var pfb = prop.FindPropertyRelative("prefab");
                var contaionCommand = prop.FindPropertyRelative("containsCommand");
                contaionCommand.boolValue = false;
                var contaionPickUp = prop.FindPropertyRelative("containsPickup");
                contaionPickUp.boolValue = false;

                if (pfb.objectReferenceValue != null)
                {
                    var go = pfb.objectReferenceValue as GameObject;
                    Utility.RetriveCommand(go.transform, (x) =>
                    {
                        if (x != null)
                        {
                            contaionCommand.boolValue = true;
                            if (!commandList.Contains(x) && !ignore.boolValue) commandList.Add(x);
                        }
                    });
                    Utility.RetivePickElement(go.transform, (x) =>
                    {
                        if (x != null)
                        {
                            contaionPickUp.boolValue = true;
                        }
                    });
                }
            }
            totalCommandProp.intValue = commandList.Count;
            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyWorpPrefabList()
        {
            List<SerializedProperty> needAdd = new List<SerializedProperty>();

            if (prefabListWorp.arraySize > 0)
            {
                for (int i = 0; i < prefabListWorp.arraySize; i++)
                {
                    var newProp = prefabListWorp.GetArrayElementAtIndex(i);
                    var newprefabProp = newProp.FindPropertyRelative("prefab");
                    var newrematrixProp = newProp.FindPropertyRelative("rematrix");
                    bool contain = false;
                    for (int j = 0; j < prefabListProp.arraySize; j++)
                    {
                        var prop = prefabListProp.GetArrayElementAtIndex(j);
                        var prefabProp = prop.FindPropertyRelative("prefab");
                        var rematrixProp = prop.FindPropertyRelative("rematrix");
                        if (prefabProp.objectReferenceValue == newprefabProp.objectReferenceValue)
                        {
                            if (newrematrixProp.boolValue == rematrixProp.boolValue && rematrixProp.boolValue == false)
                            {
                                ActionEditorUtility.CopyPropertyValue(prop, newProp);
                            }
                            contain = true;
                        }
                    }
                    if (!contain)
                    {
                        needAdd.Add(newProp);
                    }
                }
            }

            for (int i = 0; i < needAdd.Count; i++)
            {
                prefabListProp.InsertArrayElementAtIndex(0);
                var newProp = needAdd[i];
                var prop = prefabListProp.GetArrayElementAtIndex(0);
                ActionEditorUtility.CopyPropertyValue(prop, newProp);
            }
        }
    }
}