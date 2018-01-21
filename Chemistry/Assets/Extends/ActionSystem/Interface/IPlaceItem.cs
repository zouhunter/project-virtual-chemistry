using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
namespace WorldActionSystem
{

    public interface IPlaceItem
    {
        string Name { get; }
        Collider Collider { get; }
        void NormalInstall(PlaceObj target, bool complete = true, bool binding = true);
        void QuickInstall(PlaceObj target, bool complete = true, bool binding = true);
        void NormalUnInstall();
        void QuickUnInstall();
    }
   
}