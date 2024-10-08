using Newtonsoft.Json;
using System.IO;

namespace DDPM.UI.Common
{
    public class PenActions
    {
        public SelectedAction TopButtonClickAction = new(73, new AssignedAction(73));
        public SelectedAction TopButtonDoubleClickAction = new(90, new AssignedAction(90));
        public SelectedAction TopButtonPressHoldAction = new(75, new AssignedAction(75));
        public SelectedAction TopBarrelButtonClickAction = new(27, new AssignedAction(27));
        public SelectedAction BottomBarrelButtonClickAction = new(26, new AssignedAction(26));
        public bool IsTopBarrelHoverClickOn = false;
        public bool IsBottomBarrelHoverClickOn = false;

        //public List<PenButtonName> Buttons = new();
        public Dictionary<int, string> RadialLabels = new();
        public Dictionary<int, SelectedAction> RadialActions = new();
        public bool IsUseCenter = true;

        public PenActions()
        {
        }

        public PenActions(string _model)
        {
            //var model = _model.ToUpper();
            //switch (model)
            //{
            //    case "PN7522W":
            //    case "PN9315A":
            //        Buttons.Add(PenButtonName.TopButton);
            //        Buttons.Add(PenButtonName.TopBarrelButton);
            //        Buttons.Add(PenButtonName.BottomBarrelButton);
            //        break;

            //    default:
            //        Buttons.Add(PenButtonName.TopBarrelButton);
            //        Buttons.Add(PenButtonName.BottomBarrelButton);
            //        break;
            //}
            ResetRadialMenu();
        }

    public void ResetRadialMenu() {
      RadialLabels = new() {
        { 1, Strings.VolumeUp },
        { 2, Strings.PlayPause },
        { 3, Strings.VolumeDown },
        { 4, Strings.PreviousTrack },
        { 5, Strings.EMail },
        { 6, Strings.Mute },
        { 7, Strings.WebBrowser },
        { 8, Strings.NextTrack },
      };
      RadialActions.Clear();
      RadialActions.Add(1, new SelectedAction(84, new AssignedAction(84)));
      RadialActions.Add(2, new SelectedAction(81, new AssignedAction(81)));
      RadialActions.Add(3, new SelectedAction(85, new AssignedAction(85)));
      RadialActions.Add(4, new SelectedAction(83, new AssignedAction(83)));
      RadialActions.Add(5, new SelectedAction(80, new AssignedAction(80)));
      RadialActions.Add(6, new SelectedAction(86, new AssignedAction(86)));
      RadialActions.Add(7, new SelectedAction(79, new AssignedAction(79)));
      RadialActions.Add(8, new SelectedAction(82, new AssignedAction(82)));
      IsUseCenter = true;
    }

        public class RadialLabel
        {
            public string Default = "";
            public string Customized = "";
        }
    }

    public class KeyboardActions
    {
        public Dictionary<KeyName, SelectedAction> KeyActions = new();

        public KeyboardActions()
        { }

