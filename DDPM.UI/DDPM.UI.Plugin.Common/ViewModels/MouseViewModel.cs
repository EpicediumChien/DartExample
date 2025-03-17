using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.UI.Common;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MouseButton = DPeMPublic.Common.Enums.MouseButton;

namespace DDPM.UI.Plugin.ViewModels
{
    public class MouseViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        private Collection<string> _buttonCollection = new() { LangHelper.Instance["Left"], LangHelper.Instance["Right"] };
        private int _primaryButtonIndex = 0;
        private int _touchScrollSensitivityLevel = -1;
        private int _DPIValue = -1;

        private int _pollingRateSelectedIndex = -1;

        private Visibility _isAllButtonsVisible = Visibility.Visible;

        private Dictionary<string, string> AppGuids = new() {
            {"AllApp","{76824745-CE06-4358-835D-7BB991CB71A0}" },
            {"Word","{E0C9145B-BE8B-4423-B520-8CA71BE88E11}" },
            {"Excel","{37743697-4B39-45CD-B7F8-30027D1521ED}" },
            {"PowerPoint","{7BBECD91-F12A-4CC4-B005-526BA66BA657}" },
            {"Outlook","{CCCE4E6F-C690-4EF5-BA19-F270C26C21B6}" }
        };

        #endregion Variables

        public ICommand Hz125ClickedCommand { get; }
        public ICommand Hz133ClickedCommand { get; }
        public ICommand Hz2501ClickedCommand { get; }
        public ICommand Hz2502ClickedCommand { get; }
        public ICommand Hz333ClickedCommand { get; }

        public string TouchScrollCaption { get; set; } = Strings.TouchScrollCaption;
        public string TouchScrollInfoTip { get; set; } = Strings.TouchScrollInfoTip;
        public string MouseSettingCaption { get; set; } = Strings.MouseSettingsCaption;
        public string PrimaryButtonCaption { get; set; } = Strings.PrimaryButtonCaption;
        public string DPISettingCaption { get; set; } = Strings.DPISettingCaption;
        public string PollingRateCaption { get; set; } = Strings.PollingRateCaption;
        public string PollingRateInfoTip { get; set; } = "";
        public int ButtonCount { get; set; } = 0;

        private string _selectedApp = "";
        public string SelectedApp
        {
            get => _selectedApp;
            set
            {
                _selectedApp = value;
                DdpmCommonHelper.DeviceManagerSA!.SetCurrentSelectedAppSpecificProfile(CurrentDeviceID.ToString(), AppGuids[value]);
            }
        }

        public string RestoreToDefaultText
        {
            get => SelectedApp == "AllApp" ? Strings.RestoreToDefaultActions : Strings.ButtonCustomizeRestoreCaption;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public MouseViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA!)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;

