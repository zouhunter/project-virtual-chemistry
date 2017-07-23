using UnityEngine;
using System.Collections;

public class HitCamera : MonoBehaviour {
    public HitController HitCtrl { get { return Laboratory.Current.hitCtrl; } }
    private Vector3 pos;
    void Update()
    {
        if (Input.GetMouseButtonDown(2) && HitCtrl.GetHitPoint(ref pos))
        {
            //Laboratory.Current.Camera.SetTarget(pos);
        }
    }
}
