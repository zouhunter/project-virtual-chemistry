using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RunTimeObjectBehaiver : MonoBehaviour
{
    public List<RunTimeTrigger> triggerList;
    private event UnityAction onSceneSwitch;
    void Start()
    {
        RegisterTriggerEvents();
    }

    void OnDestroy()
    {
        if (onSceneSwitch != null) onSceneSwitch();
    }
    private void RegisterTriggerEvents()
    {
        for (int i = 0; i < triggerList.Count; i++)
        {
            RunTimeTrigger trigger = triggerList[i];
            switch (trigger.type)
            {
                case RunTimeTrigger.TriggerType.button:
                    RegisterButtonEvents(trigger);
                    break;
                case RunTimeTrigger.TriggerType.toggle:
                    RegisterToggleEvents(trigger);
                    break;
                case RunTimeTrigger.TriggerType.message:
                    RegisterMessageEvents(trigger);
                    break;
                case RunTimeTrigger.TriggerType.action:
                    RegisterActionEvents(trigger);
                    break;
                default:
                    break;
            }
        }
    }

    private GameObject CreateInstence(RunTimeTrigger trigger)
    {
        GameObject obj = GameObject.Instantiate(trigger.prefab);
        obj.transform.SetParent(trigger.parent, trigger.isWorld);
        obj.name = trigger.prefab.name;
        obj.SetActive(true);
        trigger.OnCreate(obj);
        return obj;
    }

    private void RegisterButtonEvents(RunTimeTrigger trigger)
    {
        UnityAction CreateByButton = () =>
        {
            CreateInstence(trigger);
        };
        trigger.button.onClick.AddListener(CreateByButton);
        onSceneSwitch += () => { trigger.button.onClick.RemoveAllListeners(); };
        trigger.OnCreate = (x) =>
        {
            IRunTimeButton ib = x.GetComponent<IRunTimeButton>();
            if (ib != null)
            {
                ib.Btn = trigger.button;
                trigger.button.onClick.RemoveListener(CreateByButton);

                ib.OnDelete += () =>
                {
                    trigger.button.onClick.AddListener(CreateByButton);
                };
            }
        };
    }

    private void RegisterToggleEvents(RunTimeTrigger trigger)
    {
        UnityAction<bool> CreateByToggle = (x) =>
        {
            if (x)
            {
                trigger.Data = CreateInstence(trigger);
            }
            else
            {
                if (trigger.Data != null && trigger.Data is GameObject)
                {
                    Destroy((GameObject)trigger.Data);
                }
            }
        };
        trigger.toggle.onValueChanged.AddListener(CreateByToggle);
        onSceneSwitch += () => {
            trigger.toggle.onValueChanged.RemoveAllListeners();
        };

        trigger.OnCreate = (x) =>
        {
            IRunTimeToggle it = x.GetComponent<IRunTimeToggle>();
            if (it != null)
            {
                it.toggle = trigger.toggle;

                trigger.toggle.onValueChanged.RemoveListener(CreateByToggle);

                it.OnDelete += () =>
                {
                    trigger.toggle.onValueChanged.AddListener(CreateByToggle);
                };
            }
        };
    }

    private void RegisterMessageEvents(RunTimeTrigger trigger)
    {
        UnityAction<object> action = (x) =>
        {
            trigger.Data = x;
            CreateInstence(trigger);
        };

        trigger.OnCreate = (x) =>
        {
            IRunTimeMessage irm = x.GetComponent<IRunTimeMessage>();
            if (irm != null)
            {
                irm.HandleMessage(trigger.Data);
                EventFacade.Instance.RemoveEvent<object>(trigger.message, action);
                irm.OnDelete += () =>
                {
                    EventFacade.Instance.RegisterEvent<object>(trigger.message, action);
                };
            }
        };

        EventFacade.Instance.RegisterEvent<object>(trigger.message, action);
        onSceneSwitch += () => { EventFacade.Instance.RemoveEvent<object>(trigger.message, action); };
    }

    private void RegisterActionEvents(RunTimeTrigger trigger)
    {
        UnityAction action = () =>
        {
            CreateInstence(trigger);
        };

        trigger.OnCreate = (x) =>
        {
            IRunTimeEvent irm = x.GetComponent<IRunTimeEvent>();
            if (irm != null)
            {
                EventFacade.Instance.RemoveEvent(trigger.message, action);
                irm.OnDelete += () =>
                {
                    EventFacade.Instance.RegisterEvent(trigger.message, action);
                };
            }
        };

        EventFacade.Instance.RegisterEvent(trigger.message, action);
        onSceneSwitch += () => { EventFacade.Instance.RemoveEvent(trigger.message, action); };
    }

}
