using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.RemoteManagement.Common.Interfaces
{
    public class InfoJsonGenerator
    {

        public string sid { get; set; }
        public string req { get; set; }

        public InfoJsonGenerator(string _sid, List<string> _list)
        {
            try
            {
                sid = _sid;
                req = "[" + GenReq(_list) + "]";
            }
            catch
            {
                throw new Exception("Data can't be null");
            }
        }

        private string GenReq(List<string> lst)
        {
            string result = "";
            for (int i = 0; i < lst.Count; i++)
            {
                //Console.WriteLine($"{i} = {(lst.ToArray())[i].ToString()}");

                if (i > 0)
                {
                    result = result + ",";
                }

                result = result + (lst.ToArray())[i].ToString();

            }

            return result;
        }

        public override string ToString()
        {
            string result = String.Empty;

            result = result + "\"sid\":\"" + sid + "\",";
            result = result + "\"req\":" + req;

            return "{" + result + "}";
        }

        public class TaskJson
        {
            public int tid { get; set; }
            public string active { get; set; }
            public string devicetype { get; set; }
            public string command { get; set; }

            public string value { get; set; }
            public string options { get; set; }



            public override string ToString()
            {
                string result = string.Empty;

                if (tid != null)
                {
                    result = result + $"\"tid\":{tid},";
                }



                if (!String.IsNullOrEmpty(active))
                {
                    result = result + $"\"active\":\"{active}\",";
                }



                if (!String.IsNullOrEmpty(devicetype))
                {
                    result = result + $"\"devicetype\":\"{devicetype}\",";
                }



                if (!String.IsNullOrEmpty(command))
                {
                    result = result + $"\"command\":\"{command}\",";
                }

                if (!String.IsNullOrEmpty(value))
                {
                    result = result + $"\"value\":\"{value}\",";
                }

                result = result + "\"options\":{" + options + "}";

                return result;
            }


            public class Options
            {
                public string index { get; set; }
                public string servicetag { get; set; }
                public string model { get; set; }
                //public string serialnumber { get; set; }

                // attribut for fwupdate
                public string minversion { get; set; }
                // add @ 20241110 stephen
                public bool upgradetolatest { get; set; }

                public bool uod { get; set; }


                public string toString()
                {

                    bool hasValue = false;
                    string result = "";

                    // add @ 20241113 stephen
                    if (uod)
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"uod\":true";
                        hasValue = true;
                    }


                    if (!String.IsNullOrEmpty(index))
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"index\":\"{index}\"";
                        hasValue = true;
                    }

                    if (!String.IsNullOrEmpty(servicetag))
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"servicetag\":\"{servicetag}\"";
                        hasValue = true;
                    }

                    if (!String.IsNullOrEmpty(model))
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"model\":\"{model}\"";
                        hasValue = true;
                    }

                    /*                if (!String.IsNullOrEmpty(serialnumber))
                                    {
                        result = result + $"\"serialnumber\":\"{serialnumber}\",";
                    }
                    */

                    if (!String.IsNullOrEmpty(minversion))
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"minversion\":\"{minversion}\"";
                        hasValue = true;
                    }

                    if (upgradetolatest)
                    {
                        if (hasValue)
                        {
                            result = result + ",";
                        }
                        result = result + $"\"upgradetolatest\":true";
                        hasValue = true;
                    }

                    // add @ 20241110 stephen
                    if (result.Contains("minversion") && result.Contains("upgradetolatest")) 
                    {
                        throw new ArgumentException("Command 'minversion' and 'upgradetolatest' can't be exist in the same task");
                    }


                    return result;
                }
            }
        }
    }
}
