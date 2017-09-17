using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ListIndexCtrl {
    private List<StapInfo> staps;
    private int currIndex;
    public StapInfo currStap
    {
        get { return staps[currIndex]; }
    }

    public ListIndexCtrl(Experiment experiment)
    {
        staps = experiment.staps;
        currIndex = 0;
    }

    /// <summary>
    /// 插入一步
    /// </summary>
    /// <param name="stapInfo"></param>
    public void InsertAStap(StapInfo stapInfo)
    {
        stapInfo.index = ++currIndex;
        staps.Insert(currIndex, stapInfo);
    }
    /// <summary>
    /// 删除当前步骤
    /// </summary>
    public bool RemoveAStap()
    {
        if (staps.Count > 1)
        {
            staps.Remove(currStap);
            if (currIndex > 0)
            {
                currIndex--;
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 跳步
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool SetIndex(int index)
    {
        if (index >= 0 && index < staps.Count)
        {
            currIndex = index;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 下一步
    /// </summary>
    /// <returns></returns>
    public bool NextStap()
    {
        if (currIndex < staps.Count - 1)
        {
            currIndex++;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 上一步
    /// </summary>
    /// <returns></returns>
    public bool LastStap()
    {
        if (currIndex >= 1)
        {
            currIndex--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 步骤进度
    /// </summary>
    /// <param name="current"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    public int GetStapProgress(out int current,out int total)
    {
        current = currIndex;
        total = staps.Count;
        if (current == 0)
        {
            if (total == 1)
            {
                return -2;
            }
            else
            {
                return -1;
            }
        }
        else if (current == total - 1)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
