
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FlowSystem
{
    public static class LayoutOption
    {
        public static GUILayoutOption minWidth = GUILayout.Width(20);
        public static GUILayoutOption shortWidth = GUILayout.Width(50);
        public static GUILayoutOption mediaWidth = GUILayout.Width(75);
        public static GUILayoutOption longWidth = GUILayout.Width(100);
        public static GUILayoutOption maxWidth = GUILayout.Width(200);

        public static GUILayoutOption shortHigh = GUILayout.Height(EditorGUIUtility.singleLineHeight);
        public static GUILayoutOption mediaHigh = GUILayout.Height(EditorGUIUtility.singleLineHeight * 2);
        public static GUILayoutOption longHigh = GUILayout.Height(EditorGUIUtility.singleLineHeight * 5);
        public static GUILayoutOption maxHight = GUILayout.Height(EditorGUIUtility.singleLineHeight * 10);
    }

    public class ConnectConfigWindow : EditorWindow
    {
        private NodeItemBehaiver node_A;
        private NodeItemBehaiver node_B;
        private InOutItemBehaiver item_A;
        private InOutItemBehaiver item_B;

        GameObject selected = null;

        [MenuItem("Window/ConnectConfig")]
        static void OpenConnectConfigWindow()
        {
            EditorWindow window = GetWindow<ConnectConfigWindow>("连接配制", true);
            window.position = new Rect(400, 300, 500, 300);
            window.Show();
        }

        void OnGUI()
        {
            TryConnect();

            if (Selection.activeGameObject != null)
            {
                selected = Selection.activeGameObject;
            }
            using (var group = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("ClampRot"))
                {
                    if (selected == null) return;

                    Vector3 newRot = selected.transform.eulerAngles;
                    System.Func<float, float> clamp = (value) =>
                    {
                        float newValue = value % 360;
                        if (newValue < 45)
                        {
                            newValue = 0;
                        }
                        else
                        {
                            if (newValue < 135)
                            {
                                newValue = 90;
                            }
                            else
                            {
                                if (newValue < 225)
                                {
                                    newValue = 180;
                                }
                                else
                                {
                                    if (newValue < 315)
                                    {
                                        newValue = -90;
                                    }
                                }
                            }
                        }
                        return newValue;
                    };

                    newRot.x = clamp(newRot.x);
                    newRot.y = clamp(newRot.y);
                    newRot.z = clamp(newRot.z);

                    selected.transform.eulerAngles = newRot;
                }
            }
        }

        void TryConnect()
        {
            //itemA = EditorGUILayout.ObjectField("元素A", itemA, typeof(InOutItemBehaiver), true) as InOutItemBehaiver;
            node_A = EditorGUILayout.ObjectField("A子节点", node_A, typeof(NodeItemBehaiver), true) as NodeItemBehaiver;
            //itemB = EditorGUILayout.ObjectField("元素B", itemB, typeof(InOutItemBehaiver), true) as InOutItemBehaiver;
            node_B = EditorGUILayout.ObjectField("B子节点", node_B, typeof(NodeItemBehaiver), true) as NodeItemBehaiver;

            if (node_A != null)
            {
                 item_A = FindInoutItem(node_A);
            }

            if (node_B != null)
            {
                item_B = FindInoutItem(node_B);
            }

            if (node_A == null || node_B == null || item_A == null || item_B == null)
            {
                return;
            }
            using (var group = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("建立坐标关系"))
                {
                    CreateConnect();
                }
                if (GUILayout.Button("加载已经连接"))
                {
                    LoadConnect();
                }
            }
            
        }

        private void LoadConnect()
        {
            ConnectAble nodeArecored = node_A.connectAble.Find((x) => x.itemName == item_B.itemInfo.name && x.nodeId == node_B.Info.nodeID);
            if (nodeArecored != null)
            {
                item_A.transform.position = item_B.transform.TransformPoint(nodeArecored.relativePos);
                item_A.transform.forward = item_B.transform.TransformDirection(nodeArecored.relativeDir);
            }
        }

        InOutItemBehaiver FindInoutItem(NodeItemBehaiver node)
        {
            Transform parent = node.transform.parent;
            while (parent.GetComponent<InOutItemBehaiver>() == null)
            {
                parent = parent.parent;
            }
            return parent.GetComponent<InOutItemBehaiver>();
        }

        void CreateConnect()
        {
            RecordInout();

            ConnectAble nodeArecored = node_A.connectAble.Find((x) => x.itemName == item_B.itemInfo.name && x.nodeId == node_B.Info.nodeID);
            ConnectAble nodeBrecored = node_B.connectAble.Find((x) => x.itemName == item_A.itemInfo.name && x.nodeId == node_A.Info.nodeID);
            //已经记录过
            if (nodeArecored == null)
            {
                nodeArecored = new ConnectAble();
                node_A.connectAble.Add(nodeArecored);
            }
            if (nodeBrecored == null)
            {
                nodeBrecored = new ConnectAble();
                node_B.connectAble.Add(nodeBrecored);
            }

            RecoreNameAndID(nodeArecored, nodeBrecored);
            RecordTransform(nodeArecored, nodeBrecored, item_A.transform, item_B.transform);

            EditorUtility.SetDirty(node_A);
            EditorUtility.SetDirty(node_B);
            EditorUtility.DisplayDialog("通知", "Complete", "确认");

        }

        void RecordInout()
        {
            EditorUtility.SetDirty(node_A);
            EditorUtility.SetDirty(node_B);
        }

        void RecoreNameAndID(ConnectAble nodeArecored, ConnectAble nodeBrecored)
        {
            nodeArecored.itemName = item_B.itemInfo.name;
            nodeBrecored.itemName = item_A.itemInfo.name;
            nodeArecored.nodeId = node_B.Info.nodeID;
            nodeBrecored.nodeId = node_A.Info.nodeID;
        }

        void RecordTransform(ConnectAble nodeArecored, ConnectAble nodeBrecored, Transform ourItem, Transform otherItem)
        {
            nodeArecored.relativePos = otherItem.InverseTransformPoint(ourItem.position);
            nodeArecored.relativeDir = otherItem.InverseTransformDirection(ourItem.forward); //Quaternion.Inverse(ourItem.rotation) * otherItem.rotation;

            nodeBrecored.relativePos = ourItem.InverseTransformPoint(otherItem.position);
            nodeBrecored.relativeDir = ourItem.InverseTransformDirection(otherItem.forward);// Quaternion.Inverse(otherItem.rotation) * ourItem.rotation;
        }
    }
}