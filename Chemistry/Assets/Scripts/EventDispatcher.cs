using UnityEngine;
using UnityEngine.EventSystems;
public class EventDispatcher : EventTrigger
{
    public delegate void EventHandler(GameObject sender);
    public delegate void CollisionHandler(GameObject sender, Collision c);

    public event EventHandler onMouseOver;
    public event EventHandler onMouseDown;
    public event EventHandler onMouseUp;
    public event EventHandler onMouseUpAsButton;
    public event EventHandler onMouseEnter;
    public event EventHandler onMouseExit;
    public event EventHandler onBecameVisible;
    public event EventHandler onBecameInvisible;
    public event CollisionHandler onCollisionEnter;
    public event CollisionHandler onCollisionExit;

    public event EventHandler onPointerClick;
    public event EventHandler onPointerDown;
    public event EventHandler onPointerEnter;
    public event EventHandler onPointerExit;
    public event EventHandler onPointerUp;
    public Entry e = new Entry();
    void OnMouseOver()
    {
        if (onMouseOver != null)
            onMouseOver(gameObject);
        TriggerEvent even = new TriggerEvent();
        Entry e = new Entry();
        e.callback = even;
        e.eventID = EventTriggerType.Submit;
    }
    void OnMouseDown()
    {
        if (onMouseDown != null)
            onMouseDown(gameObject);
    }
    void OnMouseUp() { if (onMouseUp != null) { onMouseUp(gameObject); } }
    void OnMouseUpAsButton() { if (onMouseUpAsButton != null) { onMouseUpAsButton(gameObject); } }
    void OnMouseEnter() { if (onMouseEnter != null) onMouseEnter(gameObject); }
    void OnMouseExit() { if (onMouseExit != null) onMouseExit(gameObject); }
    void OnBecameVisible() { if (onBecameVisible != null) onBecameVisible(gameObject); }

    void OnBecameInvisible() { if (onBecameInvisible != null) onBecameInvisible(gameObject); }
    void OnCollisionEnter(Collision c) { if (onCollisionEnter != null) onCollisionEnter(gameObject, c); }
    void OnCollisionExit(Collision c) { if (onCollisionExit != null) onCollisionExit(gameObject, c); }



    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onPointerClick != null) onPointerClick(gameObject);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onPointerDown != null) onPointerDown(gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null) onPointerEnter(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null) onPointerExit(gameObject);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onPointerUp != null) onPointerUp(gameObject);
    }

    public static EventDispatcher Get(Component component)
    {
        if (!component.GetComponent<EventDispatcher>())
        {
            component.gameObject.AddComponent<EventDispatcher>();
        }
        return component.GetComponent<EventDispatcher>();
    }
    public static EventDispatcher Get(GameObject go)
    {
        if (!go.GetComponent<EventDispatcher>())
        {
            go.AddComponent<EventDispatcher>();
        }
        return go.GetComponent<EventDispatcher>();
    }
}
