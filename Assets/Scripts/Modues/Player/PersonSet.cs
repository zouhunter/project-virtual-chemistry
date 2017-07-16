using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class PersonSet : MonoBehaviour {
    public CharacterControl control;
    void Start()
    {
        //Facade.Instance.RegisterEvent<Transform>("movePlayer", OnPlayerPosChange);
    }

    void OnDestroy()
    {
        //Facade.Instance.RemoveEvent<Transform>("movePlayer", OnPlayerPosChange);
    }

    void OnPlayerPosChange(Transform target)
    {
        control.ImmediateMove(target.position, target.rotation);
    }
}
