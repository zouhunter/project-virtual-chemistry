using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace Connector
{
    [Serializable]
    public class GameObjectEvent : UnityEvent<GameObject> { }
}