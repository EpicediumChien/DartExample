using Newtonsoft.Json.Linq;
using System;
using System.Linq;

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

                minversion = (string)options["minversion"];

                model = (string)options["model"];

                // add @ 20241110 stephen
                try
                {
                    upgradetolatest = (bool)options["upgradetolatest"];
                }
                catch 
                { }

                // add @ 20241113 stephen
                try
                {
                    uod = (bool)options["uod"];
                }
                catch
                { }

            }

            public string index { get; set; } = string.Empty;
            public string servicetag { get; set; } = string.Empty;
            public string minversion { get; set; } = string.Empty;
            public string model { get; set; } = string.Empty;
            // add @ 20241110 stephen
            public bool upgradetolatest { get; set; } = false;
            // add @ 20241113 stephen
            public bool uod { get; set; } = false;

        }

    }

}
