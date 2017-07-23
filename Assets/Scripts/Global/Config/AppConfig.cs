using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class AppConfig {
    //路径
    public static class PathConfig {
        public const string insideDataPath = "DataAsset/";
        public const string elementperfabPath = "Prefabs/Element/";
        public const string medicineperfabPath = "Prefabs/Medicine/";
    }

    public static class EventKey
    {
        internal static string TIP;
        internal static string FillImage;
        internal static string ClickEmpty;
        internal static string SettngData;

        static EventKey()
        {
            TIP = System.Guid.NewGuid().ToString();
            FillImage = System.Guid.NewGuid().ToString();
            ClickEmpty = System.Guid.NewGuid().ToString();
            SettngData = System.Guid.NewGuid().ToString();
        }
    }
}