        public KeyboardActions(string _model)
        {
            var model = _model.ToUpper();
            KeyActions.Add(KeyName.F1, new SelectedAction(39, new AssignedAction(39)));
            KeyActions.Add(KeyName.F2, new SelectedAction(38, new AssignedAction(38)));
            KeyActions.Add(KeyName.F3, new SelectedAction(40, new AssignedAction(40)));
            switch (model)
            {
                case "KB700":
                case "KB7221W":
                    KeyActions.Add(KeyName.F4, new SelectedAction(24, new AssignedAction(24)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F8, new SelectedAction(15, new AssignedAction(15)));
                    break;

                case "KB740":
                case "KB7120W":
                    KeyActions.Add(KeyName.F4, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(6, new AssignedAction(6)));
                    KeyActions.Add(KeyName.F8, new SelectedAction(24, new AssignedAction(24)));
                    break;

                case "KB555":
                    KeyActions.Add(KeyName.F4, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(5, new AssignedAction(5)));
                    KeyActions.Add(KeyName.F8, new SelectedAction(24, new AssignedAction(24)));
                    KeyActions.Add(KeyName.PrtSc, new SelectedAction(41, new AssignedAction(41)));
                    KeyActions.Add(KeyName.Calculator, new SelectedAction(16, new AssignedAction(16)));
                    KeyActions.Add(KeyName.Home, new SelectedAction(44, new AssignedAction(44)));
                    KeyActions.Add(KeyName.End, new SelectedAction(45, new AssignedAction(45)));
                    KeyActions.Add(KeyName.PgUp, new SelectedAction(46, new AssignedAction(46)));
                    KeyActions.Add(KeyName.PgDown, new SelectedAction(47, new AssignedAction(47)));
                    break;

                case "KB900":
                    KeyActions.Add(KeyName.F4, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(5, new AssignedAction(5)));
                    KeyActions.Add(KeyName.PrtSc, new SelectedAction(41, new AssignedAction(41)));
                    KeyActions.Add(KeyName.ScrollLock, new SelectedAction(42, new AssignedAction(42)));
                    KeyActions.Add(KeyName.PauseBreak, new SelectedAction(43, new AssignedAction(43)));
                    KeyActions.Add(KeyName.Calculator, new SelectedAction(16, new AssignedAction(16)));
                    break;

                case "KB525C":
                    KeyActions.Add(KeyName.F4, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(5, new AssignedAction(5)));
                    KeyActions.Add(KeyName.F8, new SelectedAction(16, new AssignedAction(16)));
                    KeyActions.Add(KeyName.PrtSc, new SelectedAction(41, new AssignedAction(41)));
                    KeyActions.Add(KeyName.ScrollLock, new SelectedAction(42, new AssignedAction(42)));
                    KeyActions.Add(KeyName.PauseBreak, new SelectedAction(43, new AssignedAction(43)));
                    break;

                case "KB500":
                case "KB3121W":
                    KeyActions.Add(KeyName.F4, new SelectedAction(-1, new AssignedAction(-1)));
                    KeyActions.Add(KeyName.F5, new SelectedAction(35, new AssignedAction(35)));
                    KeyActions.Add(KeyName.F6, new SelectedAction(34, new AssignedAction(34)));
                    KeyActions.Add(KeyName.F7, new SelectedAction(33, new AssignedAction(33)));
                    KeyActions.Add(KeyName.F8, new SelectedAction(-1, new AssignedAction(-1)));
                    break;

                default:
                    break;
            }
            KeyActions.Add(KeyName.F9, new SelectedAction(3, new AssignedAction(3)));

            if (model == "KB740" || model == "KB7120W")
            {
                KeyActions.Add(KeyName.F10, new SelectedAction(41, new AssignedAction(41)));
                KeyActions.Add(KeyName.F11, new SelectedAction(44, new AssignedAction(44)));
                KeyActions.Add(KeyName.F12, new SelectedAction(45, new AssignedAction(45)));
            }
            else
            {
                KeyActions.Add(KeyName.F10, new SelectedAction(13, new AssignedAction(13)));
                KeyActions.Add(KeyName.F11, new SelectedAction(8, new AssignedAction(8)));
                KeyActions.Add(KeyName.F12, new SelectedAction(6, new AssignedAction(6)));
            }
        }
    }

    public class MouseActions
    {
        public Dictionary<MouseButtonName, SelectedMouseAction> ButtonActions = new();

        public MouseActions()
        { }

        public MouseActions(string _model)
        {
            var model = _model.ToUpper();
            switch (model)
            {
                case "MS5120W":
                case "MS5320W":
                case "MS7421W":
                    ButtonActions.Add(MouseButtonName.ScrollWheelClick, new SelectedMouseAction(-1, new AssignedAction(-1)));
                    ButtonActions.Add(MouseButtonName.ScrollTiltLeft, new SelectedMouseAction(-1, new AssignedAction(-1)));
                    ButtonActions.Add(MouseButtonName.ScrollTiltRight, new SelectedMouseAction(-1, new AssignedAction(-1)));
                    ButtonActions.Add(MouseButtonName.SideButtonForward, new SelectedMouseAction(21, new AssignedAction(21)));
                    ButtonActions.Add(MouseButtonName.SideButtonBack, new SelectedMouseAction(15, new AssignedAction(15)));
                    break;

                case "MS3220":
                case "MS3220T":
                case "MS900":
                    ButtonActions.Add(MouseButtonName.ScrollWheelClick, new SelectedMouseAction(-1, new AssignedAction(-1)));
                    ButtonActions.Add(MouseButtonName.SideButtonForward, new SelectedMouseAction(21, new AssignedAction(21)));
                    ButtonActions.Add(MouseButtonName.SideButtonBack, new SelectedMouseAction(15, new AssignedAction(15)));
                    break;

                case "MS300":
                case "MS355":
                case "MS3320W":
                    ButtonActions.Add(MouseButtonName.ScrollWheelClick, new SelectedMouseAction(-1, new AssignedAction(-1)));
                    break;

                default:
                    break;
            }
        }
    }

