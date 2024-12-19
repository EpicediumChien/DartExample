using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using System.ComponentModel;
using Windows.System;

namespace DDPM.UI.Module.EzSettings
{
    public class EzSettingsViewModel : ObservableObject
    {
        #region Private members
        //Recent Hotkey TextBox
        private string _recentHotkey = "None";
        //ToggleSwitch IsChecked
        private bool _isWithoutGap = true;
        private bool _isOnlyAllowWhenShiftKeyPressed, _isSpanAcrossMultiMonitors, _isAwsEnabled = false;
        //Span across multiple monitors IsEnabled
        private bool _isSpanAcrossEnabled = false;

        public HomeDevice _homeDevice;
        private readonly IDeviceManagerSA _deviceManagerSA;
        #endregion Private members

        public IModuleOwner? ModuleOwner { get; set; }

        #region ctor
        public EzSettingsViewModel(IModuleOwner moduleOwner)
        {
            _homeDevice = moduleOwner.SelectedHomeDevice;

            //Robert_Lin, 2024-11-28, to get the Span across multiple monitor state changed,
            //Add IDeviceManagerSA, and add a handler for EANotify event
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            if (_deviceManagerSA == null)
                if (DdpmCommonHelper.DeviceManagerSA != null)
                    _deviceManagerSA = DdpmCommonHelper.DeviceManagerSA;

            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EANotify += _deviceManagerSA_EANotify;
            }

        }
        #endregion

        #region RefreshSettings
        public void RefreshSettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    //Read Recent Hotkey settings (TO be implemented by Gavin)
                    var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(_homeDevice.MonitorInfo).Result;
                    HotkeySettings curHotkey = temp.Item1;
                    if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
                    {
                        HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.ToggleEzRecentSetting));
                        if (hotkeyInfo != null)
                        {
                            List<VirtualKey> hotkeys = hotkeyInfo?.Hotkey;
                            string swHortcutText = string.Empty;
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            RecentHotkey = swHortcutText;
                        }
                    }
                    //Read other settings
                    DDPM.SA.Common.Settings.EzSettings ezSettings =
                    DdpmCommonHelper.DeviceManagerSA.ReadEzSettings().Result;

                    //Robert_Lin, 2024-11-28, Get IsSpanEnabled as init state
                    bool isSpanEnabled = _deviceManagerSA.GetIsSpanEnabled().Result;
                    IsSpanAcrossEnabled = isSpanEnabled;

                    //Apply to ViewModel properties
                    IsWithoutGap = ezSettings.IsWidthoutGap;
                    IsOnlyAllowWhenShiftKeyPressed = ezSettings.IsOnlyAllowWhenShiftKeyPressed;
                    IsSpanAcrossMultiMonitors = ezSettings.IsSpanAcrossMultiMonitors;
                    IsAwsEnabled = ezSettings.IsAwsEnabled;

                }
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
        }
        #endregion

        #region Recent Hotkey
        public string RecentHotkey
        {
            get => _recentHotkey;
            set
            {
                SetProperty(ref _recentHotkey, value);
                OnPropertyChanged("RecentHotkey");
            }

        }
        #endregion Recent Hotkey

        #region Without gap
        //Bining to IsChecked
        public bool IsWithoutGap
        {
            get => _isWithoutGap;
            set
            {
                SetProperty(ref _isWithoutGap, value);
                OnPropertyChanged("IsWithoutGap_String");

                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };
                bw.DoWork += delegate
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        DdpmCommonHelper.DeviceManagerSA.WriteEzSettings_IsWidthoutGap(_isWithoutGap);
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    IsBusy = false;
                };
                IsBusy = true;
                bw.RunWorkerAsync();

            }
        }

        //Binding to Content
        public string IsWithoutGap_String
        {
            get
            {
                return IsWithoutGap ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }
        #endregion

        #region Shift Pressed
        //Bining to IsChecked
        public bool IsOnlyAllowWhenShiftKeyPressed
        {
            get => _isOnlyAllowWhenShiftKeyPressed;
            set
            {
                SetProperty(ref _isOnlyAllowWhenShiftKeyPressed, value);
                OnPropertyChanged("IsOnlyAllowWhenShiftKeyPressed_String");

                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };
                bw.DoWork += delegate
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        LogInfo($"Calling to DeviceManagerSA.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed({_isOnlyAllowWhenShiftKeyPressed})");
                        DdpmCommonHelper.DeviceManagerSA.WriteEzSettings_IsOnlyAllowWhenShiftKeyPressed(_isOnlyAllowWhenShiftKeyPressed);
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    IsBusy = false;
                };
                IsBusy = true;
                bw.RunWorkerAsync();
            }
        }

        //Binding to Content
        public string IsOnlyAllowWhenShiftKeyPressed_String
        {
            get
            {
                return IsOnlyAllowWhenShiftKeyPressed ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }
        #endregion

        #region Span Across
        //Bining to IsChecked
        public bool IsSpanAcrossMultiMonitors
        {
            get => _isSpanAcrossMultiMonitors;
            set
            {
                SetProperty(ref _isSpanAcrossMultiMonitors, value);
                OnPropertyChanged("IsSpanAcrossMultiMonitors_String");

                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };
                bw.DoWork += delegate
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        DdpmCommonHelper.DeviceManagerSA.WriteEzSettings_IsSpanAcrossMultiMonitors(_isSpanAcrossMultiMonitors);
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    IsBusy = false;
                };
                IsBusy = true;
                bw.RunWorkerAsync();
            }
        }

        //Binding to Content
        public string IsSpanAcrossMultiMonitors_String
        {
            get
            {
                return IsSpanAcrossMultiMonitors ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }

        //Binding to IsEnabled
        public bool IsSpanAcrossEnabled
        {
            get => _isSpanAcrossEnabled;
            set
            {
                SetProperty(ref _isSpanAcrossEnabled, value);
            }
        }

        //Something changed from Subagent
        private void _deviceManagerSA_EANotify(object? sender, EAArgs e)
        {
            //Command = "SetIsSpanEnabled", ask update IsSpanEnabled from Result
            if (e.Command.Equals(EAEMConstants.EACommand_SetIsSpanEnabled))
            {
                bool newIsSpanEnabled = e.Result;
                IsSpanAcrossEnabled = newIsSpanEnabled;
            }
        }
        #endregion

        #region AWS Enabled
        //Bining to IsChecked
        public bool IsAwsEnabled
        {
            get => _isAwsEnabled;
            set
            {
                SetProperty(ref _isAwsEnabled, value);
                OnPropertyChanged("IsAwsEnabled_String");

                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };
                bw.DoWork += delegate
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        DdpmCommonHelper.DeviceManagerSA.WriteEzSettings_IsAwsEnabled(_isAwsEnabled);
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    IsBusy = false;
                };
                IsBusy = true;
                bw.RunWorkerAsync();
            }
        }

        //Binding to Content
        public string IsAwsEnabled_String
        {
            get
            {
                return IsAwsEnabled ? LangHelper.Instance["On"] : LangHelper.Instance["Off"];
            }
        }
        #endregion

        #region IsBusy

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion IsBusy

        #region Log
        public ILog? Log { get; set; }

        private void LogInfo(string msg)
        {
            if (Log != null)
            {
                Log.Info(msg);
            }
        }
        #endregion Log
    }
}