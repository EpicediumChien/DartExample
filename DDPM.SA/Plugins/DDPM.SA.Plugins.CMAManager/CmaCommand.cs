using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.CMAManager
{
    public class CmaCommand
    {

        public CmaCommand(string json)
        {
            gid = "";
            JObject jObject = JObject.Parse(json);

            sid = (string)jObject["sid"];
            req = jObject["req"].ToArray();
        }
        public CmaCommand(string _gid, string json)
        {
            gid = _gid;
            JObject jObject = JObject.Parse(json);

            sid = (string)jObject["sid"];
            req = jObject["req"].ToArray();
        }
        public string gid { get; set; }

        public string sid { get; set; }
        public Array req { get; set; }

        public class CmaTask
        {
            public CmaTask(string _sid, string json)
            {
                JObject jObject = JObject.Parse(json);

                sid = _sid;
                id = (int)jObject["id"];
                active = (string)jObject["active"];
                devicetype = (string)jObject["devicetype"];
                command = (string)jObject["command"];
                value = (string)jObject["value"];

                options = jObject["options"].ToArray();
            }

            public string sid { get; set; }
            public int id { get; set; }
            public string active { get; set; }
            public string devicetype { get; set; }
            public string command { get; set; }
            public string value { get; set; }
            public Array options { get; set; }



        }
    }
}
