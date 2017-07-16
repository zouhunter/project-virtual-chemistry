using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IRunTimeButton
{
    Button Btn {  set; }
    event UnityAction OnDelete;
}
