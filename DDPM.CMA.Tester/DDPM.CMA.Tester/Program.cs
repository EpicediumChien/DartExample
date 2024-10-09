using DDPM.SA.Plugins.CMAManager;
using System;
using System.Threading.Tasks;

namespace DDPM.CMA.Tester
{
    internal class Program
    {
        /* private static async Task Main(string[] args)
         {
             *//*
              * Only allow this application in debug mode
              *//*
             //#if DEBUG
             CMAAgent agent = new CMAAgent(Guid.NewGuid(), Guid.NewGuid());
             await agent.StartAsync(args).ConfigureAwait(false);

             Environment.Exit(agent.GetExitCode());
         }*/

        private static void Main(string[] args)
        {

            Console.WriteLine("CMA SubAgent");
            // CMA call DDPM
            //string json = @"{""sid"":""1727362336"",""req"":[{""id"":1,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ActiveHours"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}},{""id"":2,""active"":""get"",""devicetype"":""DOCK"",""command"":""FWVersion"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}},{""id"":3,""active"":""get"",""devicetype"":""APP"",""command"":""ConnectedDevices"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";
            string json = @"{""sid"":""1727362336"",""req"":[{""id"":1,""active"":""get"",""devicetype"":""DISPLAY"",""command"":""ActiveHours"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";
            string jsonfwdisplay = @"{""sid"":""1728273741"",""req"":[{""id"":1,""active"":""fw"",""devicetype"":""DISPLAY"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";
            string jsonfwdock = @"{""sid"":""1728273741"",""req"":[{""id"":1,""active"":""fw"",""devicetype"":""DOCK"",""options"":{""index"":""1"",""uod"":true,""updatesilent"":""""}}]}";

            string jsondevice = @"{""sid"":""1728380239"",""req"":[{""id"":1,""active"":""get"",""devicetype"":""APP"",""command"":""ConnectedDevices"",""options"":{}}]}";

            CMAManager manager = new CMAManager();

            manager.Notify += Notification;

            // manager.Info(json);

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "display":
                        manager.Info(jsonfwdisplay);
                        break;

                    case "dock":
                        manager.Info(jsonfwdock);
                        break;

                    default:
                        manager.Info(json);
                        break;
                }
            }
            else {
                manager.Info(jsondevice);
            }

        }

        private static void Notification(object sender, NotifyArgs e)
        {
            Console.WriteLine("CMA Notification Alert");
            Console.WriteLine("CMA Notification Alert eventtype : " + e.eventtype);
            Console.WriteLine("CMA Notification Alert notification : " + e.notification);

        }
    }
}