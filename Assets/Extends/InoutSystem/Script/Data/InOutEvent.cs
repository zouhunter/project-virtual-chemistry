using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{
    public interface IInteract
    {
        string OutType { get; }
        string Information { get; }
        string outPutFiledinformation { get; }
        float RactTime { get; }
        UnityEvent Interact { get; }
        UnityEvent InteractOutPutFiled { get; }
    }

    [System.Serializable]
    public class Interact: IInteract
    {
        public string intype;
        public int nodeID;
        public string outtype;
        public string information;
        public string outPutFiledinformation;
        [Range(1,10)]
        public float hoadTime = 3;
        public UnityEvent interact;
        public UnityEvent interactOutPutFiled;
        public string OutType
        {
            get
            {
                return outtype;
            }
        }

        public string Information
        {
            get
            {
                return information;
            }
        }

        string IInteract.outPutFiledinformation
        {
            get
            {
                return outPutFiledinformation;
            }
        }

        UnityEvent IInteract.Interact
        {
            get
            {
                return interact;
            }
        }

        public UnityEvent InteractOutPutFiled
        {
            get
            {
                return interactOutPutFiled;
            }
        }

        public float RactTime
        {
            get
            {
               return hoadTime;
            }
        }
    }
    [System.Serializable]
    public class AutoRact: IInteract
    {
        public string defultAutoExport;
        public string information;
        public string outPutFiledinformation;
        [Range(1,10)]
        public float hoadTime = 3;
        public UnityEvent autoExportEvent;
        public UnityEvent interactOutPutFiled;
        public string OutType
        {
            get
            {
                return defultAutoExport;
            }
        }

        public string Information
        {
            get
            {
                return information;
            }
        }
        public float RactTime
        {
            get
            {
                return hoadTime;
            }
        }
        string IInteract.outPutFiledinformation
        {
            get
            {
                return outPutFiledinformation;
            }
        }

        UnityEvent IInteract.Interact
        {
            get
            {
                return autoExportEvent;
            }
        }

        public UnityEvent InteractOutPutFiled
        {
            get
            {
                return interactOutPutFiled;
            }
        }
    }
}