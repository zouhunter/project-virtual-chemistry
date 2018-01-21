using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class ElementController
    {
        public static ElementController _instence;
        private ElementController() { }
        public static ElementController Instence
        {
            get
            {
                if (_instence == null)
                {
                    _instence = new ElementController();
                }
                return _instence;
            }
        }

        private List<ISupportElement> elementList = new List<ISupportElement>();
        public bool log = false;

        /// <summary>
        /// 外部添加Element
        /// </summary>
        public void RegistElement(ISupportElement item)
        {
            if (!elementList.Contains(item))
            {
                elementList.Add(item);
            }
            else
            {
                Debug.LogError("不要重复注册：" + item);
            }
        }

        public void RemoveElement(ISupportElement item)
        {
            if(elementList.Contains(item))
            {
                elementList.Remove(item);
            }
        }

        /// <summary>
        /// 获取指定元素名的列表
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public List<T> GetElements<T>(string elementName) where T : ISupportElement
        {
            var list = elementList.FindAll(x => x.Name == elementName && x is T);
            if(list.Count > 0){
                return list.ConvertAll<T>(x => (T)x) ;   
            }
            Debug.LogWarning("配制错误,缺少" + elementName);
            return null;
            
        }

        /// <summary>
        /// 获取所有T的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetElements<T>()
        {
            var list = elementList.FindAll(x =>x is T);
            if (list.Count > 0)
            {
                return list.ConvertAll<T>(x => (T)x);
            }
            Debug.LogWarning("配制错误,缺少" + typeof(T).ToString());
            return null;
        }


    }

}
