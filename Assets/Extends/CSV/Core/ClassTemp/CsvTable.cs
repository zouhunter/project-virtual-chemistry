using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class CsvTable {
    protected string[][] grid;
    protected bool isLoaded = false;
    public bool IsLoaded(){ return isLoaded;}
    public abstract void Load(string csvData);
    public abstract string UnLoad();
}
