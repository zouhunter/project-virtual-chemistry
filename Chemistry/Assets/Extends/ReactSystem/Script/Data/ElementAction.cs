using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ReactSystem
{

    [Serializable]
    public class ElementAction
    {
        public string elementName;
        public UnityEvent action;
    }
}
