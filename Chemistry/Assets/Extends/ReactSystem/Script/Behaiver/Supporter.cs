using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
namespace ReactSystem
{

    public class Supporter :MonoBehaviour, ISupporter
    {
        public InputField.SubmitEvent onActiveSupport;
        public bool startActive;
        public List<string> supports;
        public GameObject Go
        {
            get
            {
                return gameObject;
            }
        }
        private bool active;

        public void Active(bool force = false)
        {
            if(startActive || force)
            {
                active = true;
                foreach (var item in supports)
                {
                    onActiveSupport.Invoke(item);
                }
            }
        }

        public List<string> GetSupport()
        {
            if(active)
            {
                return supports;
            }
            else
            {
                return null;
            }
        }
    }

}