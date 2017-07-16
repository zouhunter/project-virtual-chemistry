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
        public const string OPEN_PLAYEHELP = "openPlayerHelp";
        public const string OPEN_PRESENTATION = "PresentationData";
        public const string TIP = "tipInfo";
    }
}

