
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
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
    [CustomEditor(typeof(LinkItem))]
    public class LinkItemDrawer : Editor
    {
        private LinkItem targetItem;

        private void OnEnable()
        {
            targetItem = target as LinkItem;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawToolsButtons();
        }
        private void DrawToolsButtons()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                var style = EditorStyles.toolbarButton;
                if (GUILayout.Button("AutoRecord", style))
                {
                    foreach (var item in targetItem.ChildNodes)
                    {
                        List<LinkPort> otherPorts;
                        if(LinkUtil.FindTriggerNodes(item, out otherPorts))
                        {
                            for (int i = 0; i < otherPorts.Count; i++)
                            {
                                var otherPort = otherPorts[i];
                                TryRecordConnect(item, otherPort);
                            }
                        }
                    }
                }
                if (GUILayout.Button("LoadRecord", style))
                {
                    if (Selection.instanceIDs != null && Selection.instanceIDs.Length == 2)
                    {
                        
                    }
                }
                if (GUILayout.Button("ClampRot", style))
                {
                    if (Selection.activeGameObject != null)
                    {
                        var selected = Selection.activeGameObject;
                        if (selected == null) return;
                        LinkUtil.ClampRotation(selected.transform);
                    }
                }
            }

        }

        private void TryRecordConnect(LinkPort node_A,LinkPort node_B)
        {
            if (!node_A || !node_B) return;
            LinkItem item_A = node_A.GetComponentInParent<LinkItem>();
            LinkItem item_B = node_B.GetComponentInParent<LinkItem>();

            if (node_A == null || node_B == null || item_A == null || item_B == null){
                return;
            }

            LinkInfo nodeArecored = node_A.connectAble.Find((x) => x.itemName == item_B.name && x.nodeId == node_B.NodeID);
            LinkInfo nodeBrecored = node_B.connectAble.Find((x) => x.itemName == item_A.name && x.nodeId == node_A.NodeID);
            //已经记录过
            if (nodeArecored == null)
            {
                nodeArecored = new LinkInfo();
                node_A.connectAble.Add(nodeArecored);
            }
            if (nodeBrecored == null)
            {
                nodeBrecored = new LinkInfo();
                node_B.connectAble.Add(nodeBrecored);
            }

            nodeArecored.itemName = item_B.name;
            nodeBrecored.itemName = item_A.name;
            nodeArecored.nodeId = node_B.NodeID;
            nodeBrecored.nodeId = node_A.NodeID;

            RecordTransform(nodeArecored, nodeBrecored, item_A.transform, item_B.transform);

            EditorUtility.SetDirty(node_A);
            EditorUtility.SetDirty(node_B);
            EditorUtility.DisplayDialog("[connected]", item_A.Name+":" + (node_A.NodeID + 1) + "<->"+ item_B.Name + ":" + (node_B.NodeID + 1), "确认");
        }

        void RecordTransform(LinkInfo nodeArecored, LinkInfo nodeBrecored, Transform ourItem, Transform otherItem)
        {
            nodeArecored.relativePos = otherItem.InverseTransformPoint(ourItem.position);
            nodeArecored.relativeDir = otherItem.InverseTransformDirection(ourItem.forward); //Quaternion.Inverse(ourItem.rotation) * otherItem.rotation;

            nodeBrecored.relativePos = ourItem.InverseTransformPoint(otherItem.position);
            nodeBrecored.relativeDir = ourItem.InverseTransformDirection(otherItem.forward);// Quaternion.Inverse(otherItem.rotation) * ourItem.rotation;
        }
    }
}