using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
namespace Connector
{
    internal interface INodeConnectController
    {
        event UnityAction<IPortItem> onMatch;
        event UnityAction<IPortItem> onDisMatch;
        event UnityAction<IPortItem[]> onConnected;
        event UnityAction<IPortItem[]> onDisconnected;
        Dictionary<IPortParent,List<IPortItem>> ConnectedDic { get; }
        void SetActiveItem(IPortParent item);
        void SetDisableItem(IPortParent item);
        void TryConnect();
        void Update();
    }
}