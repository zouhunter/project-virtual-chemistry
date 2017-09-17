using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{
    [CreateAssetMenu(fileName = "ExperimentDataObject.asset", menuName = "InOutSystem/expConfig")]
    public class ExperimentDataObject : ScriptableObject
    {
#if UNITY_EDITOR
        [InspectorButton("ReConnectStart")]
        public int 还原初始组;
        void ReConnectStart()
        {
            GameObject parent = new GameObject(expName);
            GameObject child = null;
            for (int i = 0; i < elements.Count; i++)
            {
                child = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(elements[i].element);
                child.transform.SetParent(parent.transform, false);
                child.transform.position = elements[i].position;
                child.transform.rotation = elements[i].rotation;
            }
        }
#endif
        public string expName;
        [Array]
        public List<RunTimeElemet> elements = new List<RunTimeElemet>();
        public NodeConnect[] defultConnect;
    }
}