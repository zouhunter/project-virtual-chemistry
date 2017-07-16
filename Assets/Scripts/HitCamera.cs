using UnityEngine;
using System.Collections;

public class HitCamera : MonoBehaviour {
    public HitController HitCtrl { get { return GameManager.hitCtrl; } }
    private Vector3 pos;
    void Update()
    {
        if (Input.GetMouseButtonDown(2) && HitCtrl.GetHitPoint(ref pos))
        {
            //Laboratory.Main.Camera.SetTarget(pos);
        }
    }
}
