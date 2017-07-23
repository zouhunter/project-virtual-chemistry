using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;


public class ClassCacheSaver {
    Stream stream;
    private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

    public ClassCacheSaver(string fileName){
        stream = new FileStream(fileName,FileMode.OpenOrCreate);
    }
    public ClassCacheSaver(Stream stream)
    {
        this.stream = stream;
    }

    private byte[] Serialize<T>(T obj) where T : class
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                _binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
            return null;
        }
    }

    private void WriteLength(int len)
    {
        var lenbuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len));
        stream.Seek(0, SeekOrigin.End);
        stream.Write(lenbuf, 0, lenbuf.Length);
    }

    private void WriteObject(byte[] data)
    {
        stream.Seek(0, SeekOrigin.End);
        stream.Write(data, 0, data.Length);
    }

    private void Dispose()
    {
        stream.Flush();
        stream.Dispose();
        stream.Close();
    }
    //ExpSetting
    public void WriteObject<T>(T obj) where T:class
    {
        stream.SetLength(0);
        var data = Serialize(obj);
        if (data == null)
        {
            throw new Exception("数据序列化失败");
        }
        WriteLength(data.Length);
        WriteObject(data);
        Dispose();
    }
}
