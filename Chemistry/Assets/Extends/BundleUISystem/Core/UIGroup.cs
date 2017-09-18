using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem.Internal;

namespace BundleUISystem
{
    public class UIGroup : MonoBehaviour
    {
#if UNITY_EDITOR
        public UILoadType defultType = UILoadType.LocalBundle;
#endif
        public List<UIBundleInfo> bundles = new List<UIBundleInfo>();
        public List<BundleInfo> rbundles = new List<BundleInfo>();
        public List<PrefabInfo> prefabs = new List<PrefabInfo>();
        public List<UIGroupObj> groupObjs = new List<UIGroupObj>();
        public string assetUrl;
        public string menu;
        private EventHold eventHold = new EventHold();
        private IUILoadCtrl _localLoader;
        private IUILoadCtrl _remoteLoader;
        private event UnityAction onDestroy;
        private event UnityAction onEnable;
        private event UnityAction onDisable;
        private const string addClose = "close";

        private static List<IUILoadCtrl> controllers = new List<IUILoadCtrl>();
        private static List<EventHold> eventHolders = new List<EventHold>();
        public static UnityEngine.Events.UnityAction<string> MessageNotHandled;
        private IUILoadCtrl LocalLoader
        {
            get
            {
                if (_localLoader == null)
                {
                    _localLoader = new UIBundleLoadCtrl(transform);
                    controllers.Add(_localLoader);
                }
                return _localLoader;
            }
        }
        private IUILoadCtrl RemoteLoader
        {
            get
            {
                if (_remoteLoader == null)
                {
                    _remoteLoader = new UIBundleLoadCtrl(assetUrl, menu, transform);
                    controllers.Add(_remoteLoader);
                }
                return _remoteLoader;
            }
        }
        void Awake()
        {
            eventHolders.Add(eventHold);
            RegistBaseUIEvents();
            RegistSubUIEvents();
        }

        private void OnEnable()
        {
            if (onEnable != null)
            {
                onEnable.Invoke();
            }
        }
        private void OnDisable()
        {
            if (onDisable != null)
            {
                onDisable.Invoke();
            }
        }
        private void OnDestroy()
        {
            if (onDestroy != null)
            {
                onDestroy.Invoke();
            }

            if (_localLoader != null) controllers.Remove(_localLoader);

            if (_remoteLoader != null) controllers.Remove(_remoteLoader);
          
            eventHolders.Remove(eventHold);
        }

        private void RegistBaseUIEvents()
        {
            if (prefabs.Count > 0)
            {
                RegisterBundleEvents(LocalLoader, prefabs.ConvertAll<ItemInfoBase>(x => x));
            }

            if (bundles.Count > 0)
            {
                RegisterBundleEvents(LocalLoader, bundles.ConvertAll<ItemInfoBase>(x => x));
            }

            if (rbundles.Count > 0)
            {
                RegisterBundleEvents(RemoteLoader, rbundles.ConvertAll<ItemInfoBase>(x => x));
            }
        }

