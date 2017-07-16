using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
using UnityEngine.EventSystems;

public class HitController
{
    private Ray ray;
    private RaycastHit hit;
    private bool hited;

    public void Update()
    {
        if (Camera.main)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) hited = true;
            else hited = false;
        }
        else
        {
            hited = false;
        }
    }
    public bool GetHitCollider(ref Collider collider)
    {
        if (hited && hit.collider != null)
        {
            collider = hit.collider;
            return true;
        }
        return false;
    }
    public bool GetHitPoint(ref Vector3 pos)
    {
        if (hited)
        {
            pos = hit.point;
            return true;
        }
        return false;
    }
}