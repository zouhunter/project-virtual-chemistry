using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ClassCacheCtrl
{
    string saveFolder;
    ClassCacheSaver saver;
    ClassCacheReader reader;
    public ClassCacheCtrl(string saveFolder)
    {
        this.saveFolder = saveFolder;
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
    }

    public void SaveClassToLocal<T>(T arg0, string name = null) where T : class
    {
        string fileName = Path.Combine(saveFolder, name ?? (typeof(T).ToString()) + ".bin");
        saver = new ClassCacheSaver(fileName);
        saver.WriteObject(arg0);
    }

    public T LoadClassFromLocal<T>(string name = null) where T : class
    {
        string fileName = Path.Combine(saveFolder, name ?? (typeof(T).ToString()) + ".bin");
        reader = new ClassCacheReader(fileName);
        return reader.ReadObject<T>();
    }
    public T LoadClassFromStream<T>(Stream stream) where T : class
    {
        reader = new ClassCacheReader(stream);
        return reader.ReadObject<T>();
    }

    public T LoadClassFromBytes<T>(byte[] bytes) where T : class
    {
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            reader = new ClassCacheReader(stream);
            return reader.ReadObject<T>();
        }
    }
}