        private void RegistSubUIEvents()
        {
            foreach (var item in groupObjs)
            {
                if (item.prefabs.Count > 0)
                {
                    RegisterBundleEvents(LocalLoader, item.prefabs.ConvertAll<ItemInfoBase>(x => x));
                }

                if (item.bundles.Count > 0)
                {
                    RegisterBundleEvents(LocalLoader, item.bundles.ConvertAll<ItemInfoBase>(x => x));
                }

                if (item.rbundles.Count > 0)
                {
                    RegisterBundleEvents(RemoteLoader, item.rbundles.ConvertAll<ItemInfoBase>(x => x));
                }
            }
        }
        #region 事件注册
        private void RegisterBundleEvents(IUILoadCtrl loadCtrl, List<ItemInfoBase> bundles)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                ItemInfoBase trigger = bundles[i];
                switch (trigger.type)
                {
                    case UIBundleInfo.Type.Button:
                        RegisterButtonEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Toggle:
                        RegisterToggleEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Name:
                        RegisterMessageEvents(loadCtrl, trigger);
                        break;
                    case UIBundleInfo.Type.Enable:
                        RegisterEnableEvents(loadCtrl, trigger);
                        break;
                    default:
                        break;
                }
            }
        }
        private void RegisterMessageEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction<object> createAction = (x) =>
            {
                trigger.dataQueue.Enqueue(x);//
                loadCtrl.GetGameObjectInfo(trigger);
            };

            UnityAction<object> handInfoAction = (data) =>
            {
                IPanelName irm = trigger.instence.GetComponent<IPanelName>();
                irm.HandleData(data);
            };

            trigger.OnCreate = (x) =>
            {
                IPanelName irm = x.GetComponent<IPanelName>();
                if (irm != null)
                {
                    trigger.instence = x;
                    while (trigger.dataQueue.Count > 0){
                        irm.HandleData(trigger.dataQueue.Dequeue());
                    }
                    eventHold.Remove(trigger.assetName, createAction);
                    eventHold.Record(trigger.assetName, handInfoAction);
                    irm.OnDelete += () =>
                    {
                        trigger.instence = null;
                        eventHold.Remove(trigger.assetName, handInfoAction);
                        eventHold.Record(trigger.assetName, createAction);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };

            eventHold.Record(trigger.assetName, createAction);

            onDestroy += () =>
            {
                eventHold.Remove(trigger.assetName, createAction);
            };
        }
        private void RegisterToggleEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction<bool> CreateByToggle = (x) =>
            {
                if (x)
                {
                    trigger.toggle.interactable = false;
                    loadCtrl.GetGameObjectInfo(trigger);
                }
                else
                {
                    Destroy((GameObject)trigger.instence);
                }
            };
            trigger.toggle.onValueChanged.AddListener(CreateByToggle);

            onDestroy += () =>
            {
                trigger.toggle.onValueChanged.RemoveAllListeners();
            };

            trigger.OnCreate = (x) =>
            {
                trigger.toggle.interactable = true;

                trigger.instence = x;
                IPanelToggle it = x.GetComponent<IPanelToggle>();
                if (it != null)
                {
                    it.toggle = trigger.toggle;

                    trigger.toggle.onValueChanged.RemoveListener(CreateByToggle);

                    it.OnDelete += () =>
                    {
                        trigger.toggle.onValueChanged.AddListener(CreateByToggle);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };
        }
        private void RegisterButtonEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction CreateByButton = () =>
            {
                loadCtrl.GetGameObjectInfo(trigger);
            };
            trigger.button.onClick.AddListener(CreateByButton);
            onDestroy += () => { trigger.button.onClick.RemoveAllListeners(); };
            trigger.OnCreate = (x) =>
            {
                IPanelButton ib = x.GetComponent<IPanelButton>();
                if (ib != null)
                {
                    ib.Btn = trigger.button;
                    trigger.button.onClick.RemoveListener(CreateByButton);

                    ib.OnDelete += () =>
                    {
                        trigger.button.onClick.AddListener(CreateByButton);
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };
        }
        private void RegisterEnableEvents(IUILoadCtrl loadCtrl, ItemInfoBase trigger)
        {
            UnityAction onEnableAction = () =>
            {
                loadCtrl.GetGameObjectInfo(trigger);
            };

            trigger.OnCreate = (x) =>
            {
                trigger.instence = x;
                IPanelEnable irm = x.GetComponent<IPanelEnable>();
                if (irm != null)
                {
                    onEnable -= onEnableAction;

                    irm.OnDelete += () =>
                    {
                        onEnable += onEnableAction;
                    };
                }
                else
                {
                    onDisable += () =>
                    {
                        if (trigger.instence != null && trigger.instence is GameObject)
                        {
                            Destroy((GameObject)trigger.instence);
                        }
                    };
                }
                RegisterDestoryAction(trigger.assetName, x);
            };

            onEnable += onEnableAction;
        }
        private void RegisterDestoryAction(string assetName, GameObject x)
        {
            string key = addClose + assetName;
            eventHold.Remove(key);
            eventHold.Record(key, new UnityAction<object>((y) =>
            {
                if (x != null) Destroy(x);
            }));
        }
        #endregion

        #region 触发事件
        public static void Open(string assetName, UnityAction onClose = null, object data = null)
        {
            bool handled = true;
            TraverseHold((eventHold) =>
            {
                handled |= eventHold.NotifyObserver(assetName, data);
            });
            if (!handled)
            {
                NoMessageHandle(assetName);
            }
            else if(onClose != null)
            {
                var key = (addClose + assetName);
                TraverseHold((eventHold) =>
                {
                    if (eventHold.HaveRecord(key))
                    {
                        eventHold.Record(key,new UnityAction<object>((y) =>
                        {
                            onClose.Invoke();
                        }));
                    }   
                });
            }
        }
        public static void Open(string assetName,  object data)
        {
            bool handled = true;
            TraverseHold((eventHold) =>
            {
                handled |= eventHold.NotifyObserver(assetName, data);
            });
            if (!handled)
            {
                NoMessageHandle(assetName);
            }
        }
        public static void Open<T>(UnityAction onClose = null, object data = null) where T : UIPanelTemp
        {
            string assetName = typeof(T).ToString();
            Open(assetName, onClose, data);
        }
        public static void Open<T>(object data) where T : UIPanelTemp
        {
            string assetName = typeof(T).ToString();
            Open(assetName, null, data);
        }

        public static void Close(string assetName)
        {
            foreach (var item in controllers)
            {
                if (item != null)
                {
                    item.CansaleLoadObject(assetName);
                }
            }

            var key = (addClose + assetName);

            TraverseHold((eventHold) =>
            {
                eventHold.NotifyObserver(key);
            });
        }
        public static void Close<T>() where T : UIPanelTemp
        {
            string assetName = typeof(T).ToString();
            Close(assetName);
        }
        private static void TraverseHold(UnityAction<EventHold> OnGet)
        {
            var list = new List<EventHold>(eventHolders);
            foreach (var item in list)
            {
                OnGet(item);
            }
        }
        public static void NoMessageHandle(string rMessage)
        {
            if (MessageNotHandled == null)
            {
                Debug.LogWarning("MessageDispatcher: Unhandled Message of type " + rMessage);
            }
            else
            {
                MessageNotHandled(rMessage);
            }
        }

        #endregion

    }
}