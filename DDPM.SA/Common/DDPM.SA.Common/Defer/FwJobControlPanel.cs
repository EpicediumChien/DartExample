using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Common.Defer
{
    public class FwJobControlPanel
    {
        //C:\ProgramData\Dell\Dell Display and Peripheral Manager
        private static readonly string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static readonly string path_folder = "\\Dell\\Dell Display and Peripheral Manager\\DEFER\\";
        private static readonly string FILE_PATH = path_programdata + path_folder;
        //private static readonly string FILE_PATH = @"c:\cmacmd\defer\";
        private static readonly string FILE_NAME = @"fwjob_data.txt";



        private static List<string> listFwJob = new List<string>();


        public static void init()
        {
            listFwJob = new List<string>();
            loadFile();
        }

        public static List<string> getList()
        {
            return listFwJob;
        }
        public static void removeItems(int count)
        {
            listFwJob.RemoveRange(0, count);
            writeToFile();
        }

        public static void removeItem(string item)
        {
            listFwJob.Remove(item);
            writeToFile();
        }

        public static void addToSchedule(DeferItem item)
        {
            //loadFile();
            listFwJob.Add(item.ToString());
            writeToFile();
        }

        public static void addToSchedule(string item)
        {
            //loadFile();
            listFwJob.Add(item);
            writeToFile();
        }

        // Add Error handling.
        public static List<string> displayConnected(List<MonitorInfo> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list), "The list of MonitorInfo cannot be null.");
            }

            init();

            if (listFwJob.Count == 0)
            {
                return null;
            }

            List<string> deferItems = new List<string>();
            List<FwRule> fwRules = new List<FwRule>();

            foreach (MonitorInfo info in list)
            {
                if (info?.edid == null)
                {
                    continue; // 跳過 info 或 info.edid 為 null 的情況
                }

                FwRule fwRule = new FwRule
                {
                    devicetype = Params.DeviceType.DISPLAY,
                    model = info.edid.ModelName?.ToLower(),
                    servicetag = info.edid.ServiceTag?.ToLower()
                };

                fwRules.Add(fwRule);
            }

            if (fwRules.Count == 0)
            {
                return null;
            }

            foreach (FwRule rule in fwRules)
            {
                foreach (string fwjob in listFwJob)
                {
                    // add @ 20250206 stephen : fix bug
                    if (!fwjob.ToLower().Contains("model") && !fwjob.ToLower().Contains("servicetag"))
                    {
                        deferItems.Add(fwjob);
                        continue;
                    }

                    if (fwjob.ToLower().Contains(rule.model) || fwjob.ToLower().Contains(rule.servicetag))
                    {
                        deferItems.Add(fwjob);
                    }
                }

                foreach (string deferItem in deferItems)
                {
                    listFwJob.Remove(deferItem);
                }
            }

            writeToFile();

            return deferItems;
        }

        public static List<string> displayConnected_old(List<MonitorInfo> list)
        {

            init();

            if (listFwJob.Count == 0)
            {
                return null;
            }

            List<string> deferItems = new List<string>();

            List<FwRule> fwRules = new List<FwRule>();
            FwRule fwRule = new FwRule();

            foreach (MonitorInfo info in list)
            {
                fwRule = new FwRule();

                fwRule.devicetype = Params.DeviceType.DISPLAY;
                fwRule.model = info.edid.ModelName.ToLower();
                fwRule.servicetag = info.edid.ServiceTag.ToLower();

                fwRules.Add(fwRule);
            }

            if (fwRules.Count == 0)
            {
                return null;
            }

            foreach (FwRule rule in fwRules)
            {
                foreach (string fwjob in listFwJob)
                {
                    // add @ 20250206 stephen : fix bug
                    if (!fwjob.ToLower().Contains("model") && !fwjob.ToLower().Contains("servicetag"))
                    {
                        deferItems.Add(fwjob);
                        continue;
                    }

                    if (fwjob.ToLower().Contains(fwRule.model) || fwjob.ToLower().Contains(fwRule.servicetag))
                    {
                        deferItems.Add(fwjob);
                    }
                }

                foreach (string deferItem in deferItems)
                {
                    listFwJob.Remove(deferItem);
                }
            }

            writeToFile();

            return deferItems;

        }

        private static void loadFile()
        {
            try
            {
                //string[] alllines = File.ReadAllLines(FILE_PATH + FILE_NAME);
                //listDefer = alllines.ToList();

                string info = string.Empty;
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString((FILE_PATH + FILE_NAME), out info);

                //Console.WriteLine(serialized_string);

                JArray jarray = JArray.Parse(serialized_string);
                foreach (var item in jarray)
                {
                    /*Console.WriteLine("item output : ");
                    Console.WriteLine(item.ToString());*/

                    if (0 >= item.ToString().Length)
                    {
                        continue;
                    }

                    listFwJob.Add(item.ToString());
                }
            }
            catch (Exception e)
            {
                // exception
                listFwJob = new List<string>();
            }

            //Task.Delay(1000).Wait();
        }

        private static void writeToFile()
        {
            string info = string.Empty;
            try
            {
                if (!(System.IO.Directory.Exists(FILE_PATH)))
                {
                    System.IO.Directory.CreateDirectory(FILE_PATH);
                }
                //add @ 20250328 stephen
                if (!DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(FILE_PATH, out info))
                {
                    info = ("[writeToFile] SetFolderPermissions_UserReadAndExecute failed: " + info);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDirectory Exception: " + e.Message);
            }

            try
            {
                bool write = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(listFwJob).ToString(), (FILE_PATH + FILE_NAME), out info);
                Console.WriteLine(write);
            }
            catch (Exception e) {
                Console.WriteLine("SetJsonContentFromSerializedString Exception: " + e.Message);
                throw( new Exception("SetJsonContentFromSerializedString Exception:" + e.Message));
            }

            //File.WriteAllLines(FILE_PATH + FILE_NAME, listDefer.ToArray());
        }
    }
}
