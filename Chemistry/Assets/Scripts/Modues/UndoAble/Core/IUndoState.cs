using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public interface IUndoState {
    void Execute();
    void EndExecute();
    void UnDo();
}

