using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using ChargeSystem;
namespace ChargeSystem
{

    public interface ActionObj
    {
        IRemoteController RemoteCtrl { get; }
    }
}