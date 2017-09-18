
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Connector
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
        private PortItemBehaiver node_A;
        private PortItemBehaiver node_B;
        private PortParentBehaiver item_A;
        private PortParentBehaiver item_B;

        GameObject selected = null;

        [MenuItem("Window/Connector/Connect")]
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
            //itemA = EditorGUILayout.ObjectField("元素A", itemA, typeof(ElementItemBehaiver), true) as ElementItemBehaiver;
            node_A = EditorGUILayout.ObjectField("A子节点", node_A, typeof(PortItemBehaiver), true) as PortItemBehaiver;
            //itemB = EditorGUILayout.ObjectField("元素B", itemB, typeof(ElementItemBehaiver), true) as ElementItemBehaiver;
            node_B = EditorGUILayout.ObjectField("B子节点", node_B, typeof(PortItemBehaiver), true) as PortItemBehaiver;

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
            ConnectAble nodeArecored = node_A.connectAble.Find((x) => x.itemName == item_B.name && x.nodeId == node_B.NodeID);
            if (nodeArecored != null)
            {
                item_A.transform.position = item_B.transform.TransformPoint(nodeArecored.relativePos);
                item_A.transform.forward = item_B.transform.TransformDirection(nodeArecored.relativeDir);
            }
        }

        PortParentBehaiver FindInoutItem(PortItemBehaiver node)
        {
            Transform parent = node.transform.parent;
            while (parent.GetComponent<PortParentBehaiver>() == null)
            {
                parent = parent.parent;
            }
            return parent.GetComponent<PortParentBehaiver>();
        }

        void CreateConnect()
        {
            RecordInout();

            ConnectAble nodeArecored = node_A.connectAble.Find((x) => x.itemName == item_B.name && x.nodeId == node_B.NodeID);
            ConnectAble nodeBrecored = node_B.connectAble.Find((x) => x.itemName == item_A.name && x.nodeId == node_A.NodeID);
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
            nodeArecored.itemName = item_B.name;
            nodeBrecored.itemName = item_A.name;
            nodeArecored.nodeId = node_B.NodeID;
            nodeBrecored.nodeId = node_A.NodeID;
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