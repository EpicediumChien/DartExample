using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public static List<string> displayConnected(List<MonitorInfo> list)
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

            //Thread.Sleep(1000);
        }

        private static void writeToFile()
        {

            try
            {
                if (!(System.IO.Directory.Exists(FILE_PATH)))
                {
                    System.IO.Directory.CreateDirectory(FILE_PATH);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDirectory Exception: " + e.Message);
            }

            string info = string.Empty;
            bool write = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(listFwJob).ToString(), (FILE_PATH + FILE_NAME), out info);
            Console.WriteLine(write);

            //File.WriteAllLines(FILE_PATH + FILE_NAME, listDefer.ToArray());
        }
    }
}
