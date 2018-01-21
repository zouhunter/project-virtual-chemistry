using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    [System.Serializable]
    public class Config
    {
        private static Config _defult;
        public static Config Defult
        {
            get
            {
                if (_defult == null)
                {
                    _defult = new Config();
                }
                return _defult;
            }
            set
            {
                _defult = value;
            }
        }

        public int _autoExecuteTime = 3;
        public int _hitDistence = 100;
        public int _elementFoward = 1;
        public bool _highLightNotice = true;//高亮提示
        public bool _useOperateCamera = true;//使用专用相机
        public bool _angleNotice = true;//箭头提示
        public bool _quickMoveElement = false;//元素快速移动
        public bool _ignoreController = false;//忽略控制器
        public Material _lineMaterial = null;
        public float _lineWidth = 0.2f;
        public Color _highLightColor = Color.green;
        public GameObject _angleObj = null;

        public static int autoExecuteTime { get { return Defult._autoExecuteTime; } }
        public static int hitDistence { get { return Defult._hitDistence; } }
        public static int elementFoward { get { return Defult._elementFoward; } }
        public static bool highLightNotice { get { return Defult._highLightNotice; } }
        public static bool useOperateCamera { get { return Defult._useOperateCamera; } }
        public static bool angleNotice { get { return Defult._angleNotice; } }
        public static bool quickMoveElement { get { return Defult._quickMoveElement; } }
        public static bool ignoreController { get { return Defult._ignoreController; } }
        public static Material lineMaterial { get { return Defult._lineMaterial; } }
        public static float lineWidth { get { return Defult._lineWidth; } }
        public static Color highLightColor { get { return Defult._highLightColor; } }
        public static GameObject angleObj { get { return Defult._angleObj; } }
    }
}

