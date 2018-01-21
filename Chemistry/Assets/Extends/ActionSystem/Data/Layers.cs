using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    /// <summary>
    /// this Layers may changed
    /// </summary>
    public class Layers
    {
        public const string pickUpElementLayer = "w:pickupelement";
        public const string rotateItemLayer = "w:rotateitem";
        public const string clickItemLayer = "w:clickitem";
        public const string connectItemLayer = "w:connectitem";
        public const string obstacleLayer = "w:obstacle";
        public const string ropePosLayer = "w:ropepos";
        public const string ropeNodeLayer = "w:ropenode";
        public const string chargeResourceLayer = "w:chargeresource";
        public const string chargeObjLayer = "w:chargeobj";
        public const string placePosLayer = "w:placepos";
        public const string dragItemLayer = "w:dragitem";
        public const string linknodeLayer = "w:linknode";
    }

}