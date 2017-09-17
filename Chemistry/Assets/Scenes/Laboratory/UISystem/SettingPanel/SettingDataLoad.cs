using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;

public class SettingDataLoadSave : Command<bool>
{
    static ClassCacheCtrl classCache;
    static ExpSetting expSetting;
    static string filePath = "";
    static string filename = "Setting";
    static SettingDataLoadSave()
    {
        filePath = Application.dataPath + "/" + filename;
        classCache = new ClassCacheCtrl(Application.dataPath);
    }
    public override void Execute(bool notification)
    {
        //加载
        if (notification == true)
        {
            if (!File.Exists(filePath))
            {
                CreateDefultSetting();
            }
            else
            {
                expSetting = classCache.LoadClassFromLocal<ExpSetting>(filename);
                if (expSetting == null)
                {
                    CreateDefultSetting();
                }
            }
            Facade.RegisterProxy(new Proxy<ExpSetting>(AppConfig.EventKey.SettngData, expSetting));
        }
        //保存
        else
        {
            classCache.SaveClassToLocal(expSetting.GetSaveAbleCopy(), filename);
        }

    }
    static void CreateDefultSetting()
    {
        expSetting = new global::ExpSetting();
        expSetting.ResetDefult();
    }
}
