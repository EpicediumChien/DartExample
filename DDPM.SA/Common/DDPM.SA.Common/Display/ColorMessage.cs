using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;



namespace DDPM.SA.Common
{

    public interface IPlugin
    {
        public enum UserInterfaceType
        {
            LauncherUI = 1,
            MainUI = 2
        }
        public enum ThumbType
        {
            LauncherIcon = 0x1,
            MainIcon = 0x2,
            LauncherIconDisalbe = 0x3,
            MainIconDisalbe = 0x4
        }

        public enum ViewType
        {
            Normal = 0x01,
            FullPage = 0x02
        }

        public enum SupportViewPage
        {
            Brightness = 0,
            InputSource = 1,
            Color = 2,
            Gaming = 3,
            EasyArrangement = 4,
            Personalize = 5,
            Others = 6,
            SwKvm = 7
        }


        object GetView<T>(object monitorInfo);//(int instance);

#if true
        object LoadUserContrlMainView<T>(object monitorInfo);
#else
        public object LoadUserContrlMainView(DDMCommonInterface.Common.MonitorInfo monitorInfo);
#endif
        object GetFullViewPage<T>(object monitorInfo);//(int instance);


        //New string for Tooltips message
        string TooltipsMessage(UserInterfaceType type);
        IMessaging LoadMessaging(object instance);

        void DisplayRefeshMessage<T>(object monitorInfo);

    }

    //public delegate void OnSendMessage(object sender, DDMiMessagingMsg message);
    public interface IMessaging
    {
        IPlugin plugin { get; }

        object Instance { get; set; } // Common.MonitorInfo

        event EventHandler<DDMiMessagingMsg> SendMessage;

        void MessageRecived(DDMiMessagingMsg message);
    }


    public class Plugin_Messaging : IMessaging
    {
        IPlugin _plugin;
        private object _Instance;
        public object Instance
        {
            get { return _Instance; }
            set { _Instance = value; }
        }

        public IPlugin plugin { get => _plugin; }

        public Plugin_Messaging(IPlugin plugin, object instance)
        {
            _Instance = instance;
            _plugin = (IPlugin)plugin;
        }

        public event EventHandler ReceiveMessage;
        public event EventHandler<DDMiMessagingMsg> SendMessage;

        public void MessageRecived(DDMiMessagingMsg message)
        {
            Trace.WriteLine($"Receive message.{message}");
            if (ReceiveMessage != null)
            {
                ReceiveMessage(message, null);
            }
        }

        public void FireSendMessage(DDMiMessagingMsg message)
        {
            Trace.WriteLine($"[FireSendMessage] message {message}. [Instance:{_Instance}]");
            SendMessage((IMessaging)this, message);
        }
    }

    public class DDMiMessagingMsg
    {
        public string OpType;
        public string message;

        public DDMiMessagingMsg(string optype, string msg)
        {
            this.OpType = optype;
            this.message = msg;
        }
    }

    public class SendNameMsg : DDMiMessagingMsg
    {
        public string sender;

        public SendNameMsg(string _sender, string _op, string _message) : base(_op, _message)
        {
            sender = _sender;
        }
    }

    public class PipeNameMsg : SendNameMsg
    {
        //public int MonitorInfoIndex;
        public EDID MonitorEDID;

        public PipeNameMsg(string _sender, string _op, string _message, EDID _edid) : base(_sender, _op, _message)
        {
            MonitorEDID = _edid;
        }
    }

    public class DDMBorkerMsg : SendNameMsg
    {
        public EDID DeviceInfo;

        public DDMBorkerMsg(string _sender, string _op, string _message, EDID _devInfo) : base(_sender, _op, _message)
        {
            DeviceInfo = _devInfo;
        }

        public override string ToString()
        {
            return $"Sender:{sender}, OP:{OpType}, Msg:{message}, DevInfo:{DeviceInfo.ToString()}";
        }
    }
}
