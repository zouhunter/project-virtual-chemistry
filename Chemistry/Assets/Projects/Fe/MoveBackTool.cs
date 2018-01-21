using UnityEngine;
using System.Collections;
using WorldActionSystem;

public class MoveBackTool : ChargeToolAnim {
    private Vector3 startPos;
    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
    }
    protected override void OnBeforeCompleteAsync()
    {
        base.OnBeforeCompleteAsync();
        transform.position = startPos;
    }
}
