using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
public enum BuildState
{
	Normal,
	Inbuild,
}
public class BuildingItem : MonoBehaviour
{
	public BuildingInfo buildingInfo;
	private BuildingCtrl ctrl { get { return GameManager.buildCtrl; } }
	private bool canBuild;
	private bool reactive;
	private string gikey;
    private Vector3 nomalizePos;

	public void Init()
    {
		buildingInfo.locat = new Grid[buildingInfo.widght, buildingInfo.length];
        for (int i = 0; i < buildingInfo.widght; i++)
        {
            for (int j = 0; j < buildingInfo.length; j++)
            {
                buildingInfo.locat[i, j] = new global::Grid(true);
            }
        }
        buildingInfo.rectangle = new Rectangle(buildingInfo.widght, buildingInfo.length, true);
        buildingInfo.didDraw = false;
		buildingInfo.buildState = BuildState.Inbuild;
    }
    void OnMouseOver()
    {
		if (buildingInfo.buildState == BuildState.Normal) {
			if (Input.GetMouseButtonDown(0))
			{
				foreach (var item in buildingInfo.locat) {
					item.enable = true;
				}
				ctrl.RemoveBuilding (this);
				reactive = true;
			}
		}
    }

	//修建建筑物************************************
	void Update()
	{
		if (buildingInfo.buildState == BuildState.Inbuild)
		{
			//建筑物跟随鼠标
			FallowMouse();
			//设置位置的方法
			ShowPos();
			//修建或者放弃
			BulidOrNot();
		}
		if (reactive) {
			buildingInfo.buildState = BuildState.Inbuild;
			reactive = false;
		}
	}

	private void FallowMouse()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hitinfos = Physics.RaycastAll(ray);
		for (int i = 0; i < hitinfos.Length; i++)
		{
			RaycastHit hitinfo = hitinfos[i];
			if (hitinfo.collider.CompareTag("MovePos"))
			{
				//更新创建出来对象的坐标
				transform.position = hitinfo.point;
                return;
			}
		}
	}
	public void ShowPos()
	{
        //对象中心位置
        nomalizePos.x = (int)transform.position.x;
        nomalizePos.z = (int)transform.position.z;

		canBuild = true;
        buildingInfo.rectangle.enable = true;
        //将整数化的坐标记录在grids中
        for (int i = 0; i < buildingInfo.widght; i++)
		{
			for (int j = 0; j < buildingInfo.length; j++)
			{
				int gx = (int)nomalizePos.x + i - (buildingInfo.widght / 2);
				int gz = (int)nomalizePos.z + j - (buildingInfo.length / 2);
                buildingInfo.locat[i, j].pos = new Vector3(gx, transform.position.y, gz);
                buildingInfo.locat[i,j].enable = ctrl.HaveBuild(buildingInfo.locat[i, j].pos);
                //当有一个不满足，就不能够建造
                if (!buildingInfo.locat[i, j].enable)
                {
                    canBuild = false;
                    buildingInfo.rectangle.enable = false;
                }

            }
		}

        buildingInfo.rectangle.pos = transform.position;

        if (!buildingInfo.didDraw)
		{
			gikey = GridDrawer.AddRectangle(buildingInfo.rectangle);
			buildingInfo.didDraw = true;
		}
    }
	private void BulidOrNot()
	{
		if (canBuild && Input.GetMouseButtonDown(0))
		{
            //修正对象坐标
            Vector3 fixedPos = buildingInfo.locat[0, 0].pos;
            fixedPos.x += buildingInfo.widght / 2f - 0.5f;
            fixedPos.z += buildingInfo.length / 2f - 0.5f;
            fixedPos.y = transform.position.y;
            transform.position = fixedPos;

            //修改建造状态
            buildingInfo.didDraw = false;
			buildingInfo.buildState = BuildState.Normal;
			GridDrawer.DropRectangle(gikey);
			//记录建筑信息
			ctrl.AddNewBuiliding(this);
		}
		else if (Input.GetMouseButtonDown(1))
		{
			//清空建造提示
			GridDrawer.DropRectangle(gikey);
			//销毁对象
			Destroy(gameObject);
			//修改建造状态
			buildingInfo.buildState = BuildState.Normal;
		}
	}

}

