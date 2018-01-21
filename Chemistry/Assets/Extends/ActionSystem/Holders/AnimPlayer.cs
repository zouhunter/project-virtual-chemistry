using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public interface AnimPlayer
    {
        void Play(float duration, UnityAction onAutoPlayEnd);
        void EndPlay();
        void UnDoPlay();
    }

}
