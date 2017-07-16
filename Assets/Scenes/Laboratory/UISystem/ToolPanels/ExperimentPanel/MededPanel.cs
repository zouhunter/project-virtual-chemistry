using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class MededPanel : MonoBehaviour
{

    public Button add;//添加
    public Button config;//修改
    public Toggle intro;//介绍
    public Button reset;//介绍
    public Button remove;//移除
    public Button operat;//操作
    public Button demon;//演示

    public ExpConfigPanel expConfigPanel;
    public IntorPanel intorPanel;
    public GameObject handPanel;

    public ExperimentObject inerExperimentObj;
    public ExperimentObject experimentObj;

    public GameObject itemPrefab;
    private List<GameObject> loaded = new List<GameObject>();
    public ToggleGroup group;

    private const string inside = "develop";
    private Dictionary<string, Experiment> m_Exeriments;

    private string selectedName;
    private GameObject selectedGameObject;
    private bool Selected
    {
        get { return group.AnyTogglesOn(); }
    }
    void OnEnable()
    {
        LoadAllExperiments();
    }
    void Start()
    {
        RegisterButtonEvents();
    }
   

    void Update()
    {
        demon.interactable =
            operat.interactable =
            remove.interactable =
            intro.interactable =
            config.interactable = Selected;
    }

    void OnDisable()
    {
        if (intorPanel != null && intorPanel.gameObject.activeSelf)
        {
            intorPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 添加按扭事件
    /// </summary>
    void RegisterButtonEvents()
    {
        reset.onClick.AddListener(OnResetButtonClicked);
        config.onClick.AddListener(OnConfigButtonClicked);
        add.onClick.AddListener(OnAddNewExpButtonCliced);
        demon.onClick.AddListener(OnDemonButtonCliked);
        remove.onClick.AddListener(OnRemoveButtonClicked);
        intro.onValueChanged.AddListener(OnIntroductionToggleClicked);
    }

    /// <summary>
    /// 将实验加载到界面上
    /// </summary>
    void LoadAllExperiments()
    {
        ///清除已经加载
        if (loaded != null)
            for (int i = 0; i < loaded.Count; i++)
            {
                loaded[i].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                ObjectManager.Instance.SavePoolObject(loaded[i]);
            }
        ///加载预制
        if (!PlayerPrefs.HasKey(inside)|| PlayerPrefs.GetInt(inside) == 0)
        {
            for (int i = 0; i < inerExperimentObj.experiments.Count; i++)
            {
                experimentObj.experiments.Add(inerExperimentObj.experiments[i]);
            }
            PlayerPrefs.SetInt(inside,1);
        }

        Experiment experiment;
        GameObject item;
        m_Exeriments = new Dictionary<string, Experiment>();
        loaded.Clear();
        for (int i = 0; i < experimentObj.experiments.Count; i++)
        {
            experiment = experimentObj.experiments[i];
            string key = experiment.name;

            item = ObjectManager.Instance.GetPoolObject(itemPrefab, itemPrefab.transform.parent, false);
            loaded.Add(item);

            LoadAnExperimentItem(item, key);

            m_Exeriments.Add(key, experimentObj.experiments[i]);
        }
    }

    /// <summary>
    /// 加载一个item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="key"></param>
    void LoadAnExperimentItem(GameObject item, string key)
    {
        item.GetComponentInChildren<Text>().text = key;
        item.GetComponent<Toggle>().onValueChanged.AddListener((x) =>
        {

            if (x)
            {
                selectedName = key;
                selectedGameObject = item;
                if (intro.isOn)
                {
                    OnIntroductionToggleClicked(true);
                }
            }
        }
        );

    }

    /// <summary>
    /// 重置按扭
    /// </summary>
    void OnResetButtonClicked()
    {
        experimentObj.experiments.Clear();
        PlayerPrefs.SetInt(inside, 0);
        LoadAllExperiments();
    }

    /// <summary>
    /// 配制已有实验
    /// </summary>
    void OnConfigButtonClicked()
    {
        expConfigPanel.gameObject.SetActive(true);
        expConfigPanel.LoadExperiment(m_Exeriments[selectedName]);
        handPanel.SetActive(true);
        //GetComponentInParent<ToggleDoTweenPanel>().IsOn = false;

        Laboratory.ChangedOperateType(OperateType.Config);
    }

    /// <summary>
    /// 添加实验
    /// </summary>
    void OnAddNewExpButtonCliced()
    {
        expConfigPanel.gameObject.SetActive(true);
        expConfigPanel.LoadExperiment(null);
        handPanel.SetActive(true);
        //GetComponentInParent<ToggleDoTweenPanel>().IsOn = false;

        Laboratory.ChangedOperateType(OperateType.Config);
    }

    /// <summary>
    /// 演示模式
    /// </summary>
    void OnDemonButtonCliked()
    {
        Laboratory.ChangedOperateType(OperateType.Domon);
        FindObjectOfType<TeachPanel>().OnEnableTeachPanel(m_Exeriments[selectedName].staps);
        //............tghk
        //GetComponentInParent<ToggleDoTweenPanel>().IsOn = false;
    }

    /// <summary>
    /// 移除实验
    /// </summary>
    void OnRemoveButtonClicked()
    {
        experimentObj.experiments.Remove(m_Exeriments[selectedName]);
        selectedGameObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        ObjectManager.Instance.SavePoolObject(selectedGameObject);
    }

    /// <summary>
    /// 实验介绍
    /// </summary>
    void OnIntroductionToggleClicked(bool isOpen)
    {
        intorPanel.gameObject.SetActive(isOpen);
        intorPanel.LoadIntroduceInfomation(m_Exeriments[selectedName]);
    }



}
