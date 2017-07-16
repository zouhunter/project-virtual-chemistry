using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
#if UNITY_EDITOR || UNITY_STANDALONE
using System.IO;

#endif

/// <summary>
/// 读取Csv中的数据并加载到属性
/// </summary>
public class CsvConfigManager:Singleton<CsvConfigManager>
{
    public static string UrlPath { get; private set; }
#if UNITY_EDITOR||UNITY_STANDALONE
    public static string FilesPath { get; private set; }

#endif

    public static Dictionary<string, CsvTable> loadedTables = new Dictionary<string, CsvTable>();

    //public TextureLoader textureLoader;

    void Awake()
    {
#if UNITY_EDITOR||UNITY_STANDALONE
        UrlPath = "file:///" + Application.streamingAssetsPath + "/Config/"; ;
#elif UNITY_WEBGL
        UrlPath = Application.streamingAssetsPath + "/Config/"; ;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        FilesPath = Application.streamingAssetsPath + "/Config/";
#endif
    }

    //public CsvConfigManager(){
    //    //textureLoader = new TextureLoader(this);
    //}
    /// <summary>
    /// 异步加载配制表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public void LoadConfigAsync<T>(string fileName, UnityAction<T> action) where T : CsvTable
    {
        if (action == null)
        {
            Debug.Log("null");
            return;
        }
        //Debug.Log(fileName);
        if (loadedTables.ContainsKey(fileName))
        {
            action((T)loadedTables[fileName]);
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        else if (File.Exists(FilesPath + fileName))
        {
            string csvText = LoadTextAsset(fileName);
            CsvTable table = Activator.CreateInstance<T>();
            table.Load(csvText);
            loadedTables.Add(fileName, table);
            action((T)table);
        }
#endif
        else
        {
            UnityAction<string> txtAction = (csvText) =>
            {
                CsvTable table = Activator.CreateInstance<T>();
                table.Load(csvText);
                loadedTables.Add(fileName, table);
                action((T)table);
#if UNITY_EDITOR || UNITY_STANDALONE
                //保存到本地文件
                SaveTextAsset(fileName, csvText);
#endif
            };
            StartCoroutine(DownTextAssetAsync(fileName, txtAction));
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="fileName">配置文件名, CSV文件</param>
    /// <param name="index">表示从第index行开始读取文件, 从0开始</param>
    public static T LoadConfig<T>(string fileName) where T : CsvTable
    {
        if (!loadedTables.ContainsKey(fileName))
        {
            string csvText = LoadTextAsset(fileName);
            CsvTable table = Activator.CreateInstance<T>();
            table.Load(csvText);
            loadedTables.Add(fileName, table);
            return (T)table;
        }
        else
        {
            return (T)loadedTables[fileName];
        }
    }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE

    /// <summary>
    /// 将数据保存到配制表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="index"></param>
    /// <param name="objs"></param>
    public static void SaveConfig(string tableName, CsvTable table)
    {
        string csvData = table.UnLoad();
        SaveTextAsset(tableName, csvData);
    }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static string LoadTextAsset(string fileName)
    {
        if (File.Exists(FilesPath + fileName))
        {
            string text = File.ReadAllText(FilesPath + fileName);
            return text;
        }
        else
        {
            Debug.LogError(fileName + "不存在");
        }
        return null;
    }
#endif


    /// <summary>
    /// 异步下载文件并读取
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="action"></param>
    static IEnumerator DownTextAssetAsync(string fileName,UnityAction<string> action) 
    {
        WWW www = new WWW(UrlPath + fileName);
        Debug.Log(UrlPath + fileName);
        yield return www;
        if (www.isDone && www.error == null)
        {
            if(action != null)
            {
                action(www.text);
            }
            yield break;
        }
        Debug.LogWarning(www.error);
    }
#if UNITY_EDITOR || UNITY_STANDALONE
    /// <summary>
    /// 保存字符串
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static void SaveTextAsset(string fileName, string text)
    {
        try
        {
            File.WriteAllText(FilesPath + fileName, text);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

#endif
}
