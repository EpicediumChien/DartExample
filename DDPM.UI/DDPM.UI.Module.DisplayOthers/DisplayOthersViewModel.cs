using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.UI.Common;
using DDPM.SA.Common.Display;
using System.Windows.Forms;
using System.Windows;
using DDPM.SA.Common.Settings;
using DDPM.UI.Plugin.Common;
using System.IO;
using Dell.Client.Framework.Common;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DDPM.UI.Module.DisplayOthers
{
    public class DisplayOthersViewModel : ObservableObject
    {
        private System.Windows.Media.Brush _powerNapColor;

        public IModuleOwner? ModuleOwner { get; set; }
        public DisplayOthersModule DisplayOthersModule { get; set; }

        public readonly ILog _log;

        private string _powerNapText = Strings.Off;

        public string PowerNap_text
        {
            get => _powerNapText;
            set
            {
                SetProperty(ref _powerNapText, value);
                OnPropertyChanged("PowerNap_text");
            }
        }

        private bool _powerNapEnabled;

        public bool PowerNap_Enable
        {
            get => _powerNapEnabled;
            set
            {

                SetProperty(ref _powerNapEnabled, value);
                PowerNap_text = _powerNapEnabled ? Strings.On : Strings.Off;
                OnPropertyChanged("PowerNap_Enable");
                OnPropertyChanged("PowerNap_text");
                savePowerNapSetting();
            }
        }

        private bool _reducebrtChecked;

        public bool Reducebrt_Checked
        {
            get => _reducebrtChecked;
            set
            {
                SetProperty(ref _reducebrtChecked, value);
                OnPropertyChanged("Reducebrt_Checked");
                savePowerNapSetting();
            }
        }

        private bool _putTosleepChecked;

        public bool PutTosleep_Checked
        {
            get => _putTosleepChecked;
            set
            {
                SetProperty(ref _putTosleepChecked, value);
                OnPropertyChanged("PutTosleep_Checked");
                savePowerNapSetting();
            }
        }

        private void savePowerNapSetting()
        {
            PowerNapSetting setting = new PowerNapSetting
            {
                Status = PowerNap_Enable,
                ModelName = DisplayOthersModule?.SelectedHomeDevice?.MonitorInfo?.modelName,
                SerialNumber = DisplayOthersModule?.SelectedHomeDevice?.MonitorInfo?.edid.SerialNumber,
                ServiceTag = DisplayOthersModule?.SelectedHomeDevice?.MonitorInfo?.edid.ServiceTag
            };
            if (!(PutTosleep_Checked || Reducebrt_Checked))
            {
                setting.RunType = PowerNapType.Off;
            }
            else
            {
                setting.RunType = PutTosleep_Checked ? PowerNapType.SleepIfRunning : PowerNapType.ReduceBrightness;
            }
            DdpmCommonHelper.DeviceManagerSA?.SavePowerNapSetting(setting);
        }

        public System.Windows.Media.Brush PowerNap_Color { get; set; }

        private bool _autoApply_Checked;

        public bool AutoApply_Checked
        {
            get => _autoApply_Checked;
            set
            {
                SetProperty(ref _autoApply_Checked, value);
                DdpmCommonHelper.DeviceManagerSA.SetSameModel(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, _autoApply_Checked).Wait();
                if (_autoApply_Checked)
                {
                    Tooltip_Settings = Strings.ImpExp_Tooltip2;
                }
                else
                {
                    Tooltip_Settings = Strings.ImpExp_Tooltip1;
                }
                OnPropertyChanged("Tooltip_Settings");
            }
        }

        public bool isSettingsEnable { get; set; } = true;

        public Visibility LockSettings_Visibility { get; set; } = Visibility.Collapsed;

        public double Settings_Opacity { get; set; } = 1;

        public bool isLockPowerNapEnable { get; set; } = true;

        public Visibility LockPowerNap_Visibility { get; set; } = Visibility.Collapsed;

        public double LockPowerNap_Opacity { get; set; } = 1;

        public string Tooltip_Settings { get; set; } = Strings.ImpExp_Tooltip1;

        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker bwk = (BackgroundWorker)sender;
                AutoApply_Checked = DdpmCommonHelper.DeviceManagerSA.GetSameModel(DisplayOthersModule.SelectedHomeDevice.MonitorInfo).Result;
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null &&
                    data.LockSettings.Lock_Display_PowerNap)
                {
                    //Do lock ui init here (direct set or binding via vm)
                    LockPowerNap_Visibility = Visibility.Visible;
                    isLockPowerNapEnable = true;
                    LockPowerNap_Opacity = 0.5;
                }
                updatePowerNapUISetting();
            }
            catch (Exception)
            {
                ;
            }
        }

        public void updatePowerNapUISetting()
        {
            _powerNapEnabled = false;
            List<SA.Common.Display.PowerNapSetting> settings = DdpmCommonHelper.DeviceManagerSA.ReadPowerNapSettings().Result;
            string crtSn = DisplayOthersModule.SelectedHomeDevice.MonitorInfo.edid.ServiceTag;
            settings.RemoveAll(x => x.SerialNumber == null);
            PowerNapSetting crtSetting = settings.FirstOrDefault(x => x.ServiceTag == crtSn);
            if (crtSetting != null)
            {
                _powerNapEnabled = crtSetting.Status;
                _powerNapText = _powerNapEnabled ? Strings.On : Strings.Off;
                switch (crtSetting.RunType)
                {
                    case PowerNapType.ReduceBrightness:
                        _reducebrtChecked = true;
                        _putTosleepChecked = false;
                        break;

                    case PowerNapType.SleepIfRunning:
                        _reducebrtChecked = false;
                        _putTosleepChecked = true;
                        break;
                }
            }
            OnPropertyChanged("PowerNap_text");
            OnPropertyChanged("PowerNap_Enable");
            OnPropertyChanged("Reducebrt_Checked");
            OnPropertyChanged("PutTosleep_Checked");
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            if (AutoApply_Checked)
            {
                Tooltip_Settings = Strings.ImpExp_Tooltip2;
            }
            else
            {
                Tooltip_Settings = Strings.ImpExp_Tooltip1;
            }
            OnPropertyChanged("AutoApply_Checked");
            OnPropertyChanged("Tooltip_Settings");
            IsBusy = false;
        }

        #region Imp/Exp Loading

        public void ImpExpSettings(string ImpExp, string path)
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += ImpExpSettings_Dowork;
            bw.RunWorkerCompleted += ImpExpSettings_Done;
            string ImpExppath = ImpExp + path;
            bw.RunWorkerAsync(ImpExppath);
            //IsBusy = true;
            //OnPropertyChanged("IsBusy");
        }

        private void ImpExpSettings_Dowork(object sender, DoWorkEventArgs e)
        {
            string ImpExppath = e.Argument.ToString();
            if (DisplayOthersModule.SelectedHomeDevice.MonitorInfo != null)
            {
                if (ImpExppath.Substring(0, 3) == "Imp")
                {
                    string impPath = ImpExppath.Substring(3);

                    //Elsa add to fix PIMS-313843&PIMS-313845
                    string model = "", serviceTag = "";
                    string strReadJson = string.Empty;
                    string info;
                    strReadJson = DDPMFileSecurity.GetSerializedJsonString(impPath, out info);
                    if (!string.IsNullOrEmpty(strReadJson))
                    {
                        try
                        {
                            using (StreamReader jsonf = new StreamReader(impPath))
                            {
                                string json = jsonf.ReadToEnd();
                                dynamic data = JsonConvert.DeserializeObject(json);
                                model = data.MonitorSettings.Model;
                                serviceTag = data.MonitorSettings.ServiceTag;
                            }
                        }
                        catch (Exception ex)
                        {
                            DdpmCommonHelper.WriteUILog($"[ImpExpSettings_Dowork][Imp] exception: {ex.Message}");
                            OnMessageDlgInvoke("close_loading");
                            OnMessageDlgInvoke("file_corrupted");
                            return;
                        }
                    }
                    if (string.IsNullOrEmpty(strReadJson) || strReadJson.Length == 0)
                    {
                        OnMessageDlgInvoke("close_loading");
                        OnMessageDlgInvoke("file_corrupted");
                        return;
                    }
                    else if (DisplayOthersModule.SelectedHomeDevice.MonitorInfo.modelName != model)
                    {
                        OnMessageDlgInvoke("close_loading");
                        OnMessageDlgInvoke("result_fail");
                        return;
                    }
                    //else if (DisplayOthersModule.SelectedHomeDevice.MonitorInfo.edid.ServiceTag == serviceTag)
                    //{
                    //OnMessageDlgInvoke("import_confirm");
                    //}
                    DisplayImportResultCode importResult = DdpmCommonHelper.DeviceManagerSA.DisplayImportSettings(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, AutoApply_Checked, impPath).Result;
                    if ((int)importResult > 0)
                    {
                        //string fileName = Path.GetFileNameWithoutExtension(impPath);
                        OnMessageDlgInvoke("close_loading");
                        if (importResult == DisplayImportResultCode.DoneWithEzMemoryCleared)
                            OnMessageDlgInvoke($"result_success_{DisplayOthersModule.SelectedHomeDevice.MonitorInfo.modelName}_EzMemory");
                        else
                            OnMessageDlgInvoke($"result_success_{DisplayOthersModule.SelectedHomeDevice.MonitorInfo.modelName}");
                    }
                    else
                    {
                        OnMessageDlgInvoke("close_loading");
                    }
                }
                else if (ImpExppath.Substring(0, 3) == "Exp")
                {
                    if (DdpmCommonHelper.DeviceManagerSA.DisplayExportSettings(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, ImpExppath.Substring(3)).Result)
                    {
                        OnMessageDlgInvoke("close_loading");
                        OnMessageDlgInvoke("result_success");
                    }
                    else
                    {
                        OnMessageDlgInvoke("close_loading");
                    }
                }
            }
            else
            {
                _log.Debug("MonitorInfo is null.");
            }
        }
        private void ImpExpSettings_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            //IsBusy = false;
            //OnMessageDlgInvoke("close_loading");
            //OnMessageDlgInvoke("result_success");
            //OnPropertyChanged("IsBusy");
        }

        #endregion

        public bool ExportSettings()
        {
            //IsBusy = true;
            //OnPropertyChanged("IsBusy");
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "json files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;
                string info = string.Empty;
                if (!DDPM.SA.Common.Security.InputHelper.InputValidation_FilePathFileName(filename, false, out info))
                {
                    //IsBusy = false;
                    OnMessageDlgInvoke("close_loading");
                    //OnPropertyChanged("IsBusy");
                }
                else
                {
                    ImpExpSettings("Exp", filename);
                    return true;
                }
            }
            else
            {
                //IsBusy = false;
                OnMessageDlgInvoke("close_loading");
                //OnPropertyChanged("IsBusy");
            }

            return false;
        }
        public bool ImportSettings()
        {
            //IsBusy = true;
            //OnPropertyChanged("IsBusy");
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "jason files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                string info = string.Empty;
                if (!DDPM.SA.Common.Security.InputHelper.InputValidation_FilePathFileName(filename, true, out info))
                {
                    //IsBusy = false;
                    OnMessageDlgInvoke("close_loading");
                    //OnPropertyChanged("IsBusy");
                }
                else
                {
                    ImpExpSettings("Imp", filename);
                    return true;
                }
            }
            else
            {
                //IsBusy = false;
                OnMessageDlgInvoke("close_loading");
                //OnPropertyChanged("IsBusy");
            }

            return false;
        }

        public EventHandler<string>? ImportExportResult;
        private void OnMessageDlgInvoke(string type)
        {
            EventHandler<string>? handler = ImportExportResult;
            if (handler != null)
            {
                handler.Invoke(this, type);
            }
        }

        public void OnPropertyChanged_Lock()
        {
            OnPropertyChanged("isSettingsEnable");
            OnPropertyChanged("LockSettings_Visibility");
            OnPropertyChanged("Settings_Opacity");
            OnPropertyChanged("isLockPowerNapEnable");
            OnPropertyChanged("LockPowerNap_Visibility");
            OnPropertyChanged("LockPowerNap_Opacity");
            OnPropertyChanged("Tooltip_Settings");
        }

        private static LoadingScreen _dlg_loading = null;
        private void LoadingWindow()
        {
            Window parentWindow = Window.GetWindow(DisplayOthersModule.GetRightView());
            LoadingScreen loadDialog = new LoadingScreen(parentWindow.ActualWidth, parentWindow.ActualHeight);
            if (parentWindow != null)
            {
                loadDialog.Owner = parentWindow;
            }
            _dlg_loading = loadDialog;
            loadDialog.ShowDialog();
        }
    }
}