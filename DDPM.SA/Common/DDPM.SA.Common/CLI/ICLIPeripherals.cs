using System.Collections.Generic;
using System.Linq;

namespace DDPM.SA.Common
{
    public interface ICLIPeripherals : ICLIBase
    {
        //Write your own function here
    }

    public class CLI_PeripheralRESPONSE
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Guid { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; } = "";
        public string Result { get; set; } = "";
        public string Message { get; set; } = "";

        public CLI_PeripheralRESPONSE(string id, string command, string targetFeature, string result = "", string message = "", string name = "", string model = "")
        {
            Guid = id;
            Command = command;
            TargetFeature = targetFeature;
            Result = result;
            Message = message;
            Name = name;
            Model = model;
        }

        public CLI_PeripheralRESPONSE(DeviceInfo di, string deviceType, string targetFeature)
        {
            Name = di.Name;
            Model = di.ModelNumber;
            Guid = di.ID.ToString();
            Command = "GET";
            TargetFeature = targetFeature;

            var properties = Property.DeviceProperties[deviceType];

            if (properties.Contains(targetFeature))
            {
                Result = "PASS";
                Message = "N/A";
                var type = di.GetType();
                foreach (var prop in type.GetProperties())
                {
                    if (prop.Name.ToUpper() == targetFeature)
                    {
                        Value = prop.GetValue(di)?.ToString() ?? "";
                    }
                }
            }
            else
            {
                Result = "FAIL";
                Message = "invalid TargetFeature";
            }
        }
    }

    public class CLI_PeripheralGetRESPONSE
    {
        public string Guid;
        public string Command = "GET";
        public string Result = "PASS";
        public string Message = "N/A";
        public string Name;
        public string Model;
        public string LogicalDeviceType;
        public Dictionary<string, string> Properties;

        public CLI_PeripheralGetRESPONSE(string guid, string message)
        {
            Guid = guid;
            Result = "FAIL";
            Message = message;
        }

        public CLI_PeripheralGetRESPONSE(DeviceInfo di)
        {
            Guid = di.ID.ToString();
            Name = di.Name;
            Model = di.ModelNumber;
            LogicalDeviceType = di.LogicalDeviceType.ToString();
            var properties = Property.DeviceProperties[di.LogicalDeviceType];
            Properties = new();
            var type = di.GetType();
            foreach (var prop in type.GetProperties().OrderBy(x => x.Name))
            {
                if (properties.Contains(prop.Name.ToUpper()))
                {
                    Properties.Add(prop.Name, prop.GetValue(di)?.ToString() ?? "");
                }
            }
        }

        //internal static List<string> KeyboardProperties;//Dean 0626 SAST issue
    }

    internal static class Property
    {
        internal static readonly List<string> Keyboard = new()//Dean 0626 SAST issue
        {
            "BACKLIGHTINGCONTROLS",
            "BACKLIGHTINGLEVEL",
            "BACKLIGHTTABINDEX",
            "BATTERYLEVEL",
            "BATTERYSTATUS",
            "COLLABSKEYSSUPPORTED",
            "COLORCODE",
            "FIRMWAREVERSION",
            "INSTANCEID",
            "ISBATTERYLEVELSUPPORTED",
            "ISCOLLABORATIONBLINKEFFECTENABLE",
            "ISCOLLABORATIONCAMERAENABLE",
            "ISCOLLABORATIONCHATENABLE",
            "ISCOLLABORATIONDOUBLETAPENABLE",
            "ISCOLLABORATIONKEYENABLE",
            "ISCOLLABORATIONMICENABLE",
            "ISCOLLABORATIONSCREENSHAREENABLE",
            "ISCOLLABSKEYSSUPPORTED",
            "ISILLUMINATIONSUPPORTED",
            "PAIREDDEVICECOUNT",
            "PAIREDHOSTNAME1",
            "PAIREDHOSTNAME2",
            "PAIREDHOSTNAME3",
            "VISIBLEPAIREDHOSTNAME1",
            "VISIBLEPAIREDHOSTNAME2",
            "VISIBLEPAIREDHOSTNAME3"
        };

        internal static readonly List<string> Mouse = new()//Dean 0626 SAST issue
        {
            "BATTERYLEVEL",
            "BATTERYSTATUS",
            "COLORCODE",
            "DPIDELTA",
            "DPILEVEL",
            "DPIVALUE",
            "DPIMAX",
            "DPIMIN",
            "FIRMWAREVERSION",
            "INSTANCEID",
            "ISBATTERYLEVELSUPPORTED",
            "ISDPILEVELCHANGEPENDING",
            "ISDPILEVELSUPPORTED",
            "ISDPIVALUECHANGEPENDING",
            "ISDPIVALUESUPPORTED",
            "ISREPORTRATESUPPORTED",
            "ISTOUCHSCROLLSENSITIVITYSUPPORTED",
            "MOUSEPRIMARYBUTTON",
            "PAIREDDEVICECOUNT",
            "PAIREDHOSTNAME1",
            "PAIREDHOSTNAME2",
            "PAIREDHOSTNAME3",
            "REPORTRATE",
            "STATUS",
            "TOUCHSCROLLSENSITIVITYLEVEL",
            "TOUCHSENSITIVITYLEVELVALUE",
            "VISIBLEPAIREDHOSTNAME1",
            "VISIBLEPAIREDHOSTNAME2",
            "VISIBLEPAIREDHOSTNAME3"
        };

        internal static readonly List<string> Webcam = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            "INSTANCEID",
            "ISBATTERYLEVELSUPPORTED",
            "",
            "",
            "",
        };

        internal static readonly List<string> Pen = new()
        {
            "FIRMWAREVERSION",
            "INSTANCEID",
            "",
        };

        internal static readonly List<string> Headset = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            "INSTANCEID",
            "MUTESTATUS",
            "",
            "",
        };

        internal static readonly List<string> Audio = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            "INSTANCEID",
            "MUTESTATUS",
            "",
            "",
            "",
        };

        internal static readonly List<string> Dock = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            "INSTANCEID",
            "",
        };

        internal static readonly Dictionary<string, List<string>> DeviceProperties = new()//Dean 0626 SAST issue
        {
            { "KEYBOARD", Keyboard },
            { "MOUSE", Mouse },
            { "WEBCAM", Webcam },
            { "PEN", Pen },
            { "HEADSET", Headset },
            { "AUDIO", Audio },
            { "DOCK", Dock }
        };
    }
}