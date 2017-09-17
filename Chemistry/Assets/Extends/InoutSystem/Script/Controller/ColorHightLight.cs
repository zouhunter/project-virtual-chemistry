using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace FlowSystem
{
    public class ColorHighLightCtrl : IHighLightItems
    {
        static Dictionary<Renderer, Color> defultColor = new Dictionary<Renderer, Color>();
        IList<Color> highLightColor;
        public ColorHighLightCtrl(List<Color> highLightColor)
        {
            this.highLightColor = highLightColor;
        }

        public void HighLightTarget(Renderer render, int id = 0)
        {
            if (!defultColor.ContainsKey(render))
            {
                defultColor.Add(render, render.material.color);
            }
            render.material.color = highLightColor[id];
        }

        public void UnHighLightTarget(Renderer renderer)
        {
            Color oldColor;
            if (defultColor.TryGetValue(renderer, out oldColor))
            {
                renderer.material.color = defultColor[renderer];
                defultColor.Remove(renderer);
            }
        }
    }
}
