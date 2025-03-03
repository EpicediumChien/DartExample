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
            //    gid = _gid;
            //    JObject jObject = JObject.Parse(json);
            //    sid = (string)jObject["sid"];
            //    req = jObject["req"].ToArray();
            if (string.IsNullOrEmpty(_gid))
            {
                throw new ArgumentNullException(nameof(_gid), "gid cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");
            }

            gid = _gid;

            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid JSON format.", nameof(json), ex);
            }

            sid = (string)jObject["sid"] ?? throw new ArgumentNullException("sid", "sid cannot be null.");

            if (jObject["req"] == null)
            {
                throw new ArgumentNullException("req", "req cannot be null.");
            }

            req = jObject["req"].ToArray();
        }

        public string gid { get; set; } = string.Empty;
        public string sid { get; set; } = string.Empty;
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

            public string sid { get; set; } = string.Empty;
            public int tid { get; set; }
            public string active { get; set; } = string.Empty;
            public string devicetype { get; set; } = string.Empty;
            public string command { get; set; } = string.Empty;
            public string value { get; set; } = string.Empty;
            public JObject options { get; set; }

        }

        public class CmaTaskOption
        {
            public CmaTaskOption(JObject options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options), "Options cannot be null.");
                }

                index = (string)options["index"] ?? string.Empty;
                servicetag = (string)options["servicetag"] ?? string.Empty;
                minversion = (string)options["minversion"] ?? string.Empty;
                model = (string)options["model"] ?? string.Empty;

                // add @ 20241110 stephen
                try
                {
                    upgradetolatest = options["upgradetolatest"] != null && (bool)options["upgradetolatest"];
                }
                catch
                {
                    upgradetolatest = false;
                }

                // add @ 20241113 stephen
                try
                {
                    uod = options["uod"] != null && (bool)options["uod"];
                }
                catch
                {
                    uod = false;
                }
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
