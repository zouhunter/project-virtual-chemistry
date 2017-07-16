using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
public interface IInstaller {
    void Install();
    void UnInstall();
    void QuickInstall();
    void QuickUnInstall();
}


public class Installer : MonoBehaviour, IInstaller
{
    public void Install()
    {
        //throw new NotImplementedException();
    }

    public void QuickInstall()
    {
        //throw new NotImplementedException();
    }

    public void QuickUnInstall()
    {
        //throw new NotImplementedException();
    }

    public void UnInstall()
    {
        //throw new NotImplementedException();
    }
}
