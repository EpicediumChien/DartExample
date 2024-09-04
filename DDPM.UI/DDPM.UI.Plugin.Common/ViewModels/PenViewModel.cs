using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace DDPM.UI.Plugin.ViewModels
{
    public class PenViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceManager;

        private int _tipSensitivity = 0;
        private int _tiltSensitivity = 0;
        private string itemID = "DellPeripheral.Pen.0";

        private string _selectedButton = "";

        #endregion Variables

        public int AppSelectedIndex { get; set; } = 0;
        public string TopButtonBackground { get; set; } = "";

        public new event PropertyChangedEventHandler? PropertyChanged;

        public PenViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrepareDeviceInfo(List<DeviceInfo> deviceInfos)
        {
            DeviceInfos.Clear();
            foreach(DeviceInfo deviceInfo in deviceInfos)
            {
                if(deviceInfo.LogicalDeviceType.Contains("Pen"))
                {
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                }
            }
        }

        public override bool SetCurrentDevice(string deviceID)
        {
            if(!base.SetCurrentDevice(deviceID))
                return false;

            TiltSensitivity = CurrentDeviceInfo!.TiltSensitivity <= 0 ? 0 : (CurrentDeviceInfo.TiltSensitivity >= 2 ? 2 : 1);
            TipSensitivity = CurrentDeviceInfo.TipSensitivity;
            PrepareAction();
            InitializeButton();
            return true;
        }

        void PrepareAction()
        {
            JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(CurrentDeviceInfo!.EraserDoublePressValues));
            JsonElement jsonObject2 = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(CurrentDeviceInfo!.SideTopSwitchSinglePressSetting));
            foreach(var jo in jsonObject.EnumerateArray())
            {

            }
        }

        public PenActions PenAction = new();

        private void InitializeButton()
        {
            //Model = "PN7522W";
            //Model = "PN9315A";
            //Model = "PN5122W";
            //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}.png";

            PenAction = (PenActions)ActionList.ImportActionList(eDeviceCategory.Pen, "PEN");

            RefreshButtonImageFile(PenButtonName.TopButton.ToString());
            RefreshButtonImageFile(PenButtonName.TopBarrelButton.ToString());
            RefreshButtonImageFile(PenButtonName.BottomBarrelButton.ToString());
            CheckRestoreStatus();
            OnPropertyChanged(nameof(IsRestoreEnable));

            TopButtonBackground = $"/DDPM.UI.Resources;component/Resources/Images/{Model}Top.png";
            OnPropertyChanged(nameof(TopButtonBackground));
        }

        public void RefreshButtonImageFile(string btnName, bool IsHover = false, bool IsSelected = false)
        {
            PenButtonName _btnName = (PenButtonName)Enum.Parse(typeof(PenButtonName), btnName, true);
            var property = typeof(PenViewModel).GetProperty($"{btnName}ImageFile");
            int btnType = 1;
            switch(_btnName)
            {
                case PenButtonName.TopButton:
                    btnType = 1;
                    break;

                default:
                    if(Model == "PN7522W")
                        btnType = 2;
                    else if(Model == "PN9315A")
                        btnType = 3;
                    else if(Model == "PN5122W")
                        btnType = 4;
                    break;
            }

            var IsDefault = false;
            if(_btnName == PenButtonName.TopButton && PenAction.TopButtonClickAction.DefaultActionID == PenAction.TopButtonClickAction.AssignedAction.ID
               && PenAction.TopButtonDoubleClickAction.DefaultActionID == PenAction.TopButtonDoubleClickAction.AssignedAction.ID
               && PenAction.TopButtonPressHoldAction.DefaultActionID == PenAction.TopButtonPressHoldAction.AssignedAction.ID)
            {
                IsDefault = true;
            }
            else if(_btnName == PenButtonName.TopBarrelButton && PenAction.TopBarrelButtonClickAction.DefaultActionID == PenAction.TopBarrelButtonClickAction.AssignedAction.ID)
            {
                IsDefault = true;
            }
            else if(_btnName == PenButtonName.BottomBarrelButton && PenAction.BottomBarrelButtonClickAction.DefaultActionID == PenAction.BottomBarrelButtonClickAction.AssignedAction.ID)
            {
                IsDefault = true;
            }

            if(IsDefault)
            {
                if(IsSelected)
                {
                    property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}5.png");
                }
                else
                {
                    if(IsHover)
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
                if(IsSelected)
                {
                    property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/PButton{btnType}6.png");
                }
                else
                {
                    if(IsHover)
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

        public bool IsRestoreEnable { get; set; } = true;

        private void CheckRestoreStatus(bool? status = null)
        {
            IsRestoreEnable = false;
            if(PenAction.TopButtonClickAction.DefaultActionID != PenAction.TopButtonClickAction.AssignedAction.ID)
            {
                IsRestoreEnable = true;
            }
            else if(PenAction.TopButtonDoubleClickAction.DefaultActionID != PenAction.TopButtonDoubleClickAction.AssignedAction.ID)
            {
                IsRestoreEnable = true;
            }
            else if(PenAction.TopButtonPressHoldAction.DefaultActionID != PenAction.TopButtonPressHoldAction.AssignedAction.ID)
            {
                IsRestoreEnable = true;
            }
            else if(PenAction.TopBarrelButtonClickAction.DefaultActionID != PenAction.TopBarrelButtonClickAction.AssignedAction.ID)
            {
                IsRestoreEnable = true;
            }
            else if(PenAction.BottomBarrelButtonClickAction.DefaultActionID != PenAction.BottomBarrelButtonClickAction.AssignedAction.ID)
            {
                IsRestoreEnable = true;
            }
            OnPropertyChanged(nameof(IsRestoreEnable));
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            base.HandleNotification(changeType, di, property);

            switch(changeType)
            {
                case DeviceChangedType.Peripherals_SettingsChange:
                    if(DeviceInfos.ContainsKey(di.ID))
                    {
                        DeviceInfos.Remove(di.ID);
                        DeviceInfos.Add(di.ID, di);
                    }
                    else
                    {
                        return;
                    }
                    if(di.ID == CurrentDeviceID)
                    {
                        CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
                        switch(property)
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
                        GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }

        public int TipSensitivity
        {
            get => _tipSensitivity;
            set
            {
                if(_tipSensitivity != value)
                {
                    _tipSensitivity = value;
                    if(!IsSliderDragging)
                    {
                        SetTipSensitivity();
                    }
                    OnPropertyChanged();
                }
            }
        }
        public void SetTipSensitivity()
        {
            if(_tipSensitivity != CurrentDeviceInfo!.TipSensitivity)
                _deviceManager.SetTipSensitivity(itemID, _tipSensitivity);
        }


        public int TiltSensitivity
        {
            get => _tiltSensitivity;
            set
            {
                if(_tiltSensitivity != value)
                {
                    _tiltSensitivity = value;
                    if(!IsSliderDragging)
                    {
                        SetTiltSensitivity();
                    }
                    OnPropertyChanged();
                }
            }
        }
        public void SetTiltSensitivity()
        {
            if(_tiltSensitivity != CurrentDeviceInfo!.TiltSensitivity)
                _deviceManager.SetTiltSensitivity(itemID, _tiltSensitivity);
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
                if(IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    //return PenAction.Buttons.Contains(PenButtonName.TopButton);
                    return Model!="PN5122W";
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
                if(IsAllButtonsVisible == Visibility.Hidden)
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
                if(IsAllButtonsVisible == Visibility.Hidden)
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
                //var tp1 = "";
                string tooltip1 = Actions.PenActions[PenAction.TopButtonClickAction.AssignedAction.ID].Caption;
                string parameter1 = PenAction.TopButtonClickAction.AssignedAction.Parameter;
                if(parameter1 != "")
                {
                    var arr = parameter1.Split('|');
                    if(int.TryParse(arr[0], out int id))
                    {
                        if(id == 1)
                        {
                            tooltip1 = $"{tooltip1} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            tooltip1 = $"{tooltip1} : {Actions.OpenRunActions[id]}";
                        }
                    }
                    else
                    {
                        tooltip1 = $"{tooltip1} : {parameter1}";
                    }
                }
                if(SelectedBehavior == ButtonBehavior.ClickOnce.ToString())
                    return $"{Strings.PenButtonClickOnce}: {tooltip1}";

                string tooltip2 = Actions.PenActions[PenAction.TopButtonDoubleClickAction.AssignedAction.ID].Caption;
                string parameter2 = PenAction.TopButtonDoubleClickAction.AssignedAction.Parameter;
                if(parameter2 != "")
                {
                    var arr = parameter2.Split('|');
                    if(int.TryParse(arr[0], out int id))
                    {
                        if(id == 1)
                        {
                            tooltip2 = $"{tooltip2} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            tooltip2 = $"{tooltip2} : {Actions.OpenRunActions[id]}";
                        }
                    }
                    else
                    {
                        tooltip2 = $"{tooltip2} : {parameter2}";
                    }
                }
                if(SelectedBehavior == ButtonBehavior.DoubleClick.ToString())
                    return $"{Strings.PenButtonDoubleClick}: {tooltip2}";

                string tooltip3 = Actions.PenActions[PenAction.TopButtonPressHoldAction.AssignedAction.ID].Caption;
                string parameter3 = PenAction.TopButtonPressHoldAction.AssignedAction.Parameter;
                if(parameter3 != "")
                {
                    var arr = parameter3.Split('|');
                    if(int.TryParse(arr[0], out int id))
                    {
                        if(id == 1)
                        {
                            tooltip3 = $"{tooltip3} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
                        }
                        else
                        {
                            tooltip3 = $"{tooltip3} : {Actions.OpenRunActions[id]}";
                        }
                    }
                    else
                    {
                        tooltip3 = $"{tooltip3} : {parameter3}";
                    }
                }
                if(SelectedBehavior == ButtonBehavior.PressAndHold.ToString())
                    return $"{Strings.PenButtonPressHold}: {tooltip3}";

                return $"{Strings.PenButtonClickOnce}: {tooltip1}\n{Strings.PenButtonDoubleClick}: {tooltip2}\n{Strings.PenButtonPressHold}: {tooltip3}";
            }
        }
        public string TopBarrelButtonTooltip
        {
            get
            {
                string tooltip = Actions.PenActions[PenAction.TopBarrelButtonClickAction.AssignedAction.ID].Caption;
                string parameter = PenAction.TopBarrelButtonClickAction.AssignedAction.Parameter;
                if(parameter != "")
                {
                    var arr = parameter.Split('|');
                    if(int.TryParse(arr[0], out int id))
                    {
                        if(id == 1)
                        {
                            return $"{tooltip} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
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
                string parameter = PenAction.BottomBarrelButtonClickAction.AssignedAction.Parameter;
                if(parameter != "")
                {
                    var arr = parameter.Split('|');
                    if(int.TryParse(arr[0], out int id))
                    {
                        if(id == 1)
                        {
                            return $"{tooltip} : {Actions.OpenRunActions[id]} \"{arr[1]}\"";
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
        public void ClearSelectedButton()
        {
            if(SelectedButton != "")
            {
                RefreshButtonImageFile(SelectedButton);
                SelectedButton = "";
            }
        }
        public void RestoreToDefault()
        {
            PenAction = new PenActions(Model);
            ActionList.ExportActionList(PenAction, "PEN");
            RefreshButtonInfo();
            IsRestoreEnable = false;
            OnPropertyChanged(nameof(IsRestoreEnable));
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
            if(SelectedButton != "")
            {
                SelectedAction!.AssignedAction.ID = actionID;
                SelectedAction.AssignedAction.Parameter = parameter;
                RefreshButtonInfo();
                CheckRestoreStatus();
                switch(SelectedButton)
                {
                    case "TopButton":
                        if(SelectedBehavior == ButtonBehavior.ClickOnce.ToString())
                        {
                            var value = $"{{\"actionId\":91,\"actionName\":\"Windows 搜尋\"}}";
                            byte[] newValue = Encoding.UTF8.GetBytes(value);
                            _deviceManager.SetEraserSinglePressSetting(itemID, newValue);
                        }
                        else if(SelectedBehavior == ButtonBehavior.DoubleClick.ToString())
                        {
                            var value = $"{{\"actionId\":91,\"actionName\":\"Windows 搜尋\"}}";
                            byte[] newValue = Encoding.UTF8.GetBytes(value);
                            _deviceManager.SetEraserDoublePressSetting(itemID, newValue);
                        }
                        else
                        {
                            var value = $"{{\"actionId\":91,\"actionName\":\"Windows 搜尋\"}}";
                            byte[] newValue = Encoding.UTF8.GetBytes(value);
                            _deviceManager.SetEraserLongPressSetting(itemID, newValue);
                        }
                        break;
                    case "TopBarrelButton":
                        //var value = $"{{\"actionId\":{actionID},\"actionName\":\"{Actions.PenActions[actionID].Caption}\",\"IsHoverEnabled\":true}}";
                        var value4 = $"{{\"actionId\":79,\"actionName\":\"網路瀏覽器\",\"IsHoverEnabled\":true}}";
                        byte[] newValue4 = Encoding.UTF8.GetBytes(value4);
                        //_deviceManager.SetSideTopSwitchSinglePressSetting1(itemID, newValue4);
                        //_deviceManager.SetSideTopSwitchSinglePressSetting2(itemID, value4);
                        _deviceManager.SetSideTopSwitchSinglePressSetting3(newValue4, CurrentDeviceInfo!.ID);
                        break;
                    case "BottomBarrelButton":
                        var value5 = $"{{\"actionId\":79,\"actionName\":\"網路瀏覽器\",\"IsHoverEnabled\":true}}";
                        byte[] newValue5 = Encoding.UTF8.GetBytes(value5);
                        _deviceManager.SetSideBottomSwitchSinglePressSetting(itemID, newValue5);
                        break;
                }
                ActionList.ExportActionList(PenAction, "PEN");
            }
        }
        public ObservableCollection<int> SuggestedActionsTopButton { get => new(Actions.SuggestedActionsPenTopButton); }
        public ObservableCollection<int> SuggestedActionsBarrelButton { get => new(Actions.SuggestedActionsPenBarrelButton); }
        public ObservableCollection<int> ProductivityActionsTopButton { get; set; } = new(Actions.ProductivityActionsPenTopButton);
        public ObservableCollection<int> ProductivityActionsBarrelButton { get; set; } = new(Actions.ProductivityActionsPenBarrelButton);
        public ObservableCollection<int> WindowsActionsTopButton { get; set; } = new(Actions.WindowsActionsPenTopButton);
        public ObservableCollection<int> WindowsActionsBarrelButton { get; set; } = new(Actions.WindowsActionsPenBarrelButton);
        public ObservableCollection<int> MultimediaActionsTopButton { get; set; } = new(Actions.MultimediaActionsPenTopButton);
        public ObservableCollection<int> MultimediaActionsBarrelButton { get; set; } = new(Actions.MultimediaActionsPenBarrelButton);
        public bool IsHoverClickOn
        {
            get => (SelectedButton == PenButtonName.TopBarrelButton.ToString() && PenAction.IsTopBarrelHoverClickOn) || (SelectedButton == PenButtonName.BottomBarrelButton.ToString() && PenAction.IsBottomBarrelHoverClickOn);
            set
            {
                if(SelectedButton == PenButtonName.TopBarrelButton.ToString())
                {
                    _deviceManager.SetIsSideTopButtonHoverClick(itemID, value);
                    PenAction.IsTopBarrelHoverClickOn = value;
                    ActionList.ExportActionList(PenAction, "PEN");
                }
                if(SelectedButton == PenButtonName.BottomBarrelButton.ToString())
                {
                    _deviceManager.SetIsSideBottomButtonHoverClick(itemID, value);
                    PenAction.IsBottomBarrelHoverClickOn = value;
                    ActionList.ExportActionList(PenAction, "PEN");
                }
                OnPropertyChanged();
                //IsHoverClickToggleText = value ? Strings.On : Strings.Off;
                OnPropertyChanged(nameof(IsHoverClickToggleText));
            }
        }
        public string IsHoverClickToggleText
        {
            get => (SelectedButton == PenButtonName.TopBarrelButton.ToString() && PenAction.IsTopBarrelHoverClickOn) || (SelectedButton == PenButtonName.BottomBarrelButton.ToString() && PenAction.IsBottomBarrelHoverClickOn) ? Strings.On : Strings.Off;
        }
        public Visibility IsHoverClickVisibility { get; set; } = Visibility.Collapsed;


        public void UpdateRadialMenu(int index)
        {
            var value = $"{{\"actionId\":79,\"actionName\":\"網路瀏覽器\",\"menuIndex\":{index}}}";
            byte[] newValue = Encoding.UTF8.GetBytes(value);
            _deviceManager.SetMenuSinglePressSetting(itemID, newValue);
        }
        public void UpdateRadialMenuRightClick(bool value)
        {
            _deviceManager.SetMenuCenterRightClickSetting(itemID, value);
        }
    }
}