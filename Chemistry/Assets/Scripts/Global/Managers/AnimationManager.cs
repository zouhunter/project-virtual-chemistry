using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class AnimationManager : Singleton<AnimationManager> {
    public Transform panelSetting;
    public List<Tween> tweens;

    public void Init()
    {
        DOTween.Init(true, true, LogBehaviour.Default);
        DOTween.SetTweensCapacity(205, 5);
        DOTween.defaultEaseType = Ease.Linear;
        DOTween.defaultAutoKill = true;
    }

    //transform移动
    public void DoTransfromTo(Transform obj, Transform target, float time)
    {
        obj.DOMove(target.position, time);
        obj.DORotate(target.eulerAngles, time);
        obj.DOScale(target.lossyScale, time);
    }
    //transform移动(并设置返回值)
    public void DoTransfromTo(Transform obj, Transform target, float time, UnityAction<object> function, object vlu)
    {
        obj.DOMove(target.position, time);
        obj.DORotate(target.eulerAngles, time);
        obj.DOScale(target.lossyScale, time).OnComplete(() => function(vlu));
    }
    //单向位置
    public void DoSingleMoveTo(Transform obj, Vector3 target, float time)
    {
        //自定义tween
        obj.DOMove(obj.parent.TransformPoint(target), time);
        //DOTween.To(() => obj.localPosition, x => obj.localPosition = x, obj.parent.TransformPoint(target), time);
    }
    public void DoSingleMoveXFrom(Transform obj, float releX, float time)
    {
        Tweener t = obj.DOMoveX(releX, time).From().SetAutoKill(true);
        t.OnComplete(()=>t.Rewind());
    }
    //单向位置
    public void DoSingleMoveFrom(Transform obj, Vector3 target, float time)
    {
        //自定义tween
        obj.DOMove(obj.parent.TransformPoint(target), time).From();
        //DOTween.To(() => obj.localPosition, x => obj.localPosition = x, obj.parent.TransformPoint(target), time);
    }
    //指定移动
    public void DoSingleMoveTo(Transform obj, Vector3 start, Vector3 target, float time)
    {
        obj.localPosition = start;
        obj.DOMove(obj.parent.TransformPoint(target), time);
    }
    //单向旋转
    public void DoSingleRotageTo(Transform obj, Vector3 taret, float time)
    {
        obj.DORotate(taret, time);
    }
    //单向尺寸
    public void DoSingleScaleTo(Transform obj, Vector3 target, float time)
    {
        //自定义tween
        DOTween.To(() => obj.localScale, x => obj.localScale = x, target, time);
    }
    //单向尺寸
    public void DoSingleScaleFrom(Transform obj, Vector3 target, float time)
    {
        ////Debug.Log("??" + obj.name);
        obj.DOScale(target, time).From().SetAutoKill(true);
    }
    //单向颜色
    public void DoSingleColorTo(Material obj, Color target, float time)
    {
        //自定义tween
        DOTween.To(() => obj.color, x => obj.color = x, target, time);
    }
    //双向移动
    public void DoMoveAndBack(Transform obj, Vector3 target, float time, int loopTime)
    {
        obj.DOPunchPosition(target, time).SetLoops(loopTime, LoopType.Yoyo);
    }
    //双向颜色
    public void DoColorAndBack(Material obj, Color start, Color target, float time)
    {
        obj.DOColor(target, time).OnComplete (()=>obj.color = start);
    }
    //双向颜色
    public void DoColorAndBack(Image obj, Color start, Color target, float time)
    {
        obj.DOColor(target, time).OnComplete(() => obj.color = start);
    }
    //双向fade
    public void DoFade(Image obj, float time, int loopTime)
    {
        obj.DOFade(0, time).SetLoops(loopTime, LoopType.Yoyo);
    }
    //反向大小
    public void DoSingleScaleBack(Transform obj, Vector3 target, float time)
    {
        Vector3 start = obj.transform.localScale;
        obj.transform.localScale = target;
        obj.DOScale(start, time);
    }
    //显示文字
    public void DoTextShos(Text text,string value,float time)
    {
        text.text = "";
        text.DOText(value, time);
    }
    public void TimeEvent(float time,UnityAction action)
    {
        StartCoroutine(WaitToExecute(time,action));
    }
    IEnumerator WaitToExecute(float time,UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    /// <summary>
    /// x方向进行移动
    /// </summary>
    /// <param name="isOpen"></param>
    /// <param name="posx"></param>
    /// <param name="animTime"></param>
    /// <param name="panel"></param>
    public void PlayToggleTween(bool isOpen,Tweener open, Tweener close)
    {
        if (isOpen)
        {
            open.Restart();
            close.Pause();
        }
        else
        {
            open.Pause();
            close.Restart();
        }
    }
    /// <summary>
    /// 关闭所有动画
    /// </summary>
    /// <param name="clear"></param>
    public void StopAllTween(bool clear)
    {
        DOTween.Clear(clear);
    }
}
