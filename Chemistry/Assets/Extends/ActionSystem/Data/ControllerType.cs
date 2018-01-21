using UnityEngine;
using System.Collections;
namespace WorldActionSystem
{
    public enum ControllerType
    {
        Install = 1 << 0,//安装
        Match = 1 << 1,//匹配
        Click = 1 << 2,//点击
        Rotate = 1 << 3,//旋转
        Connect = 1 << 4,//连接
        Rope = 1 << 5,//绳索
        Drag = 1 << 6,//拖拽
        Link = 1 << 7,//关联
        Charge = 1 << 8,//填充
    }
}