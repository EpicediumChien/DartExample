using Newtonsoft.Json.Linq;
using System;

namespace DDPM.SA.Common.Defer
{
    public struct FwRule
    {
        public string devicetype;
        public string servicetag;
        public string model;
        //public string jsonconfig;
    }

    public class DeferItem
    {
        public string deferid { get; set; }
        public string guid { get; set; } = string.Empty;
        //public string createTime { get; set; }
        public string createtime { get; set; }
        public int count { get; set; } = -1;
        // -1: show defer 3rd, execute now
        //  0: show defer 2nd
        //  1: show defer 1st
        public int commandfrom { get; set; }
        public string commanddata { get; set; }

        public DeferItem()
        {
        }

        public DeferItem(int _from, string _guid, string _commanddata)
        {
            DateTimeOffset dateTime = DateTimeOffset.Now;
            long id = dateTime.ToUnixTimeSeconds();

            deferid = id.ToString();
            guid = _guid;
            createtime = dateTime.ToString();
            count = 2;
            commandfrom = _from;
            commanddata = _commanddata;
        }

        public DeferItem(string json)
        {

            JObject jObject = JObject.Parse(json);

            deferid = (string)jObject["deferid"];
            guid = (string)jObject["guid"];
            createtime = (string)jObject["createtime"];
            count = (int)jObject["count"];
            commandfrom = (int)jObject["commandfrom"];
            commanddata = (string)jObject["commanddata"];
        }

        public override string ToString()
        {
            string result = string.Empty;

            result = result + $"\"deferid\":\"{deferid}\",";
            result = result + $"\"guid\":\"{guid}\",";
            result = result + $"\"createtime\":\"{createtime}\",";
            result = result + $"\"count\":{count},";
            result = result + $"\"commandfrom\":{commandfrom},";
            result = result + "\"commanddata\":\"" + commanddata.Replace("\"", "\\\"") + "\"";

            result = "{" + result + "}";

            return result;
        }
    }
}
