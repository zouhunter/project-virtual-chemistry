using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
public class TeachPanel : MonoBehaviour {
    public Toggle close;
    /// <summary>
    /// 实验数据
    /// </summary>
    private List<StapInfo> m_Experiments;
    private int stapCount;
    private int currIndex;
    private StapInfo currStap { get
        {
            return m_Experiments[currIndex];
        } }
    public List<StapInfo> Experiments { set { m_Experiments = value; stapCount = value.Count;currIndex = 0; } }

    public Transform parent;
    public string[] paths;
    public Button lastBtn;
    public Button executeBtn;
    public Button nextBtn;

    private List<GameObject> loadedGameObjects = new List<GameObject>();

    void Start()
    {
        executeBtn.onClick.AddListener(StartAction);
        lastBtn.onClick.AddListener(Undo);
        nextBtn.onClick.AddListener(Skip);
        executeBtn.gameObject.SetActive(false);
        lastBtn.gameObject.SetActive(false);
        nextBtn.gameObject.SetActive(false);
    }

    public void OnEnableTeachPanel(List<StapInfo> experiments)
    {
        Experiments = experiments;
        SetActives(false, true, false);
        executeBtn.gameObject.SetActive(true);
        executeBtn.GetComponentInChildren<Text>().text = "Start";

        lastBtn.gameObject.SetActive(true);
        nextBtn.gameObject.SetActive(true);

        close.onValueChanged.AddListener(ClosePanel);
        close.isOn = false;
    }

    void ClosePanel(bool x)
    {
        if (close.isOn == true)
        {
            BuildSuccess();
            close.onValueChanged.RemoveListener(ClosePanel);
        }
    }

    void StartAction()
    {
        executeBtn.GetComponentInChildren<Text>().text = "执行";
        executeBtn.onClick.RemoveListener(StartAction);
        executeBtn.onClick.AddListener(Execute);
        SetActives(false, true, true);
    }

    void Undo()
    {

    }

    void Execute()
    {
        LoopAction();
        currIndex++;
    }

    void Skip()
    {

    }


    void LoopAction()
    {
        if (currIndex >= stapCount)
        {
            BuildSuccess();
            return;
        }

        if (currStap.needOperate)
        {
            ToNextStap();
            SetActives(true, true, true);
        }
        else
        {
            //PresentationData data = PresentationData.Allocate(currStap.name, currStap.infomation, currStap.tipInfo);
            //Facade.Instance.SendNotification<PresentationData>("PresentationData", data);

            //Laboratory.Main.panelSelect = OnPresentationDataSelected;
            SetActives(true, false, true);
        }
    }

    void OnPresentationDataSelected()
    {
        //Laboratory.Main.panelSelect = null;
        currIndex++;
        SetActives(true, true, true);
    }

    void ToLastStap()
    {

    }

    void ToNextStap()
    {
        ClearLoaded();
        GameObject item;
        ElementInstall element;
        for (int i = 0; i < currStap.installs.Count; i++)
        {
            element = currStap.installs[i];
            item = LoadPfb(element.name);
            item = ObjectManager.Instance.GetPoolObject(item, parent, true,false);
            item.transform.DOMove(element.translation.position,1f).SetAutoKill(true);
            loadedGameObjects.Add(item);
        }
    }

    GameObject LoadPfb(string name)
    {
        GameObject item = null;
        for (int i = 0; i < paths.Length; i++)
        {
            item = Resources.Load<GameObject>(paths[i] + name);
            if (item)
                break;
        }
        return item;
    }

    void ClearLoaded()
    {
        for (int i = 0; i < loadedGameObjects.Count; i++)
        {
            ObjectManager.Instance.SavePoolObject(loadedGameObjects[i],true);
        }
        loadedGameObjects.Clear();
    }

    void BuildSuccess()
    {
        ClearLoaded();
        SetActives(false, true, true);
        executeBtn.gameObject.SetActive(false);
        lastBtn.gameObject.SetActive(false);
        nextBtn.gameObject.SetActive(false);
    }

    void SetActives(bool last, bool exe, bool next)
    {
        lastBtn.interactable = last;
        executeBtn.interactable = exe;
        nextBtn.interactable = next;
    }
}
