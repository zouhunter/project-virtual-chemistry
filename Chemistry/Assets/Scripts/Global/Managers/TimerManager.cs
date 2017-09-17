using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public interface ITimerBehaviour
{
    void TimerUpdate();
}
public class TimerInfo
{
    public long tick;
    public int executtick;
    public bool stop;
    public bool delete;
    public object target;
    public string timerName;

    public TimerInfo(string timerName, object target,int executtick)
    {
        this.timerName = timerName;
        this.target = target;
        delete = false;
        this.executtick = executtick;
    }
}


public class TimerManager:Singleton<TimerManager>
{
    public enum TimerType{
        Update,
        FixedUpdate,
        Interval
    }
    private float interval = 0.02f;
    private Dictionary<string, TimerInfo> objects = new Dictionary<string, TimerInfo>();
    private TimerType type = TimerType.Update;
    public float Interval
    {
        get { return interval; }
        set { interval = value; }
    }
    /// <summary>
    /// 启动计时器
    /// </summary>
    /// <param name="interval"></param>
    public void StartTimer(TimerType type,float timer = 0.02f)
    {
        this.type = type;
        if (type == TimerType.Interval)
        {
            InvokeRepeating("Run", 0, interval);
        }
    }

    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        CancelInvoke("Run");
    }

    /// <summary>
    /// 添加计时器事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="o"></param>
    public void AddTimerEvent(TimerInfo info)
    {
        if (!objects.ContainsKey(info.timerName))
        {
            objects.Add(info.timerName,info);
        }
    }

    /// <summary>
    /// 删除计时器事件
    /// </summary>
    /// <param name="name"></param>
    public void RemoveTimerEvent(string timerName)
    {
        if (objects.ContainsKey(timerName) && timerName != null)
        {
            objects[timerName].delete = true;
            objects.Remove(timerName);
        }
    }

    /// <summary>
    /// 停止计时器事件
    /// </summary>
    /// <param name="info"></param>
    public void StopTimerEvent(string info)
    {
        if (objects.ContainsKey(info) && info != null)
        {
            objects[info].stop = true;
        }
    }

    /// <summary>
    /// 继续计时器事件
    /// </summary>
    /// <param name="info"></param>
    public void ResumeTimerEvent(string info)
    {
        if (objects.ContainsKey(info) && info != null)
        {
            objects[info].delete = false;
        }
    }

    /// <summary>
    /// 计时器运行
    /// </summary>
    void Run()
    {
        if (objects.Count == 0) return;
        string[] list = new string[objects.Count];
        objects.Keys.CopyTo(list,0);

        for (int i = 0; i < list.Length; i++)
        {
            TimerInfo o = objects[list[i]];
            if (o.delete)
            {
                objects.Remove(o.timerName);
                continue;
            }
            if (o.stop) { continue; }
            ITimerBehaviour timer = o.target as ITimerBehaviour;
            
            if (o.tick++ % o.executtick == 0f)
            {
                timer.TimerUpdate();
            }
            /////////////////////////清除标记为删除的事件///////////////////////////
            
        }
    }
    void FixedUpdate()
    {
        if (type == TimerType.FixedUpdate)
        {
            Run();
        }
    }
    void Update()
    {
        if (type == TimerType.Update)
        {
            Run();
        }
    }

}
