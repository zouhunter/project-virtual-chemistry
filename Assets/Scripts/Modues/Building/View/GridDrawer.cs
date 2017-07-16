using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridDrawer : MonoBehaviour {

    public Material red;
    public Material green;
    public float height;
	//private static Dictionary<string,Grid[,]> gridDic = new Dictionary<string,Grid[,]>();
    private static Dictionary<string, Rectangle> rectDic = new Dictionary<string, Rectangle>();
    public static string AddRectangle(Rectangle rect)
    {
        string key = Time.time.ToString();
        rectDic.Add(key, rect);
        return key;
    }

  //  public static string AddGrid(Grid[,] grids)
  //  {
		//string key = Time.time.ToString ();
		//gridDic.Add(key,grids);
		//return key;
  //  }

	public static void DropRectangle(string key)
    {
		if (rectDic.ContainsKey(key)) {
            rectDic.Remove (key);
		}
    }

    public void OnPostRender()
    {
		//foreach (KeyValuePair<string,Grid[,]> gridGroup in gridDic)
  //      {
		//	foreach (Grid grid in gridGroup.Value)
  //          {
  //              DrawByGrid(grid);
  //          }
  //      }
        foreach (var item in rectDic)
        {
            DrawByRectangle(item.Value);
        }
    }

    //public void DrawByGrid(Grid grid)
    //{
    //    GL.PushMatrix();

    //    if (grid.enable)
    //    {
    //        green.SetPass(0);
    //    }
    //    else
    //        red.SetPass(0);
    //    GL.Begin(GL.QUADS);
    //    Vector3 center = grid.pos;

    //    GL.Vertex3(center.x - Grid.scale/2, center.y, center.z + Grid.scale/2);
    //    GL.Vertex3(center.x + Grid.scale/2, center.y, center.z + Grid.scale/2);
    //    GL.Vertex3(center.x + Grid.scale/2, center.y, center.z - Grid.scale/2);
    //    GL.Vertex3(center.x - Grid.scale/2, center.y, center.z - Grid.scale/2);

    //    GL.End();
    //    GL.PopMatrix();
    //}
    public void DrawByRectangle(Rectangle rect)
    {
        GL.PushMatrix();

        if (rect.enable)
        {
            green.SetPass(0);
        }
        else
            red.SetPass(0);
        GL.Begin(GL.QUADS);
        Vector3 center = rect.pos;

        GL.Vertex3(center.x - rect.widght / 2, height, center.z + rect.length / 2);
        GL.Vertex3(center.x + rect.widght / 2, height, center.z + rect.length / 2);
        GL.Vertex3(center.x + rect.widght / 2, height, center.z - rect.length / 2);
        GL.Vertex3(center.x - rect.widght / 2, height, center.z - rect.length / 2);

        GL.End();
        GL.PopMatrix();
    }
}
