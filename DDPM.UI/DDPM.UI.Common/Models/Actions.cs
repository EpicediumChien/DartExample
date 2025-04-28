using Newtonsoft.Json;
using System.Buffers;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json.Linq;
using static DDPM.UI.Common.PenActions;
using DDPM.SA.Common;

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

        public PenActions() { }
        public PenActions(bool hasFile)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;

            try
            {
                Task<string> task1 = DdpmCommonHelper.DeviceManagerSA.GetEraserDoublePressSetting();
                string jsonResult = task1.Result;
                if (string.IsNullOrEmpty(jsonResult))
                {
                    DdpmCommonHelper.WriteUILog($"[Actions] PenActions cannot get json data from DTP!!");
                    return;
                }
                JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonResult)!;
                TopButtonDoubleClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
                if (TopButtonDoubleClickAction.AssignedAction.ID == 65)
                {
                    TopButtonDoubleClickAction.AssignedAction.ID = 64;
                }
                else if (TopButtonDoubleClickAction.AssignedAction.ID == 8 || TopButtonDoubleClickAction.AssignedAction.ID == 23)
                {
                    TopButtonDoubleClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
                }

                task1 = DdpmCommonHelper.DeviceManagerSA.GetEraserSinglePressSetting();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
                TopButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
                if (TopButtonClickAction.AssignedAction.ID == 8 || TopButtonClickAction.AssignedAction.ID == 23)
                {
                    TopButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
                }

                task1 = DdpmCommonHelper.DeviceManagerSA.GetEraserLongPressSetting();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
                TopButtonPressHoldAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
                if (TopButtonPressHoldAction.AssignedAction.ID == 77)
                {
                    TopButtonPressHoldAction.AssignedAction.ID = 64;
                }
                else if (TopButtonPressHoldAction.AssignedAction.ID == 8 || TopButtonPressHoldAction.AssignedAction.ID == 23)
                {
                    TopButtonPressHoldAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
                }

                task1 = DdpmCommonHelper.DeviceManagerSA.GetSideTopSwitchSinglePressSetting();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
                TopBarrelButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
                if (TopBarrelButtonClickAction.AssignedAction.ID == 8 || TopBarrelButtonClickAction.AssignedAction.ID == 23)
                {
                    TopBarrelButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
                }

                task1 = DdpmCommonHelper.DeviceManagerSA.GetSideBottomSwitchSinglePressSetting();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task1.Result)!;
                BottomBarrelButtonClickAction.AssignedAction.ID = jsonObject.GetProperty("actionId").GetInt32();
                if (BottomBarrelButtonClickAction.AssignedAction.ID == 8 || BottomBarrelButtonClickAction.AssignedAction.ID == 23)
                {
                    BottomBarrelButtonClickAction.AssignedAction.Parameter = jsonObject.GetProperty("actionName").GetString()!;
                }


                Task<bool> task2 = DdpmCommonHelper.DeviceManagerSA.GetMenuCenterRightClickSetting();
                IsUseCenter = task2.Result;
                task2 = DdpmCommonHelper.DeviceManagerSA.GetIsSideTopButtonHoverClick();
                IsTopBarrelHoverClickOn = task2.Result;
                task2 = DdpmCommonHelper.DeviceManagerSA.GetIsSideBottomButtonHoverClick();
                IsBottomBarrelHoverClickOn = task2.Result;

                ResetRadialMenu();
                //task1 = DdpmCommonHelper.DeviceManagerSA.GetMenuSinglePressSetting();
                //var result = task1.Result;
                //var RadialMenus = JsonConvert.DeserializeObject<List<RadialMenuItem>>(task1.Result)!;
                //RadialActions.Clear();
                //RadialLabels.Clear();
                //foreach(var rm in RadialMenus)
                //{
                //    if (rm.menuIndex < 8)
                //    {
                //        RadialLabels.Add(rm.menuIndex, rm.actionName);
                //        RadialActions.Add(rm.menuIndex, new SelectedAction(rm.actionId, new AssignedAction(rm.actionId)));
                //    }
                //}
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Actions] PenActions throws exception: {ex.Message} StackTrace: {ex.StackTrace}");
                return;
            }
        }

        public bool RestoreToDefault()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null || !DdpmCommonHelper.DeviceManagerSA.RestoreToDefaultPen().Result)
                return false;

            TopButtonClickAction = new(73, new AssignedAction(73));
            TopButtonDoubleClickAction = new(90, new AssignedAction(90));
            TopButtonPressHoldAction = new(75, new AssignedAction(75));
            TopBarrelButtonClickAction = new(27, new AssignedAction(27));
            BottomBarrelButtonClickAction = new(26, new AssignedAction(26));

            //RestoreRadialMenu();
            return true;
        }

        public bool RestoreRadialMenu()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null || !DdpmCommonHelper.DeviceManagerSA.RestoreRadialMenuToDefault().Result)
                return false;

            ResetRadialMenu();
            return true;
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
            ActionList.ExportActionList(this, "PEN");
        }
    }

    public class KeyboardActions
    {
        public Dictionary<KeyName, SelectedAction> KeyActions = new();
        //public bool IsCollaborationChecked = false;

        public KeyboardActions()
        { }

        public KeyboardActions(string _model, string guid = "")
        {
            if (string.IsNullOrEmpty(_model))
            {
                DdpmCommonHelper.WriteUILog("[KeyboardActions] _model is null");
                return;
            }

            var model = _model.ToUpper();

            DdpmCommonHelper.WriteUILog($"[KeyboardActions] _model is {model}, guid: {guid}");


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

            if (!string.IsNullOrEmpty(guid))
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    try
                    {
                        Task<JArray> task1 = DdpmCommonHelper.DeviceManagerSA.GetKbAssignedActions(guid);
                        var jArray = JArray.FromObject(task1.Result);
                        if (jArray == null)
                        {
                            DdpmCommonHelper.WriteUILog("[GetKbAssignedActions]Failed to convert task result to JArray.");
                            return;
                        }

                        DdpmCommonHelper.WriteUILog($"[GetKbAssignedActions] :{jArray}");
                        List<ActionDetail>? assignedActions = jArray.ToObject<List<ActionDetail>>();

                        if (assignedActions != null)
                        {
                            foreach (var actionDetail in assignedActions)
                            {
                                //var btn = (KeyName)actionDetail.ProgrammableKeyId;
                                var btn = GetKeyName(_model, actionDetail);
                                if (KeyActions.TryGetValue(btn, out SelectedAction? keyAction))
                                {
                                    if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                    {
                                        keyAction.AssignedAction.ID = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid).Key;
                                        if (Actions.AdvancedActions.Contains(keyAction.AssignedAction.ID)) //AssignKeystroke, OpenFile, OpenWebpage, OpenFolder
                                        {
                                            keyAction.AssignedAction.Parameter = actionDetail.DisplayData;
                                        }
                                    }
                                }
                                else
                                {
                                    DdpmCommonHelper.WriteUILog("[KeyboardActions] - KeyActions.TryGetValue == false. ");
                                }
                            }
                            //List<ProgrambleKey> ProgrambleKeys = jArray.ToObject<List<ProgrambleKey>>()!;
                            //foreach (var programbleKey in ProgrambleKeys)
                            //{
                            //    var btn = (KeyName)programbleKey.Id;
                            //    if (KeyActions.ContainsKey(btn) && programbleKey.AssignedAction != null)
                            //    {
                            //        KeyActions[btn].AssignedAction.ID = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == programbleKey.AssignedAction.BaseGuid).Key;
                            //    }
                            //}
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog("[KeyboardActions] - GetKbAssignedActions assignedActions is null");
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"  [KeyboardActions] - GetKbAssignedActions Exception: {ex.Message}");
                    }
                }
                else
                {
                    DdpmCommonHelper.WriteUILog("[KeyboardActions] DdpmCommonHelper.DeviceManagerSA == null");
                }
            }
        }

        private KeyName GetKeyName(string _model, ActionDetail actionDetail)
        {
            switch (_model)
            {
                case "KB525C":
                case "KB900":
                    if (actionDetail.ProgrammableKeyId == 14)      // M2
                        return (KeyName)19;                        // ScrollLock
                    else if (actionDetail.ProgrammableKeyId == 15) // M3
                        return (KeyName)20;                        // PauseBreak
                    else
                        return (KeyName)actionDetail.ProgrammableKeyId;
                default:
                    return (KeyName)actionDetail.ProgrammableKeyId;
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

            if (!string.IsNullOrEmpty(guid))
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    //AllApp
                    DdpmCommonHelper.DeviceManagerSA.SetCurrentSelectedAppSpecificProfile(guid, "{76824745-CE06-4358-835D-7BB991CB71A0}");
                    DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] AllApp Guid:{76824745-CE06-4358-835D-7BB991CB71A0}");
                    Task<JArray> task1 = DdpmCommonHelper.DeviceManagerSA.GetMouseAssignedActions(guid);

                    try
                    {
                        var jArray = JArray.FromObject(task1.Result);
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] AllApp:{jArray}");
                        List<ActionDetail> assignedActions = jArray.ToObject<List<ActionDetail>>()!;
                        foreach (var actionDetail in assignedActions)
                        {
                            var btn = (MouseButtonName)actionDetail.ProgrammableKeyId;
                            if (ButtonActions.TryGetValue(btn, out SelectedMouseAction? buttonAction))
                            {
                                if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                {
                                    buttonAction.AssignedAction.ID = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid).Key;
                                    if (buttonAction.AssignedAction.ID == 14) //AssignKeystroke
                                    {
                                        buttonAction.AssignedAction.Parameter = actionDetail.DisplayData;
                                    }
                                }
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Unknown ButtonAction. {btn.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] got exceptoin: {ex.ToString()}");
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////

                    //Word
                    DdpmCommonHelper.DeviceManagerSA.SetCurrentSelectedAppSpecificProfile(guid, "{E0C9145B-BE8B-4423-B520-8CA71BE88E11}");
                    DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] Word Guid:{E0C9145B-BE8B-4423-B520-8CA71BE88E11}");
                    task1 = DdpmCommonHelper.DeviceManagerSA.GetMouseAssignedActions(guid);

                    try
                    {
                        var jArray = JArray.FromObject(task1.Result);
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Word:{jArray}");
                        List<ActionDetail> assignedActions = jArray.ToObject<List<ActionDetail>>()!;
                        foreach (var actionDetail in assignedActions)
                        {
                            var btn = (MouseButtonName)actionDetail.ProgrammableKeyId;
                            if (ButtonActions.TryGetValue(btn, out SelectedMouseAction? buttonAction))
                            {
                                if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                    buttonAction.OfficeActions["Word"] = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid && x.Key >= 100).Key;
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Word Unknown ButtonAction. {btn.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Word got exceptoin: {ex.ToString()}");
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////

                    //Excel
                    DdpmCommonHelper.DeviceManagerSA.SetCurrentSelectedAppSpecificProfile(guid, "{37743697-4B39-45CD-B7F8-30027D1521ED}");
                    DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] Excel Guid:{37743697-4B39-45CD-B7F8-30027D1521ED}");
                    task1 = DdpmCommonHelper.DeviceManagerSA.GetMouseAssignedActions(guid);
                    try
                    {
                        var jArray = JArray.FromObject(task1.Result);
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Excel:{jArray}");
                        List<ActionDetail> assignedActions = jArray.ToObject<List<ActionDetail>>()!;
                        foreach (var actionDetail in assignedActions)
                        {
                            var btn = (MouseButtonName)actionDetail.ProgrammableKeyId;
                            if (ButtonActions.TryGetValue(btn, out SelectedMouseAction? buttonAction))
                            {
                                if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                    buttonAction.OfficeActions["Excel"] = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid && x.Key >= 200).Key;
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Excel Unknown ButtonAction. {btn.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Excel got exceptoin: {ex.ToString()}");
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////

                    //PowerPoint
                    DdpmCommonHelper.DeviceManagerSA.SetCurrentSelectedAppSpecificProfile(guid, "{7BBECD91-F12A-4CC4-B005-526BA66BA657}");
                    DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] PowerPoint Guid:{7BBECD91-F12A-4CC4-B005-526BA66BA657}");
                    task1 = DdpmCommonHelper.DeviceManagerSA.GetMouseAssignedActions(guid);
                    try
                    {
                        var jArray = JArray.FromObject(task1.Result);
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] PowerPoint:{jArray}");
                        List<ActionDetail> assignedActions = jArray.ToObject<List<ActionDetail>>()!;
                        foreach (var actionDetail in assignedActions)
                        {
                            var btn = (MouseButtonName)actionDetail.ProgrammableKeyId;
                            if (ButtonActions.TryGetValue(btn, out SelectedMouseAction? buttonAction))
                            {
                                if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                    buttonAction.OfficeActions["PowerPoint"] = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid && x.Key >= 300).Key;
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] PowerPoint Unknown ButtonAction. {btn.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] PowerPoint got exceptoin: {ex.ToString()}");
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////


                    //Outlook
                    DdpmCommonHelper.DeviceManagerSA.SetCurrentSelectedAppSpecificProfile(guid, "{CCCE4E6F-C690-4EF5-BA19-F270C26C21B6}");
                    DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] Outlook Guid:{CCCE4E6F-C690-4EF5-BA19-F270C26C21B6}");
                    task1 = DdpmCommonHelper.DeviceManagerSA.GetMouseAssignedActions(guid);
                    try
                    {
                        var jArray = JArray.FromObject(task1.Result);
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Outlook:{jArray}");
                        List<ActionDetail> assignedActions = jArray.ToObject<List<ActionDetail>>()!;
                        foreach (var actionDetail in assignedActions)
                        {
                            var btn = (MouseButtonName)actionDetail.ProgrammableKeyId;
                            if (ButtonActions.TryGetValue(btn, out SelectedMouseAction? buttonAction))
                            {
                                if (Actions.ActionIdToGuid.Any(x => x.Value == actionDetail.BaseGuid))
                                    buttonAction.OfficeActions["Outlook"] = Actions.ActionIdToGuid.FirstOrDefault(x => x.Value == actionDetail.BaseGuid && x.Key >= 400).Key;
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Outlook Unknown ButtonAction. {btn.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DdpmCommonHelper.WriteUILog($"[GetMouseAssignedActions] Outlook got exceptoin: {ex.ToString()}");
                    }
                }
                /////////////////////////////////////////////////////////////////////////////////////////

                //AllApp
                DdpmCommonHelper.DeviceManagerSA?.SetCurrentSelectedAppSpecificProfile(guid, "{76824745-CE06-4358-835D-7BB991CB71A0}");
                DdpmCommonHelper.WriteUILog("[SetCurrentSelectedAppSpecificProfile] AllApp Guid:{76824745-CE06-4358-835D-7BB991CB71A0}");
            }
            else
            {
                DdpmCommonHelper.WriteUILog("[MouseActions] DdpmCommonHelper.DeviceManagerSA == null");
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

                string info = "";
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(fileFolder, out info);   // 20241004 Add for Security
                if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(fileFolder, out info))
                {
                    string strPath = Path.Combine(fileFolder, $"{model}.json");
                    //File.WriteAllText(strPath, json);
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        return DdpmCommonHelper.DeviceManagerSA.WriteSerializedContentToFile(strPath, json).Result;//1007 apply signature
                    }
                }
                else
                    DdpmCommonHelper.WriteUILog($"[ExportActionList] ValidateFilePath failed: {info}");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[ExportActionList] exception: {ex.Message}");
            }
            return false;
        }

        public static object ImportActionList(eDeviceCategory type, string model, string guid = "")
        {
            //var filePath = Path.Combine(Application.StartupPath, @$"ActionList\{model}_{instanceID}.json");
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions\{model}.json");
            var hasFile = File.Exists(filePath);
            string info = "";
            string jsonString = "";
            switch (type)
            {
                case eDeviceCategory.KB:
                    if (hasFile)
                    {
                        //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(filePath, out info))
                        {
                            if (DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                            }
                            if (!string.IsNullOrEmpty(jsonString))
                                return JsonConvert.DeserializeObject<KeyboardActions>(jsonString)!;// File.ReadAllText(filePath))!;
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog($"[ImportActionList] ValidateFilePath failed(model:{model}): {info}");
                        }
                    }
                    var ka = new KeyboardActions(model, guid);
                    ExportActionList(ka, model);
                    return ka;

                case eDeviceCategory.Mouse:
                    if (hasFile)
                    {
                        //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(filePath, out info))
                        {
                            if (DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                            }
                            if (!string.IsNullOrEmpty(jsonString))
                                return JsonConvert.DeserializeObject<MouseActions>(jsonString)!; //File.ReadAllText(filePath))!;
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog($"[ImportActionList] ValidateFilePath failed(model:{model}): {info}");
                        }
                    }
                    var ma = new MouseActions(model, guid);
                    ExportActionList(ma, model);
                    return ma;

                case eDeviceCategory.Pen:
                    if (hasFile)
                    {
                        //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                        if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(filePath, out info))
                        {
                            if (DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                            }
                            if (!string.IsNullOrEmpty(jsonString))
                                return JsonConvert.DeserializeObject<PenActions>(jsonString)!; //File.ReadAllText(filePath))!;
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog($"[ImportActionList] ValidateFilePath failed(model:{model}): {info}");
                        }
                    }
                    var pen = new PenActions(false);
                    return pen;

                default:
                    break;
            }
            return new object();
        }
    }

    public class ProgrambleKey
    {
        public int Id;
        public string Name = "";
        public string ActionName = "";
        public ProgrambleAction AssignedAction = new();
        public List<ProgrambleAction> SuggestedActions = new();
    }

    public class ActionDetail
    {
        public string BaseGuid = "";
        public int ProgrammableKeyId;
        public string DisplayData = "";
        public int ButtonOrKeyId;
        public string DataOnPress = "";
        public string DataOnRelease = "";
    }
    public class ProgrambleAction
    {
        public string BaseGuid = "";
        public string Id = "";
        public string Name = "";
        public string Category = "";
        public List<int> ProgrammableKeys = new();

    }

    public class RadialMenuItem
    {
        public int actionId;
        public string actionName = "";
        public int menuIndex;
    }
}