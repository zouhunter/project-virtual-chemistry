using UnityEngine;
using System.Collections;
[System.Serializable]
public class BuildingInfo {
	public int widght;
	public int length;
	public int height;

    public Grid[,] locat;
    public Rectangle rectangle;
	public bool didDraw { get; set; }
	public BuildState buildState;
}
