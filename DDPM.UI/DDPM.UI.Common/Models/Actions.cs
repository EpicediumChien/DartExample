using Newtonsoft.Json;
using System.Buffers;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DDPM.UI.Common
{
    public class PenActions
    {
        private const string ItemID = "DellPeripheral.Pen.0";

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

            Task<string> task1 = DdpmCommonHelper.DeviceManagerSA!.GetEraserDoublePressSetting();
            JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            TopButtonDoubleClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            if (TopButtonDoubleClickAction.AssignedAction.ID == 65)
            {
                TopButtonDoubleClickAction.AssignedAction.ID = 64;
            }
            else if (TopButtonDoubleClickAction.AssignedAction.ID == 23)
            {
                TopButtonDoubleClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
            }

            task1 = DdpmCommonHelper.DeviceManagerSA!.GetEraserSinglePressSetting();
            jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            TopButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            if (TopButtonClickAction.AssignedAction.ID == 23)
            {
                TopButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
            }

            task1 = DdpmCommonHelper.DeviceManagerSA!.GetEraserLongPressSetting();
            jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            TopButtonPressHoldAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            if (TopButtonPressHoldAction.AssignedAction.ID == 77)
            {
                TopButtonPressHoldAction.AssignedAction.ID = 64;
            }
            else if (TopButtonPressHoldAction.AssignedAction.ID == 23)
            {
                TopButtonPressHoldAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
            }

            task1 = DdpmCommonHelper.DeviceManagerSA!.GetSideTopSwitchSinglePressSetting();
            jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            TopBarrelButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            if (TopBarrelButtonClickAction.AssignedAction.ID == 23)
            {
                TopBarrelButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
            }

            task1 = DdpmCommonHelper.DeviceManagerSA!.GetSideBottomSwitchSinglePressSetting();
            jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            BottomBarrelButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            if (BottomBarrelButtonClickAction.AssignedAction.ID == 23)
            {
                BottomBarrelButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
            }

            task1 = DdpmCommonHelper.DeviceManagerSA!.GetMenuSinglePressSetting();
            jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
            //BottomBarrelButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
            //foreach (var jo in jsonObject.EnumerateArray())
            //{
            //    //_EraserActions.Add(jo.GetProperty("actionId").GetInt32(), jo.GetProperty("actionName").GetString()!);
            //}

            Task<bool> task2 = DdpmCommonHelper.DeviceManagerSA!.GetMenuCenterRightClickSetting();
            IsUseCenter = task2.Result;
            task2 = DdpmCommonHelper.DeviceManagerSA!.GetIsSideTopButtonHoverClick();
            IsTopBarrelHoverClickOn = task2.Result;
            task2 = DdpmCommonHelper.DeviceManagerSA!.GetIsSideBottomButtonHoverClick();
            IsBottomBarrelHoverClickOn = task2.Result;

            ResetRadialMenu();
        }

        public void RestoreToDefault()
        {
            TopButtonClickAction = new(73, new AssignedAction(73));
            TopButtonDoubleClickAction = new(90, new AssignedAction(90));
            TopButtonPressHoldAction = new(75, new AssignedAction(75));
            TopBarrelButtonClickAction = new(27, new AssignedAction(27));
            BottomBarrelButtonClickAction = new(26, new AssignedAction(26));

            byte[] newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":73,\"actionName\":\"\"}}");
            DdpmCommonHelper.DeviceManagerSA!.SetEraserSinglePressSetting(ItemID, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":90,\"actionName\":\"\"}}");
            DdpmCommonHelper.DeviceManagerSA!.SetEraserDoublePressSetting(ItemID, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":75,\"actionName\":\"\"}}");
            DdpmCommonHelper.DeviceManagerSA!.SetEraserLongPressSetting(ItemID, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":27,\"actionName\":\"\"}}");
            DdpmCommonHelper.DeviceManagerSA!.SetSideTopSwitchSinglePressSetting(ItemID, newValue);
            newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":26,\"actionName\":\"\"}}");
            DdpmCommonHelper.DeviceManagerSA!.SetSideBottomSwitchSinglePressSetting(ItemID, newValue);

            ResetRadialMenu();
        }

        public void ResetRadialMenu()
        {
            RadialLabels = new() {
                { 0, Strings.NextTrack },
                { 1, Strings.WebBrowser },
                { 2, Strings.Mute },
                { 3, Strings.EMail },
                { 4, Strings.PreviousTrack },
                { 5, Strings.VolumeDown },
                { 6, Strings.PlayPause },
                { 7, Strings.VolumeUp },
            };
            RadialActions.Clear();
            RadialActions.Add(0, new SelectedAction(82, new AssignedAction(82)));
            RadialActions.Add(1, new SelectedAction(79, new AssignedAction(79)));
            RadialActions.Add(2, new SelectedAction(86, new AssignedAction(86)));
            RadialActions.Add(3, new SelectedAction(80, new AssignedAction(80)));
            RadialActions.Add(4, new SelectedAction(83, new AssignedAction(83)));
            RadialActions.Add(5, new SelectedAction(85, new AssignedAction(85)));
            RadialActions.Add(6, new SelectedAction(81, new AssignedAction(81)));
            RadialActions.Add(7, new SelectedAction(84, new AssignedAction(84)));
            IsUseCenter = true;
            foreach (var action in RadialActions)
            {
                byte[] newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":{action.Key},\"actionId\":{action.Value.DefaultActionID},\"actionName\":\"{RadialLabels[action.Key]}\"}}");
                DdpmCommonHelper.DeviceManagerSA!.SetMenuSinglePressSetting("DellPeripheral.Pen.0", newValue);
            }
            ActionList.ExportActionList(this, "PEN");
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

        public MouseActions(string _model, string guid = "")
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

        public static object ImportActionList(eDeviceCategory type, string model, string guid = "")
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
                        if (DdpmCommonHelper.DeviceManagerSA != null)
                        {
                            jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                        }
                        if (!string.IsNullOrEmpty(jsonString))
                            return JsonConvert.DeserializeObject<KeyboardActions>(jsonString)!;// File.ReadAllText(filePath))!;
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
                            return JsonConvert.DeserializeObject<MouseActions>(jsonString)!; //File.ReadAllText(filePath))!;
                    }
                    var ma = new MouseActions(model, guid);
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
                            return JsonConvert.DeserializeObject<PenActions>(jsonString)!; //File.ReadAllText(filePath))!;
                    }
                    //var pen = new PenActions(model);
                    var pen = new PenActions();
                    ExportActionList(pen, model);
                    return pen;

                default:
                    break;
            }
            return new object();
        }
    }
}