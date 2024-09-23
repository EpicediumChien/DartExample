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
                PowerNap_text = _powerNapEnabled ? Strings.On : Strings.Off;
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

        public bool AutoApply_Checked { get; set; }

        public bool isSettingsEnable {  get; set; } = true;

        public Visibility LockSettings_Visibility { get; set; } = Visibility.Collapsed;

        public double Settings_Opacity { get; set; } = 1;

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
                PowerNapSetting crtSetting = settings.Find(x => x.SerialNumber == crtSn);
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
            if (ImpExppath.Substring(0,3) == "Imp")
            {
                bool b = DdpmCommonHelper.DeviceManagerSA.DisplayImportSettings(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, AutoApply_Checked, ImpExppath.Substring(3)).Result;
            }
            else if (ImpExppath.Substring(0, 3) == "Exp")
            {
                bool b = DdpmCommonHelper.DeviceManagerSA.DisplayExportSettings(DisplayOthersModule.SelectedHomeDevice.MonitorInfo, ImpExppath.Substring(3)).Result;
            }
        }
        private void ImpExpSettings_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            OnPropertyChanged("IsBusy");
        }

        #endregion

        public bool ExportSettings()
        {
            IsBusy = true;
            OnPropertyChanged("IsBusy");
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "json files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;
                ImpExpSettings("Exp", filename);
                return true;
            }
            else 
            {
                IsBusy = false;
                OnPropertyChanged("IsBusy");
            }

            return false;
        }
        public bool ImportSettings()
        {
            IsBusy = true;
            OnPropertyChanged("IsBusy");
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "jason files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                ImpExpSettings("Imp", filename);
                return true;
            }
            else
            {
                IsBusy = false;
                OnPropertyChanged("IsBusy");
            }

            return false;
        }
    }
}