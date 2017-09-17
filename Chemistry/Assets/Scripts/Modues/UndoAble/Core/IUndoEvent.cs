using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public interface IUndoEvent  {
    event UnityAction<bool> executeAction;
    event UnityAction endExecuteAction;
}

