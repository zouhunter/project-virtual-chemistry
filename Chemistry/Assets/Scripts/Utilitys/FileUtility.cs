using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
public class FileUtility  {
    /// <summary>
    /// 判断路径是否存在
    /// </summary>
    public static bool ISPathExistencePath(string path)
    {
        if (Directory.Exists(path))
        {
            return true;
        }
        else
        {
            DirectoryInfo info = Directory.CreateDirectory(path);
            return info.Exists;
        }
    }
}
