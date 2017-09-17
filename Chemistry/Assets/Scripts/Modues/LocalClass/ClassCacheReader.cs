using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ClassCacheReader{
    private Stream fileStream;
    private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();
    const int lensize = sizeof(int);
    public ClassCacheReader(string fileName)
    {
        fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
    }
    public ClassCacheReader(Stream stream)
    {
        fileStream = stream;
    }

    private int ReadLength()
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        var lenbuf = new byte[lensize];
        var bytesRead = fileStream.Read(lenbuf, 0, lensize);

        if (bytesRead == 0)
        {
            return 0;
        }
        if (bytesRead != lensize)
            throw new IOException(string.Format("Expected {0} bytes but read {1}", lensize, bytesRead));

        return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenbuf, 0));
    }

    private T ReadObject<T>(int len) where T:class
    {
        fileStream.Seek(lensize, SeekOrigin.Begin);

        var data = new byte[len];
        fileStream.Read(data, 0, len);

        Dispose();

        using (var memoryStream = new MemoryStream(data))
        {
            return (T)_binaryFormatter.Deserialize(memoryStream);
        }

    }
    private void Dispose()
    {
        fileStream.Flush();
        fileStream.Dispose();
        fileStream.Close();
    }

    public T ReadObject<T>() where T : class
    {
        var len = ReadLength();
        if (len == 0)
        {
            Dispose();
            return default(T);
        }
        return ReadObject<T>(len);
    }
}
