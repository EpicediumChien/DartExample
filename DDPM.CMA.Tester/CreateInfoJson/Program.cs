using DDPM.RemoteManagement.Common.Interfaces;

namespace CreateInfoJson
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // InfoJsonGenerator

            List<string> list = new List<string>();

            // create task 1
            InfoJsonGenerator.TaskJson task1 = new InfoJsonGenerator.TaskJson()
            {
                tid = 1,
                active = Params.Active.GET,
                devicetype = Params.DeviceType.APP,
                command = Params.App.DiagnosticsReport,
                value = @"c:\temp",
                options = new InfoJsonGenerator.TaskJson.Options()
                {
                    index = "1",
                    servicetag = "aaaaa",
                    model = "dell ea",
                    minversion = "1.0.0.5",
                    upgradetolatest = true
                }.toString()
            };

            // add task 1
            list.Add("{" + task1.ToString() + "}");

            // create task 2
            InfoJsonGenerator.TaskJson task2 = new InfoJsonGenerator.TaskJson()
            {
                tid = 2,
                active = Params.Active.GET,
                devicetype = Params.DeviceType.DISPLAY,
                command = Params.App.ConnectedDevices,
                value = String.Empty,
                options = new InfoJsonGenerator.TaskJson.Options().toString()
            };

            // add task 2
            list.Add("{" + task2.ToString() + "}");

            // Generator Info Json String
            InfoJsonGenerator obj = new InfoJsonGenerator($"{DateTimeOffset.Now.ToUnixTimeSeconds()}", list);


            Console.WriteLine($"InfoJsonGenerator.TaskJson task 1 = {task1.ToString()}");
            Console.WriteLine($"InfoJsonGenerator.TaskJson task 2 = {task2.ToString()}");
            Console.WriteLine($"InfoJsonGenerator = {obj.ToString()}");
        }
    }
}
