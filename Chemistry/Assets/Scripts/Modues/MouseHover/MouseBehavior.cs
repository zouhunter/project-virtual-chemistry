using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
public interface MouseEvent
{
    void Start();
    bool Enable { get; set; }
    void OnMouseEnter();
    void OnMouseOver();
    void OnMouseExit();
}

public class MouseBehavior : MonoBehaviour
{
    public HoverOutLine outline;
    public HoverHandShow hand;

    public List<MouseEvent> mouseEvents = new List<MouseEvent>();

    void Start(){
        if (outline.Enable) mouseEvents.Add(outline);
        if (hand.Enable) mouseEvents.Add(hand);

        for (int i = 0; i < mouseEvents.Count; i++)
        {
            mouseEvents[i].Start();
        }
    }

    void OnMouseEnter()
    {
        for (int i = 0; i < mouseEvents.Count; i++)
        {
            mouseEvents[i].OnMouseEnter();
        }
    }
    void OnMouseOver()
    {
        for (int i = 0; i < mouseEvents.Count; i++)
        {
            mouseEvents[i].OnMouseOver();
        }
    }
    void OnMouseExit()
    {
        for (int i = 0; i < mouseEvents.Count; i++)
        {
            mouseEvents[i].OnMouseExit();
        }
    }
}

