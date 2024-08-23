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

[assembly: InternalsVisibleTo("DDPM.UI.Module.Gaming.Tests")]

namespace DDPM.UI.Module.GamingVisionEngine
{
    internal class VisionEngineViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
        public VisionEngineModule MyModule { get; set; }
        public List<UI_VisionEngine> VisionEngineList { get; set; }
        public Debouncer VisionEngine_Debouncer;
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
            VisionEngine_Debouncer = new Debouncer(1000, Set_VisionEngine);
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
                VisionEngineList = new List<UI_VisionEngine>();
                GamingDisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetGamingProperties(MyModule.SelectedHomeDevice.MonitorInfo).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {
                    for (int i = 0; i < displayPropertiesInfo.Supported_VisionEngineType.Count; i++)
                    {
                        VisionEngineList.Add(new UI_VisionEngine(displayPropertiesInfo.IsEnable_VisionEngineType[i], displayPropertiesInfo.Supported_VisionEngineType[i]));
                    }
                }));
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
        private void Set_VisionEngine(object o)
        {
            bool[] b = new bool[VisionEngineList.Count];
            for (int i = 0; i < VisionEngineList.Count; i++)
            {
                if (VisionEngineList[i].VisionEngine_Enable)
                {
                    b[i] = true;
                }
            }
            bool ret = DdpmCommonHelper.DeviceManagerSA.SetGaming_VisionEngineEnableType(MyModule.SelectedHomeDevice.MonitorInfo, b).Result;
        }

        public void RefreshUI()
        {
            OnPropertyChanged("VisionEngineList");
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