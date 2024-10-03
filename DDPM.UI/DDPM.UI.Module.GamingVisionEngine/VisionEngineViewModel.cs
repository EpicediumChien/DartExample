using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VcpCore.Common;
using DDPM.SA.Common.Display;
using System.Diagnostics;
using Windows.System;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Gaming.Tests")]

namespace DDPM.UI.Module.GamingVisionEngine
{
    internal class VisionEngineViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
        public VisionEngineModule MyModule { get; set; }
        public ObservableCollection<UI_VisionEngine> VisionEngineList { get; set; }

        public bool VisionEngineIsEnable { get; set; } = true;
        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags
        public void GamingParamChang(object o, GamingDisplayPropertiesInfo e)
        {
            if (e != null)
            {
                if (VisionEngineList != null && e.IsEnable_VisionEngineType != null && VisionEngineList.Count == e.IsEnable_VisionEngineType.Length)
                {
                    for (int i = 0; i < VisionEngineList.Count; i++)
                    {
                        VisionEngineList[i].VisionEngine_Enable = e.IsEnable_VisionEngineType[i];
                    }
                }
            }
            RefreshUI();
        }
        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync();
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                VisionEngineList = new ObservableCollection<UI_VisionEngine>();
                MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                GamingDisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetGamingProperties_SupportedList(currentMonitorInfo).Result;
                displayPropertiesInfo.IsEnable_VisionEngineType = DdpmCommonHelper.DeviceManagerSA.GetCurrentGaming_VisionEngineEnableType(currentMonitorInfo, displayPropertiesInfo).Result;
                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    for (int i = 0; i < displayPropertiesInfo.Supported_VisionEngineType.Count; i++)
                    {
                        VisionEngineList.Add(new UI_VisionEngine(displayPropertiesInfo.IsEnable_VisionEngineType[i], displayPropertiesInfo.Supported_VisionEngineType[i]));
                    }
                }));
                Invoke_RefreshHotkeySettings();
                RefreshUI();
            }
            catch (Exception)
            {
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //Handling the result and final process
        }
        public void SetVisionEngine()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Set_VisionEngine_Dowork;
            bw.RunWorkerCompleted += Set_VisionEngine_Done;
            bw.RunWorkerAsync(ApartmentState.STA);
        }
        private void Set_VisionEngine_Dowork(object sender, DoWorkEventArgs e)
        {
            bool[] b = new bool[VisionEngineList.Count];
            for (int i = 0; i < VisionEngineList.Count; i++)
            {
                if (VisionEngineList[i].VisionEngine_Enable)
                {
                    b[i] = true;
                }
            }
            MonitorInfo currentMonitorInfo = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
            bool ret = DdpmCommonHelper.DeviceManagerSA.SetGaming_VisionEngineEnableType(currentMonitorInfo, b).Result;
        }
        private void Set_VisionEngine_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            VisionEngineIsEnable = true;
            OnPropertyChanged("VisionEngineIsEnable");
        }
        #region hotkey
        private string _visionEngineToggleKey = "None";

        public string VisionEngineToggleKey
        {
            get => _visionEngineToggleKey;
            set
            {
                SetProperty(ref _visionEngineToggleKey, value);
                OnPropertyChanged("VisionEngineToggleKey");
                //NotifyPropertyChanged("VisionEngineToggleKey");
            }
        }

        public void Invoke_RefreshHotkeySettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshHotkeyData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshHotkeyData;
            bw.RunWorkerAsync(ApartmentState.STA);
        }
        private void DoWork_RefreshHotkeyData(object sender, DoWorkEventArgs e)
        {
            HotkeySettings curHotkey = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.MyModule.SelectedHomeDevice.MonitorInfo.edid).Result;
            string swHortcutText = string.Empty;

            if (curHotkey.HotkeyInfo.Count > 0)
            {
                foreach (var hotkeyInfo in curHotkey.HotkeyInfo)
                {
                    List<VirtualKey> hotkeys = hotkeyInfo.Hotkey;
                    switch (hotkeyInfo.Job)
                    {
                        case HotkeyType.VisionEngineToggle:
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            VisionEngineToggleKey = swHortcutText;
                            break;
                    }
                }
            }
        }
        private void RunWorkerCompleted_RefreshHotkeyData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            Debug.WriteLine("RefreshHotkeySettings done");
        }
        #endregion

        public void RefreshUI()
        {
            OnPropertyChanged("VisionEngineList");
            OnPropertyChanged("VisionEngineList_UI");
            OnPropertyChanged("VisionEngineIsEnable");
        }
    }
    internal class UI_VisionEngine
    {
        public bool VisionEngine_Enable { get; set; }
        public Gaming_VisionEngineType VisionEngineType { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{VisionEngineType.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
        public UI_VisionEngine(bool isEnable, Gaming_VisionEngineType type)
        {
            VisionEngine_Enable = isEnable;
            VisionEngineType = type;
        }
    }
}