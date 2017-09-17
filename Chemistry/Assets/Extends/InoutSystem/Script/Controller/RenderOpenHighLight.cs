using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace FlowSystem
{
    public class RenderOpenHighLight : IHighLightItems
    {
        enum RenderType:int
        {
            hide = 0,
            hightLight = 1,
            normal = 2
        }
        private List<Color> colors = new List<Color>();
        public RenderOpenHighLight(List<Color> colors)
        {
            this.colors = colors;
        }
        // 0->隐藏，1-> 显示并高亮，2->正常显示
        public void HighLightTarget(Renderer render, int id = 0)
        {
            if (render != null)
            {
                switch ((RenderType) id)
                {
                    case RenderType.hide:
                        render.enabled = false;
                        break;
                    case RenderType.hightLight:
                    case RenderType.normal:
                        render.enabled = true;
                        render.material.color = colors[id];
                        break;
                    default:
                        break;
                }
            }
        }

        public void UnHighLightTarget(Renderer renderer)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}