using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DDPM.SA.Common.Defer
{
    public static class DeferControlPanel
    {
        private const string FILE_PATH = @"c:\cmacmd\defer\";
        private const string FILE_NAME = @"defer_data.txt";

        public const int SRC_FROM_CLI = 0;
        public const int SRC_FROM_CMA = 1;

        //public const int MAX_COUNT = 3;


        private static List<string> listDefer = new List<string>();

        /*private static bool isToastClose = false;
        private static bool isDefer = false;*/

        /*        public DeferControlPanel() {
                    listDefer = new List<string>();
                }*/
        //private static IDeviceManagerSA _DevManagerPlugin;

        public static void init()
        {
            listDefer = new List<string>();
            loadFile();
        }

        public static List<string> getList()
        {
            return listDefer;
        }
        public static void removeItems(int count)
        {
            listDefer.RemoveRange(0, count);
            writeToFile();
        }

        public static void removeItem(string item)
        {
            listDefer.Remove(item);
            writeToFile();
        }

        public static void addToSchedule(DeferItem item)
        {
            listDefer.Add(item.ToString());
            writeToFile();
        }

        public static void addToSchedule(string item)
        {
            listDefer.Add(item);
            writeToFile();
        }

        private static void loadFile()
        {
            try
            {
                string[] alllines = File.ReadAllLines(FILE_PATH + FILE_NAME);
                listDefer = alllines.ToList();
            }
            catch (Exception e)
            {
                // exception
                listDefer = new List<string>();
            }

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

            File.WriteAllLines(FILE_PATH + FILE_NAME, listDefer.ToArray());
        }
    }
}
