using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine.Events;
using System.IO;
using BundleUISystem;
public class PlayerHelpPanel : UIPanelTemp
{
    public Button closeBtn;
    public GameObject itemPfb;
    public Transform itemParent;
    public Text infotext;
    private List<GameObject> items = new List<GameObject>();
    private HelpTable _helpTable;
    protected override void OnEnable()
    {
        base.OnEnable();
        if (LoadTableData())
        {
            LoadHelpInfo();
            closeBtn.onClick.AddListener(() => { Destroy(gameObject); });
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private bool LoadTableData()
    {
        var path = Application.dataPath + "/Help.csv";
        _helpTable = new HelpTable();
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "title,information");
            SceneMain.Current.InvokeEvents(AppConfig.EventKey.TIP, "请先配制程序目录下的Help.csv");
            return false;
        }
        else
        {
            try
            {
                var txt = File.ReadAllText(path, System.Text.Encoding.GetEncoding("gb2312"));
                _helpTable.Load(txt);
            }
            catch (Exception e)
            {
                SceneMain.Current.InvokeEvents(AppConfig.EventKey.TIP, "数据加载错误：" + e);
                return false;
            }
            return true;
        }
    }
    /// <summary>
    /// 加载出配制信息
    /// </summary>
    public void LoadHelpInfo()
    {
        if (items.Count > 0)
        {
            foreach (var go in items)
            {
                go.GetComponent<Toggle>().isOn = false;
                ObjectManager.Instance.SavePoolObject(go);
            }
            items.Clear();
        }

        Toggle tog;
        HelpTable.Row pitem;
        GameObject item;

        for (int i = 0; i < _helpTable.GetRowList().Count; i++)
        {
            pitem = _helpTable.GetAt(i);
            item = ObjectManager.Instance.GetPoolObject(itemPfb, itemParent, true, true, true);
            tog = item.GetComponent<Toggle>();
            tog.GetComponentInChildren<Text>().text = pitem.title;
            string info = pitem.information;
            tog.onValueChanged.AddListener((x) => { if (x) infotext.text = info; });
            items.Add(item);

            if (i == 0)
            {
                tog.isOn = true;
                infotext.text = info;
            }
        }
        itemPfb.SetActive(false);
    }

}

/// <summary>
/// 显示帮助信息，提供翻阅功能
/// </summary>
//public class PlayerHelpPanel : MonoBehaviour {
//    private Text m_InfoTitle;
//    private Image m_Image;
//    private Text m_Infomation;

//    private Button m_LastOne;
//    private Button m_NextOne;
//    private InputField m_CurrentOne;

//    public PlayerHelpObject playerHelpObj;
//    private List<PlayerHelp> playerHelp { get { return playerHelpObj.playerHelps; } }

//    private int MaxNum {
//        get { return int.Parse(m_MaxNum.text); }
//        set { m_MaxNum.text = value.ToString();}
//    }//最在值
//    private Text m_MaxNum;
//    private int CurrentNum {
//        get
//        {
//            return int.Parse(m_CurrentOne.text);
//        }
//        set {
//            if (value>0 && value<=MaxNum){
//                m_CurrentOne.text = value.ToString();
//            }
//        }
//    }

//    void Awake()
//    {
//        m_InfoTitle = transform.Find("Image/title").GetComponent<Text>();
//        m_Image = transform.Find("Image").GetComponent<Image>();
//        m_Infomation = transform.Find("Scroll View/Viewport/Text").GetComponent<Text>();

//        m_LastOne = transform.Find("Ctrl/lastOne/lastOne").GetComponent<Button>();
//        m_NextOne = transform.Find("Ctrl/nextOne/nextOne").GetComponent<Button>();
//        m_CurrentOne = transform.Find("Ctrl/InputField/InputField").GetComponent<InputField>();
//        m_MaxNum = transform.Find("Ctrl/MaxNum/MaxNum/Text").GetComponent<Text>();

//        m_LastOne.interactable = false;
//        m_NextOne.interactable = false;

//        m_LastOne.onClick.AddListener(()=> { --CurrentNum; LoadPlayerHelpInfo(CurrentNum); });
//        m_NextOne.onClick.AddListener(()=> { ++CurrentNum; LoadPlayerHelpInfo(CurrentNum); });
//        m_CurrentOne.onValueChanged.AddListener((x)=> {
//            if (x.Length != 1) return;
//            int value = int.Parse(x);
//            if (value > 0 && value <= MaxNum) LoadPlayerHelpInfo(value);
//            else CurrentNum = 1;});
//    }
//    void OnEnable()
//    {
//        LoadHelpInfo();
//    }
//    /// <summary>
//    /// 加载出配制信息
//    /// </summary>
//    void LoadHelpInfo()
//    {
//        m_LastOne.interactable = true;
//        m_NextOne.interactable = true;
//        MaxNum = playerHelp.Count;
//        //加载出第一个
//        LoadPlayerHelpInfo(1);
//    }
//    /// <summary>
//    /// 将获取到的数据模型加载到UI界面上
//    /// </summary>
//    /// <param name="infos"></param>
//    void LoadPlayerHelpInfo(int id)
//    {
//        PlayerHelp item = playerHelp[id - 1];
//        m_InfoTitle.text = item.title;
//        m_Infomation.text = item.infomation.Replace("\\n","\n");
//        //m_Image.sprite = item.sprite;
//        //StartCoroutine(Utility.LoadSprite(false,item.sprite, m_Image));
//    }
//}
