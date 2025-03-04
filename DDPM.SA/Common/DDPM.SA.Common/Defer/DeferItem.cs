using Newtonsoft.Json;
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
        public string deferid { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        //public string createTime { get; set; }
        public string createtime { get; set; } = string.Empty;
        public int count { get; set; } = -1;
        // -1: show defer 3rd, execute now
        //  0: show defer 2nd
        //  1: show defer 1st
        public int commandfrom { get; set; } = DeferControlPanel.SRC_FROM_CLI;
        public string commanddata { get; set; } = string.Empty;

        public DeferItem()
        {
        }

        public DeferItem(int _from, string _guid, string _commanddata)
        {
            try
            {
                DateTimeOffset dateTime = DateTimeOffset.Now;
                long id = dateTime.ToUnixTimeSeconds();

                deferid = id.ToString();
                guid = _guid ?? throw new ArgumentNullException(nameof(_guid), "GUID cannot be null.");
                createtime = dateTime.ToString();
                count = 2;
                commandfrom = _from;
                commanddata = _commanddata ?? throw new ArgumentNullException(nameof(_commanddata), "Command data cannot be null.");
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                throw; // Re-throw the exception after logging it
            }
        }

        public DeferItem(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));
            }

            try
            {
                JObject jObject = JObject.Parse(json);

                deferid = (string)jObject["deferid"] ?? string.Empty;
                guid = (string)jObject["guid"] ?? string.Empty;
                createtime = (string)jObject["createtime"] ?? string.Empty;
                count = jObject["count"] != null ? (int)jObject["count"] : -1;
                commandfrom = jObject["commandfrom"] != null ? (int)jObject["commandfrom"] : 0;
                commanddata = (string)jObject["commanddata"] ?? string.Empty;
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException("Invalid JSON format.", nameof(json), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while parsing the JSON string.", ex);
            }
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