    public class SelectedAction
    {
        public int DefaultActionID = -1;
        public AssignedAction AssignedAction = new();

        public SelectedAction()
        { }

        public SelectedAction(int defaultActionID, AssignedAction assignedAction)
        {
            DefaultActionID = defaultActionID;
            AssignedAction = assignedAction;
        }
    }

    public class SelectedMouseAction
    {
        public int DefaultActionID = -1;
        public AssignedAction AssignedAction = new();
        public Dictionary<string, int> OfficeActions = new() { { "Word", -1 }, { "Excel", -1 }, { "PowerPoint", -1 }, { "Outlook", -1 } };

        public SelectedMouseAction()
        { }

        public SelectedMouseAction(int defaultActionID, AssignedAction assignedAction)
        {
            DefaultActionID = defaultActionID;
            AssignedAction = assignedAction;
        }
    }

    public class AssignedAction
    {
        public int ID = -1;
        public string Parameter = "";

        public AssignedAction()
        { }

        public AssignedAction(int id, string parameter = "")
        {
            ID = id;
            Parameter = parameter;
        }
    }

    public static class ActionList
    {
        public static bool ExportActionList(object actions, string model, int instanceID = 0)
        {
            try
            {
                string json = JsonConvert.SerializeObject(actions, Formatting.Indented);                
                var fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions");

                string info = string.Empty;
                DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(fileFolder, out info);   // 20241004 Add for Security
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                string strPath = Path.Combine(fileFolder, $"{model}.json");
                //File.WriteAllText(strPath, json);
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    return DdpmCommonHelper.DeviceManagerSA.WriteSerializedContentToFile(strPath, json).Result;//1007 apply signature
                }
                //return true;
            }
            catch (Exception)
            {
                //return false;
            }
            return false;
        }

        public static object ImportActionList(eDeviceCategory type, string model, int instanceID = 0)
        {
            //var filePath = Path.Combine(Application.StartupPath, @$"ActionList\{model}_{instanceID}.json");
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
            var hasFile = File.Exists(filePath);
            string info = string.Empty;
            string jsonString = string.Empty;
            switch (type)
            {
                case eDeviceCategory.KB:
                    if (hasFile)
                    {                       
                        DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if(DdpmCommonHelper.DeviceManagerSA != null)
                        {
                            jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                        }
                        if(!string.IsNullOrEmpty(jsonString))
                            return JsonConvert.DeserializeObject<KeyboardActions>(jsonString);// File.ReadAllText(filePath))!;
                    }
                    var ka = new KeyboardActions(model);
                    ExportActionList(ka, model);
                    return ka;

                case eDeviceCategory.Mouse:
                    if (hasFile)
                    {
                        DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if (DdpmCommonHelper.DeviceManagerSA != null)
                        {
                            jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                        }
                        if (!string.IsNullOrEmpty(jsonString))
                            return JsonConvert.DeserializeObject<MouseActions>(jsonString); //File.ReadAllText(filePath))!;
                    }
                    var ma = new MouseActions(model);
                    ExportActionList(ma, model);
                    return ma;
                    
                case eDeviceCategory.Pen:
                    if (hasFile)
                    {
                        DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if (DdpmCommonHelper.DeviceManagerSA != null)
                        {
                            jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                        }
                        if (!string.IsNullOrEmpty(jsonString)) 
                            return JsonConvert.DeserializeObject<PenActions>(jsonString); //File.ReadAllText(filePath))!;
                    }
                    var pen = new PenActions(model);
                    ExportActionList(pen, model);
                    return pen;

                default:
                    break;
            }
            return new object();
        }
    }
}