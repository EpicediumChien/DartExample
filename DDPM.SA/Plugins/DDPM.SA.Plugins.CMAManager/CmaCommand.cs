using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

                tid = (int)jObject["tid"];

                active = (string)jObject["active"];

                devicetype = (string)jObject["devicetype"];

                command = (string)jObject["command"];

                //value = (string)jObject["value"];

                if (null != jObject["value"])
                {

                    try

                    {

                        value = ((JObject)jObject["value"]).ToString();

                    }

                    catch

                    {

                        value = (string)jObject["value"];

                    }

                }


                options = (JObject)(jObject["options"]);

            }

            public string sid { get; set; }

            public int tid { get; set; }

            public string active { get; set; }

            public string devicetype { get; set; }

            public string command { get; set; } = string.Empty;

            public string value { get; set; } = string.Empty;

            public JObject options { get; set; }

        }

        public class CmaTaskOption

        {

            public CmaTaskOption(JObject options)

            {

                //JObject jObject = JObject.Parse(options);

                index = (string)options["index"];

                servicetag = (string)options["servicetag"];

                modelname = (string)options["modelname"];

            }

            public string index { get; set; } = string.Empty;

            public string servicetag { get; set; } = string.Empty;

            public string modelname { get; set; } = string.Empty;

        }

    }

}
