using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    [System.Serializable]
    public struct ChargeData
    {
        public string type;
        public float value;

        public ChargeData(string type,float value)
        {
            this.type = type;
            this.value = value;
        }
    }
}
