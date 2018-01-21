using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WorldActionSystem
{
    [AddComponentMenu(MenuName.ConnectObj)]
    public class ConnectObj : ActionObj
    {
        public float lineWight = 0.1f;
        public Material lineMaterial;

        private float autoTime { get { return Config.autoExecuteTime; } }
        [System.Serializable]
        public class PointGroup
        {
            public int p1;
            public int p2;
        }
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Connect;
            }
        }

        public List<PointGroup> connectGroup;
        private List<Collider> nodes = new List<Collider>();
        private Dictionary<int, LineRenderer> lineRenders = new Dictionary<int, LineRenderer>();
        private Dictionary<int, Vector3[]> positionDic = new Dictionary<int, Vector3[]>();
        private Coroutine antoCoroutine;

        protected override void Start()
        {
            base.Start();
            RegistNodes();
        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            if (auto)
            {
                antoCoroutine = StartCoroutine(AutoConnect());
            }
        }

        public override void OnEndExecute(bool force)
        {
            base.OnEndExecute(force);
            if (force)
            {
                for (int i = 0; i < connectGroup.Count; i++)
                {
                    var group = connectGroup[i];
                    var id = 1 << group.p1 | 1 << group.p2;
                    positionDic[id] = new Vector3[] { nodes[group.p1].transform.position, nodes[group.p2].transform.position };
                    RefeshState(id);
                }
            }
            if (antoCoroutine != null) StopCoroutine(antoCoroutine);
        }
        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();
            positionDic.Clear();
            ResetLinRenders();
            if (antoCoroutine != null) StopCoroutine(antoCoroutine);
        }

        private IEnumerator AutoConnect()
        {
            for (int i = 0; i < connectGroup.Count; i++)
            {
                var group = connectGroup[i];
                var id = 1 << group.p1 | 1 << group.p2;
                positionDic[id] = new Vector3[] { nodes[group.p1].transform.position, nodes[group.p1].transform.position };
                angleCtrl.UnNotice(anglePos);
                anglePos = nodes[group.p2].transform;
                for (float timer = 0; timer < autoTime; timer += Time.deltaTime)
                {
                    positionDic[id][1] = Vector3.Lerp(nodes[group.p1].transform.position, nodes[group.p2].transform.position, timer / autoTime);
                    RefeshState(id);
                    yield return null;
                }

                OnOneNodeConnected();
            }

        }

        private void RegistNodes()
        {
            foreach (Transform child in transform)
            {
                var collider = child.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.gameObject.layer = LayerMask.NameToLayer(Layers.connectItemLayer);
                    nodes.Add(collider);
                }
            }
        }
        public void AutoConnectNodes()
        {
            foreach (var item in connectGroup)
            {
                var id1 = item.p1;
                var id2 = item.p2;
                Vector3[] positions = new Vector3[2];
                positions[0] = nodes[id1].transform.position;
                positions[1] = nodes[id2].transform.position;
                if (id1 > id2)
                {
                    Array.Reverse(positions);
                }
                var id = 1 << id1 | 1 << id2;
                positionDic[id] = positions;
                RefeshState(id);
                OnOneNodeConnected();
            }
        }
        public void TrySelectFirstCollider(Collider collider)
        {
            if (nodes.Contains(collider))
            {
                var id = nodes.IndexOf(collider);
                for (int i = 0; i < connectGroup.Count; i++)
                {
                    var group = connectGroup[i];
                    var anotherid = -1;
                    if (group.p1 == id)
                    {
                        anotherid = group.p2;
                    }
                    else if (group.p2 == id)
                    {
                        anotherid = group.p1;
                    }

                    if (anotherid != -1)
                    {
                        var gid = 1 << group.p1 | 1 << group.p2;

                        if (!positionDic.ContainsKey(gid))
                        {
                            var another = nodes[anotherid];
                            angleCtrl.UnNotice(anglePos);
                            anglePos = another.transform;
                        }

                    }
                }
            }
        }
        public bool TryConnectNode(Collider collider1, Collider collider2)
        {
            if (nodes.Contains(collider1) && nodes.Contains(collider2))
            {
                var id1 = nodes.IndexOf(collider1);
                var id2 = nodes.IndexOf(collider2);

                if (CanConnect(Mathf.Min(id1, id2), Mathf.Max(id1, id2)))
                {
                    var id = 1 << id1 | 1 << id2;
                    positionDic[id] = new Vector3[] { collider1.transform.position, collider2.transform.position };
                    RefeshState(id);
                    OnOneNodeConnected();
                    if (connectGroup.Count > positionDic.Count)
                    {
                        foreach (var item in connectGroup)
                        {
                            var gid = 1 << item.p1 | 1 << item.p2;
                            if (!positionDic.ContainsKey(gid))
                            {
                                var notSelectedItem = nodes[item.p1];
                                angleCtrl.UnNotice(anglePos);
                                anglePos = notSelectedItem.transform;
                            }
                        }
                    }
                    return true;
                }


            }
            return false;
        }

        private void OnOneNodeConnected()
        {
            bool allConnected = true;
            foreach (var item in connectGroup)
            {
                var key = 1 << item.p1 | 1 << item.p2;
                allConnected &= positionDic.ContainsKey(key);
            }
            if (allConnected)
            {
                OnEndExecute(false);
            }
        }

        public bool DeleteConnect(Collider collider1, Collider collider2)
        {
            if (nodes.Contains(collider1) && nodes.Contains(collider2))
            {
                var id1 = nodes.IndexOf(collider1);
                var id2 = nodes.IndexOf(collider2);
                var id = 1 << id1 | 1 << id2;
                if (positionDic.ContainsKey(id)) positionDic.Remove(id);
                RefeshState(id);
                return true;
            }
            else
            {
                return false;
            }

        }
        private void RefeshState(int id)
        {
            if (!positionDic.ContainsKey(id)) return;

            var positionList = positionDic[id];

            var lineRender = GetLineRender(id);
#if UNITY_5_6_OR_NEWER
            lineRender.positionCount = positionList.Length;
#else
            lineRender.SetVertexCount(positionList.Length);
#endif
            lineRender.SetPositions(positionList);
        }
        private bool CanConnect(int min, int max)
        {
            return connectGroup.Find(x =>
            {
                return Mathf.Min(x.p2, x.p1) == min && Mathf.Max(x.p2, x.p1) == max;
            }) != null;
        }
        private LineRenderer GetLineRender(int index)
        {
            if (lineRenders.ContainsKey(index))
            {
                return lineRenders[index];
            }
            else
            {
                var obj = new GameObject(index.ToString());
                obj.transform.SetParent(transform);
                var lineRender = obj.AddComponent<LineRenderer>();
                lineRender.material = lineMaterial;
#if UNITY_5_6_OR_NEWER
                lineRender.textureMode = LineTextureMode.Tile;
                lineRender.startWidth = lineWight;
                lineRender.endWidth = lineWight;
                lineRender.positionCount = 1;
#else
                lineRender.SetWidth(lineWight, lineWight);
                lineRender.SetVertexCount(1);
#endif
                lineRenders.Add(index, lineRender);
                return lineRender;
            }


        }
        private void ResetLinRenders()
        {
            Debug.Log("ResetLinRenders");
            foreach (var lineRender in lineRenders)
            {
#if UNITY_5_6_OR_NEWER
                lineRender.Value.positionCount = 1;
#else
                lineRender.Value.SetVertexCount(1);
#endif
            }
        }
    }
}