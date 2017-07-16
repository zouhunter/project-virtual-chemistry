using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ReceiveAbleObject : ScriptableObject {
    public List<ReceiveSystem> systems = new List<ReceiveSystem>();
}