            Hz125ClickedCommand = new RelayCommand(OnHz125Clicked);
            Hz133ClickedCommand = new RelayCommand(OnHz133Clicked);
            Hz2501ClickedCommand = new RelayCommand(OnHz2501Clicked);
            Hz2502ClickedCommand = new RelayCommand(OnHz2502Clicked);
            Hz333ClickedCommand = new RelayCommand(OnHz333Clicked);
        }

        private void OnHz125Clicked()
        {
            if (_pollingRateSelectedIndex == 0)
            { return; }
            _pollingRateSelectedIndex = 0;
            SwitchPollingRate(0, 125, true);
        }
        private void OnHz2501Clicked()
        {
            if (_pollingRateSelectedIndex == 1)
            { return; }
            _pollingRateSelectedIndex = 1;
            SwitchPollingRate(1, 250, true);
        }
        private void OnHz333Clicked()
        {
            if (_pollingRateSelectedIndex == 2)
            { return; }
            _pollingRateSelectedIndex = 2;
            SwitchPollingRate(2, 333, true);
        }
        private void OnHz133Clicked()
        {
            if (_pollingRateSelectedIndex == 3)
            { return; }
            _pollingRateSelectedIndex = 3;
            SwitchPollingRate(3, 133, true);
        }
        private void OnHz2502Clicked()
        {
            if (_pollingRateSelectedIndex == 4)
            { return; }
            _pollingRateSelectedIndex = 4;
            SwitchPollingRate(4, 250, true);
        }
        private void SwitchPollingRate(int index, int hz = 0, bool NeedSetting = false)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Hz125Focused = true;
                        Hz250Focused = false;
                        Hz333Focused = false;
                        OnPropertyChanged(nameof(Hz125Focused));
                        OnPropertyChanged(nameof(Hz250Focused));
                        OnPropertyChanged(nameof(Hz333Focused));
                        break;

                    case 1:
                        Hz125Focused = false;
                        Hz250Focused = true;
                        Hz333Focused = false;
                        OnPropertyChanged(nameof(Hz125Focused));
                        OnPropertyChanged(nameof(Hz250Focused));
                        OnPropertyChanged(nameof(Hz333Focused));
                        break;

                    case 2:
                        Hz125Focused = false;
                        Hz250Focused = false;
                        Hz333Focused = true;
                        OnPropertyChanged(nameof(Hz125Focused));
                        OnPropertyChanged(nameof(Hz250Focused));
                        OnPropertyChanged(nameof(Hz333Focused));
                        break;

                    case 3:
                        Hz133Focused = true;
                        Hz250Focused = false;
                        OnPropertyChanged(nameof(Hz133Focused));
                        OnPropertyChanged(nameof(Hz250Focused));
                        OnPropertyChanged(nameof(Hz333Focused));
                        break;

                    case 4:
                        Hz133Focused = false;
                        Hz250Focused = true;
                        OnPropertyChanged(nameof(Hz133Focused));
                        OnPropertyChanged(nameof(Hz250Focused));
                        break;
                }
                if (NeedSetting)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetReportRate(CurrentDeviceInfo!.ID.ToString(), hz);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs MouseSettingsRightView ex:" + ex.Message);
            }
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
                    if ((deviceInfo.LogicalDeviceType.Contains("Mouse") || EOLMouseList.Contains(deviceInfo.ModelNumber)) && !DeviceInfos.ContainsKey(deviceInfo.ID))
                    {
                        DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs PrepareDeviceInfo ex:" + ex.Message);
            }
        }

        public void RemoveCopilotAction()
        {
            try
            {
                var btn = SelectedButton;
                foreach (var ba in MouseAction.ButtonActions)
                {
                    if (ba.Value.AssignedAction.ID == 1)
                    {
                        SelectedButton = ba.Key.ToString();
                        UpdateAction(ba.Value.DefaultActionID, "", false);
                    }
                }
                SelectedButton = btn;
                InitializeButton();
                RefreshButtonInfo();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs RemoveCopilotAction ex:" + ex.Message);
            }
        }


        public override bool SetCurrentDevice(string instanceIDs)
        {
            if (!base.SetCurrentDevice(instanceIDs))
                return false;

            if (SelectedApp != "AllApp")
                SelectedApp = "AllApp";

            if (!IsCopilotEnabled)
                RemoveCopilotAction();

            IsTouchScrollSensitivitySupported = CurrentDeviceInfo!.IsTouchScrollSensitivitySupported;
            //IsTouchScrollSensitivitySupported = true;
            if (IsTouchScrollSensitivitySupported)
            {
                if (DdpmCommonHelper.DeviceManagerSA == null)
                {
                    if (CurrentDeviceInfo.TouchScrollSensitivityLevel > 0 && CurrentDeviceInfo.TouchScrollSensitivityLevel < 4)
                        _touchScrollSensitivityLevel = 4 - CurrentDeviceInfo.TouchScrollSensitivityLevel;
                    else
                        _touchScrollSensitivityLevel = 1;
                }
                else
                {
                    _touchScrollSensitivityLevel = DdpmCommonHelper.DeviceManagerSA.GetTouchScrollSensitivityLevel(CurrentDeviceInfo!.ID.ToString()).Result;
                    if (_touchScrollSensitivityLevel == -1)
                    {
                        DdpmCommonHelper.WriteUILog("Get TouchScrollSensitivityLevel fail!");
                        _touchScrollSensitivityLevel = 1;
                    }
                    else
                        _touchScrollSensitivityLevel = 4 - _touchScrollSensitivityLevel;
                }
            }
            OnPropertyChanged(nameof(TouchScrollSensitivityLevel));
            OnPropertyChanged(nameof(IsTouchScrollSensitivitySupported));

            //IsDPIValueVisible = CurrentDeviceInfo.IsDPIValueSupported;
            //IsDPIValueVisible = false;
            if (IsDPIValueVisible && !EOLMouseList.Contains(Model))
            {
                //DPIMax = CurrentDeviceInfo.DpiMax;
                //DPIMin = CurrentDeviceInfo.DpiMin;
                //DpiDelta = CurrentDeviceInfo.DpiDelta;
                DPIValue = CurrentDeviceInfo.IsDPILevelSupported ? CurrentDeviceInfo.DpiLevel : int.Parse(CurrentDeviceInfo.DpiValue);
                //OnPropertyChanged(nameof(DPIMax));
                //OnPropertyChanged(nameof(DPIMin));
                //OnPropertyChanged(nameof(DpiDelta));
                OnPropertyChanged(nameof(DPIValue));
                OnPropertyChanged(nameof(IsDPIEnalble));
            }
            OnPropertyChanged(nameof(IsDPIValueVisible));

            IsReportRateSupported = CurrentDeviceInfo.IsReportRateSupported || Model == "MS355";
            //IsReportRateSupported = true;
            if (IsReportRateSupported)
            {
                switch (CurrentDeviceInfo.ReportRate)
                {
                    case 125:
                        _pollingRateSelectedIndex = 0;
                        break;

                    case 250:
                        _pollingRateSelectedIndex = 1;
                        break;

                    case 333:
                        _pollingRateSelectedIndex = 2;
                        break;

                    case 133:
                        _pollingRateSelectedIndex = 3;
                        break;

                    default:
                        _pollingRateSelectedIndex = -1;
                        break;
                }
                if (ConnectionType == "Bluetooth")
                {
                    //_pollingRateSelectedIndex = 3;
                    PollingRateInfoTip = Strings.PollingRateInfoTip2;
                    IsDongleRateVisible = false;
                    IsBluetoothRateVisible = true;
                    if (_pollingRateSelectedIndex == 1)
                    {
                        _pollingRateSelectedIndex = 4;
                    }
                }
                else
                {
                    PollingRateInfoTip = Strings.PollingRateInfoTip1;
                    IsDongleRateVisible = true;
                    IsBluetoothRateVisible = false;
                }
                OnPropertyChanged(nameof(PollingRateInfoTip));
                OnPropertyChanged(nameof(IsDongleRateVisible));
                OnPropertyChanged(nameof(IsBluetoothRateVisible));

                if (_pollingRateSelectedIndex != -1)
                {
                    SwitchPollingRate(_pollingRateSelectedIndex);
                }
            }
            OnPropertyChanged(nameof(IsReportRateSupported));
            ReportRate = CurrentDeviceInfo.ReportRate;

            //PrimaryButtonIndex = CurrentDeviceInfo.MousePrimaryButton == MouseButton.Left ? 0 : 1;
            PrimaryButtonIndex = CallUser32dll.IsPrimaryButtonLeft() ? 0 : 1;
            OnPropertyChanged(nameof(ButtonCollection));

            InitializeButton();
            IsSliderDragging = false;
            isDpiChangePanding = false;
            return true;
        }

        public MouseActions MouseAction = new();
        private void InitializeButton()
        {

            try
            {
                //Model = "MS355";
                //Model = "MS700";
                //Model = "MS900";
                //Model = "MS7421W";
                //Model = "MS300";
                //Model = "MS5120W";
                //Model = "MS3220";
                //Model = "MS5320W";
                //Model = "MS3320W";
                //Model = "WM126";
                //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}.png";

                //MouseAction = (MouseActions)ActionList.ImportActionList(eDeviceCategory.Mouse, Model, CurrentInstanceID);
                MouseAction = (MouseActions)ActionList.ImportActionList(eDeviceCategory.Mouse, Model, CurrentDeviceID.ToString());

                foreach (var kvp in MouseAction.ButtonActions)
                {
                    if (kvp.Value.AssignedAction.ID > 400)
                    {
                        if (OutlookVisibility == Visibility.Collapsed)
                        { kvp.Value.AssignedAction.ID = kvp.Value.DefaultActionID; }
                    }
                    else if (kvp.Value.AssignedAction.ID > 300)
                    {
                        if (PowerPointVisibility == Visibility.Collapsed)
                        { kvp.Value.AssignedAction.ID = kvp.Value.DefaultActionID; }
                    }
                    else if (kvp.Value.AssignedAction.ID > 200)
                    {
                        if (ExcelVisibility == Visibility.Collapsed)
                        { kvp.Value.AssignedAction.ID = kvp.Value.DefaultActionID; }
                    }
                    else if (kvp.Value.AssignedAction.ID > 100)
                    {
                        if (WordVisibility == Visibility.Collapsed)
                        { kvp.Value.AssignedAction.ID = kvp.Value.DefaultActionID; }
                    }
                    RefreshButtonImageFile(kvp.Key.ToString());
                }

                CheckRestoreStatus();
                //OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs InitializeButton ex:" + ex.Message);
            }
        }

        public Visibility WordVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ExcelVisibility { get; set; } = Visibility.Collapsed;
        public Visibility PowerPointVisibility { get; set; } = Visibility.Collapsed;
        public Visibility OutlookVisibility { get; set; } = Visibility.Collapsed;
        public bool IsRestoreEnable { get; set; } = true;
        public void CheckRestoreStatus()
        {
            try
            {
                IsRestoreEnable = false;
                foreach (var btnAction in MouseAction.ButtonActions.Values)
                {
                    if (SelectedApp == "AllApp")
                    {
                        if (btnAction.DefaultActionID != btnAction.AssignedAction.ID)
                        {
                            IsRestoreEnable = true;
                            break;
                        }
                    }
                    else
                    {
                        if (btnAction.OfficeActions[SelectedApp] != -1)
                        {
                            IsRestoreEnable = true;
                            break;
                        }
                    }
                }
                OnPropertyChanged(nameof(IsRestoreEnable));
                OnPropertyChanged(nameof(RestoreToDefaultText));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs CheckRestoreStatus() ex:" + ex.Message);
            }
        }
        public void RefreshButtonImageFile(string btnName, bool IsHover = false, bool IsSelected = false)
        {
            try
            {
                MouseButtonName _btnName = (MouseButtonName)Enum.Parse(typeof(MouseButtonName), btnName, true);
                var property = typeof(MouseViewModel).GetProperty($"{btnName}ImageFile");
                var btnType = _btnName switch
                {
                    MouseButtonName.ScrollTiltLeft => 2,
                    MouseButtonName.ScrollTiltRight => 3,
                    MouseButtonName.SideButtonForward => 4,
                    MouseButtonName.SideButtonBack => 4,
                    _ => 1
                };

                var action = MouseAction.ButtonActions[_btnName];
                if ((SelectedApp == "AllApp" && action.AssignedAction.ID == action.DefaultActionID)
                  || (SelectedApp != "AllApp" && action.OfficeActions[SelectedApp] == -1))
                {
                    if (IsSelected)
                    {
                        property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}5.png");
                    }
                    else
                    {
                        if (IsHover)
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}2.png");
                        }
                        else
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}1.png");
                        }
                    }
                }
                else
                {
                    if (IsSelected)
                    {
                        property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}6.png");
                    }
                    else
                    {
                        if (IsHover)
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}4.png");
                        }
                        else
                        {
                            property!.SetValue(this, $"/DDPM.UI.Resources;component/Resources/Images/MButton{btnType}3.png");
                        }
                    }
                }
                var a = property.GetValue(this);
                OnPropertyChanged(property!.Name);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs RefreshButtonImageFile ex:" + ex.Message);
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
                        if (di.ModelNumber == Model && property == "RestoreToDefault")
                            ResetAction();

                        if (di.ID == CurrentDeviceID)
                        {
                            CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
                            switch (property)
                            {
                                case "MousePrimaryButtonChanged":
                                    _primaryButtonIndex = di.MousePrimaryButton == MouseButton.Left ? 0 : 1;
                                    OnPropertyChanged(nameof(PrimaryButtonIndex));
                                    break;

                                case "TouchScrollSensitivityLevelChanged":
                                    _touchScrollSensitivityLevel = 4 - di.TouchScrollSensitivityLevel;
                                    OnPropertyChanged(nameof(TouchScrollSensitivityLevel));
                                    break;

                                case "DpiValueChanged":
                                    if (!di.IsDPIValueChangePending)
                                    {
                                        if (int.TryParse(di.DpiValue, out int v) && !IsSliderDragging)
                                            DPIValue = v;

                                        CurrentDeviceInfo.DpiValue = di.DpiValue;
                                    }
                                    break;
                                case "DpiLevelChanged":
                                    if (!di.IsDPILevelChangePending && !IsSliderDragging)
                                    {
                                        DPIValue = di.DpiLevel;
                                        CurrentDeviceInfo.DpiLevel = di.DpiLevel;
                                    }
                                    break;

                                case "BatteryLevelChanged":
                                    OnPropertyChanged(nameof(IsDPIEnalble));
                                    break;
                                case "DPILevelChangePendingChanged":
                                case "DPIValueChangePendingChanged":
                                    isDpiChangePanding = di.IsDPILevelChangePending || di.IsDPIValueChangePending;
                                    if (!isDpiChangePanding)
                                        SetDPIValue();

                                    OnPropertyChanged(nameof(DpiChangePandingVisibility));
                                    break;

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
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs HandleNotification ex:" + ex.Message);
            }
        }

        private bool isDpiChangePanding = false;
        public Visibility DpiChangePandingVisibility
        {
            get => isDpiChangePanding ? Visibility.Visible : Visibility.Collapsed;
        }

        public Collection<string> ButtonCollection
        {
            get => _buttonCollection;
            set { _buttonCollection = value; }
        }
        public int TouchScrollSensitivityLevel
        {
            get => _touchScrollSensitivityLevel;
            set
            {
                try
                {
                    if (_touchScrollSensitivityLevel != value)
                    {
                        _touchScrollSensitivityLevel = value;
                        if (value == 2)
                            IsMediumVisible = 1;
                        else
                            IsMediumVisible = 0;

                        if (!IsSliderDragging)
                            SetTouchScrollSensitivityLevel();
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(IsMediumVisible));
                    }
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs ButtonCollection set ex:" + ex.Message);
                }
            }
        }
        public void SetTouchScrollSensitivityLevel()
        {
            //DdpmCommonHelper.DeviceManagerSA!.SetTouchScrollSensitivityLevel(4 - _touchScrollSensitivityLevel, CurrentDeviceInfo!.ID);
            DdpmCommonHelper.DeviceManagerSA!.SetTouchScrollSensitivityLevel(CurrentDeviceInfo!.ID.ToString(), 4 - _touchScrollSensitivityLevel);
        }

        public bool IsTouchScrollSensitivitySupported { get; set; }
        public int IsMediumVisible { get; set; }
        public int PrimaryButtonIndex
        {
            get => _primaryButtonIndex;
            set
            {
                if (_primaryButtonIndex != value)
                {
                    _primaryButtonIndex = value;
                    OnPropertyChanged();
                    //MouseButton button = _primaryButtonIndex == 0 ? MouseButton.Left : MouseButton.Right;
                    //DdpmCommonHelper.DeviceManagerSA!.SetPrimaryMouseButton(button, CurrentDeviceInfo!.ID);
                    CallUser32dll.SetPrimaryButtonToLeft(_primaryButtonIndex == 0);
                }
            }
        }
        public bool IsDPIValueVisible { get => CurrentDeviceInfo!.IsDPILevelSupported || CurrentDeviceInfo.IsDPIValueSupported; }
        public string DPIMinText
        {
            get
            {
                if (CurrentDeviceInfo!.IsDPIValueSupported)
                    return CurrentDeviceInfo.DpiMin.ToString();
                else if (CurrentDeviceInfo.IsDPILevelSupported)
                    return CurrentDeviceInfo.DpiLevelValues[0];
                else
                    return "0";
            }
        }
        public string DPIMaxText
        {
            get
            {
                if (CurrentDeviceInfo!.IsDPIValueSupported)
                    return CurrentDeviceInfo.DpiMax.ToString();
                else if (CurrentDeviceInfo.IsDPILevelSupported)
                    return CurrentDeviceInfo.DpiLevelValues[CurrentDeviceInfo.DpiLevelValues.Length - 1];
                else
                    return "0";
            }
        }
        public int DPIMin
        {
            get
            {
                if (CurrentDeviceInfo!.IsDPIValueSupported)
                    return CurrentDeviceInfo.DpiMin;
                else if (CurrentDeviceInfo.IsDPILevelSupported)
                    return 1;
                else
                    return 0;
            }
        }
        public int DPIMax
        {
            get
            {
                if (CurrentDeviceInfo!.IsDPIValueSupported)
                    return CurrentDeviceInfo.DpiMax;
                else if (CurrentDeviceInfo.IsDPILevelSupported)
                    return CurrentDeviceInfo.DpiLevelValues.Length;
                else
                    return 0;
            }
        }
        public int DpiDelta
        {
            get
            {
                if (CurrentDeviceInfo!.IsDPIValueSupported)
                    return CurrentDeviceInfo.DpiDelta;
                else
                    return 1;
            }
        }

        public int DpiTempValue = 0;
        public int DPIValue
        {
            get => _DPIValue;
            set
            {
                try
                {
                    DPIValueText = CurrentDeviceInfo!.IsDPIValueSupported ? value.ToString() : value < CurrentDeviceInfo.DpiLevelValues.Length ? CurrentDeviceInfo.DpiLevelValues[value - 1] : "";
                    DPITextMargin = GetDpiTextMargin(value);
                    if (value == DPIMax || value == DPIMin || value == -1)
                    {
                        DPIValueText = "";
                    }
                    if (_DPIValue != value)
                    {
                        _DPIValue = value;
                        if (!IsSliderDragging)
                            SetDPIValue();
                        OnPropertyChanged();
                    }
                    OnPropertyChanged(nameof(DPITextMargin));
                    OnPropertyChanged(nameof(DPIValueText));
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs DPIValue set ex:" + ex.Message);
                }
            }
        }
        private double[] GetDpiTextMargin(int value)
        {
            try
            {
                DdpmCommonHelper.WriteUILog($"In GetDpiTextMargin:{Model}");
                switch (Model)
                {
                    case "MS300":
                    case "MS700":
                    case "MS3320W":
                    case "MS5120W":
                    case "MS5320W":
                    case "MS7421W":
                        switch (value)
                        {
                            case 2:
                                return new double[] { 169, 0, 0, 0 };
                            case 3:
                                return new double[] { 287, 0, 0, 0 };
                            default:
                                return new double[] { 0, 0, 0, 0 };
                        }
                    default:
                        var digit = (int)Math.Log10(value);
                        return new double[]
                        {
                            (double)(value - DPIMin) / (double)(DPIMax - DPIMin) * 365.0 + 62 - (double)digit * 5,
                            0,
                            0,
                            1
                        };
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"Error calculating DPI text margin. {ex.Message}");
                // Return a default value or handle the error as needed
                return new double[] { 0, 0, 0, 1 };
            }

        }
        public string DPIValueText { get; set; } = "";
        public double[] DPITextMargin { get; set; } = { 0 };//SDL, change to use array
        public void SetDPIValue()
        {
            try
            {
                if (_DPIValue != int.Parse(CurrentDeviceInfo!.DpiValue) && !isDpiChangePanding)
                {
                    if (CurrentDeviceInfo.IsDPIValueSupported)
                        DdpmCommonHelper.DeviceManagerSA!.SetDPIValue(_DPIValue, CurrentDeviceInfo.ID);
                    if (CurrentDeviceInfo.IsDPILevelSupported)
                        DdpmCommonHelper.DeviceManagerSA!.SetDPILevel(_DPIValue, CurrentDeviceInfo.ID);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs SetDPIValue set ex:" + ex.Message);
            }
        }
        public bool IsReportRateSupported { get; set; }
        public bool IsDongleRateVisible { get; set; }
        public bool IsBluetoothRateVisible { get; set; }
        public int ReportRate { get; set; }
        public bool Hz125Focused { get; set; }
        public bool Hz133Focused { get; set; }
        public bool Hz250Focused { get; set; }
        public bool Hz333Focused { get; set; }

        public Visibility IsDPIEnalble
        {
            get => BatteryLevel == -1 ? Visibility.Visible : Visibility.Hidden;
        }
        public bool RestoreToDefault()
        {
            try
            {
                if (DdpmCommonHelper.DeviceManagerSA == null || !DdpmCommonHelper.DeviceManagerSA.RestoreToDefaultMouse(CurrentDeviceID.ToString(), false).Result)
                    return false;
                DdpmCommonHelper.DeviceManagerSA!.DeleteMouseAllAssignedActions(CurrentDeviceID.ToString());
                foreach (var btn in MouseAction.ButtonActions)
                {
                    if (SelectedApp == "AllApp")
                    {
                        btn.Value.AssignedAction.ID = btn.Value.DefaultActionID;
                        btn.Value.AssignedAction.Parameter = "";
                    }
                    else
                    {
                        //btn.Value.OfficeActions[SelectedApp] = btn.Value.DefaultActionID;
                        btn.Value.OfficeActions[SelectedApp] = -1;
                    }
                }
                ActionList.ExportActionList(MouseAction, Model);
                RefreshButtonInfo();
                IsRestoreEnable = false;
                OnPropertyChanged(nameof(IsRestoreEnable));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs RestoreToDefault  ex:" + ex.Message);
            }
            return true;
        }
        private void ResetAction()
        {
            MouseAction = new(Model);
            RefreshButtonInfo();
            IsRestoreEnable = false;
            OnPropertyChanged(nameof(IsRestoreEnable));
        }

        public string ScrollWheelClickImageFile { get; set; } = "";
        public string ScrollTiltLeftImageFile { get; set; } = "";
        public string ScrollTiltRightImageFile { get; set; } = "";
        public string SideButtonForwardImageFile { get; set; } = "";
        public string SideButtonBackImageFile { get; set; } = "";
        public Visibility IsAllButtonsVisible
        {
            get => _isAllButtonsVisible;
            set
            {
                try
                {
                    if (_isAllButtonsVisible != value)
                    {
                        _isAllButtonsVisible = value;
                        OnPropertyChanged(nameof(IsScrollWheelClickVisible));
                        OnPropertyChanged(nameof(IsScrollTiltLeftVisible));
                        OnPropertyChanged(nameof(IsScrollTiltRightVisible));
                        OnPropertyChanged(nameof(IsSideButtonForwardVisible));
                        OnPropertyChanged(nameof(IsSideButtonBackVisible));
                        OnPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs IsAllButtonsVisible set  ex:" + ex.Message);
                }
            }
        }
        public bool IsScrollWheelClickVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollWheelClick);
            }
            set
            {
                OnPropertyChanged();
            }
        }
        public bool IsScrollTiltLeftVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollTiltLeft);
            }
            set
            {
                OnPropertyChanged();
            }
        }
        public bool IsScrollTiltRightVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollTiltRight);
            }
            set
            {
                OnPropertyChanged();
            }
        }
        public bool IsSideButtonForwardVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return MouseAction.ButtonActions.ContainsKey(MouseButtonName.SideButtonForward);
            }
            set
            {
                OnPropertyChanged();
            }
        }
        public bool IsSideButtonBackVisible
        {
            get
            {
                if (IsAllButtonsVisible == Visibility.Hidden)
                    return false;
                else
                    return MouseAction.ButtonActions.ContainsKey(MouseButtonName.SideButtonBack);
            }
            set
            {
                OnPropertyChanged();
            }
        }
        public string SelectedButton { get; set; } = "";
        public SelectedMouseAction? SelectedMouseAction => string.IsNullOrEmpty(SelectedButton) ? null : MouseAction.ButtonActions[(MouseButtonName)Enum.Parse(typeof(MouseButtonName), SelectedButton, true)];
        public int SelectedActionID => SelectedApp == "AllApp" ? (string.IsNullOrEmpty(SelectedButton) ? -1 : SelectedMouseAction?.AssignedAction.ID ?? -1) : SelectedMouseAction?.OfficeActions[SelectedApp] ?? -1;
        public string ScrollWheelClickTooltip
        {
            get
            {
                return GetButtonTooltip(MouseButtonName.ScrollWheelClick);
            }
        }
        public string ScrollTiltLeftTooltip
        {
            get
            {
                return GetButtonTooltip(MouseButtonName.ScrollTiltLeft);
            }
        }
        public string ScrollTiltRightTooltip
        {
            get
            {
                return GetButtonTooltip(MouseButtonName.ScrollTiltRight);
            }
        }
        public string SideButtonForwardTooltip
        {
            get
            {
                return GetButtonTooltip(MouseButtonName.SideButtonForward);
            }
        }
        public string SideButtonBackTooltip
        {
            get
            {
                return GetButtonTooltip(MouseButtonName.SideButtonBack);
            }
        }
        public void RefreshButtonInfo()
        {
            try
            {
                if (MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollWheelClick))
                {
                    RefreshButtonImageFile(MouseButtonName.ScrollWheelClick.ToString(), false, MouseButtonName.ScrollWheelClick.ToString() == SelectedButton);
                    OnPropertyChanged(nameof(ScrollWheelClickTooltip));
                    OnPropertyChanged(nameof(ScrollWheelClickImageFile));
                }
                if (MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollTiltLeft))
                {
                    RefreshButtonImageFile(MouseButtonName.ScrollTiltLeft.ToString(), false, MouseButtonName.ScrollTiltLeft.ToString() == SelectedButton);
                    OnPropertyChanged(nameof(ScrollTiltLeftTooltip));
                    OnPropertyChanged(nameof(ScrollTiltLeftImageFile));
                }
                if (MouseAction.ButtonActions.ContainsKey(MouseButtonName.ScrollTiltRight))
                {
                    RefreshButtonImageFile(MouseButtonName.ScrollTiltRight.ToString(), false, MouseButtonName.ScrollTiltRight.ToString() == SelectedButton);
                    OnPropertyChanged(nameof(ScrollTiltRightTooltip));
                    OnPropertyChanged(nameof(ScrollTiltRightImageFile));
                }
                if (MouseAction.ButtonActions.ContainsKey(MouseButtonName.SideButtonForward))
                {
                    RefreshButtonImageFile(MouseButtonName.SideButtonForward.ToString(), false, MouseButtonName.SideButtonForward.ToString() == SelectedButton);
                    OnPropertyChanged(nameof(SideButtonForwardTooltip));
                    OnPropertyChanged(nameof(SideButtonForwardImageFile));
                }
                if (MouseAction.ButtonActions.ContainsKey(MouseButtonName.SideButtonBack))
                {
                    RefreshButtonImageFile(MouseButtonName.SideButtonBack.ToString(), false, MouseButtonName.SideButtonBack.ToString() == SelectedButton);
                    OnPropertyChanged(nameof(SideButtonBackTooltip));
                    OnPropertyChanged(nameof(SideButtonBackImageFile));
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs RefreshButtonInfo()  ex:" + ex.Message);
            }
        }
        private string GetButtonTooltip(MouseButtonName btnName)
        {
            if (MouseAction.ButtonActions.TryGetValue(btnName, out SelectedMouseAction? action))
            {
                if (SelectedApp == "AllApp")
                {
                    return GetAllAppTooltip(action);
                }
                else
                {
                    var actionID = action.OfficeActions[SelectedApp];
                    if (actionID == -1)
                    {
                        if (action.AssignedAction.ID == -1)
                        {
                            return string.IsNullOrEmpty(SelectedButton) ? Strings.NullActionTooltip1 : Strings.NullActionTooltip2;
                        }
                        else
                        {
                            return GetAllAppTooltip(action);
                        }
                    }
                    else
                        return Actions.OfficeActions[actionID].Caption;
                }
            }
            return string.Empty;
        }
        private string GetAllAppTooltip(SelectedMouseAction action)
        {
            var parameter = "";
            ActionItem actionItem;
            if (action.AssignedAction.ID == -1)
            {
                if (action.DefaultActionID == -1)
                {
                    return string.IsNullOrEmpty(SelectedButton) ? Strings.NullActionTooltip1 : Strings.NullActionTooltip2;
                }
                actionItem = Actions.KnMActions[action.DefaultActionID];
            }
            else
            {
                actionItem = Actions.KnMActions[action.AssignedAction.ID];
                parameter = action.AssignedAction.Parameter;
            }
            string tooltip = actionItem.Caption!;
            if (!string.IsNullOrEmpty(parameter))
                tooltip += " : " + parameter;
            return tooltip;
        }

        public ObservableCollection<int> SuggestedActions { get => new(Actions.SuggestedActionsM()); }
        public ObservableCollection<int> ProductivityActions { get; set; } = new(Actions.ProductivityActionsKnM());
        public ObservableCollection<int> WindowsActions { get; set; } = new(Actions.WindowsActionsKnM());
        public ObservableCollection<int> MultimediaActions { get; set; } = new(Actions.MultimediaActionsKnM());
        public ObservableCollection<int> WordActions { get; set; } = new(Actions.WordActions());
        public ObservableCollection<int> ExcelActions { get; set; } = new(Actions.ExcelActions());
        public ObservableCollection<int> PowerPointActions { get; set; } = new(Actions.PowerPointActions());
        public ObservableCollection<int> OutlookActions { get; set; } = new(Actions.OutlookActions());
        public void UpdateAction(int actionID, string parameter = "", bool RefreshImage = true)
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedButton))
                {
                    if (SelectedApp == "AllApp")
                    {
                        SelectedMouseAction!.AssignedAction.ID = actionID;
                        SelectedMouseAction.AssignedAction.Parameter = parameter;
                    }
                    else
                    {
                        SelectedMouseAction!.OfficeActions[SelectedApp] = actionID;
                    }

                    int pkId = (int)(MouseButtonName)Enum.Parse(typeof(MouseButtonName), SelectedButton, true);
                    if (actionID == -1)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.DeleteMouseAssignedAction(CurrentDeviceID.ToString(), pkId);
                    }
                    else
                    {
                        JObject jobj = new()
                    {
                        { "PkId", pkId },
                        { "ActionId", Actions.ActionIdToGuid[actionID] }
                    };

                        if (string.IsNullOrEmpty(parameter))
                        {
                            byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                            DdpmCommonHelper.DeviceManagerSA!.SetMouseAction(CurrentDeviceID.ToString(), newValue);
                        }
                        else
                        {
                            jobj.Add("Command", parameter);
                            byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                            if (actionID == 14)
                                DdpmCommonHelper.DeviceManagerSA!.SetMouseAssignKeystrokeAction(CurrentDeviceID.ToString(), newValue);
                            else
                                DdpmCommonHelper.DeviceManagerSA!.SetMouseAssignDialogAction(CurrentDeviceID.ToString(), newValue);
                        }
                    }
                    if (RefreshImage)
                        RefreshButtonInfo();
                    CheckRestoreStatus();
                    ActionList.ExportActionList(MouseAction, Model);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\MouseViewModel.cs UpdateAction  ex:" + ex.Message);
            }

        }
        public void ClearSelectedButton()
        {
            if (!string.IsNullOrEmpty(SelectedButton))
            {
                RefreshButtonImageFile(SelectedButton);
                SelectedButton = "";
            }
            if (SelectedApp != "AllApp")
                SelectedApp = "AllApp";
        }

        public Visibility IsTouchScrollHilighted { get; set; } = Visibility.Collapsed;

        private bool isBatteryUnavailable { get; set; } = false;

        public bool IsBatteryUnavailable
        {
            get { return isBatteryUnavailable; }
            set
            {
                isBatteryUnavailable = value;
                OnPropertyChanged(nameof(IsBatteryUnavailable));
            }
        }
    }
}