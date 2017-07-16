using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerInfoMationInsetPanel : MonoBehaviour
{
    public PlayerHelpPanel helpPanel;
    public Button insetBtn;
    public Button deleteBtn;
    public Button retureBtn;
    public PlayerHelpObject playerHelpObj;
    private List<PlayerHelp> data { get { return playerHelpObj.Data; } }

    public InputField titleInput;
    public InputField infoInput;

    public GameObject itemPfb;
    private GameObject item;
    private PlayerHelpItem hItem;
    private Dictionary<string, PlayerHelpItem> itemDic = new Dictionary<string, PlayerHelpItem>();
    void Start()
    {
        insetBtn.onClick.AddListener(InsertBtnClicked);
        deleteBtn.onClick.AddListener(DeleteBtnClicked);
        retureBtn.onClick.AddListener(
            () => {
                gameObject.SetActive(false);
                helpPanel.LoadHelpInfo();});
        LoadPlayerHelpInfo();
    }

    private void LoadPlayerHelpInfo()
    {
        for (int i = 0; i < data.Count; i++)
        {
            PlayerHelpItem hitem = CreateAnItem(data[i]);
            itemDic.Add(hitem.title.text, hitem);
        }
    }

    private void DeleteBtnClicked()
    {
        string title = titleInput.text;
        if (title != null && itemDic.ContainsKey(title))
        {
            data.RemoveAll((x) => x.title == titleInput.text);
            ObjectManager.Instance.SavePoolObject(itemDic[title].gameObject);
            itemDic.Remove(title);
        }
    }

    private void InsertBtnClicked()
    {
        string title = titleInput.text;
        if (title != null && itemDic.ContainsKey(title))
        {
            data.RemoveAll((x) => x.title == titleInput.text);
            PlayerHelp help = new PlayerHelp(titleInput.text, infoInput.text);
            data.Insert(0, help);
            itemDic[title].LoadBack();
        }
        else if(title != "")
        {
            PlayerHelp help = new PlayerHelp(titleInput.text, infoInput.text);
            data.Insert(0, help);
            hItem = CreateAnItem(help);
            itemDic.Add(hItem.title.text, hItem);
        }

    }

    private PlayerHelpItem CreateAnItem(PlayerHelp help)
    {
        item = ObjectManager.Instance.GetPoolObject(itemPfb, itemPfb.transform.parent, false);
        hItem = item.GetComponent<PlayerHelpItem>();
        hItem.InitPlayerHelpItem(help.title, help.infomation, titleInput, infoInput);
        return hItem;
    }
}

