using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.QAM
{
    public class EventMsg
    {
        public string? DeviceType {  get; set; }
        public string? EventType { get; set; }
        public string? DeviceId { get; set; }
        public string? NewValue { get; set; }

        public static EventMsg? CreateEventMsgFromEventMsg(string eventMsg)
        {
            if (eventMsg == null || eventMsg.Length == 0)
                return null;

            string[] msgs = eventMsg!.Split(';');

            if (5 == msgs.Length)
            {
                EventMsg result = new EventMsg();

                string[] subMsg = msgs[1].Split(':');
                if (2 == subMsg.Length)
                    result.DeviceType = subMsg[1];
                else
                    return null;

                subMsg = msgs[2].Split(':');
                if (2 == subMsg.Length)
                    result.EventType = subMsg[1];
                else
                    return null;

                subMsg = msgs[3].Split(':');
                if (2 == subMsg.Length)
                    result.DeviceId = subMsg[1];
                else
                    return null;

                subMsg = msgs[4].Split(':');
                if (2 == subMsg.Length)
                    result.NewValue = subMsg[1];
                else
                    return null;

                return result;
            }
            else
                return null;
        }
    }
}
