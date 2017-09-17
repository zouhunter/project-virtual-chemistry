using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SelectablePlane : MonoBehaviour
{
    public Transform target;
    [Range(0, 1)]
    public float distence;
    public string audioName = "pickup";
    HitController HitCtrl { get { return Laboratory.Current.hitCtrl; } }
    Vector3 hitpos;
    Collider m_Collider;

    void Update()
    {
        if (target == null)
        {
            SelectedTarget();
        }

        if (target == null) return;

        if (Input.GetMouseButton(0))
        {
            UpdateTargetPos();
            if (Input.GetMouseButtonDown(0))
            {
                if (HitCtrl.GetHitPoint(ref hitpos))
                {
                    AudioManager.Instance.PlayAtPosition(audioName, hitpos);
                }
            }
        }
        else
        {
            target.GetComponent<Collider>().enabled = true;
            target = null;
        }
    }

    /// <summary>
    /// 选中对象
    /// </summary>
    void SelectedTarget()
    {
        if (HitCtrl.GetHitCollider(ref m_Collider) && !m_Collider.CompareTag("MovePos"))
        {
            if (m_Collider.transform.parent != null && m_Collider.transform.parent.name == name)
            {
                target = m_Collider.transform;
                target.GetComponent<Collider>().enabled = false;
            }
        }
    }

    /// <summary>
    /// 更新所控制对象坐标
    /// </summary>
    void UpdateTargetPos()
    {
        if (HitCtrl.GetHitCollider(ref m_Collider) && m_Collider.CompareTag("MovePos"))
        {
            HitCtrl.GetHitPoint(ref hitpos);
            target.transform.position = hitpos;
            if (m_Collider.name == "CilliderA")
            {
                target.transform.position += Vector3.back * distence;
            }
        }
    }
}

