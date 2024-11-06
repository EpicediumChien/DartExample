using DDPM.RemoteManagement.Common.Interfaces;

namespace TransferCmdJson
{
    internal class Program
    {

        private static string txtOutput = string.Empty;
        private static int tCount = 1;
        static void Main(string[] args)
        {
            Console.WriteLine("CMA Command Transfer!");
            readFile();
        }

        private static string cmdParser(string src)
        {

            string strJson = string.Empty;
            string[] cmds = src.Split(' ');
            string active = string.Empty;
            string devicetype = string.Empty;
            string command = string.Empty;
            string value = string.Empty;
            string servicetag = string.Empty;
            string minversion = string.Empty;
            string model = string.Empty;

            for (int i = 0; i < cmds.Length; i++)
            {

                if (0 == i)
                {
                    active = cmds[i];
                    continue;
                }

                if (1 == i)
                {

                    string[] s1 = cmds[i].Split('=');

                    devicetype = s1[0];
                    command = s1[1];
                    continue;

                }

                if (2 == i)
                {

                    string[] s2 = cmds[i].ToLower().Split('=');

                    if (s2[1].Contains("display") || s2[1].Contains("dock")
                        || s2[1].Contains("webcam") || s2[1].Contains("audio")
                        || s2[1].Contains("keyboard") || s2[1].Contains("mouse"))
                    {

                        if (s2[1].Contains(","))
                        {

                            string[] s3 = s2[1].Split(',');

                            devicetype = s3[0];
                            value = s3[1];
                            continue;

                        }

                        devicetype = s2[1];
                        continue;

                    }

                    value = s2[1];
                    continue;

                }

                // others, options

                string[] s = cmds[i].ToLower().Split('=');

                if (s[0].Equals("value"))
                {

                    string[] ss = s[1].Split(',');

                    if ("servicetag".Equals(ss[1]))
                    {
                        servicetag = ss[0];
                    }

                    if ("miniversion".Equals(ss[1]))
                    {
                        minversion = ss[0];
                    }

                    if ("minversion".Equals(ss[1]))
                    {
                        minversion = ss[0];
                    }

                    if ("model".Equals(ss[1]))
                    {
                        model = ss[0];
                    }

                }

            }

            InfoJsonGenerator.TaskJson task = new InfoJsonGenerator.TaskJson()
            {
                tid = tCount,
                active = active,
                devicetype = devicetype,
                command = command,
                value = value,
                options = new InfoJsonGenerator.TaskJson.Options()
                {
                    servicetag = servicetag,
                    model = model,
                    minversion = minversion
                }.toString()
            };

            return task.ToString();

        }

        private static void readFile()
        {
            const string PATH = @"C:\temp\cmd\";

            List<string> list = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(PATH);
            FileInfo[] files = dir.GetFiles("*");

            foreach (FileInfo file in files)
            {
                string[] alllines = File.ReadAllLines(file.FullName);

                for (int i = 0; i < alllines.Length; i++)
                {
                    list = new List<string>();

                    string strTask = cmdParser(alllines[i].Trim());

                    list.Add("{" + strTask + "}");

                    // Generator Info Json String
                    InfoJsonGenerator obj = new InfoJsonGenerator($"{DateTimeOffset.Now.ToUnixTimeSeconds()}", list);
                    txtOutput = txtOutput + obj.ToString() + "\n";
                }

                Console.WriteLine(txtOutput);

                // write files in the folder

                if (!(System.IO.Directory.Exists(PATH + "out")))
                {
                    System.IO.Directory.CreateDirectory(PATH + "out");
                }

                File.WriteAllLines(@$"{PATH}out\out_" + file.Name, txtOutput.Split('\n'));

            }

        }
    }
}
