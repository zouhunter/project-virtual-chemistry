using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    /// <summary>
    /// 记录安装对象,并操作对象
    /// </summary>
    public class InstallStart : MonoBehaviour,IInstallStart
    {
        [Range(1, 10)]
        public int distence = 1;
        public int Distence { get { return distence; } }

        private InstallObj pickedUpObj;
        /// <summary>
        /// 按名称将元素进行记录
        /// </summary>
        Dictionary<string, List<InstallObj>> objectList = new Dictionary<string, List<InstallObj>>();

        void Start()
        {
            foreach (Transform item in transform)
            {
                InstallObj obj = item.GetComponent<InstallObj>();
                if (objectList.ContainsKey(obj.name))
                {
                    objectList[obj.name].Add(obj);
                }
                else
                {
                    objectList[obj.name] = new List<InstallObj>() { obj };
                }
            }
        }

        /// <summary>
        /// 拿起元素
        /// </summary>
        /// <param name="pickedUpObj"></param>
        public bool PickUpObject(InstallObj pickedUpObj)
        {
            if (!pickedUpObj.Installed)
            {
                this.pickedUpObj = pickedUpObj;
                pickedUpObj.OnPickUp();
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 放下元素
        /// </summary>
        public void PickDownPickedUpObject()
        {
            if (pickedUpObj != null)
            {
                pickedUpObj.OnPickDown();
            }
        }

        /// <summary>
        /// 是否可以安装到指定坐标（名称条件）
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool CanInstallToPos(InstallPos pos)
        {
            if (pickedUpObj != null)
            {
                return pickedUpObj.name == pos.name;
            }
            return false;
        }

        /// <summary>
        /// 安装元素到指定坐标
        /// </summary>
        /// <param name="pos"></param>
        public void InstallPickedUpObject(InstallPos pos)
        {
            if (pickedUpObj != null && !pickedUpObj.Installed)
            {
                pickedUpObj.NormalInstall(pos);
                pos.Attach(pickedUpObj);
            }
        }
        /// <summary>
        /// 快速安装元素到指定坐标
        /// </summary>
        /// <param name="pos"></param>
        public void QuickInstallPickedUpObject(InstallPos pos)
        {
            if (pickedUpObj != null && !pickedUpObj.Installed)
            {
                pickedUpObj.QuickInstall(pos);
                pos.Attach(pickedUpObj);
            }
        }

        /// <summary>
        /// 将未安装的元素安装到指定的坐标
        /// </summary>
        /// <param name="posList"></param>
        public void InstallPosListObjects(List<InstallPos> posList)
        {
            InstallPos pos;
            for (int i = 0; i < posList.Count; i++)
            {
                pos = posList[i];
                pickedUpObj = GetUnInstalledObj(pos.name);
                pickedUpObj.NormalInstall(pos);
                pos.Attach(pickedUpObj);
            }
        }
        /// <summary>
        /// 快速安装 列表 
        /// </summary>
        /// <param name="posList"></param>
        public void QuickInstallPosListObjects(List<InstallPos> posList)
        {
            InstallPos pos;
            for (int i = 0; i < posList.Count; i++)
            {
                pos = posList[i];
                pickedUpObj = GetUnInstalledObj(pos.name);
                pickedUpObj.QuickInstall(pos);
                pos.Attach(pickedUpObj);
            }
        }
        /// <summary>
        /// uninstll
        /// </summary>
        /// <param name="posList"></param>
        public void UnInstallPosListObjects(List<InstallPos> posList)
        {
            InstallPos pos;
            for (int i = 0; i < posList.Count; i++)
            {
                pos = posList[i];
                pickedUpObj = pos.Detach();
                pickedUpObj.NormalUnInstall();
            }
        }
        /// <summary>
        /// QuickUnInstall
        /// </summary>
        /// <param name="posList"></param>
        public void QuickUnInstallPosListObjects(List<InstallPos> posList)
        {
            InstallPos pos;
            for (int i = 0; i < posList.Count; i++)
            {
                pos = posList[i];
                pickedUpObj = pos.Detach();
                pickedUpObj.QuickUnInstall();
            }
        }

        /// <summary>
        /// 找出一个没有安装的元素
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        InstallObj GetUnInstalledObj(string elementName)
        {
            List<InstallObj> listObj;

            if (objectList.TryGetValue(elementName, out listObj))
            {
                for (int i = 0; i < listObj.Count; i++)
                {
                    if (!listObj[i].Installed)
                    {
                        return listObj[i];
                    }
                }
            }
            return null;
        }

    }

}