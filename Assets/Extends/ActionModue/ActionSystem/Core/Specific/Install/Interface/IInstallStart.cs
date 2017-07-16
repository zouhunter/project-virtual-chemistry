using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public interface IInstallStart
    {
        int Distence { get; }
        bool PickUpObject(InstallObj pickedUpObj);
        void PickDownPickedUpObject();

        bool CanInstallToPos(InstallPos pos);
        void InstallPickedUpObject(InstallPos pos);
        void InstallPosListObjects(List<InstallPos> posList);
        void QuickInstallPosListObjects(List<InstallPos> posList);
        void UnInstallPosListObjects(List<InstallPos> posList);
        void QuickUnInstallPosListObjects(List<InstallPos> posList);
    }

}
