using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DDPM.UI.Plugin.ViewModels
{
    public class PenViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        private readonly ILog _log;

        private double _tipSensitivity = 0;
        private int _tiltSensitivity = 0;
        private string itemID = "DellPeripheral.Pen.0";

        private string _selectedButton = "";

        private bool IsActionItemsReady = false;
        private Dictionary<int, string> _EraserActions = new();
        private Dictionary<int, string> _SideSwitchActions = new();
        private Dictionary<int, string> _MenuActions = new();
        //private Dictionary<int, string> LaunchableAppValues = new();

        #endregion Variables

        public PenActions PenAction = new();
        public Dictionary<int, string> ActionNames = new();
        public List<string> LaunchableAppValues = new();
        public List<int> RadialMenuActions = new();

        public int AppSelectedIndex { get; set; } = 0;
        public string TopButtonBackground { get; set; } = "";

        public new event PropertyChangedEventHandler? PropertyChanged;

        public PenViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA!)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrepareDeviceInfo(List<DeviceInfo> deviceInfos)
        {
            try
            {
                DeviceInfos.Clear();
                foreach (DeviceInfo deviceInfo in deviceInfos)
                {
                    if (deviceInfo.LogicalDeviceType.Contains("Pen"))
                    {
                        DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs  PrepareDeviceInfo ex:" + ex.Message);
            }
        }

        public override bool SetCurrentDevice(string instanceIDs)
        {
            if (!base.SetCurrentDevice(instanceIDs))
                return false;

            PenAction = (PenActions)ActionList.ImportActionList(eDeviceCategory.Pen, "PEN");
            TiltSensitivity = CurrentDeviceInfo!.TiltSensitivity <= 0 ? 0 : (CurrentDeviceInfo.TiltSensitivity >= 2 ? 100 : 50);
            _tipSensitivity = CurrentDeviceInfo.TipSensitivity switch
            {
                0 => 0,
                1 => 12.5,
                2 => 25,
                3 => 50,
                4 => 62.5,
                5 => 75,
                _ => 100
            };

            if (!IsActionItemsReady)
                PrepareActionItems();

            InitializeButton();
            return true;
        }

        private void PrepareActionItems()
        {
            try
            {
                ////JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(CurrentDeviceInfo!.EraserDoublePressValues))!;
                Task<string> task;
                task = DdpmCommonHelper.DeviceManagerSA!.GetEraserSinglePressValues();
                var str1 = task.Result;
                JsonElement jsonObject;
                jsonObject = JsonSerializer.Deserialize<JsonElement>(str1)!;
                foreach (var jo in jsonObject.EnumerateArray())
                {
                    _EraserActions.Add(jo.GetProperty("actionId").GetInt32(), jo.GetProperty("actionName").GetString()!);
                }

                task = DdpmCommonHelper.DeviceManagerSA!.GetSideSwitchSinglePressValues();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task.Result)!;
                foreach (var jo in jsonObject.EnumerateArray())
                {
                    //var id = jo.GetProperty("actionId").GetInt32();
                    //if (id != 63)
                    _SideSwitchActions.Add(jo.GetProperty("actionId").GetInt32(), jo.GetProperty("actionName").GetString()!);
                }

                task = DdpmCommonHelper.DeviceManagerSA!.GetMenuSinglePressValues();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task.Result)!;
                foreach (var jo in jsonObject.EnumerateArray())
                {
                    var id = jo.GetProperty("actionId").GetInt32();
                    if (id != 63)
                        _MenuActions.Add(id, jo.GetProperty("actionName").GetString()!);
                }

                task = DdpmCommonHelper.DeviceManagerSA!.GetLaunchableAppValues();
                jsonObject = JsonSerializer.Deserialize<JsonElement>(task.Result)!;
                //var i = 1;
                //LaunchableAppValues.Add(Strings.Browse);
                foreach (var jo in jsonObject.EnumerateArray())
                {
                    LaunchableAppValues.Add(jo.GetString()!);
                }
                LaunchableAppValues.Sort();
                for (int i = 0; i < LaunchableAppValues.Count; i++)
                {
                    string str = LaunchableAppValues[i];
                    if (str.Length > 2 && str.Substring(str.Length - 3, 3) == "...")
                    {
                        LaunchableAppValues.Remove(str);
                        LaunchableAppValues.Insert(0, str);
                        i = 100;
                    }
                }
                ActionNames = _EraserActions.Union(_SideSwitchActions).Union(_MenuActions).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                RadialMenuActions = _MenuActions.OrderBy(x => x.Value).Select(x => x.Key).ToList();
                IsActionItemsReady = true;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs PrepareActionItems ex:" + ex.Message);
            }

        }

        private void InitializeButton()
        {
            try
            {
                //Model = "PN7522W";
                //Model = "PN9315A";
                //Model = "PN5122W";
                //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}.png";

                RefreshButtonImageFile(PenButtonName.TopButton.ToString());
                RefreshButtonImageFile(PenButtonName.TopBarrelButton.ToString());
                RefreshButtonImageFile(PenButtonName.BottomBarrelButton.ToString());
                CheckRestoreStatus();
                OnPropertyChanged(nameof(IsRestoreEnable));

                TopButtonBackground = $"/DDPM.UI.Resources;component/Resources/Images/{Model}Top.png";
                OnPropertyChanged(nameof(TopButtonBackground));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs InitializeButton() ex:" + ex.Message);
            }
        }

        public void RefreshButtonImageFile(string btnName, bool IsHover = false, bool IsSelected = false)
        {
            try
            {
                PenButtonName _btnName = (PenButtonName)Enum.Parse(typeof(PenButtonName), btnName, true);
                var property = typeof(PenViewModel).GetProperty($"{btnName}ImageFile");
                int btnType = 1;
                switch (_btnName)
                {
                    case PenButtonName.TopButton:
                        btnType = 1;
                        break;

                    default:
                        if (Model == "PN7522W")
                            btnType = 2;
                        else if (Model == "PN9315A")
                            btnType = 3;
                        else if (Model == "PN5122W")
                            btnType = 4;
                        break;
                }

                var IsDefault = false;
                if (_btnName == PenButtonName.TopButton && PenAction.TopButtonClickAction.DefaultActionID == PenAction.TopButtonClickAction.AssignedAction.ID
                   && PenAction.TopButtonDoubleClickAction.DefaultActionID == PenAction.TopButtonDoubleClickAction.AssignedAction.ID
                   && PenAction.TopButtonPressHoldAction.DefaultActionID == PenAction.TopButtonPressHoldAction.AssignedAction.ID)
                {
                    IsDefault = true;
                }
                else if (_btnName == PenButtonName.TopBarrelButton && PenAction.TopBarrelButtonClickAction.DefaultActionID == PenAction.TopBarrelButtonClickAction.AssignedAction.ID)
                {
                    IsDefault = true;
                }
                else if (_btnName == PenButtonName.BottomBarrelButton && PenAction.BottomBarrelButtonClickAction.DefaultActionID == PenAction.BottomBarrelButtonClickAction.AssignedAction.ID)
                {
                    IsDefault = true;
                }

                if (IsDefault)
                {
                    if (IsSelected)
                    {
                        property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}5.png");
                    }
                    else
                    {
                        if (IsHover)
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}2.png");
                        }
                        else
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}1.png");
                        }
                    }
                }
                else
                {
                    if (IsSelected)
                    {
                        property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}6.png");
                    }
                    else
                    {
                        if (IsHover)
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}4.png");
                        }
                        else
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}3.png");
                        }
                    }
                }
                var a = property.GetValue(this);
                OnPropertyChanged(property!.Name);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs RefreshButtonImageFile ex:" + ex.Message);
            }
        }

        public bool IsRestoreEnable { get; set; } = true;

        private void CheckRestoreStatus(bool? status = null)
        {
            try
            {
                IsRestoreEnable = false;
                if (PenAction.TopButtonClickAction.DefaultActionID != PenAction.TopButtonClickAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.TopButtonDoubleClickAction.DefaultActionID != PenAction.TopButtonDoubleClickAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.TopButtonPressHoldAction.DefaultActionID != PenAction.TopButtonPressHoldAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.TopBarrelButtonClickAction.DefaultActionID != PenAction.TopBarrelButtonClickAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.BottomBarrelButtonClickAction.DefaultActionID != PenAction.BottomBarrelButtonClickAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.IsTopBarrelHoverClickOn)
                {
                    IsRestoreEnable = true;
                }
                else if (PenAction.IsBottomBarrelHoverClickOn)
                {
                    IsRestoreEnable = true;
                }
                else if (TiltSensitivity != 0)
                {
                    IsRestoreEnable = true;
                }
                else if (TipSensitivity != 50)
                {
                    IsRestoreEnable = true;
                }
                OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs CheckRestoreStatus ex:" + ex.Message);
            }
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            try
            {
                base.HandleNotification(changeType, di, property);

                switch (changeType)
                {
                    case DeviceChangedType.Peripherals_SettingsChange:
                        if (DeviceInfos.ContainsKey(di.ID))
                        {
                            DeviceInfos.Remove(di.ID);
                            DeviceInfos.Add(di.ID, di);
                        }
                        else
                        {
                            return;
                        }
                        if (di.ID == CurrentDeviceID)
                        {
                            CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
                            switch (property)
                            {
                                //case "MousePrimaryButtonChanged":
                                //  PrimaryButtonIndex = (int)di.MousePrimaryButton;
                                //  break;
                                //case "TouchScrollSensitivityLevelChanged":
                                //  TouchScrollSensitivityLevel = di.TouchScrollSensitivityLevel;
                                //  break;
                                //case "DpiValueChanged":
                                //  DPIValue = int.Parse(di.DPIValue);
                                //  break;
                                default:
                                    break;
                            }
                            //GenerateInfo();
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs HandleNotification ex:" + ex.Message);
            }
        }

        public double TipSensitivity
        {
            get => _tipSensitivity;
            set
            {
                if (_tipSensitivity != value)
                {
                    _tipSensitivity = value;
                    if (!IsSliderDragging)
                    {
                        SetTipSensitivity();
                    }
                    OnPropertyChanged();
                }
            }
        }
        public void SetTipSensitivity()
        {
            if (_tipSensitivity != CurrentDeviceInfo!.TipSensitivity)
            {
                var value = _tipSensitivity switch
                {
                    0 => 0,
                    12.5 => 1,
                    25 => 2,
                    50 => 3,
                    62.5 => 4,
                    75 => 5,
                    _ => 6
                };
                DdpmCommonHelper.DeviceManagerSA!.SetTipSensitivity(itemID, value);
                CheckRestoreStatus();
            }
        }

        public int TiltSensitivity
        {
            get => _tiltSensitivity;
            set
            {
                if (_tiltSensitivity != value)
                {
                    _tiltSensitivity = value;
                    if (!IsSliderDragging)
                    {
                        SetTiltSensitivity();
                    }
                    OnPropertyChanged();
                }
            }
        }
        public void SetTiltSensitivity()
        {
            try
            {
                if (_tiltSensitivity != CurrentDeviceInfo!.TiltSensitivity)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetTiltSensitivity(itemID, _tiltSensitivity / 50);
                    CheckRestoreStatus();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs SetTiltSensitivity() ex:" + ex.Message);
            }
        }

        public string SelectedButton
        {
            get => _selectedButton;
            set
            {
                _selectedButton = value;
                OnPropertyChanged(nameof(IsHoverClickOn));
                OnPropertyChanged(nameof(IsHoverClickToggleText));
                IsHoverClickVisibility = value == PenButtonName.TopBarrelButton.ToString() || value == PenButtonName.BottomBarrelButton.ToString() ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged(nameof(IsHoverClickVisibility));
            }
        }

        private string _selectedBehavior = "";

        public string SelectedBehavior
        {
            get => _selectedBehavior;
            set
            {
                _selectedBehavior = value;
                OnPropertyChanged(nameof(TopButtonTooltip));
                OnPropertyChanged(nameof(IsSearchEnabled));
            }
        }

        public bool IsSearchEnabled
        {
            get => SelectedButton != PenButtonName.TopButton.ToString() ? true : (SelectedBehavior != "" ? true : false);
        }

        public SelectedAction? SelectedAction => SelectedButton == "" ? null :
                              (SelectedButton == PenButtonName.TopBarrelButton.ToString() ? PenAction.TopBarrelButtonClickAction :
                              (SelectedButton == PenButtonName.BottomBarrelButton.ToString() ? PenAction.BottomBarrelButtonClickAction :
                              (SelectedBehavior == ButtonBehavior.ClickOnce.ToString() ? PenAction.TopButtonClickAction :
                              (SelectedBehavior == ButtonBehavior.DoubleClick.ToString() ? PenAction.TopButtonDoubleClickAction : PenAction.TopButtonPressHoldAction))));

        public int SelectedActionID => SelectedButton == "" ? -1 : SelectedAction?.AssignedAction.ID ?? -1;
        public string TopButtonImageFile { get; set; } = "";
        public string TopBarrelButtonImageFile { get; set; } = "";
        public string BottomBarrelButtonImageFile { get; set; } = "";

        private Visibility _isAllButtonsVisible = Visibility.Visible;

        public Visibility IsAllButtonsVisible
        {
            get => _isAllButtonsVisible;
            set
            {
                _isAllButtonsVisible = value;
                OnPropertyChanged(nameof(IsTopButtonVisible));
                OnPropertyChanged(nameof(IsTopBarrelButtonVisible));
                OnPropertyChanged(nameof(IsBottomBarrelButtonVisible));
            }
        }

        public bool IsTopButtonVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    //return PenAction.Buttons.Contains(PenButtonName.TopButton);
                    return Model != "PN5122W";
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsTopBarrelButtonVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return true;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsBottomBarrelButtonVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return true;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public string TopButtonTooltip
        {
            get
            {
                string tooltip1 = Actions.PenActions[PenAction.TopButtonClickAction.AssignedAction.ID].Caption;
                //string tooltip1 = _EraserActions[PenAction.TopButtonClickAction.AssignedAction.ID];
                string parameter1 = PenAction.TopButtonClickAction.AssignedAction.Parameter;
                if (parameter1 != "")
                {
                    var arr = parameter1.Split('|');
                    if (int.TryParse(arr[0], out int id))
                    {
                        if (id == 1)
                        {
                            //tooltip1 = $"{tooltip1} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                            tooltip1 = $"{tooltip1} : {LaunchableAppValues[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            //tooltip1 = $"{tooltip1} : {Actions.OpenRunActions[id]}";
                            tooltip1 = $"{tooltip1} : {LaunchableAppValues[id]}";
                        }
                    }
                    else
                    {
                        tooltip1 = $"{tooltip1} : {parameter1}";
                    }
                }
                if (SelectedBehavior == ButtonBehavior.ClickOnce.ToString())
                    return $"{Strings.PenButtonClickOnce}: {tooltip1}";

                string tooltip2 = Actions.PenActions[PenAction.TopButtonDoubleClickAction.AssignedAction.ID].Caption;
                var id2 = PenAction.TopButtonDoubleClickAction.AssignedAction.ID;
                //string tooltip2 = _EraserActions[id2];
                string parameter2 = PenAction.TopButtonDoubleClickAction.AssignedAction.Parameter;
                if (parameter2 != "")
                {
                    var arr = parameter2.Split('|');
                    if (int.TryParse(arr[0], out int id))
                    {
                        if (id == 1)
                        {
                            //tooltip2 = $"{tooltip2} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                            tooltip2 = $"{tooltip2} : {LaunchableAppValues[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            //tooltip2 = $"{tooltip2} : {Actions.OpenRunActions[id]}";
                            tooltip2 = $"{tooltip2} : {LaunchableAppValues[id]}";
                        }
                    }
                    else
                    {
                        tooltip2 = $"{tooltip2} : {parameter2}";
                    }
                }
                if (SelectedBehavior == ButtonBehavior.DoubleClick.ToString())
                    return $"{Strings.PenButtonDoubleClick}: {tooltip2}";

                //string tooltip3 = Actions.PenActions[PenAction.TopButtonPressHoldAction.AssignedAction.ID].Caption;
                string tooltip3 = _EraserActions[PenAction.TopButtonPressHoldAction.AssignedAction.ID];
                string parameter3 = PenAction.TopButtonPressHoldAction.AssignedAction.Parameter;
                if (parameter3 != "")
                {
                    var arr = parameter3.Split('|');
                    if (int.TryParse(arr[0], out int id))
                    {
                        if (id == 1)
                        {
                            //tooltip3 = $"{tooltip3} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                            tooltip3 = $"{tooltip3} : {LaunchableAppValues[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            //tooltip3 = $"{tooltip3} : {Actions.OpenRunActions[id]}";
                            tooltip3 = $"{tooltip3} : {LaunchableAppValues[id]}";
                        }
                    }
                    else
                    {
                        tooltip3 = $"{tooltip3} : {parameter3}";
                    }
                }
                if (SelectedBehavior == ButtonBehavior.PressAndHold.ToString())
                    return $"{Strings.PenButtonPressHold}: {tooltip3}";

                return $"{Strings.PenButtonClickOnce}: {tooltip1}\n{Strings.PenButtonDoubleClick}: {tooltip2}\n{Strings.PenButtonPressHold}: {tooltip3}";
            }
        }
        public string TopBarrelButtonTooltip
        {
            get
            {
                string tooltip = Actions.PenActions[PenAction.TopBarrelButtonClickAction.AssignedAction.ID].Caption;
                //string tooltip = _SideSwitchActions[PenAction.TopBarrelButtonClickAction.AssignedAction.ID];
                string parameter = PenAction.TopBarrelButtonClickAction.AssignedAction.Parameter;
                if (parameter != "")
                {
                    var arr = parameter.Split('|');
                    if (int.TryParse(arr[0], out int id))
                    {
                        if (id == 1)
                        {
                            //return $"{tooltip} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                            return $"{tooltip} : {LaunchableAppValues[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            return $"{tooltip} : {Actions.OpenRunActions[id]}";
                        }
                    }
                    else
                    {
                        return $"{tooltip} : {parameter}";
                    }
                }
                return tooltip;
            }
        }
        public string BottomBarrelButtonTooltip
        {
            get
            {
                string tooltip = Actions.PenActions[PenAction.BottomBarrelButtonClickAction.AssignedAction.ID].Caption;
                //string tooltip = _SideSwitchActions[PenAction.BottomBarrelButtonClickAction.AssignedAction.ID];
                string parameter = PenAction.BottomBarrelButtonClickAction.AssignedAction.Parameter;
                if (parameter != "")
                {
                    var arr = parameter.Split('|');
                    if (int.TryParse(arr[0], out int id))
                    {
                        if (id == 1)
                        {
                            //return $"{tooltip} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                            return $"{tooltip} : {LaunchableAppValues[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            //return $"{tooltip} : {Actions.OpenRunActions[id]}";
                            return $"{tooltip} : {LaunchableAppValues[id]}";
                        }
                    }
                    else
                    {
                        return $"{tooltip} : {parameter}";
                    }
                }
                return tooltip;
            }
        }
        public void ClearSelectedButton()
        {
            if (SelectedButton != "")
            {
                RefreshButtonImageFile(SelectedButton);
                SelectedButton = "";
            }
        }
        public bool RestoreToDefault()
        {
            if (PenAction.RestoreToDefault())
            {
                RefreshButtonInfo();
                CurrentDeviceInfo!.TiltSensitivity = 0;
                TiltSensitivity = 0;
                CurrentDeviceInfo!.TipSensitivity = 3;
                TipSensitivity = 50;
                PenAction.IsTopBarrelHoverClickOn = false;
                PenAction.IsBottomBarrelHoverClickOn = false;
                IsRestoreEnable = false;
                OnPropertyChanged(nameof(IsHoverClickOn));
                OnPropertyChanged(nameof(IsHoverClickToggleText));
                OnPropertyChanged(nameof(IsRestoreEnable));
                PenAction = (PenActions)ActionList.ImportActionList(eDeviceCategory.Pen, "PEN");
                return true;
            }
            return false;
        }

        public void RefreshButtonInfo()
        {
            RefreshButtonImageFile(PenButtonName.TopButton.ToString(), false, PenButtonName.TopButton.ToString() == SelectedButton);
            OnPropertyChanged(nameof(TopButtonTooltip));
            OnPropertyChanged(nameof(TopButtonImageFile));
            RefreshButtonImageFile(PenButtonName.TopBarrelButton.ToString(), false, PenButtonName.TopBarrelButton.ToString() == SelectedButton);
            OnPropertyChanged(nameof(TopBarrelButtonTooltip));
            OnPropertyChanged(nameof(TopBarrelButtonImageFile));
            RefreshButtonImageFile(PenButtonName.BottomBarrelButton.ToString(), false, PenButtonName.BottomBarrelButton.ToString() == SelectedButton);
            OnPropertyChanged(nameof(BottomBarrelButtonTooltip));
            OnPropertyChanged(nameof(BottomBarrelButtonImageFile));
        }
        public void UpdateAction(int actionID, string parameter = "")
        {
            try
            {
                if (SelectedButton != "")
                {
                    SelectedAction!.AssignedAction.ID = actionID;
                    SelectedAction.AssignedAction.Parameter = parameter;
                    RefreshButtonInfo();
                    CheckRestoreStatus();
                    var actionName = actionID == 8 || actionID == 23 ? parameter : "";
                    JObject jobj = new()
                {
                    { "actionId", actionID }
                };
                    switch (SelectedButton)
                    {
                        case "TopButton":
                            //var value = $"{{\"actionId\":{actionID},\"actionName\":\"{Actions.PenActions[actionID].Caption}\"}}";
                            if (string.IsNullOrEmpty(actionName))
                            { actionName = _EraserActions[actionID]; }

                            jobj.Add("actionName", actionName);
                            byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                            if (SelectedBehavior == ButtonBehavior.ClickOnce.ToString())
                            {
                                DdpmCommonHelper.DeviceManagerSA!.SetEraserSinglePressSetting(itemID, newValue);
                            }
                            else if (SelectedBehavior == ButtonBehavior.DoubleClick.ToString())
                            {
                                if (actionID == 64)
                                {
                                    newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":65,\"actionName\":\"{actionName}\"}}");
                                }
                                DdpmCommonHelper.DeviceManagerSA!.SetEraserDoublePressSetting(itemID, newValue);
                            }
                            else
                            {
                                if (actionID == 64)
                                {
                                    newValue = Encoding.UTF8.GetBytes($"{{\"actionId\":77,\"actionName\":\"{actionName}\"}}");
                                }
                                DdpmCommonHelper.DeviceManagerSA!.SetEraserLongPressSetting(itemID, newValue);
                            }
                            break;
                        case "TopBarrelButton":
                            if (string.IsNullOrEmpty(actionName))
                            { actionName = _SideSwitchActions[actionID]; }

                            jobj.Add("actionName", actionName);
                            byte[] newValue2 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                            DdpmCommonHelper.DeviceManagerSA!.SetSideTopSwitchSinglePressSetting(itemID, newValue2);
                            break;
                        case "BottomBarrelButton":
                            if (string.IsNullOrEmpty(actionName))
                            { actionName = _SideSwitchActions[actionID]; }

                            jobj.Add("actionName", actionName);
                            byte[] newValue3 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                            DdpmCommonHelper.DeviceManagerSA!.SetSideBottomSwitchSinglePressSetting(itemID, newValue3);
                            break;
                    }
                    ActionList.ExportActionList(PenAction, "PEN");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs UpdateAction ex:" + ex.Message);
            }
        }
        public ObservableCollection<int> SuggestedActionsTopButton { get => new(Actions.SuggestedActionsPenTopButton()); }
        public ObservableCollection<int> SuggestedActionsBarrelButton { get => new(Actions.SuggestedActionsPenBarrelButton()); }
        public ObservableCollection<int> ProductivityActionsTopButton { get; set; } = new(Actions.ProductivityActionsPenTopButton());
        public ObservableCollection<int> ProductivityActionsBarrelButton { get; set; } = new(Actions.ProductivityActionsPenBarrelButton());
        public ObservableCollection<int> WindowsActionsTopButton { get; set; } = new(Actions.WindowsActionsPenTopButton());
        public ObservableCollection<int> WindowsActionsBarrelButton { get; set; } = new(Actions.WindowsActionsPenBarrelButton());
        public ObservableCollection<int> MultimediaActionsTopButton { get; set; } = new(Actions.MultimediaActionsPenTopButton());
        public ObservableCollection<int> MultimediaActionsBarrelButton { get; set; } = new(Actions.MultimediaActionsPenBarrelButton());
        public bool IsHoverClickOn
        {
            get => (SelectedButton == PenButtonName.TopBarrelButton.ToString() && PenAction.IsTopBarrelHoverClickOn) || (SelectedButton == PenButtonName.BottomBarrelButton.ToString() && PenAction.IsBottomBarrelHoverClickOn);
            set
            {
                try
                {
                    if (SelectedButton == PenButtonName.TopBarrelButton.ToString())
                    {
                        DdpmCommonHelper.DeviceManagerSA!.SetIsSideTopButtonHoverClick(itemID, value);
                        PenAction.IsTopBarrelHoverClickOn = value;
                        ActionList.ExportActionList(PenAction, "PEN");
                    }
                    if (SelectedButton == PenButtonName.BottomBarrelButton.ToString())
                    {
                        DdpmCommonHelper.DeviceManagerSA!.SetIsSideBottomButtonHoverClick(itemID, value);
                        PenAction.IsBottomBarrelHoverClickOn = value;
                        ActionList.ExportActionList(PenAction, "PEN");
                    }
                    CheckRestoreStatus();
                    OnPropertyChanged();
                    //IsMicEnumerationOnText = value ? Strings.On : Strings.Off;
                    OnPropertyChanged(nameof(IsHoverClickToggleText));
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs IsHoverClickOn set ex:" + ex.Message);
                }
            }
        }
        public string IsHoverClickToggleText
        {
            get => (SelectedButton == PenButtonName.TopBarrelButton.ToString() && PenAction.IsTopBarrelHoverClickOn) || (SelectedButton == PenButtonName.BottomBarrelButton.ToString() && PenAction.IsBottomBarrelHoverClickOn) ? Strings.On : Strings.Off;
        }
        public Visibility IsHoverClickVisibility { get; set; } = Visibility.Collapsed;


        public void UpdateRadialMenu(int index, int id)
        {
            try
            {
                //byte[] newValue = Encoding.UTF8.GetBytes($"{{\"menuIndex\":{index},\"actionId\":{id},\"actionName\":\"{parameter}\"}}");
                JObject jobj = new()
                {
                    { "menuIndex", index },
                    { "actionId", id },
                    { "actionName", PenAction.RadialLabels[index] },
                    { "menuLabel", PenAction.RadialLabels[index] }
                };
                byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                DdpmCommonHelper.DeviceManagerSA!.SetMenuSinglePressSetting(itemID, newValue);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\PenViewModel.cs UpdateRadialMenu set ex:" + ex.Message);
            }
        }
        public void UpdateRadialMenuRightClick(bool value)
        {
            DdpmCommonHelper.DeviceManagerSA!.SetMenuCenterRightClickSetting(itemID, value);
        }

        public void UnpairPen()
        {
            DdpmCommonHelper.DeviceManagerSA!.UnPairPen(CurrentDeviceID.ToString());
            OnGoBackClicked();
        }
    }
}