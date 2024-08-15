using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Interfaces;
using DDPM.SA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.UI.Common;
using DDPM.SA.Common.Display;
using System.Windows.Forms;

namespace DDPM.UI.Module.DisplayOthers
{
    public class DisplayOthersViewModel : ObservableObject
    {
        private System.Windows.Media.Brush _powerNapColor;

        public IModuleOwner? ModuleOwner { get; set; }
        public DisplayOthersModule DisplayOthersModule { get; set; }

        private string _powerNapText;
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
                PowerNap_text = _powerNapEnabled ? "ON" : "OFF";
                OnPropertyChanged("PowerNap_Enable");
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
                ModelName = DisplayOthersModule.SelectedHomeDevice.MonitorInfo.modelName,
                SerialNumber = DisplayOthersModule.SelectedHomeDevice.MonitorInfo.edid.SerialNumber
            };
            if (!(PutTosleep_Checked || Reducebrt_Checked))
            {
                setting.RunType = PowerNapType.Off;
            }
            else 
            {
                setting.RunType = PutTosleep_Checked ? PowerNapType.SleepIfRunning : PowerNapType.ReduceBrightness;
            }
            DdpmCommonHelper.DeviceManagerSA.SavePowerNapSetting(setting);
        }

        public System.Windows.Media.Brush PowerNap_Color { get; set; }
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
        }
        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try 
            {
                BackgroundWorker bwk = (BackgroundWorker)sender;
                _powerNapEnabled = false;
                //todo get powerNapSupport
                List<SA.Common.Display.PowerNapSetting> settings = DdpmCommonHelper.DeviceManagerSA.ReadPowerNapSettings().Result;
                string crtSn = DisplayOthersModule.SelectedHomeDevice.MonitorInfo.edid.SerialNumber;
                settings.RemoveAll(x => x.SerialNumber == null);
                PowerNapSetting crtSetting =  settings.Find(x => x.SerialNumber == crtSn);
                if (crtSetting != null) 
                {
                    _powerNapEnabled = crtSetting.Status;
                    _powerNapText = _powerNapEnabled ? "ON" : "OFF";
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
                OnPropertyChanged("PowerNap_Enable");
                OnPropertyChanged("Reducebrt_Checked");
                OnPropertyChanged("PutTosleep_Checked");
            }
            catch (Exception) 
            {
                ;
            }
        }
        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
        }

        public bool ExportSettings()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "json files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;
                bool b = DdpmCommonHelper.DeviceManagerSA.DisplayExportSettings(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, filename).Result;
                if (b)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
