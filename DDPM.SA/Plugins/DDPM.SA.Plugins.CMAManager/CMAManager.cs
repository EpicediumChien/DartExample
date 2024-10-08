using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.CMAManager
{
    public class CMAManager
    {

        private List<string> commands = new List<string>();
        private CMAAgent agent;

        public CMAManager()
        {

        }

        public async void Info(String json)
        {
            commands = new List<string>();
            Guid uniqueAgentGuid = Guid.NewGuid();
            agent = new CMAAgent(uniqueAgentGuid, Guid.NewGuid());

            agent.CMAAgentEvent += OnCMAAgentEvent;

            CmaCommand cmd = new CmaCommand(uniqueAgentGuid.ToString(), json);

            //Console.WriteLine("@@Stephen Info Json Commands = " + json);
            //Console.WriteLine("@@Stephen cmd.sid : " + cmd.sid);
            //Console.WriteLine("@@Stephen cmd.gid : " + cmd.gid);
            //Console.WriteLine("cmd.req : " + cmd.req);
            //Console.WriteLine("@@Stephen cmd.req : ");


            List<CmaCommand.CmaTask> tasks = new List<CmaCommand.CmaTask>();
            foreach (var s in cmd.req)
            {
                Console.WriteLine(s.ToString());
                tasks.Add(new CmaCommand.CmaTask(cmd.sid, s.ToString()));
            }

            foreach (CmaCommand.CmaTask task in tasks)
            {

/*                Console.WriteLine($"@@Stephen task.id = {task.id}");
                Console.WriteLine($"@@Stephen task.active = {task.active}");
                Console.WriteLine($"@@Stephen task.devicetype = {task.devicetype}");
                Console.WriteLine($"@@Stephen task.command = {task.command}");
                Console.WriteLine($"@@Stephen task.sid = {task.sid}");*/

                string command = "";

                if ("get".Equals(task.active))
                {
                    command = command + ("get ");
                    command = command + (task.devicetype + "=" + task.command);
                }

                if ("set".Equals(task.active))
                {
                    command = command + ("set ");
                    command = command + (task.devicetype + "=" + task.command);
                }

                if ("fw".Equals(task.active))
                {
                    command = command + ("set ");
                    command = command + ("display=firmwareupdate");
                    if (!Params.DeviceType.DISPLAY.Equals(task.devicetype))
                    {
                        command = command + (" value=" + task.devicetype);
                    }
                }

                commands.Add(command);

                await agent.StartAsync(command.Split(' '), task.sid, task.id).ConfigureAwait(false);
            }
            /*            foreach (string command in commands)
                        {

                            await agent.StartAsync(command.Split(' '),).ConfigureAwait(false);
                            //new Thread(StartTask).start(command);
                        }*/
        }


        private void OnCMAAgentEvent(object sender, CMAAgentArgs e)
        {
            NotifyArgs args = new NotifyArgs();
            args.eventtype = e.ExitCode.ToString();
            args.notification = e.response;
            OnEventNotify(args);
        }


        private void OnEventNotify(NotifyArgs e)
        {
            EventHandler<NotifyArgs> Handler = Notify;
            if (Handler != null)
            {
                Handler.Invoke(this, e);
                //WriteLog($"CLIActionEvent Invoked: ID:{e.command_guid_string}");
            }
        }

        public event EventHandler<NotifyArgs> Notify;
    }

    public class NotifyArgs : EventArgs
    {
        public string eventtype;
        public string notification;
    }
}
