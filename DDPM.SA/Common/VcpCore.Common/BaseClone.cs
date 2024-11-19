using System;
using System.Text.Json;

namespace VcpCore.Common
{
    /// <summary>
    /// Univerasl Deep Clone
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class BaseClone<T>
    {
        public virtual T Clone()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(this);
            return JsonSerializer.Deserialize<T>(bytes);

            //the .NET team marked it as obsolete (as error) in .NET 7 and onwards.
            //You can add the following property to your project file to make it compile again:
            //<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>

            //MemoryStream memoryStream = new MemoryStream();
            //BinaryFormatter formatter = new BinaryFormatter();
            //formatter.Serialize(memoryStream, this);
            //memoryStream.Position = 0;
            //return (T)formatter.Deserialize(memoryStream);
        }
    }
}