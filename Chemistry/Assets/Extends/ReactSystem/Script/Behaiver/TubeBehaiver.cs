using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace ReactSystem
{
    public class TubeBehaiver : MonoBehaviour, ITube
    {
        public TextAsset portText;
        public InputField.SubmitEvent onExportError;//元素导出失败
        public InputField.SubmitEvent onImportElemnt;//元素生成事件

        private List<Port> ports = new List<Port>();

        public GameObject Go
        {
            get
            {
                return gameObject;
            }
        }
        public event Func<ITube, int, string[], bool> onExport;
        private void Start()
        {
            LoadConfigData();
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadConfigData()
        {
            if (portText != null)
            {
                var grid = ParserCSV.Parse(portText.text);
                for (int i = 1; i < grid.Length; i++)
                {
                    Port row = new Port();
                    row.id = int.Parse(grid[i][0]);
                    row.active = bool.Parse(grid[i][1]);
                    row.supportTypes = grid[i][2].Split('|');
                    row.closeinfomation = grid[i][3];
                    ports.Add(row);
                }
            }
        }
        public void Import(int nodeID, string[] type)
        {
            Debug.Log("imprort to tube:" +name + "port:" + nodeID);
            //判断状态
            Debug.Assert(onExport != null);

            Dictionary<int, List<string>> exportDic = new Dictionary<int, List<string>>();

            var mightExporters = ports.FindAll(x => nodeID != x.id);


            if (mightExporters.Count > 0)
            {
                foreach (var item in mightExporters)
                {
                    if (item.active)
                    {
                        for (int i = 0; i < type.Length; i++)
                        {
                            if (exportDic.ContainsKey(item.id))
                            {
                                exportDic[item.id].Add(type[i]);
                            }
                            else
                            {
                                exportDic[item.id] = new List<string>() { type[i] };
                            }
                        }

                    }
                    else
                    {
                        Debug.Log(item.id + "接口关闭");
                        Debug.Log(item.closeinfomation);
                    }
                }
            }

            foreach (var item in exportDic)
            {
                var status = onExport.Invoke(this, item.Key, item.Value.ToArray());
                if (!status)
                {
                    for (int i = 0; i < type.Length; i++)
                    {
                        onExportError.Invoke(type[i]);
                        Debug.Log("导出失败:" + type[i]);
                    }

                }
            }
        }
    }
}
